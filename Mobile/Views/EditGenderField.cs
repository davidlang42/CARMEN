using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditGenderField : EditBase
    {
        public EditGenderField(ApplicantField<Gender?> field)
            : base(field.Applicant, GenerateEditView(field))
        {
            BindingContext = field;
        }

        static View GenerateEditView(ApplicantField<Gender?> model)
        {
            var label = new Label { Text = model.Label };
            var group = new VerticalStackLayout
            {
                RadioButtonWithHandler("Male", Gender.Male, model),
                RadioButtonWithHandler("Female", Gender.Female, model),
                RadioButtonWithHandler("Not Specified", null, model)
            };
            return new VerticalStackLayout()
            {
                label,
                group
            };
        }

        static RadioButton RadioButtonWithHandler(string label, Gender? value, ApplicantField<Gender?> model)
        {
            var radio = new RadioButton
            {
                Content = label,
                Value = value,
                IsChecked = model.Value == value
            };
            radio.CheckedChanged += (s, e) => model.Value = value;
            return radio;
        }
    }
}
