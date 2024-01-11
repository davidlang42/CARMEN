using Carmen.CastingEngine.Allocation;
using Carmen.Desktop.Converters;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelCastingView : INotifyPropertyChanged
    {
        readonly ContentControl parent;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title => $"Parallel casting {Roles.Length} roles in {Node.Name}";

        public Node Node { get; }

        public ParallelRole[] Roles { get; }

        public ListBoxItem[] RoleItems { get; }

        private int? selectedRoleIndex = null;
        public int? SelectedRoleIndex
        {
            get => selectedRoleIndex;
            set
            {
                if (selectedRoleIndex == value)
                    return;
                selectedRoleIndex = value;
                OnPropertyChanged();
            }
        }

        public ParallelApplicant[] Applicants { get; }

        public ListBoxItem[] ApplicantItems { get; }

        public Canvas Canvas { get; } = new Canvas();

        public ParallelCastingView(ContentControl applicants_panel, IAllocationEngine engine, Node node, IEnumerable<Role> roles, IEnumerable<Applicant> applicants, Criteria[] primary_criterias)
        {
            parent = applicants_panel;
            Node = node;
            Roles = roles.Select(r => new ParallelRole(r)).ToArray();
            RoleItems = Roles.Select(pr => new ListBoxItem { Content = ControlForRoleItem(pr) }).ToArray();
            //requiredCastGroups = role.CountByGroups.Where(cbg => cbg.Count != 0).Select(cbg => cbg.CastGroup).ToHashSet();
            Applicants = applicants.AsParallel().Select(a =>
            {
                var afrs = new ApplicantForRole[Roles.Length];
                for (var r = 0; r < Roles.Length; r++)
                {
                    afrs[r] = new ApplicantForRole(engine, a, Roles[r].Role, primary_criterias);
                }
                var pa = new ParallelApplicant(this, a, afrs);
                //pa.PropertyChanged += ParallelApplicant_PropertyChanged;
                return pa;
            }).ToArray();
            ApplicantItems = Applicants.Select(pa => new ListBoxItem { Content = ControlForApplicantItem(pa) }).ToArray();
            //var view = (CollectionView)CollectionViewSource.GetDefaultView(Applicants);
            //view.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(ApplicantForRole.CastGroupAndCast)}.{nameof(CastGroupAndCast.Name)}"));
            //ConfigureFiltering(show_unavailable, show_ineligible, show_unneeded);
            //ConfigureSorting(new[] { (nameof(ApplicantForRole.Suitability), ListSortDirection.Descending) });
        }

        //private void ParallelApplicant_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ParallelApplicant.SelectedForRoles))
        //    {
        //        UpdateLines();
        //    }
        //}

        public int UpdateLinesCount { get; private set; } = 0;

        public void UpdateLines()//TODO UpdateLinePositions
        {
            UpdateLinesCount += 1;
            OnPropertyChanged(nameof(UpdateLinesCount));
            Canvas.Children.Clear();
            try
            {
                for (var r = Roles.Length - 1; r < Roles.Length; r++)
                {
                    for (var a = Applicants.Length - 1; a < Applicants.Length; a++)
                    {
                        var role_point = RoleItems[r].TransformToAncestor(parent).Transform(new Point(0, 0)); // by top left points
                        var applicant_point = ApplicantItems[a].TransformToAncestor(parent).Transform(new Point(0, 0)); // by top left points
                        Canvas.Children.Add(new Line
                        {
                            X1 = role_point.X + RoleItems[r].ActualWidth,
                            Y1 = role_point.Y + RoleItems[r].ActualHeight / 2,
                            X2 = applicant_point.X,
                            Y2 = applicant_point.Y + ApplicantItems[r].ActualHeight / 2,
                            Stroke = new SolidColorBrush
                            {
                                Color = Colors.Black
                            }
                        });
                    }
                }
            }
            catch (InvalidOperationException) { }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private TextBlock ControlForRoleItem(ParallelRole pr)
        {
            return new TextBlock { Text = pr.Description };
        }

        private CheckBox ControlForApplicantItem(ParallelApplicant pa)
        {
            var suitability = new TextBlock { TextWrapping = TextWrapping.Wrap };
            suitability.SetBinding(TextBlock.TextProperty, new Binding($"{nameof(ParallelApplicant.SelectedRole)}.{nameof(ApplicantForRole.Suitability)}")
            {
                Converter = new DoubleToPercentage(),
                StringFormat = "({0})"
            });
            var check = new CheckBox
            {
                DataContext = pa,
                VerticalAlignment = VerticalAlignment.Center,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        suitability,
                        new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Text = FullName.Format(pa.Applicant)
                        }
                    }
                }
            };
            check.SetBinding(CheckBox.IsCheckedProperty, new Binding($"{nameof(ParallelApplicant.SelectedRole)}.{nameof(ApplicantForRole.IsSelected)}"));
            check.SetBinding(CheckBox.IsEnabledProperty, new Binding(nameof(ParallelApplicant.SelectedRole))
            {
                Converter = new MultiConverter
                {
                    new TrueIfNull(),
                    new InvertBoolean()
                }
            });
            return check;
        }
    }
}
