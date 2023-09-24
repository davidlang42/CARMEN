﻿using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
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
            loading.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(nameof(MainModel.IsLoading)));

            var error = LabelWithBinding(nameof(MainModel.ErrorMessage));
            error.TextColor = Colors.DarkRed;
            error.SetBinding(Label.IsVisibleProperty, new Binding(nameof(MainModel.IsError)));

            var buttons = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    ButtonWithHandler("View Applicants", ViewApplicants_Clicked)
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

        private async void MainMenu_Loaded(object? sender, EventArgs e)
        {
            model.Loading("Connecting to server");
            if (!await Task.Run(connection.TryPing))
            {
                model.Error($"Ping failed to reach host:\n{connection.Host}");
                return;
            }
            if (await Task.Run(() => connection.TryConnection(out var e) ? null : e) is string error)
            {
                model.Error($"Unable to connect to server:\n{connection.Description}\n\n{error}");
                return;
            }
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
                else if (state == ShowContext.DatabaseState.ConnectionError)
                {
                    model.Error($"Could not open '{connection.Database}'.\nThis database either doesn't exist, or you don't have access to it.");
                    return;
                }
                else if (state == ShowContext.DatabaseState.SavedWithFutureVersion)
                {
                    model.Error("This database was saved with a newer version of CARMEN and cannot be opened.\nPlease install the latest version.");
                    return;
                }
                else if (state == ShowContext.DatabaseState.SavedWithPreviousVersion)
                {
                    model.Error("This database was saved with an older version of CARMEN and cannot be opened.\nPlease upgrade the database using CARMEN desktop.");
                    return;
                }
                model.Ready(context.ShowRoot.Name);
            }
        }

        private async void ViewApplicants_Clicked(object? sender, EventArgs e)
        {
            if (model.ShowName is not string show_name)
                return;
            await Navigation.PushAsync(new ApplicantList(connection, show_name, "Name", FilterByName));
        }

        static bool FilterByName(Applicant a, string filter)
            => $"{a.FirstName} {a.LastName}".Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}
