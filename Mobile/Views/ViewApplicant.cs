using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Carmen.Mobile.Views
{
    internal class ViewApplicant : ApplicantBase
    {
        public ViewApplicant(ConnectionDetails show, int id, string first, string last)
            : base(show, id, first, last)
        {
            Loaded += ViewApplicant_Loaded;
        }

        protected override View GenerateMainView()
        {
            return new VerticalStackLayout //TODO FlexLayout? scroll?
            {
                LabelledField("First name", nameof(Applicant.FirstName)),
                LabelledField("Last name", nameof(Applicant.LastName)),
                LabelledField("Gender", nameof(Applicant.Gender)),
                LabelledField("Date of birth", nameof(Applicant.DateOfBirth)),
                //TODO view criterias
                //TODO view notes
                //TODO view photo
            };
        }

        protected override IEnumerable<View> GenerateExtraButtons() => Enumerable.Empty<View>();

        static Label LabelledField(string label_text, string applicant_field_binding_path)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding($"{nameof(ApplicantModel.Applicant)}.{applicant_field_binding_path}", stringFormat: label_text + ": {0}"));
            return label;
        }

        private async void ViewApplicant_Loaded(object? sender, EventArgs e)
        {
            using var context = ShowContext.Open(show);
            var applicant = await Task.Run(() => context.Applicants.Single(a => a.ApplicantId == model.ApplicantId));
            model.Loaded(applicant);
        }
    }
}
