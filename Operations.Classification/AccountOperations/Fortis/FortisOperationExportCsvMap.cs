using CsvHelper.Configuration;

using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Fortis
{
    public sealed class FortisOperationExportCsvMap : CsvClassMap<FortisOperation>
    {
        public FortisOperationExportCsvMap()
        {
            Map(m => m.Reference).Name("Numéro de séquence");
            Map(m => m.ExecutionDate).Name("Date d'exécution").TypeConverterOption("dd/MM/yyyy");
            Map(m => m.ValueDate).Name("Date valeur").TypeConverterOption("dd/MM/yyyy");
            Map(m => m.Amount).Name("Montant");
            Map(m => m.Currency).Name("Devise du compte");
            Map(m => m.CounterpartyOfTheTransaction).Ignore();
            Map(m => m.Detail).Name("Détails");
            Map(m => m.Account).Name("Numéro de compte");
            Map(m => m.SourceKind).Default(SourceKind.Unknwon);
        }
    }
}