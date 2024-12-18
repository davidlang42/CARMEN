﻿using Carmen.ShowModel.Import;
using Serilog;
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

namespace Carmen.Desktop.Windows
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
            Log.Information(nameof(ImportDialog));
            ImportColumns = importer.ImportColumns;
            InputColumns = importer.InputColumns;
            LoadImportSettings();
            InitializeComponent();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ImportColumns.Any(c => c.SelectedInput != null))
            {
                MessageBox.Show("At least one field must be mapped to an input column.", Window.Title);
                return;
            }
            foreach (var column in ImportColumns)
            {
                if (column.SelectedInput == null && column.MatchExisting)
                {
                    if (MessageBox.Show($"'{column.Name}' is selected to match existing applicants but does not have an input column selected.\nDo you want to untick 'match existing applicants' and continue?", Window.Title, MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    column.MatchExisting = false;
                }
            }
            if (!ImportColumns.Any(c => c.MatchExisting))
            {
                if (MessageBox.Show("No fields are selected to match existing applicants. This means every row will be added as a NEW applicant and may cause duplicates.\nDo you want to continue?", Window.Title, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            SaveImportSettings();
            DialogResult = true;
        }

        private void ClearInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
            => ((ImportColumn)e.Parameter).SelectedInput = null;

        private void LoadImportSettings()
        {
            foreach (var input in InputColumns)
                if (Properties.Imports.Default.ColumnMappings.TryGetValue(input.Header, out var import_name)
                    && ImportColumns.FirstOrDefault(c => c.Name.Equals(import_name, StringComparison.OrdinalIgnoreCase)) is ImportColumn import)
                    import.SelectedInput = input;
            foreach (var import in ImportColumns)
                if (Properties.Imports.Default.MatchingColumns.TryGetValue(import.Name, out var match_existing))
                    import.MatchExisting = match_existing;
        }

        private void SaveImportSettings()
        {
            foreach (var import in ImportColumns.Where(c => c.SelectedInput != null))
            {
                Properties.Imports.Default.ColumnMappings[import.SelectedInput!.Header] = import.Name;
                Properties.Imports.Default.MatchingColumns[import.Name] = import.MatchExisting;
            }
        }
    }
}
