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
    internal class EditApplicant : ApplicantBase
    {
        ShowContext? context;

        public EditApplicant(ConnectionDetails show, int id, string first, string last)
            : base(show, id, first, last)
        {
            Loaded += EditApplicant_Loaded;
            Unloaded += EditApplicant_Unloaded;
        }

        protected override View GenerateMainView()
        {
            //TODO editable layout
            return new Label { Text = "TODO: editable layout" };
        }

        protected override IEnumerable<View> GenerateExtraButtons()
        {
            var save = new Button
            {
                Text = "Save",
            };
            save.Clicked += Save_Clicked;
            yield return save;
        }
        private async void EditApplicant_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            var applicant = await Task.Run(() => context.Applicants.Single(a => a.ApplicantId == model.ApplicantId));
            model.Loaded(applicant);
        }

        private void EditApplicant_Unloaded(object? sender, EventArgs e)
        {
            //TODO check when this is called
            context?.Dispose();
            context = null;
        }

        private async void Save_Clicked(object? sender, EventArgs e)
        {
            if (context != null)
            {
                await context.SaveChangesAsync();
                await Navigation.PopAsync();
            }
        }
    }
}
