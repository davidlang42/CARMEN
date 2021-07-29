using System.Collections.Generic;
using System.Windows;

namespace DatabaseExplorer
{
    static class WpfExtensions
    {
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
    }
}
