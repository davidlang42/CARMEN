using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal abstract class EditBase : ContentPage
    {
        public EditBase(string first, string last, View edit_view)
        {
            Title = $"{first} {last}";

            var main = new ScrollView
            {
                Content = edit_view
            };

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
            grid.Add(main);
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
    }
}
