using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
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
    internal class ApplicantDetails : ContentPage
    {
        readonly ApplicantModel model;
        readonly ConnectionDetails show;
        readonly Action onChange;
        ShowContext? context;

        public ApplicantDetails(ConnectionDetails show, int id, string first, string last, Action on_change)
        {
            this.show = show;
            model = new(id, first, last);
            onChange = on_change;
            BindingContext = model;
            SetBinding(TitleProperty, new Binding(nameof(ApplicantModel.FullName)));
            Loaded += ViewApplicant_Loaded;
            Unloaded += ViewApplicant_Unloaded;

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(ApplicantModel.IsLoading)));

            var main = new ScrollView
            {
                Content = GenerateMainView()
            };

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                ColumnSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    new(GridLength.Star)
                }
            };
            grid.Add(loading);
            grid.Add(main);
            var c = 0;
            var back = new Button
            {
                Text = "Back",
                BackgroundColor = Colors.Gray
            };
            back.Clicked += Back_Clicked;
            grid.Add(back, row: 1, column: c++);
            var delete = new Button
            {
                Text = "Delete",
                BackgroundColor = Colors.Red
            };
            delete.Clicked += Delete_Clicked; ;
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            grid.Add(delete, row: 1, column: c++);
            var save = new Button
            {
                Text = "Save",
            };
            save.Clicked += Save_Clicked;
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            grid.Add(save, row: 1, column: c++);
            grid.SetColumnSpan(loading, c);
            grid.SetColumnSpan(main, c);
            Content = grid;
        }

        private async void ViewApplicant_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            var applicant = await Task.Run(() => context.Applicants.SingleOrDefault(a => a.ApplicantId == model.ApplicantId));
            if (applicant == null)
            {
                await DisplayAlert("Applicant does not exist", "This is probably because someone else has deleted them.", "Ok");
                await Navigation.PopAsync();
                return;
            }
            model.Loaded(applicant);
            var image = await Task.Run(() => applicant.Photo); //TODO cache photos
            var source = image == null ? null : await MauiImageSource(image);
            model.LoadedPhoto(source);
        }

        static async Task<ImageSource?> MauiImageSource(SM.Image photo)
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

        private void ViewApplicant_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            if (context?.ChangeTracker.HasChanges() == true)
            {
                if (!await DisplayAlert($"Are you sure you want to discard your changes?", "", "Yes", "No"))
                    return;
            }
            await Navigation.PopAsync();
        }

        private View GenerateMainView()
        {
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
            
            var existing = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateNoteDataTemplate),
            };
            existing.SetBinding(ListView.ItemsSourceProperty, new Binding(ApplicantModel.Path(nameof(Applicant.Notes))));
            var empty = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateEmptyNoteDataTemplate),
                ItemsSource = new[] { "Add notes" }
            };
            var multi = new MultiBinding
            {
                Converter = new AndBooleans(),
                Bindings =
                {
                    new Binding(ApplicantModel.Path(nameof(Applicant.Notes)), converter: new TrueIfEmpty()),
                    new Binding(nameof(ApplicantModel.IsLoading), converter: new InvertBoolean())
                }
            };
            empty.SetBinding(ListView.IsVisibleProperty, multi);
            var notes = new Grid
            {
                existing,
                empty
            };

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

        private async void Save_Clicked(object? sender, EventArgs e)
        {
            if (context == null)
                return;
            if (!context.ChangeTracker.HasChanges())
                await DisplayAlert($"No changes were made.", "", "Ok");//TODO dont wait if dont care about result
            await context.SaveChangesAsync();//TODO dont save if no changes
            await Task.Run(onChange);
            await Navigation.PopAsync();
        }

        private async void Delete_Clicked(object? sender, EventArgs e)
        {
            if (context == null || model.Applicant == null)
                return;
            if (await DisplayAlert($"Are you sure you want to delete '{model.FullName}'?", "This cannot be undone.", "Yes", "No"))
            {
                context.Applicants.Remove(model.Applicant);
                await context.SaveChangesAsync();
                await Task.Run(onChange);
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
            cell.Tapped += AbilityCell_Tapped;
            return cell;
        }

        private async void AbilityCell_Tapped(object? sender, EventArgs e)
        {
            //TODO unselect item
            if (sender is not Cell cell)
                return;
            if (cell.BindingContext is not Ability ability)
                return;
            if (ability.Criteria is BooleanCriteria boolean)
            {
                await Navigation.PushAsync(new EditBooleanAbility(boolean, ability));
            }
            //TODO (MVP) edit numeric ability
            //else if (ability.Criteria is NumericCriteria numeric)
            //{
            //    await Navigation.PushAsync(new EditNumericAbility(numeric, ability));
            //}
            //TODO (MVP) edit select ability
            //else if (ability.Criteria is SelectCriteria select)
            //{
            //    await Navigation.PushAsync(new EditSelectAbility(select, ability));
            //}
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
            cell.Tapped += NoteCell_Tapped;
            return cell;
        }

        private object GenerateEmptyNoteDataTemplate()
        {
            // BindingContext will be set to a String
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding());
            cell.Tapped += NoteCell_Tapped;
            return cell;
        }

        private async void NoteCell_Tapped(object? sender, EventArgs e)
        {
            //TODO unselect item
            if (model.Applicant is Applicant applicant)
                await Navigation.PushAsync(new AddNote(applicant, show.User));
        }
    }
}
