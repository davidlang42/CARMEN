using Carmen.Mobile.Collections;
using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class CastList : ContentPage
    {
        readonly Cast model;
        readonly ConnectionDetails show;
        ShowContext? context;

        public CastList(ConnectionDetails show, string show_name)
        {
            model = new();
            this.show = show;
            Loaded += CastList_Loaded;
            Unloaded += CastList_Unloaded;
            BindingContext = model;
            Title = "Cast of " + show_name;

            var loading = new VerticalStackLayout
            {
                new ActivityIndicator { IsRunning = true }
            };
            loading.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(nameof(Applicants.IsLoading)));

            var search = new Entry
            {
                Placeholder = $"Filter by Name"
            };
            search.SetBinding(Entry.TextProperty, new Binding(nameof(Applicants.FilterText)));
            var detail = new Picker
            {
                ItemDisplayBinding = new Binding(nameof(DetailOption.Name))
            };
            detail.SetBinding(Picker.ItemsSourceProperty, new Binding(nameof(Cast.DetailOptions)));
            detail.SetBinding(Picker.SelectedItemProperty, new Binding(nameof(Cast.SelectedOption)));
            var top_bar = new Grid
            {
                ColumnSpacing = 5,
                ColumnDefinitions = new()
                {
                    new(GridLength.Star),
                    new(GridLength.Auto)
                }
            };
            top_bar.Add(search);
            top_bar.Add(detail, column: 1);
            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(Applicants.Collection)));

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };
            grid.Add(top_bar);
            grid.Add(loading, row: 1);
            grid.Add(list, row: 1);
            var back = new Button
            {
                Text = "Back"
            };
            back.Clicked += Back_Clicked;
            grid.Add(back, row: 2);
            Content = grid;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void CastList_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show, MauiProgram.USE_LAZY_LOAD_PROXIES);
            var criterias = await context.Criterias.ToArrayAsync();
            var tags = await context.Tags.ToArrayAsync();
            var collection = await context.Applicants.Include(a => a.Roles).ThenInclude(r => r.Items).Where(a => a.CastGroup != null).ToArrayAsync();
            model.Loaded(collection, criterias, tags);
        }

        private void CastList_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
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
            var detail = new MultiBinding
            {
                Converter = new DynamicConverter(),
                TargetNullValue = "(Detail not set)"
            };
            detail.Bindings.Add(new Binding());
            detail.Bindings.Add(new Binding($"{nameof(Cast.SelectedOption)}.{nameof(DetailOption.DetailGetter)}", source: model));
            cell.SetBinding(TextCell.DetailProperty, detail);
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private async void Cell_Tapped(object? sender, EventArgs e)
        {
            if (sender is not Cell cell || cell.BindingContext is not Applicant applicant || context == null)
                return;
            await Navigation.PushAsync(new BasicList<ItemRole>($"Roles for {applicant.FirstName} {applicant.LastName}", "No roles allocated", () =>
            {
                context.Nodes.Load();
                var roles = applicant.Roles.ToArray();
                var item_roles = new List<ItemRole>();
                foreach (var item in context.ShowRoot.ItemsInOrder())
                    foreach (var role in roles.Where(r => r.Items.Contains(item)))
                        item_roles.Add(new ItemRole { Item = item, Role = role });
                return item_roles.ToArray();
            }));
        }
    }
}
