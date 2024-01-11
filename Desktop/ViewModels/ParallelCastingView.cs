using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Audition;
using Carmen.Desktop.Converters;
using Carmen.Desktop.Windows;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelCastingView : IDisposable, INotifyPropertyChanged
    {
        Dictionary<ParallelApplicant, ApplicantDetailsWindow> detailsWindows = new();

        bool disposed;

        readonly ContentControl parent;
        readonly AlternativeCast[] alternativeCasts;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title => $"Parallel casting {Roles.Length} roles in {Node.Name}";

        public Node Node { get; }

        public ParallelRole[] Roles { get; }

        public ListBoxItem[] RoleItems { get; }

        private int selectedRoleIndex = -1;
        public int SelectedRoleIndex
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

        private int selectedApplicantIndex = -1;
        public int SelectedApplicantIndex
        {
            get => selectedApplicantIndex;
            set
            {
                if (selectedApplicantIndex == value)
                    return;
                selectedApplicantIndex = value;
                OnPropertyChanged();
            }
        }

        public ParallelApplicant[] Applicants { get; }

        public ListBoxItem[] ApplicantItems { get; }

        public Canvas Canvas { get; } = new() { ClipToBounds = true };

        public ParallelCastingView(ContentControl applicants_panel, IAllocationEngine engine, Node node, IEnumerable<Role> roles, IEnumerable<Applicant> applicants, Criteria[] primary_criterias, AlternativeCast[] alternative_casts)
        {
            alternativeCasts = alternative_casts;
            parent = applicants_panel;
            Node = node;
            Roles = roles.Select(r => new ParallelRole(r)).ToArray();
            RoleItems = Roles.Select(pr => new ListBoxItem { DataContext = pr, Content = ControlForRoleItem(pr) }).ToArray();
            Applicants = applicants.AsParallel().Select(a =>
            {
                var afrs = new ApplicantForRole[Roles.Length];
                for (var r = 0; r < Roles.Length; r++)
                {
                    afrs[r] = new ApplicantForRole(engine, a, Roles[r].Role, primary_criterias);
                }
                return new ParallelApplicant(this, a, afrs, primary_criterias);
            }).ToArray();
            ApplicantItems = Applicants.Select(pa => new ListBoxItem { DataContext = pa, Content = ControlForApplicantItem(pa) }).ToArray();
        }

        public void UpdateLinePositions()
        {
            Canvas.Children.Clear();
            var canvas_point = Canvas.TransformToAncestor(parent).Transform(new Point(0, 0)); // by top left points
            for (var r = 0; r < Roles.Length; r++)
            {
                for (var a = 0; a < Applicants.Length; a++)
                {
                    var role_point = RoleItems[r].TransformToAncestor(parent).Transform(new Point(0, 0)); // by top left points
                    var applicant_point = ApplicantItems[a].TransformToAncestor(parent).Transform(new Point(0, 0)); // by top left points
                    var line = new Line
                    {
                        X1 = 0,
                        Y1 = role_point.Y + RoleItems[r].ActualHeight / 2 - canvas_point.Y,
                        X2 = Canvas.ActualWidth,
                        Y2 = applicant_point.Y + ApplicantItems[r].ActualHeight / 2 - canvas_point.Y,
                        DataContext = Applicants[a]
                    };
                    var line_color = new MultiBinding
                    {
                        Converter = new ParallelLineColorSelector(alternativeCasts, Colors.Red, Colors.Green, Colors.Black),
                    };
                    line_color.Bindings.Add(new Binding($"{nameof(ParallelApplicant.ApplicantForRoles)}[{r}].{nameof(ApplicantForRole.Role)}")); // the real binding for "Role Fully Cast"
                    line_color.Bindings.Add(new Binding($"{nameof(ParallelApplicant.ApplicantForRoles)}[{r}].{nameof(ApplicantForRole.Role)}.{nameof(Role.Cast)}.{nameof(ICollection<Applicant>.Count)}")); // the fake to make it update when IsSelected is changed on *any* instance of this Role
                    for (var i = 0; i < Roles.Length; i++)
                    {
                        // many bindings for all the roles this applicant could be selected in
                        line_color.Bindings.Add(new Binding($"{nameof(ParallelApplicant.ApplicantForRoles)}[{i}].{nameof(ApplicantForRole.IsSelected)}"));
                    }
                    line.SetBinding(Line.StrokeProperty, line_color);
                    line.SetBinding(Line.StrokeThicknessProperty, new Binding(nameof(ParallelApplicant.SelectedRole))
                    {
                        ConverterParameter = Applicants[a].ApplicantForRoles[r],
                        Converter = new MultiConverter() {
                            new EqualityConverter(),
                            new BooleanToValue(2, 1)
                        }
                    });
                    line.SetBinding(Line.VisibilityProperty, new Binding($"{nameof(ParallelApplicant.ApplicantForRoles)}[{r}].{nameof(ApplicantForRole.IsSelected)}")
                    {
                        Converter = new BooleanToVisibilityConverter()
                    });
                    var r_copy = r;
                    var a_copy = a;
                    line.MouseDown += (o, e) =>
                    {
                        SelectedRoleIndex = r_copy;
                        SelectedApplicantIndex = a_copy;
                        e.Handled = true;
                    };
                    Canvas.Children.Add(line);
                }
            }
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
            suitability.SetBinding(TextBlock.TextDecorationsProperty, new Binding($"{nameof(ParallelApplicant.SelectedRole)}.{nameof(ApplicantForRole.IsCastGroupNeeded)}")
            {
                Converter = new StrikeThroughIfFalse()
            });
            var name = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Text = FullName.Format(pa.Applicant)
            };
            name.SetBinding(TextBlock.TextDecorationsProperty, new Binding($"{nameof(ParallelApplicant.SelectedRole)}.{nameof(ApplicantForRole.IsCastGroupNeeded)}")
            {
                Converter = new StrikeThroughIfFalse()
            });
            var check = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        suitability,
                        name
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

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var window in detailsWindows.Values)
                    window.Close();
                detailsWindows.Clear();
                disposed = true;
            }
        }

        public void ShowDetailsWindow(ShowConnection connection, ParallelApplicant pa, Window owner, Criteria[] criterias, IAuditionEngine audition_engine)
        {
            if (!detailsWindows.TryGetValue(pa, out var window) || window.IsClosed)
            {
                window = new ApplicantDetailsWindow(connection, criterias, audition_engine, pa)
                {
                    Owner = owner
                };
                detailsWindows[pa] = window;
                window.Show();
            }
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }
}
