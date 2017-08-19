using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAccounts.Business.GererMesComptes
{
    public class AccountInfo : ObjectFields
    {
        public AccountInfo()
            : this(GetAccountInfoFields())
        {
        }

        private AccountInfo(Dictionary<string, string> fields)
            : base(fields)
        {
        }

        public string Id => GetValue("id_account");

        public string Name => GetValue("name");

        public static AccountInfo Create(Dictionary<string, string> fields, bool validateKnownSignature = true)
        {
            var info = new AccountInfo(fields);
            if (validateKnownSignature)
            {
                var readableProperties = typeof(AccountInfo).GetProperties().Where(p => p.CanRead);
                foreach (var readableProperty in readableProperties)
                {
                    try
                    {
                        readableProperty.GetValue(info);
                    }
                    catch (Exception exn)
                    {
                        throw new InvalidOperationException("signature missmatch, could not read property " + readableProperty.Name, exn);
                    }
                }
            }

            return info;
        }

        private static Dictionary<string, string> GetAccountInfoFields()
        {
            return new Dictionary<string, string>
            {
                { "id_account", string.Empty },
                { "description", string.Empty },
                { "rib_1", string.Empty },
                { "rib_2", string.Empty },
                { "rib_3", string.Empty },
                { "rib_4", string.Empty },
                { "iban_1", string.Empty },
                { "iban_2", string.Empty },
                { "iban_3", string.Empty },
                { "iban_4", string.Empty },
                { "iban_5", string.Empty },
                { "iban_6", string.Empty },
                { "iban_7", string.Empty },
                { "bic_1", string.Empty },
                { "bic_2", string.Empty },
                { "bic_3", string.Empty },
                { "bic_4", string.Empty },
                { "bank_address", string.Empty },
                { "account_owner", string.Empty },
                { "name", string.Empty },
                { "color", string.Empty },
                { "type", string.Empty },
                { "id_account_parent", string.Empty },
                { "group", string.Empty },
                { "currency", string.Empty },
                { "credit_card_day_of_consideration", string.Empty },
                { "credit_card_day_of_consideration_workday", string.Empty },
                { "credit_card_day_of_order", string.Empty },
                { "credit_card_day_of_order_workday", string.Empty },
                { "credit_card_order_active", string.Empty },
                { "credit_card_order_category", string.Empty },
                { "beginning_balance", string.Empty },
                { "debit_credit", string.Empty }
            };
        }
    }
}