using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CarmenUI.Converters
{
    public class ComboBoxFaceTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FaceTemplate { get; set; }
        public DataTemplate? ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element
                && element.TemplatedParent is ComboBox)
                return FaceTemplate ?? throw new ApplicationException("FaceTemplate not set.");
            else
                return ItemTemplate ?? throw new ApplicationException("ItemTemplate not set.");
        }
    }
}
