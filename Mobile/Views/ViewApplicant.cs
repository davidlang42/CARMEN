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
            //TODO (EDIT) maybe remove the idea of edit/view and just make it view by default then edit INDIVIDUAL values by clicking the TextCells (which opens an editor in a new page)
            var fields = new ListView
            {
                ItemsSource = new[] {
                    new ApplicantField("First name", a => a.FirstName, model),
                    new ApplicantField("Last name", a => a.LastName, model),
                    new ApplicantField("Gender", a => a.Gender, model),
                    new ApplicantField("Date of birth", a => a.DateOfBirth, model)
                },
                ItemTemplate = new DataTemplate(GenerateFieldDataTemplate),
            };

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

        protected override IEnumerable<View> GenerateExtraButtons() => Enumerable.Empty<View>();

        private object GenerateFieldDataTemplate()
        {
            // BindingContext will be set to an ApplicantField
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(ApplicantField.Label)));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(ApplicantField.Value)));
            return cell;
        }

        private object GenerateAbilityDataTemplate()
        {
            // BindingContext will be set to an Ability
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding($"{nameof(Ability.Criteria)}.{nameof(Criteria.Name)}"));
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(Ability.Mark))); //TODO nicer formatting for marks
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
            return cell;
        }
    }
}
