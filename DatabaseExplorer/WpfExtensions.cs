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
            foreach (var child in LogicalTreeHelper.GetChildren(depObj))
            {
                if (child is T)
                    yield return (T)child;

                if (child is DependencyObject childDepObj)
                    foreach (T childOfChild in AllControls<T>(childDepObj))
                        yield return childOfChild;
            }
        }
        //TODO I can do a better job of this, maybe using .Children and Include()
    }
}
