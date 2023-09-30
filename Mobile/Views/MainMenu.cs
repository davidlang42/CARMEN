using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
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
                    ButtonWithHandler("Edit Applicants by Name", ViewApplicantsName_Clicked),
                    ButtonWithHandler("Edit Applicants by Age/Gender", ViewApplicantsAgeGender_Clicked),
                    ButtonWithHandlerAndDropdown("Edit Applicants by: ", ViewApplicantsCriteria_Clicked, nameof(MainModel.Criterias), nameof(Criteria.Name)),
                    ButtonWithHandler("View Casting by Name", ViewCastList_Clicked)
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

        static View ButtonWithHandlerAndDropdown(string text, Action<object?, EventArgs, object> handler, string dropdown_items_binding, string item_display_binding)
        {
            var button = new Button
            {
                Text = text,
            };
            var dropdown = new Picker
            {
                ItemDisplayBinding = new Binding(item_display_binding)
            };
            dropdown.SetBinding(Picker.ItemsSourceProperty, new Binding(dropdown_items_binding));
            button.Clicked += (s, e) => handler(s, e, dropdown.SelectedItem);
            return new HorizontalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    button,
                    dropdown
                }
            };
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
                var criterias = await context.Criterias.ToArrayAsync();
                model.Ready(context.ShowRoot.Name, criterias);
            }
        }

        private async void ViewApplicantsName_Clicked(object? sender, EventArgs e)
        {
            if (model.ShowName is not string show_name)
                return;
            await Navigation.PushAsync(new ApplicantList(connection, show_name, "Name", FilterByName, DescriptionDetail));
        }

        private async void ViewApplicantsAgeGender_Clicked(object? sender, EventArgs e)
        {
            if (model.ShowName is not string show_name)
                return;
            await Navigation.PushAsync(new ApplicantList(connection, show_name, "Age, Gender", FilterByDescription, DescriptionDetail));
        }

        private async void ViewApplicantsCriteria_Clicked(object? sender, EventArgs e, object selected_item)
        {
            if (model.ShowName is not string show_name || selected_item is not Criteria criteria)
                return;
            await Navigation.PushAsync(new ApplicantList(connection, show_name, criteria.Name, (a, f) => FilterByCriteria(criteria, a, f), a => CriteriaDetail(criteria, a), a => SortByCriteria(criteria, a)));
        }

        static string? DescriptionDetail(Applicant a) => a.Description ?? "(Age/Gender not set)";

        static string? CriteriaDetail(Criteria c, Applicant a)
        {
            var mark = a.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == c.CriteriaId)?.Mark; // don't use MarkFor() because reference equal doesn't work across contexts
            if (!mark.HasValue)
                return $"({c.Name} not set)";
            return $"{c.Name}: {c.Format(mark.Value)}";
        }

        static bool FilterByName(Applicant a, string filter)
        {
            var name = FullNameFormatter.Format(a.FirstName, a.LastName);
            if (filter.Length == 1)
            {
                switch (filter[0])
                {
                    case '=':
                        return !string.IsNullOrEmpty(name);
                    case '!':
                        return string.IsNullOrEmpty(name);
                }
            }
            if (string.IsNullOrEmpty(name))
                return false;
            return name.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        static bool FilterByDescription(Applicant a, string filter)
        {
            var desc = a.Description;
            if (filter.Length == 1)
            {
                switch (filter[0])
                {
                    case '=':
                        return desc != null;
                    case '!':
                        return desc == null;
                }
            }
            if (desc == null)
                return false;
            return desc.Contains(filter); // case sensitive compare, otherwise 'Female' will always contain 'Male'
        }

        static bool FilterByCriteria(Criteria c, Applicant a, string filter)
        {
            var mark = a.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == c.CriteriaId)?.Mark; // don't use MarkFor() because reference equal doesn't work across contexts
            if (filter.Length == 1)
            {
                switch (filter[0])
                {
                    case '=':
                        return mark != null;
                    case '!':
                        return mark == null;
                }
            }
            if (!mark.HasValue)
                return false;
            if (filter.Length > 1 && uint.TryParse(filter[1..], out var number))
            {
                switch (filter[0])
                {
                    case '<':
                        return mark < number;
                    case '>':
                        return mark > number;
                    case '=':
                        return mark == number;
                    case '!':
                        return mark != number;
                }
            }
            return c.Format(mark.Value).Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        static uint? SortByCriteria(Criteria c, Applicant a)
        {
            var mark = a.Abilities.SingleOrDefault(ab => ab.Criteria.CriteriaId == c.CriteriaId)?.Mark; // don't use MarkFor() because reference equal doesn't work across contexts
            return mark;
        }

        private async void ViewCastList_Clicked(object? sender, EventArgs e)
        {
            if (model.ShowName is not string show_name)
                return;
            await Navigation.PushAsync(new CastList(connection, show_name));
        }
    }
}
