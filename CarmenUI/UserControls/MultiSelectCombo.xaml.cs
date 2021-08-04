using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarmenUI.UserControls
{
    /// <summary>
    /// Interaction logic for MultiSelectCombo.xaml
    /// </summary>
    public partial class MultiSelectCombo : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IList), typeof(MultiSelectCombo), new PropertyMetadata(null));

        public IList? ItemsSource
        {
            get => (IList?)GetValue(ItemsSourceProperty);
            set
            {
                SetValue(ItemsSourceProperty, value);
                PopulateComboBox();
            }
        }

        public static readonly DependencyProperty SelectionSourceProperty = DependencyProperty.Register(
            nameof(SelectionSource), typeof(IList), typeof(MultiSelectCombo), new PropertyMetadata(null));

        public IList? SelectionSource
        {
            get => (IList?)GetValue(SelectionSourceProperty);
            set
            {
                SetValue(SelectionSourceProperty, value);
                PopulateComboBox();
            }
        }

        public MultiSelectCombo()
        {
            InitializeComponent();
        }

        private void PopulateComboBox()
        {
            //comboBox.ItemsSource = 
        }
    }
}
