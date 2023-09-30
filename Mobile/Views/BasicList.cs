using Carmen.Mobile.Models;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class BasicList<T> : ContentPage
        where T : IBasicListItem
    {
        readonly ListModel<T> model;
        readonly Func<T[]> loader;
        readonly Action<T>? tap_action;

        public BasicList(string title, string empty_list, Func<T[]> loader, Action<T>? tap_action = null)
        {
            model = new();
            BindingContext = model;
            Title = title;
            this.loader = loader;
            this.tap_action = tap_action;
            Loaded += BasicList_Loaded;

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(ListModel<T>.IsLoading)));

            var empty = new Label { Text = empty_list };
            empty.SetBinding(Label.IsVisibleProperty, new Binding(nameof(ListModel<T>.IsEmpty)));

            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(ListModel<T>.Collection)));

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };
            grid.Add(loading);
            grid.Add(empty);
            grid.Add(list);
            var back = new Button
            {
                Text = "Back"
            };
            back.Clicked += Back_Clicked;
            grid.Add(back, row: 1);
            Content = grid;
        }

        private async void BasicList_Loaded(object? sender, EventArgs e)
        {
            var roles = await Task.Run(loader);
            model.Loaded(roles);
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private object GenerateDataTemplate()
        {
            // BindingContext will be set to an IBasicListItem
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(IBasicListItem.MainText)));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(IBasicListItem.DetailText)));
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private void Cell_Tapped(object? sender, EventArgs e)
        {
            if (tap_action == null || sender is not Cell cell || cell.BindingContext is not T obj)
                return;
            tap_action(obj);
        }
    }
}
