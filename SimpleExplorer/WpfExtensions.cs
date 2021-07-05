using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SimpleExplorer
{
    static class WpfExtensions
    {
        // Source: https://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
        public static IEnumerable<T> AllControls<T>(this DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child is T)
                    yield return (T)child;

                foreach (T childOfChild in AllControls<T>(child))
                    yield return childOfChild;
            }
        }
        //TODO I can do a better job of this, maybe using .Children and Include()
    }
}
