using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MC = Microsoft.Maui.Controls;
using SM = Carmen.ShowModel;

namespace Carmen.Mobile.Views
{
    internal class ViewApplicant : ApplicantBase
    {
        public ViewApplicant(ConnectionDetails show, int id, string first, string last)
            : base(show, id, first, last)
        { }

        protected override View GenerateMainView()
        {
            //TODO (EDIT) move ApplicantBase code into ViewApplicant & remove EditApplicant/ApplicantBase
            var fields = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateFieldDataTemplate),
            };
            fields.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(ApplicantModel.Fields)));

            var abilities = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateAbilityDataTemplate),
            };
            abilities.SetBinding(ListView.ItemsSourceProperty, new Binding(ApplicantModel.Path(nameof(Applicant.Abilities))));

            var notes = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateNoteDataTemplate),
            };
            notes.SetBinding(ListView.ItemsSourceProperty, new Binding(ApplicantModel.Path(nameof(Applicant.Notes))));

            var activity = new ActivityIndicator();
            activity.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(ApplicantModel.IsLoadingPhoto)));
            var image = new MC.Image()
            {
                WidthRequest = 300,
            };
            image.SetBinding(MC.Image.SourceProperty, new Binding(nameof(ApplicantModel.Photo)));
            var photo = new Grid
            {
                image,
                activity
            };

            var layout = new FlexLayout
            {
                Padding = 10,
                AlignContent = Microsoft.Maui.Layouts.FlexAlignContent.SpaceEvenly,
                VerticalOptions = LayoutOptions.Start,
                AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start,
                Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
                Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
                Children =
                {
                    fields,
                    abilities,
                    notes,
                    photo
                }
            };
            return layout;
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

        private object GenerateFieldDataTemplate()
        {
            // BindingContext will be set to an ApplicantField
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(IApplicantField.Label)));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(IApplicantField.Value)));
            cell.Tapped += FieldCell_Tapped;
            return cell;
        }

        private async void FieldCell_Tapped(object? sender, EventArgs e)
        {
            //TODO unselect item
            if (sender is not Cell cell)
                return;
            if (cell.BindingContext is ApplicantField<string> string_field)
            {
                await Navigation.PushAsync(new EditStringField(string_field));
            }
            else if (cell.BindingContext is ApplicantField<Gender?> gender_field)
            {
                await Navigation.PushAsync(new EditGenderField(gender_field));
            }
            else if (cell.BindingContext is ApplicantField<DateTime?> date_field)
            {
                await Navigation.PushAsync(new EditDateField(date_field));
            }
        }

        private object GenerateAbilityDataTemplate()
        {
            // BindingContext will be set to an Ability
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding($"{nameof(Ability.Criteria)}.{nameof(Criteria.Name)}"));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(Ability.Mark))); //TODO nicer formatting for marks
            //TODO (EDIT) edit marks: cell.Tapped += FieldCell_Tapped;
            return cell;
        }

        private object GenerateNoteDataTemplate()
        {
            // BindingContext will be set to a Note
            var cell = new TextCell();
            var description = new MultiBinding
            {
                Converter = new NoteDescription()
            };
            description.Bindings.Add(new Binding(nameof(Note.Author)));
            description.Bindings.Add(new Binding(nameof(Note.Timestamp)));
            cell.SetBinding(TextCell.TextProperty, description);
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(Note.Text)));
            //TODO (EDIT) add notes: cell.Tapped += FieldCell_Tapped;
            return cell;
        }
    }
}
