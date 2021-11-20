using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
            //TODO real data
            string[] header = new[] { "1", "2", "3" };
            object[][] data = new[]
            {
                 new object[] {"a",1,true} ,
                 new object[] {"d",2,false} ,
                 new object[] { "g", 11, true },
            };
            PopulateDataGrid(header, data);
        }

        public void Refresh()
        {
            //TODO refresh
        }

        private void PopulateDataGrid(string[] headers, object[][] data)
        {
            MainData.Columns.Clear();
            int array_index = 0;
            foreach (var header in headers)
                MainData.Columns.Add(new DataGridTextColumn
                {
                    Header = header,
                    Binding = new Binding($"[{array_index++}]"),
                    IsReadOnly = true
                });
            MainData.ItemsSource = data;
        }
    }
}
