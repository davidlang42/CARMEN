﻿using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DbContextOptions<ShowContext> contextOptions = new DbContextOptionsBuilder<ShowContext>()
            .UseSqlite("Data Source=test.db").Options; //TODO real connection string

        private ShowContext _context;

        private readonly CollectionViewSource showsViewSource;

        public MainWindow()
        {
            InitializeComponent();
            showsViewSource = (CollectionViewSource)FindResource(nameof(showsViewSource));
            _context = new ShowContext(contextOptions);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _context.Database.EnsureCreated(); // for demo purposes
            PopulateViews();
        }

        private void PopulateViews()
        {
            // Load the entities into EF Core
            _context.Shows.Load();
            // Put collections into view source
            showsViewSource.Source = _context.Shows.Local.ToObservableCollection();
        }

        private void RefreshViews()
        {
            // This updates the views with data generated by DB commit, eg. auto-ids, cascaded-deletes
            showsDataGrid.Items.Refresh();
            sectionsDataGrid.Items.Refresh();
            itemsDataGrid.Items.Refresh();
            rolesDataGrid.Items.Refresh();
            applicantsDataGrid.Items.Refresh();
            groupsDataGrid.Items.Refresh();
            countsDataGrid.Items.Refresh();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.SaveChanges(); // all changes were tracked, including deletes
                RefreshViews();
            } catch (InvalidOperationException ex)
            {
                MessageBox.Show("Could not save changes: \n" + ex.Message);
                _context.Dispose();
                _context = new ShowContext(contextOptions);
                PopulateViews();
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _context.Dispose();
            _context = new ShowContext(contextOptions);
            PopulateViews();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _context.Dispose();
            base.OnClosing(e);
        }
    }
}
