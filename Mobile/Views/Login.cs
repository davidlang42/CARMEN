﻿using Carmen.Mobile.Models;
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
            //TODO change app icon/splash/colours
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
                CheckBoxAndLabel("Save login details", nameof(ConnectionDetails.SaveLogin)),
                ConnectButton(Connect_Clicked)
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
                Text = "Connect"
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
            if (details.SaveLogin)
                SaveLastConnectionDetails(details);
            else
                ClearLastConnectionDetails();
            await Navigation.PushAsync(new MainMenu(details));
        }
    }
}
