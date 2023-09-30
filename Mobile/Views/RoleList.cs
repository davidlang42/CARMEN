using Carmen.Mobile.Models;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class RoleList : ContentPage
    {
        readonly Roles model;
        readonly Func<ItemRole[]> loader;

        public RoleList(string title, Func<ItemRole[]> loader)
        {
            model = new();
            BindingContext = model;
            Title = title;
            this.loader = loader;
            Loaded += RoleList_Loaded;

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(Roles.IsLoading)));

            var empty = new Label { Text = "No roles" };
            empty.SetBinding(Label.IsVisibleProperty, new Binding(nameof(Roles.IsEmpty)));

            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(Roles.Collection)));

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

        private async void RoleList_Loaded(object? sender, EventArgs e)
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
            // BindingContext will be set to an ItemRole
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(ItemRole.MainText)));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(ItemRole.DetailText)));
            return cell;
        }
    }
}
