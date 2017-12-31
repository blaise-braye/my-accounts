using System.Linq;
using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class RowModel : ObservableObject
    {
        private CompareCellModel[] _cells;

        private string _header;

        public string Header
        {
            get => _header;
            set => Set(nameof(Header), ref _header, value);
        }

        public CompareCellModel[] Cells
        {
            get => _cells;
            set => Set(nameof(Cells), ref _cells, value);
        }

        public CompareCellModel this[int cellId] => Cells[cellId];

        public CompareCellModel this[string cellId] => Cells.First(c => c.Title == cellId);
    }
}