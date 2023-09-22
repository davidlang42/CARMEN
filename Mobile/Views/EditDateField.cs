using Carmen.Mobile.Models;
using Carmen.ShowModel;
using CommunityToolkit.Maui.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditDateField : EditBase
    {
        public EditDateField(ApplicantField<DateTime?> field)
            : base(field.Applicant, GenerateEditView(field))
        {
            BindingContext = field;
        }

        static View GenerateEditView(ApplicantField<DateTime?> model)
        {
            var label = new Label { Text = model.Label };
            var enable = new Switch
            {
                IsToggled = model.Value != null
            };
            var picker = new DatePicker
            {
                MaximumDate = DateTime.Now
            };
            picker.SetBinding(DatePicker.DateProperty, new Binding(nameof(ApplicantField<DateTime?>.Value)));
            picker.SetBinding(DatePicker.IsEnabledProperty, new Binding(nameof(ApplicantField<DateTime?>.Value), converter: new IsNotNullConverter()));
            enable.Toggled += (s, e) => model.Value = e.Value ? picker.Date : null;
            return new VerticalStackLayout()
            {
                label,
                enable,
                picker,
            };
        }
    }
}
