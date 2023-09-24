using Carmen.Mobile.Collections;
using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class ApplicantList : ContentPage
    {
        readonly Applicants model;
        readonly ConnectionDetails show;
        readonly ListView list;
        readonly FieldGetter<Applicant> detailGetter;
        readonly ApplicantComparer sortBy;
        ShowContext? context;

        public ApplicantList(ConnectionDetails show, string show_name, string filter_name, Func<Applicant, string, bool> filter_function, Func<Applicant, string> detail_getter, Func<Applicant, IComparable?>? sort_getter = null)
        {
            model = new(filter_function);
            this.show = show;
            detailGetter = new FieldGetter<Applicant>(detail_getter);
            sortBy = new ApplicantComparer
            {
                ByFields =
                {
                    a => a.FirstName,
                    a => a.LastName
                }
            };
            if (sort_getter != null)
                sortBy.ByFields.Insert(0, sort_getter);
            Loaded += ApplicantList_Loaded;
            this.Unloaded += ApplicantList_Unloaded;
            BindingContext = model;
            Title = "Applicants for " + show_name;

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(Applicants.IsLoading)));

            var search = new Entry
            {
                Placeholder = $"Filter by {filter_name}"
            };
            search.SetBinding(Entry.TextProperty, new Binding(nameof(Applicants.FilterText)));
            list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(Applicants.Collection)));

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                ColumnSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star)
                }
            };
            grid.Add(search);
            grid.Add(loading, row: 1);
            grid.Add(list, row: 1);
            var c = 0;
            var back = new Button
            {
                Text = "Back"
            };
            back.Clicked += Back_Clicked;
            grid.Add(back, row: 2, column: c++);
            var add = new Button
            {
                Text = "Add new applicant",
                BackgroundColor = Colors.SeaGreen
            };
            add.Clicked += AddButton_Clicked;
            add.SetBinding(Button.IsEnabledProperty, new Binding(nameof(Applicants.IsLoading), converter: new InvertBoolean()));
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            grid.Add(add, row: 2, column: c++);
            grid.SetColumnSpan(loading, c);
            grid.SetColumnSpan(search, c);
            grid.SetColumnSpan(list, c);
            Content = grid;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void ApplicantList_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            var collection = await context.Applicants.Include(a => a.Abilities).ToArrayAsync();
            model.Loaded(collection, sortBy);
        }

        private void ApplicantList_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
        }

        private async void AddButton_Clicked(object? sender, EventArgs e)
        {
            if (context == null)
                return;
            model.Adding();
            var applicant = new Applicant { ShowRoot = context.ShowRoot };
            context.Applicants.Add(applicant);
            await context.SaveChangesAsync();
            model.Added(applicant);
            list.SelectedItem = applicant;
            list.ScrollTo(applicant, ScrollToPosition.MakeVisible, true);
            await EditApplicant(applicant);
        }

        private object GenerateDataTemplate()
        {
            // BindingContext will be set to an Applicant
            var cell = new TextCell();
            var full_name = new MultiBinding
            {
                Converter = new FullNameFormatter(),
                TargetNullValue = "(Name not set)"
            };
            full_name.Bindings.Add(new Binding(nameof(Applicant.FirstName)));
            full_name.Bindings.Add(new Binding(nameof(Applicant.LastName)));
            cell.SetBinding(TextCell.TextProperty, full_name);
            cell.SetBinding(TextCell.DetailProperty, new Binding() {
                Converter = detailGetter,
                TargetNullValue = "(details not set)"
            });
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private async void Cell_Tapped(object? sender, EventArgs e)
        {
            if (sender is not Cell cell || cell.BindingContext is not Applicant applicant)
                return;
            await EditApplicant(applicant);
        }

        private async Task EditApplicant(Applicant applicant)
        {
            await Navigation.PushAsync(new ApplicantDetails(show, applicant.ApplicantId, applicant.FirstName, applicant.LastName, () =>
            {
                if (context == null)
                    return;
                var entry = context.Entry(applicant);
                entry.Reload();
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    model.Removed(applicant);
                else
                    applicant.NotifyChanged();
            }));
        }
    }
}
