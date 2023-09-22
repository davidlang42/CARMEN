using Carmen.Mobile.Models;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using MC = Microsoft.Maui.Controls;
using SM = Carmen.ShowModel;

namespace Carmen.Mobile.Views
{
    internal abstract class ApplicantBase : ContentPage
    {
        protected readonly ApplicantModel model;
        protected readonly ConnectionDetails show;
        protected ShowContext? context;

        protected abstract View GenerateMainView();
        protected abstract IEnumerable<View> GenerateExtraButtons();

        public ApplicantBase(ConnectionDetails show, int id, string first, string last)
        {
            model = new(id);
            this.show = show;
            BindingContext = model;
            Title = $"{first} {last}";
            Loaded += ApplicantBase_Loaded;
            Unloaded += ApplicantBase_Unloaded;

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

        private async void ApplicantBase_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            var applicant = await Task.Run(() => context.Applicants.Single(a => a.ApplicantId == model.ApplicantId));
            model.Loaded(applicant);
            var image = await Task.Run(() => applicant.Photo); //TODO cache photos
            var source = image == null ? null : await MauiImageSource(image);
            model.LoadedPhoto(source);
        }

        private async Task<ImageSource?> MauiImageSource(SM.Image photo)
        {
            try
            {
                return await Task.Run(() =>
                {
                    Stream? stream = null;
                    return ImageSource.FromStream(() =>
                    {
                        stream?.Dispose();
                        stream = new MemoryStream(photo.ImageData);
                        return stream;
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Invalid image data of length {photo.ImageData.Length}, id {photo.ImageId}, {photo.Name}");
                return null;
            }
        }

        private void ApplicantBase_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            if (context?.ChangeTracker.HasChanges() == true)
            {
                if (!await DisplayAlert($"Are you sure you want to discard your changes?", "", "Yes", "No"))
                    return;
            }
            await Navigation.PopAsync();
        }
    }
}
