using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Carmen.Desktop.ViewModels
{
    public class CastingError
    {
        public string Message { get; }
        public Action? DoubleClick { get; }
        public ContextMenu? ContextMenu { get; }

        public CastingError(string message, Action? double_click = null, IEnumerable<(string, Action)>? right_click = null)
        {
            Message = message;
            DoubleClick = double_click;
            if (right_click != null)
            {
                ContextMenu = new ContextMenu();
                foreach (var (text, action) in right_click)
                {
                    var menu_item = new MenuItem() { Header = text };
                    menu_item.Click += (s,e) => action();
                    ContextMenu.Items.Add(menu_item);
                }
            }
        }
    }
}
