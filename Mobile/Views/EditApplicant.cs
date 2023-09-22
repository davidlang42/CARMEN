using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class EditApplicant : ApplicantBase
    {
        public EditApplicant(ConnectionDetails show, int id, string first, string last)
            : base(show, id, first, last)
        { }

        protected override View GenerateMainView()
        {
            //TODO editable layout: first, last, gender, dob, criterias, notes, photo
            return new Label { Text = "TODO: editable layout" };
        }

        protected override IEnumerable<View> GenerateExtraButtons()
        {
            var delete = new Button
            {
                Text = "Delete",
                BackgroundColor = Colors.Red
            };
            delete.Clicked += Delete_Clicked; ;
            yield return delete;
            var save = new Button
            {
                Text = "Save",
            };
            save.Clicked += Save_Clicked;
            yield return save;
        }

        private async void Save_Clicked(object? sender, EventArgs e)
        {
            if (context == null)
                return;
            if (!context.ChangeTracker.HasChanges())
                await DisplayAlert($"No changes were made.", "", "Ok");
            await context.SaveChangesAsync();
            await Navigation.PopAsync();
        }

        private async void Delete_Clicked(object? sender, EventArgs e)
        {
            if (context == null || model.Applicant == null)
                return;
            if (await DisplayAlert($"Are you sure you want to delete '{model.GetFullName()}'?", "This cannot be undone.", "Yes", "No"))
            {
                context.Applicants.Remove(model.Applicant);
                await context.SaveChangesAsync();
                await Navigation.PopAsync();
            }
        }
    }
}
