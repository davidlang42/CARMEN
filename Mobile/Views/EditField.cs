using Carmen.Mobile.Models;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditField : EditBase
    {
        public EditField(string first, string last, ApplicantField model)
            : base(first, last, GenerateEditView(model))
        {
            BindingContext = model;
        }

        static View GenerateEditView(ApplicantField model)
        {
            var label = new Label { Text = model.Label };
            var entry = new Entry();
            entry.SetBinding(Entry.TextProperty, new Binding(nameof(ApplicantField.Value)));
            return new VerticalStackLayout()
            {
                label,
                entry
            };
        }
    }
}
