using CsvHelper.Configuration;
using MyAccounts.Business.AccountOperations.Contracts;

namespace MyAccounts.Business.AccountOperations.Fortis
{
    public sealed class FortisOperationArchiveCsvMap : ClassMap<FortisOperation>
    {
        public FortisOperationArchiveCsvMap()
        {
            Map(m => m.Reference).Name("ANNEE + REFERENCE");
            Map(m => m.ExecutionDate).Name("DATE DE L'EXECUTION").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.ExecutionDate).Name("DATE DE L'EXECUTION").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.ValueDate).Name("DATE VALEUR").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.Amount).Name("MONTANT");
            Map(m => m.Currency).Name("DEVISE DU COMPTE");
            Map(m => m.CounterpartyOfTheTransaction).Name("CONTREPARTIE DE L'OPERATION");
            Map(m => m.Detail).Name("DETAIL");
            Map(m => m.Account).Name("NUMERO DE COMPTE");
            Map(m => m.SourceKind).Default(SourceKind.Unknwon);
        }
    }
}