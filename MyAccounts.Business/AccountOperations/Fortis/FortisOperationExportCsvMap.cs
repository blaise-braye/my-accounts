using CsvHelper.Configuration;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Fortis
{
    public sealed class FortisOperationExportCsvMap : ClassMap<FortisOperation>
    {
        public FortisOperationExportCsvMap()
        {
            Map(m => m.Reference).Name("Numéro de séquence");
            Map(m => m.ExecutionDate).Name("Date d'exécution").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.ValueDate).Name("Date valeur").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.Amount).Name("Montant");
            Map(m => m.Currency).Name("Devise du compte");
            Map(m => m.CounterpartyOfTheTransaction).Name("CONTREPARTIE DE LA TRANSACTION");
            Map(m => m.Detail).Name("Détails");
            Map(m => m.Account).Name("Numéro de compte");
            Map(m => m.SourceKind).Default(SourceKind.Unknwon);
        }
    }
}