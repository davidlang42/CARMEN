using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
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
        {
            Loaded += ViewApplicant_Loaded;
        }

        protected override View GenerateMainView()
        {
            var fields = new VerticalStackLayout //TODO scroll?
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

            return new HorizontalStackLayout //TODO flexlayout?
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

        private async void ViewApplicant_Loaded(object? sender, EventArgs e)
        {
            using var context = ShowContext.Open(show);
            var applicant = await Task.Run(() => context.Applicants.Single(a => a.ApplicantId == model.ApplicantId));
            model.Loaded(applicant);
            var image = await Task.Run(() => applicant.Photo); //TODO cache photos
            var source = image == null ? null : await MauiImageSource(image);
            model.LoadedPhoto(source);
        }

        private async Task<ImageSource?> MauiImageSource(SM.Image photo)
        {
            try
            {
                return await Task.Run(() =>
                {
                    Stream? stream = null;
                    return ImageSource.FromStream(() =>
                    {
                        stream?.Dispose();
                        stream = new MemoryStream(photo.ImageData);
                        return stream;
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Invalid image data of length {photo.ImageData.Length}, id {photo.ImageId}, {photo.Name}");
                return null;
            }
        }

        private object GenerateAbilityDataTemplate()//TODO
        {
            return null;
            // BindingContext will be set to an Ability
            //var cell = new TextCell();
            //var full_name = new MultiBinding
            //{
            //    Converter = new FullName()
            //};
            //full_name.Bindings.Add(new Binding(nameof(Applicant.FirstName)));
            //full_name.Bindings.Add(new Binding(nameof(Applicant.LastName)));
            //cell.SetBinding(TextCell.TextProperty, full_name);
            //cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(Applicant.Description)));
            //cell.Tapped += Cell_Tapped;
            //return cell;
        }

        private object GenerateNoteDataTemplate()//TODO
        {
            return null;
            // BindingContext will be set to a Note
            //var cell = new TextCell();
            //var full_name = new MultiBinding
            //{
            //    Converter = new FullName()
            //};
            //full_name.Bindings.Add(new Binding(nameof(Applicant.FirstName)));
            //full_name.Bindings.Add(new Binding(nameof(Applicant.LastName)));
            //cell.SetBinding(TextCell.TextProperty, full_name);
            //cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(Applicant.Description)));
            //cell.Tapped += Cell_Tapped;
            //return cell;
        }
    }
}
