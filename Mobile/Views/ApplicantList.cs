﻿using Carmen.Mobile.Converters;
using Carmen.Mobile.Models;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Microsoft.Graphics.Canvas.Text;
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

            //TODO add list filtering
            var list = new ListView
            {
                ItemTemplate = new DataTemplate(GenerateDataTemplate),
            };
            list.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(Applicants.Collection)));

            var grid = new Grid
            {
                Margin = 5,
                RowSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star)
                }
            };
            grid.Add(loading);
            grid.Add(list);
            if (allowEditing)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                var button = new Button { Text = "Add new applicant" };
                button.Clicked += AddButton_Clicked;
                grid.Add(button, row: 1);
            }
            Content = grid;
        }

        private async void ApplicantList_Loaded(object? sender, EventArgs e)
        {
            context = ShowContext.Open(show);
            var collection = await Task.Run(() => context.Applicants.ToObservableCollection());
            model.Loaded(collection);
        }

        private void ApplicantList_Unloaded(object? sender, EventArgs e)
        {
            //TODO check when this is called
            context?.Dispose();
            context = null;
        }

        private void AddButton_Clicked(object? sender, EventArgs e)
        {
            //TODO add new applicant
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
            return cell;//TODO click to edit/view
        }
    }
}
