using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public interface IReport : INotifyPropertyChanged
    {
        public string ReportType { get; }
        public ObservableCollection<SortColumn> SortColumns { get; }
        public IColumn? GroupColumn { get; set; }
        public int IndexOf(IColumn column);
        public string FullDescription { get; }
        public int ExportCsv(string file_name);
        public IColumn[] Columns { get; }
    }
}
