using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Views
{
    internal class AddNote : ContentPage
    {
        readonly string author;

        public Applicant Applicant { get; }
        public string NewNote { get; set; } = "";

        public AddNote(Applicant applicant, string author)
        {
            Applicant = applicant;
            this.author = author;
            BindingContext = this;

            Title = $"Add note to {applicant.FirstName} {applicant.LastName}";

            var existing = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateNoteDataTemplate),
            };
            existing.SetBinding(ListView.ItemsSourceProperty, new Binding($"{nameof(Applicant)}.{nameof(Applicant.Notes)}"));
            var editor = new Editor
            {
                Placeholder = "Add your comments here",
                AutoSize = EditorAutoSizeOption.TextChanges,
                MaximumHeightRequest = 200 // ~10 lines
            };
            editor.SetBinding(Entry.TextProperty, new Binding(nameof(NewNote)));
            var main = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 5,
                    Children = {
                        existing,
                        editor
                    }
                }
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
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                }
            };
            grid.Add(main);
            var c = 0;
            var back = new Button
            {
                Text = "Back"
            };
            back.Clicked += Back_Clicked;
            grid.Add(back, row: 1, column: c++);
            var add = new Button
            {
                Text = "Add note"
            };
            add.Clicked += Add_Clicked;
            grid.Add(add, row: 1, column: c++);
            grid.SetColumnSpan(main, c);
            Content = grid;
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

        private async void Add_Clicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewNote))
            {
                DisplayAlert("Type your comments into the box above", "", "Ok");
                return;
            }
            Applicant.Notes.Add(new Note
            {
                Applicant = Applicant,
                Text = NewNote,
                Author = author
            });
            await Navigation.PopAsync();
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewNote))
            {
                if (!await DisplayAlert("Are you sure you want to discard your notes?", "", "Yes", "No"))
                    return;
            }
            await Navigation.PopAsync();
        }
    }
}
