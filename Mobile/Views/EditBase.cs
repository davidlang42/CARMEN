using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
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
        public EditBase(Applicant applicant, View edit_view)
        {
            Title = $"Editing {applicant.FirstName} {applicant.LastName}";

            var main = new ScrollView
            {
                Content = edit_view
            };

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                ColumnSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };
            grid.Add(main);
            var c = 0;
            foreach (var button in GenerateButtons())
            {
                grid.ColumnDefinitions.Add(new(GridLength.Star));
                grid.Add(button, row: 1, column: c++);
            }
            grid.SetColumnSpan(main, c);
            Content = grid;
        }

        protected virtual IEnumerable<View> GenerateButtons()
        {
            var back = new Button
            {
                Text = "Back"
            };
            back.Clicked += Back_Clicked;
            yield return back;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
