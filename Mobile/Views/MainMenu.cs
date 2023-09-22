using Carmen.Mobile.Models;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class MainMenu : ContentPage
    {
        readonly MainModel model;
        readonly ConnectionDetails connection;

        public MainMenu(ConnectionDetails connection)
        {
            model = new();
            this.connection = connection;
            Loaded += MainMenu_Loaded;
            BindingContext = model;
            SetBinding(TitleProperty, new Binding(nameof(MainModel.PageTitle)));

            var loading = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new ActivityIndicator { IsRunning = true },
                    LabelWithBinding(nameof(MainModel.LoadingMessage))
                }
            };
            loading.SetBinding(ProgressBar.IsVisibleProperty, new Binding(nameof(MainModel.IsLoading)));

            var error = LabelWithBinding(nameof(MainModel.ErrorMessage));
            error.TextColor = Colors.DarkRed;
            error.SetBinding(Label.IsVisibleProperty, new Binding(nameof(MainModel.IsError)));

            var buttons = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    ButtonWithHandler("View Applicants", ViewApplicants_Clicked),
                    ButtonWithHandler("Edit Applicants", EditApplicants_Clicked),
                }
            };
            buttons.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(nameof(MainModel.IsReady)));

            Content = new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = 10,
                Children =
                {
                    loading,
                    error,
                    buttons
                }
            };
        }

        static Label LabelWithBinding(string text_binding_path)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding(text_binding_path));
            return label;
        }

        static Button ButtonWithHandler(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
            };
            button.Clicked += handler;
            return button;
        }

        private async Task TestUi()
        {
            model.Loading("Connecting to server");
            await Task.Run(() => Thread.Sleep(1000));
            model.Loading("Preparing show model");
            await Task.Run(() => Thread.Sleep(1000));
            model.Loading("Checking database integrity");
            var state = ShowContext.DatabaseState.SavedWithFutureVersion;
            if (state == ShowContext.DatabaseState.Empty)
            {
                model.Loading("Creating new database");
                await Task.Run(() => Thread.Sleep(1000));
            }
            else if (state == ShowContext.DatabaseState.SavedWithFutureVersion)
            {
                model.Error("This database was saved with a newer version of CARMEN and cannot be opened. Please install the latest version.");
                return;
            }
            else if (state == ShowContext.DatabaseState.SavedWithPreviousVersion)
            {
                model.Error("This database was saved with an older version of CARMEN and cannot be opened. Please upgrade it using CARMEN desktop.");
                return;
            }
            model.Ready();
        }

        private async void MainMenu_Loaded(object? sender, EventArgs e)
        {
            await TestUi();
            return;
            //TODO
            model.Loading("Connecting to server");
            using (var context = ShowContext.Open(connection))
            {
                model.Loading("Preparing show model");
                await context.PreloadModel(); // do this here while the overlay is shown to avoid a synchronous delay when the MainMenu is loaded
                model.Loading("Checking database integrity");
                var state = await context.CheckDatabaseState();
                if (state == ShowContext.DatabaseState.Empty)
                {
                    model.Loading("Creating new database");
                    await context.CreateNewDatabase(connection.DefaultShowName);
                }
                else if (state == ShowContext.DatabaseState.SavedWithFutureVersion)
                {
                    model.Error("This database was saved with a newer version of CARMEN and cannot be opened. Please install the latest version.");
                    return;
                }
                else if (state == ShowContext.DatabaseState.SavedWithPreviousVersion)
                {
                    model.Error("This database was saved with an older version of CARMEN and cannot be opened. Please upgrade it using CARMEN desktop.");
                    return;
                }
            }
            model.Ready();
        }

        private static void ViewApplicants_Clicked(object? sender, EventArgs e)
        {
        }

        private static void EditApplicants_Clicked(object? sender, EventArgs e)
        {
        }
    }
}
