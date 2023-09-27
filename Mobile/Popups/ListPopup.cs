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
        Func<Binding> displayBinding;
        public ListPopup(T[] items, Func<Binding> display_binding)
        {
            displayBinding = display_binding;
            Content = new ListView //TODO this UI could be nicer
            {
                ItemsSource = items,
                ItemTemplate = new DataTemplate(GenerateDataTemplate)
            };
        }

        private object GenerateDataTemplate()
        {
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, displayBinding());
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private async void Cell_Tapped(object? sender, EventArgs e)
        {
            var cell = (Cell)sender;
            var list = (ListView)cell.Parent;
            await CloseAsync(list.SelectedItem);
        }
    }
}
