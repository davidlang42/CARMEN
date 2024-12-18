﻿using Carmen.Desktop.Converters;
using Carmen.Mobile.Converters;
using Carmen.Mobile.Extensions;
using Carmen.Mobile.Models;
using Carmen.Mobile.Popups;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using CommunityToolkit.Maui.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
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

            var loading = new VerticalStackLayout
            {
                new ActivityIndicator { IsRunning = true }
            };
            loading.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(nameof(ApplicantModel.IsLoading)));

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
                }
            };
            grid.Add(loading);
            grid.Add(main);
            var c = 0;
            var back = new Button
            {
                Text = "Back"
            };
            back.Clicked += Back_Clicked;
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            grid.Add(back, row: 1, column: c++);
            var delete = new Button
            {
                Text = "Delete",
                BackgroundColor = Colors.LightCoral
            };
            delete.Clicked += Delete_Clicked;
#if !IOS
            // iOS doesn't change text color back to normal when enabled, so never disable it
            delete.SetBinding(Button.IsEnabledProperty, new Binding(nameof(ApplicantModel.IsLoading), converter: new InvertBoolean()));
#endif
            grid.ColumnDefinitions.Add(new(GridLength.Star));
            grid.Add(delete, row: 1, column: c++);
            var save = new Button
            {
                Text = "Save",
                BackgroundColor = Colors.SeaGreen
            };
            save.Clicked += Save_Clicked;
#if !IOS
            // iOS doesn't change text color back to normal when enabled, so never disable it
            save.SetBinding(Button.IsEnabledProperty, new Binding(nameof(ApplicantModel.IsLoading), converter: new InvertBoolean()));
#endif
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
            var criterias = await context.Criterias.ToArrayAsync();
            model.Loaded(applicant, criterias);
            ImageSource? source;
            if (applicant.PhotoImageId is int image_id)
                source = await CachedImage(image_id, applicant.ShowRoot, () => applicant.Photo ?? throw new ApplicationException("Applicant photo not set, but photo ID was."));
            else if (await Task.Run(() => applicant.Photo) is SM.Image image)
                source = await ActualImage(image);
            else
                source = ImageSource.FromFile("no_photo.png");
            model.LoadedPhoto(source);
        }

        const string ImageCacheExtension = "BMP";
        static string GetCachePath(ShowRoot show)
            => $"{FileSystem.Current.CacheDirectory}{Path.DirectorySeparatorChar}{string.Concat(show.Name.Split(Path.GetInvalidFileNameChars()))}{Path.DirectorySeparatorChar}";

        static async Task<ImageSource> CachedImage(int image_id, ShowRoot show, Func<SM.Image> lazy_loading_photo_getter)
        {
            var cache_path = GetCachePath(show);
            var filename = $"{cache_path}{image_id}.{ImageCacheExtension}";
            if (!File.Exists(filename))
                await Task.Run(() =>
                {
                    if (!Directory.Exists(cache_path))
                        UserException.Handle(() => Directory.CreateDirectory(cache_path), "Error creating image cache path.");
                    UserException.Handle(() => File.WriteAllBytes(filename, lazy_loading_photo_getter().ImageData), "Error caching image.");
                });
            return ImageSource.FromFile(filename);
        }

        static async Task<ImageSource?> ActualImage(SM.Image photo)
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
            var fields = ListViewNoScroll(GenerateFieldDataTemplate, nameof(ApplicantModel.Fields));
            var abilities = ListViewNoScroll(GenerateAbilityDataTemplate, ApplicantModel.Path(nameof(Applicant.Abilities)));
            var no_abilities = ListViewNoScroll(GenerateCriteriaDataTemplate, nameof(ApplicantModel.MissingCriterias));
            abilities.ItemAppearing += (_, e) =>
            {
                if (addingAbility == e.Item) // if the item appear is the one we know we are adding
                {
                    addingAbility = null;
                    abilities.SelectedItem = e.Item; // then select the new ability
                }
            };
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
            notes.ItemAppearing += (_, e) =>
            {
                if (notes.SelectedItem != null // if another note was selected
                    || no_notes.SelectedItem != null) // or "add notes" was selected
                    notes.SelectedItem = e.Item; // then select the new note
            };
            return new VerticalStackLayout
            {
                fields,
                abilities,
                no_abilities,
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
            var loading = new VerticalStackLayout
            {
                new ActivityIndicator { IsRunning = true }
            };
            loading.SetBinding(VerticalStackLayout.IsVisibleProperty, new Binding(nameof(ApplicantModel.IsLoadingPhoto)));
            var image = new MC.Image();
            image.SetBinding(MC.Image.SourceProperty, new Binding(nameof(ApplicantModel.Photo)));
            image.AddTapHandler(Image_Clicked);
            return new Grid
            {
                image,
                loading
            };
        }

        private async void Image_Clicked(object? sender, EventArgs e)
        {
            if (model.Applicant == null)
                return;
            // choose how we get the image
            Func<MediaPickerOptions?, Task<FileResult?>> getter;
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var options = new[]
                {
                    "Take a photo",
                    "Pick an existing photo"
                };
                var popup = new ListPopup<string>(options, s => s);
                var result = await this.ShowPopupAsync(popup);
                if (result == options[0])
                    getter = MediaPicker.Default.CapturePhotoAsync;
                else if (result == options[1])
                    getter = MediaPicker.Default.PickPhotoAsync;
                else
                    return;
            }
            else
            {
                getter = MediaPicker.Default.PickPhotoAsync;
            }
            // actually get the image
            if (await getter(null) is FileResult file)
            {
                var photo = new SM.Image
                {
                    Name = file.FileName
                };
                using Stream source_stream = await file.OpenReadAsync();
                using (var memory_stream = new MemoryStream())
                {
                    source_stream.CopyTo(memory_stream);
                    photo.ImageData = memory_stream.ToArray();
                }
                model.Applicant.Photo = photo;
                model.LoadedPhoto(await ActualImage(photo));
            }
        }

        private async void Save_Clicked(object? sender, EventArgs e)
        {
            if (context == null || model.IsLoading)
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
            if (context == null || model.IsLoading || model.Applicant is not Applicant applicant)
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
            await EditAbility(ability);
        }

        private void DeleteAbility(Ability ability)
        {
            if (model.Applicant is not Applicant applicant)
                return;
            applicant.Abilities.Remove(ability);
        }

        private async Task EditAbility(Ability ability)
        {
            if (ability.Criteria is BooleanCriteria boolean)
            {
                await Navigation.PushAsync(new EditBooleanAbility(boolean, ability, () => DeleteAbility(ability)));
            }
            else if (ability.Criteria is NumericCriteria numeric)
            {
                await Navigation.PushAsync(new EditNumericAbility(numeric, ability, () => DeleteAbility(ability)));
            }
            else if (ability.Criteria is SelectCriteria select)
            {
                await Navigation.PushAsync(new EditSelectAbility(select, ability, () => DeleteAbility(ability)));
            }
        }

        private object GenerateCriteriaDataTemplate()
        {
            // BindingContext will be set to a Criteria (which this applicant doesn't have an Ability for)
            var cell = new TextCell();
            cell.SetBinding(TextCell.TextProperty, new Binding(nameof(Criteria.Name), stringFormat: "Add {0}"));
            cell.Tapped += CriteriaCell_Tapped;
            return cell;
        }

        Ability? addingAbility = null;
        private async void CriteriaCell_Tapped(object? sender, EventArgs e)
        {
            if (sender is not Cell cell || cell.Parent is not ListView list)
                return;
            ClearOtherSelections(list);
            if (cell.BindingContext is not Criteria criteria)
                return;
            if (model.Applicant is not Applicant applicant)
                return;
            var ability = new Ability
            {
                Criteria = criteria,
                Applicant = applicant
            };
            addingAbility = ability;
            applicant.Abilities.Add(ability);
            await EditAbility(ability);
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
