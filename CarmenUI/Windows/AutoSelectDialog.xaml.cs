﻿using CarmenUI.ViewModels;
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
    /// Interaction logic for AutoSelectDialog.xaml
    /// </summary>
    public partial class AutoSelectDialog : Window
    {
        public AutoSelectDialog(AutoSelectSettings settings)
        {
            InitializeComponent();
            this.DataContext = settings;
        }

        private void SelectCast_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
