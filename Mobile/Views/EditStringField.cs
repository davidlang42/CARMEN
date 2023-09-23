using Carmen.Mobile.Models;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditStringField : EditBase
    {
        public EditStringField(ApplicantField<string> field)
            : base(field.Applicant, GenerateEditView(field))
        {
            BindingContext = field;
        }

        static View GenerateEditView(ApplicantField<string> model)
        {
            var label = new Label { Text = model.Label };
            var entry = new Entry();
            entry.SetBinding(Entry.TextProperty, new Binding(nameof(ApplicantField<string>.Value)));
            return new VerticalStackLayout()
            {
                label,
                entry
            };
        }
    }
}
