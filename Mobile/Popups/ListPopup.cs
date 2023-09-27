using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Popups
{
    internal class ListPopup<T> : Popup
    {
        public ListPopup(T[] items, Func<T, string> display_getter)
        {
            var layout = new VerticalStackLayout
            {
                Spacing = 5
            };
            foreach (var item in items)
            {
                var button = new Button
                {
                    Text = display_getter(item)
                };
                button.Clicked += (s, e) =>
                {
                    Close(item);
                };
                layout.Children.Add(button);
            }
            Content = layout;
        }
    }
}
