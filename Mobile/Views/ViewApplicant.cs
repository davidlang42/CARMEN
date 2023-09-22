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
            var fields = new VerticalStackLayout //TODO add scrolling
            {
                LabelledField("First name", nameof(Applicant.FirstName)),
                LabelledField("Last name", nameof(Applicant.LastName)),
                LabelledField("Gender", nameof(Applicant.Gender)),
                LabelledField("Date of birth", nameof(Applicant.DateOfBirth)),
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
            var image = new MC.Image();
            image.SetBinding(MC.Image.SourceProperty, new Binding(nameof(ApplicantModel.Photo)));
            var photo = new Grid
            {
                image,
                activity
            };

            return new HorizontalStackLayout //TODO (NOW) flexlayout? and scroll if vital
            {
                fields,
                abilities,
                notes,
                photo
            };
        }

        protected override IEnumerable<View> GenerateExtraButtons() => Enumerable.Empty<View>();

        static Label LabelledField(string label_text, string applicant_field_binding_path)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding(ApplicantModel.Path(applicant_field_binding_path), stringFormat: label_text + ": {0}"));
            return label;
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
