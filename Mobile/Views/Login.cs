using Carmen.Mobile.Models;
using Carmen.ShowModel;
using CommunityToolkit.Maui.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class Login : ContentPage
    {
        readonly ConnectionDetails details = new();

        public Login()
        {
            //TODO change app icon/splash/colours
            //TODO load last used ConnectionDetails
            Title = "Connect to a CARMEN database";
            NavigatedTo += Page_NavigatedTo;
            BindingContext = details;
            Content = new VerticalStackLayout //TODO add scrolling
            {
                Spacing = 5,
                Margin = 5,
                Children =
            {
                new Label { Text = "Server Host:" },
                TextEntry(nameof(ConnectionDetails.Host)), //TODO highlight red if empty or malformed
                new Label { Text = "Server Port:" },
                PortEntry(nameof(ConnectionDetails.Port)), //TODO allow blank port, show 3306 as placeholder
                new Label { Text = "Database Name:" },
                TextEntry(nameof(ConnectionDetails.Database)), //TODO highlight red if empty
                new Label { Text = "Username:" },
                TextEntry(nameof(ConnectionDetails.User)), //TODO highlight red if empty
                new Label { Text = "Password:" },
                PasswordEntry(nameof(ConnectionDetails.Password)), //TODO highlight red if empty
                CheckBoxAndLabel("Allow editing:", nameof(ConnectionDetails.AllowEditing)),
                ConnectButton(Connect_Clicked)
            }
            };
        }

        private void Page_NavigatedTo(object? sender, NavigatedToEventArgs e)
        {
            Window.Title = "CARMEN";
        }

        static Entry TextEntry(string path)
        {
            var entry = new Entry();
            entry.SetBinding(Entry.TextProperty, path);
            return entry;
        }

        static Entry PasswordEntry(string path)
        {
            var entry = new Entry
            {
                IsPassword = true
            };
            entry.SetBinding(Entry.TextProperty, path);
            return entry;
        }

        static Entry PortEntry(string path)
        {
            var entry = new Entry
            {
                Keyboard = Keyboard.Numeric,
            };
            entry.Behaviors.Add(new NumericValidationBehavior
            {
                InvalidStyle = InvalidStyle(),
                ValidStyle = ValidStyle(),
                Flags = ValidationFlags.ValidateOnValueChanged,
                MinimumValue = 0,
                MaximumValue = 65535,
                MaximumDecimalPlaces = 0
            });
            entry.SetBinding(Entry.TextProperty, new Binding(path, BindingMode.TwoWay));
            return entry;
        }

        static Style ValidStyle()
        {
            var valid = new Style(typeof(VisualElement));
            valid.Setters.Add(new Setter
            {
                Property = VisualElement.BackgroundColorProperty,
                Value = Colors.Transparent
            });
            return valid;
        }

        static Style InvalidStyle()
        {
            var invalid = new Style(typeof(VisualElement));
            invalid.Setters.Add(new Setter
            {
                Property = VisualElement.BackgroundColorProperty,
                Value = Colors.LightCoral
            });
            return invalid;
        }

        static View ConnectButton(EventHandler handler)
        {
            var button = new Button
            {
                Text = "Connect",
                BackgroundColor = Colors.Green
            };
            button.Clicked += handler;
            return button;
        }

        static View CheckBoxAndLabel(string label, string binding_path)
        {
            var checkbox = new CheckBox();
            checkbox.SetBinding(CheckBox.IsCheckedProperty, binding_path);
            var layout = new Grid
            {
                Padding = 5,
                ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
            };
            layout.Add(new Label { Text = label });
            layout.Add(checkbox, 1);
            return layout;
        }

        private async void Connect_Clicked(object? sender, EventArgs e)
        {
            //TODO save most recent (successful) connection details
            //TODO remove stub & implement connection
            await DisplayAlert("CARMEN", $"Looks like you want to connect to a db with connection string: {details.ConnectionString}", "Ok");

            //TODO Open_Clicked()
            //if (!File.Exists(files.SchemaFile))
            //{
            //    await DisplayAlert("Error", $"The schema file does not exist: {files.SchemaFile}", "Ok");
            //    return;
            //}
            //if (!File.Exists(files.JsonFile))
            //{
            //    await DisplayAlert("Error", $"The JSON file does not exist: {files.JsonFile}", "Ok");
            //    return;
            //}
            //Regex? hide_regex = null;
            //if (!string.IsNullOrEmpty(files.HidePropertiesRegex))
            //{
            //    try
            //    {
            //        hide_regex = new Regex(files.HidePropertiesRegex, RegexOptions.Compiled);
            //    }
            //    catch (ArgumentException)
            //    {
            //        await DisplayAlert("Error", $"The pattern for hiding properties is not a valid Regular Expression: {files.HidePropertiesRegex}\nLeave this blank to show all properties.", "Ok");
            //        return;
            //    }
            //}
            //Regex? name_regex = null;
            //if (!string.IsNullOrEmpty(files.NamePropertiesRegex))
            //{
            //    try
            //    {
            //        name_regex = new Regex(files.NamePropertiesRegex, RegexOptions.Compiled);
            //    }
            //    catch (ArgumentException)
            //    {
            //        await DisplayAlert("Error", $"The pattern for finding name properties is not a valid Regular Expression: {files.NamePropertiesRegex}\nLeave this blank to show all properties.", "Ok");
            //        return;
            //    }
            //}
            //files.SaveToUserPreferences();
            //var json_file = JsonFile.Load(files.SchemaFile, files.JsonFile, hide_regex, name_regex, !files.OfferCommonObjectUpdates, files.ShortcutSingleObjectProperties);
            //var json_model = new JsonModel(json_file, new(json_file.Schema.Title ?? "Root"), json_file.Root, json_file.Schema);
            //await Navigation.PushAsync(new EditJson(json_model));

            //TODO OpenShow()
            //ShowContext.DatabaseState state;
            //using (var context = ShowContext.Open(show))
            //{
            //    using (var loading = new LoadingOverlay(this).AsSegment(nameof(StartWindow) + nameof(OpenShow)))
            //    {
            //        using (loading.Segment(nameof(StartWindow) + nameof(ShowContext.PreloadModel), "Preparing show model"))
            //            await context.PreloadModel(); // do this here while the overlay is shown to avoid a synchronous delay when the MainMenu is loaded
            //        using (loading.Segment(nameof(StartWindow) + nameof(ShowContext.CheckDatabaseState), "Checking database integrity"))
            //            state = await context.CheckDatabaseState();
            //    }
            //    if (state == ShowContext.DatabaseState.Empty)
            //    {
            //        using (new LoadingOverlay(this) { SubText = "Creating new database" })
            //            await context.CreateNewDatabase(show.DefaultShowName);
            //    }
            //    else if (state == ShowContext.DatabaseState.SavedWithFutureVersion)
            //    {
            //        MessageBox.Show("This database was saved with a newer version of CARMEN and cannot be opened. Please install the latest version.", Title);
            //        return;
            //    }
            //    else if (state == ShowContext.DatabaseState.SavedWithPreviousVersion)
            //    {
            //        if (MessageBox.Show("This database was saved with an older version of CARMEN, would you like to upgrade it?\nNOTE: Once upgraded, older versions will no longer be able to read this database.", Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //        {
            //            using (new LoadingOverlay(this) { SubText = "Upgrading database" })
            //            {
            //                show.CreateBackupIfFile();
            //                await context.UpgradeDatabase();
            //            }
            //        }
            //        else
            //            return;
            //    }
            //}
            //using (new LoadingOverlay(this) { SubText = "Opening show" })
            //    LaunchMainWindow(show);
        }
    }
}
