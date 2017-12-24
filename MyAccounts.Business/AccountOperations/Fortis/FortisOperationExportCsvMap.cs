using CsvHelper.Configuration;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Fortis
{
    public sealed class FortisOperationExportCsvMap : ClassMap<FortisOperation>
    {
        public FortisOperationExportCsvMap()
        {
            Map(m => m.Reference).Name("Num�ro de s�quence");
            Map(m => m.ExecutionDate).Name("Date d'ex�cution").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.ValueDate).Name("Date valeur").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.Amount).Name("Montant");
            Map(m => m.Currency).Name("Devise du compte");
            Map(m => m.CounterpartyOfTheTransaction).Name("CONTREPARTIE DE LA TRANSACTION");
            Map(m => m.Detail).Name("D�tails");
            Map(m => m.Account).Name("Num�ro de compte");
            Map(m => m.SourceKind).Default(SourceKind.Unknwon);
        }
    }
}