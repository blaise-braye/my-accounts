using CsvHelper.Configuration;
using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Fortis
{
    public sealed class FortisOperationArchiveCsvMap : CsvClassMap<FortisOperation>
    {
        public FortisOperationArchiveCsvMap()
        {
            Map(m => m.Reference).Name("ANNEE + REFERENCE");
            Map(m => m.ExecutionDate).Name("DATE DE L'EXECUTION").TypeConverterOption("dd/MM/yyyy");
            Map(m => m.ValueDate).Name("DATE VALEUR").TypeConverterOption("dd/MM/yyyy");
            Map(m => m.Amount).Name("MONTANT");
            Map(m => m.Currency).Name("DEVISE DU COMPTE");
            Map(m => m.CounterpartyOfTheTransaction).Name("CONTREPARTIE DE L'OPERATION");
            Map(m => m.Detail).Name("DETAIL");
            Map(m => m.Account).Name("NUMERO DE COMPTE");
            Map(m => m.SourceKind).Default(SourceKind.Unknwon);
        }
    }
}