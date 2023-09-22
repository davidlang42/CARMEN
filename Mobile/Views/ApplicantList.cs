﻿using Carmen.Mobile.Converters;
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
    internal class ApplicantList : ContentPage
    {
        readonly Applicants model;
        readonly ConnectionDetails show;
        readonly bool allowEditing;
        ShowContext? context;

        public ApplicantList(ConnectionDetails show, string show_name, bool allow_editing)
        {
            model = new();
            this.show = show;
            allowEditing = allow_editing;
            Loaded += ApplicantList_Loaded;
            this.Unloaded += ApplicantList_Unloaded;
            BindingContext = model;
            Title = "Applicants for " + show_name;

            var loading = new ActivityIndicator { IsRunning = true };
            loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(nameof(Applicants.IsLoading)));

            //TODO add list filtering & sorting
            //TODO some way to group by audition group
            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(Applicants.Collection)));

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
                    new ColumnDefinition(GridLength.Star)
                }
            };
            grid.Add(loading);
            grid.Add(list);
            var c = 0;
            var back = new Button
            {
                Text = "Back",
                BackgroundColor = Colors.Gray
            };
            back.Clicked += Back_Clicked; ;
            grid.Add(back, row: 1, column: c++);
            if (allowEditing)
            {
                var add = new Button { Text = "Add new applicant" };
                add.Clicked += AddButton_Clicked;
                grid.ColumnDefinitions.Add(new(GridLength.Star));
                grid.Add(add, row: 1, column: c++);
                grid.SetColumnSpan(loading, c);
                grid.SetColumnSpan(list, c);
            }
            Content = grid;
        }

        private async void Back_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void ApplicantList_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            var collection = await Task.Run(() => context.Applicants.ToObservableCollection());
            model.Loaded(collection);
        }

        private void ApplicantList_Unloaded(object? sender, EventArgs e)
        {
            context?.Dispose();
            context = null;
        }

        private void AddButton_Clicked(object? sender, EventArgs e)
        {
            //TODO (EDIT) add new applicant
        }

        private object GenerateDataTemplate()
        {
            // BindingContext will be set to an Applicant
            var cell = new TextCell();
            var full_name = new MultiBinding
            {
                Converter = new FullName()
            };
            full_name.Bindings.Add(new Binding(nameof(Applicant.FirstName)));
            full_name.Bindings.Add(new Binding(nameof(Applicant.LastName)));
            cell.SetBinding(TextCell.TextProperty, full_name);
            cell.SetBinding(TextCell.DetailProperty, new Binding(nameof(Applicant.Description)));
            cell.Tapped += Cell_Tapped;
            return cell;
        }

        private async void Cell_Tapped(object? sender, EventArgs e)
        {
            var cell = (Cell)sender!;
            if (cell.BindingContext is not Applicant applicant)
                return;
            if (allowEditing)
                await Navigation.PushAsync(new EditApplicant(show, applicant.ApplicantId, applicant.FirstName, applicant.LastName)); //TODO (EDIT) need to reload the edited entity on return: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.changetracking.entityentry.reload?view=efcore-1.1
            else
                await Navigation.PushAsync(new ViewApplicant(show, applicant.ApplicantId, applicant.FirstName, applicant.LastName));
        }
    }
}
