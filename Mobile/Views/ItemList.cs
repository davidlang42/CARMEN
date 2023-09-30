using Carmen.Mobile.Collections;
using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class ItemList : ContentPage
    {
        readonly Items model;
        readonly ConnectionDetails show;
        ShowContext? context;

        public ItemList(ConnectionDetails show, string show_name)
        {
            model = new();
            this.show = show;
            Loaded += ItemList_Loaded;
            Unloaded += ItemList_Unloaded;
            BindingContext = model;
            Title = show_name;

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(Items.IsLoading)));

            var empty = new Label { Text = "No items" };
            empty.SetBinding(Label.IsVisibleProperty, new Binding(nameof(Items.IsEmpty)));

            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(Items.Collection)));

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

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void ItemList_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            await context.Nodes.LoadAsync();
            var items = context.ShowRoot.ItemsInOrder().ToArray();
            model.Loaded(items);
        }

        private void ItemList_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
        }

        private object GenerateDataTemplate()
        {
            // BindingContext will be set to an Item
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(Item.Name)));
            //TODO item details: cell.SetBinding(TextCell.DetailProperty, new Binding());
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private async void Cell_Tapped(object? sender, EventArgs e)
        {
            if (sender is not Cell cell || cell.BindingContext is not Item item || context == null)
                return;
            await Navigation.PushAsync(new BasicList<RoleDetail>($"Roles in {item.Name}", "No roles",
                item.Roles.InNameOrder().Select(r => new RoleDetail { Role = r }).ToArray));
        }
    }
}
