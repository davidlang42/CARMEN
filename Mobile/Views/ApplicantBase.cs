using Carmen.Mobile.Models;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal abstract class ApplicantBase : ContentPage
    {
        protected readonly ApplicantModel model;
        protected readonly ConnectionDetails show;

        protected abstract View GenerateMainView();
        protected abstract IEnumerable<View> GenerateExtraButtons();

        public ApplicantBase(ConnectionDetails show, int id, string first, string last)
        {
            model = new(id);
            this.show = show;
            BindingContext = model;
            Title = $"{first} {last}";

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(ApplicantModel.IsLoading)));

            var main = GenerateMainView();

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                ColumnSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new(GridLength.Star)
                }
            };
            grid.Add(loading);
            grid.Add(main);
            var c = 0;
            var back = new Button
            {
                Text = "Back",
                BackgroundColor = Colors.Gray
            };
            back.Clicked += Back_Clicked;
            grid.Add(back, row: 1, column: c++);
            foreach (var button in GenerateExtraButtons())
            {
                grid.ColumnDefinitions.Add(new(GridLength.Star));
                grid.Add(button, row: 1, column: c++);
            }
            grid.SetColumnSpan(loading, c);
            grid.SetColumnSpan(main, c);
            Content = grid;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            //TODO warn if going to lose any changes
            await Navigation.PopAsync();
        }
    }
}
