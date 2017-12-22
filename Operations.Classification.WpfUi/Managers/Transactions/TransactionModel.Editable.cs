using System.ComponentModel;
using Operations.Classification.WpfUi.Technical.ChangeTracking;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public sealed partial class TransactionModel : IEditableObject
    {
        private readonly DataTracker _dataTracker = new DataTracker();

        void IEditableObject.BeginEdit()
        {
            _dataTracker.StartTracking(this);
        }

        void IEditableObject.EndEdit()
        {
        }

        void IEditableObject.CancelEdit()
        {
            _dataTracker.ResetOriginalValues();
        }

        public bool IsDirty()
        {
            return _dataTracker.IsDirty;
        }

        public void FillFromDirtyProperties(object targetData)
        {
            _dataTracker.FillFromDirtyProperties(targetData);
        }

        public void StopTracking()
        {
            _dataTracker.StopTracking();
        }
    }
}