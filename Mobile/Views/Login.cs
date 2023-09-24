using Carmen.Mobile.Models;
using Carmen.ShowModel;
using CommunityToolkit.Maui.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class Login : ContentPage
    {
        readonly ConnectionDetails details = new();

        public Login()
        {
            details = LoadLastConnectionDetails() ?? new();
            Title = "Connect to a CARMEN database";
            NavigatedTo += Page_NavigatedTo;
            BindingContext = details;
            Content = new ScrollView {
                Content = new VerticalStackLayout
                    {
                        Spacing = 5,
                        Margin = 5,
                        Children =
                    {
                        new Label { Text = "Server Host:" },
                        TextEntry(nameof(ConnectionDetails.Host)),
                        new Label { Text = "Server Port:" },
                        PortEntry(nameof(ConnectionDetails.Port)),
                        new Label { Text = "Database Name:" },
                        TextEntry(nameof(ConnectionDetails.Database)),
                        new Label { Text = "Username:" },
                        TextEntry(nameof(ConnectionDetails.User)),
                        new Label { Text = "Password:" },
                        PasswordEntry(nameof(ConnectionDetails.Password)),
                        CheckBoxAndLabel("Save login details", nameof(ConnectionDetails.SaveLogin)),
                        ConnectButton(Connect_Clicked)
                    }
                }
            };
        }

        const string LAST_CONNECTION_DETAILS = "LastConnectionDetails";

        static void SaveLastConnectionDetails(ConnectionDetails details)
        {
            Preferences.Default.Set(LAST_CONNECTION_DETAILS, JsonSerializer.Serialize(details));
        }

        static ConnectionDetails? LoadLastConnectionDetails()
        {
            var json = Preferences.Default.Get(LAST_CONNECTION_DETAILS, "");
            try
            {
                if (!string.IsNullOrEmpty(json))
                    return JsonSerializer.Deserialize<ConnectionDetails>(json);
            }
            catch { }
            return null;
        }

        static void ClearLastConnectionDetails()
        {
            Preferences.Default.Remove(LAST_CONNECTION_DETAILS);
        }

        private void Page_NavigatedTo(object? sender, NavigatedToEventArgs e)
        {
            Window.Title = "CARMEN";
        }

        static Entry TextEntry(string path)
        {
            var entry = new Entry();
            entry.SetBinding(Entry.TextProperty, path);
            entry.Behaviors.Add(new TextValidationBehavior
            {
                InvalidStyle = InvalidStyle(),
                ValidStyle = ValidStyle(),
                Flags = ValidationFlags.ValidateOnValueChanged,
                MinimumLength = 1
            });
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
                Placeholder = ConnectionDetails.DEFAULT_PORT.ToString()
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
                Text = "Connect"
            };
            button.Clicked += handler;
            return button;
        }

        static View CheckBoxAndLabel(string label, string binding_path) //TODO (NOW) make label centred in parent to match checkbox
        {
            var checkbox = new CheckBox();
            checkbox.SetBinding(CheckBox.IsCheckedProperty, binding_path);
            return new HorizontalStackLayout
            {
                new Label { Text = label },
                checkbox
            };
        }

        private async void Connect_Clicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(details.Host))
            {
                DisplayAlert("Please enter the server's Hostname or IP address", "", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(details.Database))
            {
                DisplayAlert("Please enter the Database name", "", "Ok");
                return;
            }
            if (string.IsNullOrWhiteSpace(details.User))
            {
                DisplayAlert("Please enter your User and Password", "", "Ok");
                return;
            }
            if (details.SaveLogin)
                SaveLastConnectionDetails(details);
            else
                ClearLastConnectionDetails();
            await Navigation.PushAsync(new MainMenu(details));
        }
    }
}
