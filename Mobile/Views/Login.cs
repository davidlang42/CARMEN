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

        private async void Connect_Clicked(object? sender, EventArgs e)
        {
            //TODO save most recent connection details
            await Navigation.PushAsync(new MainMenu(details));
        }
    }
}
