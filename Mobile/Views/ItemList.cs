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
        readonly ListModel<ItemDetail> model;
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

            var loading = new VerticalStackLayout
            {
                new ActivityIndicator { IsRunning = true }
            };
            loading.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(nameof(ListModel<ItemDetail>.IsLoading)));

            var empty = new Label { Text = "No items" };
            empty.SetBinding(Label.IsVisibleProperty, new Binding(nameof(ListModel<ItemDetail>.IsEmpty)));

            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(ListModel<ItemDetail>.Collection)));

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
            context = ShowContext.Open(show, MauiProgram.USE_LAZY_LOAD_PROXIES);
            await context.Nodes.LoadAsync();
            var items = context.ShowRoot.ItemsInOrder().Select(i => new ItemDetail { Item = i }).ToArray();
            model.Loaded(items);
        }

        private void ItemList_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
        }

        private object GenerateDataTemplate()
        {
            // BindingContext will be set to an ItemDetail
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(IBasicListItem.MainText)));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(IBasicListItem.DetailText)));
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private async void Cell_Tapped(object? sender, EventArgs e)
        {
            if (sender is not Cell cell || cell.BindingContext is not ItemDetail item || context == null)
                return;
            await Navigation.PushAsync(new BasicList<RoleDetail>($"Roles in {item.Item.Name}", "No roles",
                () => item.Item.Roles.InNameOrder().Select(r => new RoleDetail { Role = r }).ToArray(),//TODO wont work without lazy loading: item.Roles
                rd => Navigation.PushAsync(new BasicList<ApplicantDetail>($"Cast for {rd.Role.Name}", "No cast",
                    () => rd.Role.Cast.OrderBy(c => c.CastNumber).ThenBy(c => c.AlternativeCast?.Initial).Select(c => new ApplicantDetail { Applicant = c }).ToArray()))));//TODO wont work without lazy loading: role.Cast
        }
    }
}
