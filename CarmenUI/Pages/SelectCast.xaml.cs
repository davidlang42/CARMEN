using Microsoft.EntityFrameworkCore;
using ShowModel;
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

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for SelectCast.xaml
    /// </summary>
    public partial class SelectCast : SubPage
    {
        public CastNumberModel[] CastNumbers { get; set; }
        public SelectCast(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            CastNumbers = new CastNumberModel[]
            {
                new AlternateCastMembers
                {
                     CastNumber = 1,
                     TotalCasts = 2,
                     Names = new[] {"Name 1a", "Name 1b"}
                },
                new AlternateCastMembers
                {
                     CastNumber = 2,
                     TotalCasts = 2,
                     Names = new[] {"Name 2a", "Name 2b"}
                },
                new SingleCastMember
                {
                     CastNumber = 3,
                     TotalCasts = 2,
                     Name ="Both cast 1"
                },
                new SingleCastMember
                {
                     CastNumber = 4,
                     TotalCasts = 2,
                     Name ="Both cast 2"
                }
            };
            InitializeComponent();
            DataContext = this;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
                OnReturn(DataObjects.Nodes);
        }

        private void SelectCastButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AvailableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void SelectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addAllButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public abstract class CastNumberModel
    {
        public int CastNumber { get; set; }
        public int TotalCasts { get; set; }
        public abstract string[] GetNames { get; }
        public int CountNames => GetNames.Length;
        public IEnumerable<TextBlock> TextBlocks
            => GetNames.Select(n => new TextBlock { Text = n });
    }

    public class SingleCastMember : CastNumberModel
    {
        public string Name { get; set; } = "";
        public override string[] GetNames => new[] { Name };

    }

    public class AlternateCastMembers : CastNumberModel
    {
        public string[] Names { get; set; } = new string[0];

        public override string[] GetNames => Names;
    }
}
