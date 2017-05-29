using System;
using System.Collections.Generic;
using System.Linq;
using QifApi.Transactions;

namespace Operations.Classification.GererMesComptes
{
    public class TransactionDeltaSet
    {
        private readonly List<TransactionDelta> _deltas = new List<TransactionDelta>();
        private readonly int[] _counters = new int[Enum.GetValues(typeof(DeltaAction)).Length];
        private readonly HashSet<BasicTransaction> _processedTarget = new HashSet<BasicTransaction>();
        
        public List<TransactionDelta> ToList()
        {
            return _deltas.OrderByDescending(t=>t.Source?.Date ?? t.Target?.Date).ToList();
        }

        public int NewCount => _counters[(int)DeltaAction.Add];

        public int DeleteCount => _counters[(int)DeltaAction.Remove];

        public int UpdateMemoCount => _counters[(int)DeltaAction.UpdateMemo];

        public int NothingCount => _counters[(int)DeltaAction.Nothing];

        public DateTime? LastActionInsertTime { get; set; }
        
        public bool IsTargetProcessed(BasicTransaction transaction)
        {
            return _processedTarget.Contains(transaction);
        }

        public void SetAddAction(BasicTransaction transaction)
        {
            SetAction(transaction, null, DeltaAction.Add);
        }

        public void SetUpdateMemoAction(BasicTransaction transaction, BasicTransaction targetTransaction)
        {
            SetAction(transaction, targetTransaction, DeltaAction.UpdateMemo);
        }

        public void SetUpdateMemoAction(TransactionDelta originalDelta, BasicTransaction targetTransaction)
        {
            if (originalDelta.Action != DeltaAction.MultipleTargetsPossible)
            {
                throw new InvalidOperationException();
            }

            _deltas.Remove(originalDelta);
            _counters[(int)originalDelta.Action]--;
            SetUpdateMemoAction(originalDelta.Source, targetTransaction);
        }

        public void SetRemoveAction(BasicTransaction targetTransaction)
        {
            SetAction(null, targetTransaction, DeltaAction.Remove);
        }

        public void SetNotUniqueKeyInTarget(BasicTransaction transaction)
        {
            SetAction(transaction, null, DeltaAction.NotUniqueKeyInTarget);
        }

        public void SetNothingAction(BasicTransaction availableBt, BasicTransaction exportedItem)
        {
            SetAction(availableBt, exportedItem, DeltaAction.Nothing);
        }

        public void SetMultipleTargetsPossibleAction(BasicTransaction transaction)
        {
            SetAction(transaction, null, DeltaAction.MultipleTargetsPossible);
        }

        private void SetAction(BasicTransaction transaction, BasicTransaction targetTransaction, DeltaAction action)
        {
            var transactionLookupKey = (transaction ?? targetTransaction).GetBankTransactionLookupKey();
            var transactionDelta = new TransactionDelta
            {
                DeltaKey = transactionLookupKey,
                Source = transaction,
                Target = targetTransaction,
                Action = action
            };

            LastActionInsertTime = DateTime.Now;
            _deltas.Add(transactionDelta);
            _counters[(int)transactionDelta.Action]++;

            if (targetTransaction != null)
            {
                _processedTarget.Add(targetTransaction);
            }
        }

        public List<TransactionDelta> GetDeltaByAction(DeltaAction action)
        {
            return _deltas.Where(t => t.Action == action).ToList();
        }

        public IEnumerable<BasicTransaction> GetRemoteTransactions()
        {
            return _deltas.Select(d => d.Target).Where(t => t != null);
        }
    }
}