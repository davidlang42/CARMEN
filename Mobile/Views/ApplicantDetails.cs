using Carmen.Desktop.Converters;
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
    //TODO change app icon/splash/colours
    //TODO handle crashes when back (button or arrow) is pressed while still loading, probably requires moving loading into model and having a cancel method
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

#if ANDROID || IOS
            var main = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    GenerateMainView(),
                    GenerateSideView()
                }
            };
#else
            var main = new Grid
            {
                ColumnSpacing = 5,
                ColumnDefinitions = new()
                {
                    new(GridLength.Star),
                    new(GridLength.Auto)
                }
            };
            main.Add(new ScrollView
            {
                Content = GenerateMainView()
            });
            main.Add(GenerateSideView(), column: 1);
#endif

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
            delete.Clicked += Delete_Clicked;
            delete.SetBinding(Button.IsEnabledProperty, new Binding(nameof(ApplicantModel.IsLoading), converter: new InvertBoolean()));
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            grid.Add(delete, row: 1, column: c++);
            var save = new Button
            {
                Text = "Save",
            };
            save.Clicked += Save_Clicked;
            save.SetBinding(Button.IsEnabledProperty, new Binding(nameof(ApplicantModel.IsLoading), converter: new InvertBoolean()));
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
                DisplayAlert("Applicant does not exist", "This is probably because someone else has deleted them.", "Ok");
                await Navigation.PopAsync();
                return;
            }
            model.Loaded(applicant);
            var image = await Task.Run(() => applicant.Photo); //TODO cache photos, and/or load only if internet is not restricted (or the user clicks load)
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
            //TODO (NOW) make a way to add abilities which aren't set yet, and delete (clear) ones which are
            var fields = ListViewNoScroll(GenerateFieldDataTemplate, nameof(ApplicantModel.Fields));
            var abilities = ListViewNoScroll(GenerateAbilityDataTemplate, ApplicantModel.Path(nameof(Applicant.Abilities)));
            var notes = ListViewNoScroll(GenerateNoteDataTemplate, ApplicantModel.Path(nameof(Applicant.Notes)));
            var no_notes = ListViewNoScroll(GenerateEmptyNoteDataTemplate);
            no_notes.ItemsSource = new[] { "Add notes" };
            var multi = new MultiBinding
            {
                Converter = new AndBooleans(),
                Bindings =
                {
                    new Binding(ApplicantModel.Path(nameof(Applicant.Notes)), converter: new TrueIfEmpty()),
                    new Binding(nameof(ApplicantModel.IsLoading), converter: new InvertBoolean())
                }
            };
            no_notes.SetBinding(ListView.IsVisibleProperty, multi);
            return new VerticalStackLayout
            {
                fields,
                abilities,
                notes,
                no_notes
            };
        }

        static ListView ListViewNoScroll(Func<object> item_template_generator, string? items_source_binding_path = null)
        {
            var list = new ListView
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                ItemTemplate = new DataTemplate(item_template_generator)
            };
            if (items_source_binding_path != null)
                list.SetBinding(ListView.ItemsSourceProperty, new Binding(items_source_binding_path));
            return list;
        }

        private View GenerateSideView()
        {
            var activity = new ActivityIndicator();
            activity.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(ApplicantModel.IsLoadingPhoto)));
            var image = new MC.Image();
            image.SetBinding(MC.Image.SourceProperty, new Binding(nameof(ApplicantModel.Photo)));
            return new Grid
            {
                image,
                activity
            };
        }

        private async void Save_Clicked(object? sender, EventArgs e)
        {
            if (context == null)
                return;
            if (!context.ChangeTracker.HasChanges())
            {
                DisplayAlert($"No changes were made.", "", "Ok");
            }
            else
            {
                model.Saving();
                await context.SaveChangesAsync();
                await Task.Run(onChange);
            }
            await Navigation.PopAsync();
        }

        private async void Delete_Clicked(object? sender, EventArgs e)
        {
            if (context == null || model.Applicant is not Applicant applicant)
                return;
            if (await DisplayAlert($"Are you sure you want to delete '{model.FullName}'?", "This cannot be undone.", "Yes", "No"))
            {
                model.Saving();
                context.Applicants.Remove(applicant);
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
            if (sender is not Cell cell || cell.Parent is not ListView list)
                return;
            ClearOtherSelections(list);
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
            var multi = new MultiBinding
            {
                Converter = new FakeItTilYouUpdateIt
                {
                    new AbilityMarkFormatter()
                },
                Bindings =
                {
                    new Binding(),
                    new Binding(nameof(Ability.Mark))
                }
            };
            cell.SetBinding(TextCell.DetailProperty, multi);
            cell.Tapped += AbilityCell_Tapped;
            return cell;
        }

        private async void AbilityCell_Tapped(object? sender, EventArgs e)
        {
            if (sender is not Cell cell || cell.Parent is not ListView list)
                return;
            ClearOtherSelections(list);
            if (cell.BindingContext is not Ability ability)
                return;
            if (ability.Criteria is BooleanCriteria boolean)
            {
                await Navigation.PushAsync(new EditBooleanAbility(boolean, ability));
            }
            else if (ability.Criteria is NumericCriteria numeric)
            {
                await Navigation.PushAsync(new EditNumericAbility(numeric, ability));
            }
            else if (ability.Criteria is SelectCriteria select)
            {
                await Navigation.PushAsync(new EditSelectAbility(select, ability));
            }
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
            if (sender is not Cell cell || cell.Parent is not ListView list)
                return;
            ClearOtherSelections(list);
            if (model.Applicant is Applicant applicant)
                await Navigation.PushAsync(new AddNote(applicant, show.User));
        }

        private void ClearOtherSelections(ListView list)
        {
            if (list.Parent is IContainer container)
                foreach (var other_list in container.OfType<ListView>().Where(l => l != list))
                    other_list.SelectedItem = null;
        }
    }
}
