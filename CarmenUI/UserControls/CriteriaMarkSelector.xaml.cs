using ShowModel.Criterias;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarmenUI.UserControls
{
    /// <summary>
    /// Interaction logic for CriteriaMarkSelector.xaml
    /// </summary>
    public partial class CriteriaMarkSelector : UserControl
    {
        public static readonly DependencyProperty CriteriaProperty = DependencyProperty.Register(
            nameof(Criteria), typeof(Criteria), typeof(CriteriaMarkSelector), new PropertyMetadata(null));

        public static readonly DependencyProperty MarkProperty = DependencyProperty.Register(
            nameof(Mark), typeof(uint?), typeof(CriteriaMarkSelector), new PropertyMetadata(null));

        public Criteria Criteria
        {
            get => (Criteria)GetValue(CriteriaProperty);
            set => SetValue(CriteriaProperty, value);
        }

        public uint? Mark
        {
            get => (uint?)GetValue(MarkProperty);
            set => SetValue(MarkProperty, value);
        }
        
        public CriteriaMarkSelector()
        {
            InitializeComponent();
        }
    }
}
