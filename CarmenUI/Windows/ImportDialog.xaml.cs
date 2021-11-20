using Carmen.ShowModel.Import;
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
    /// Interaction logic for ImportDialog.xaml
    /// </summary>
    public partial class ImportDialog : Window
    {
        public ImportColumn[] ImportColumns { get; }
        public InputColumn[] InputColumns { get; }

        public ImportDialog(CsvImporter importer)
        {
            ImportColumns = importer.ImportColumns;
            InputColumns = importer.InputColumns;
            InitializeComponent();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
