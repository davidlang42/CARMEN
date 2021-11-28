using Carmen.ShowModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using CarmenUI.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.Selection;
using Carmen.CastingEngine.Allocation;
using CarmenUI.ViewModels;
using CarmenUI.UserControls;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

namespace CarmenUI.Pages
{
    /// <summary>
    /// The base class of all sub pages, which handles the database context and returns a flagged enum of what objects have changed.
    /// </summary>
    public class SubPage : PageFunction<DataObjects>, IDisposable
    {
        bool disposed = false;
        private ShowContext? _context;
        protected readonly RecentShow connection;
        Window? windowWithEventsAttached;
        DataObjects saved_changes = DataObjects.None;

        protected ShowContext context => _context
            ?? throw new ApplicationException("Tried to use context after it was disposed.");

        public SubPage(RecentShow connection)
        {
            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
            this.connection = connection;
            RecreateContext();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Information($"{GetType().Name}.{nameof(Window_Closing)}");
            CommitTextboxValue();
            if (context.ChangeTracker.HasChanges())
            {
                var result = MessageBox.Show("Do you want to save changes?", WindowTitle, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.No)
                {
                    LogChanges();
                    return;
                }
                if (result == MessageBoxResult.Cancel || !await SaveChanges(false))
                    e.Cancel = true;
            }
        }

        private void LogChanges()
        {
            Log.Information($"OUTSTANDING CHANGES");
            context.LogChanges(LogEventLevel.Warning);
        }

        /// <summary>Returns the database username, if set, otherwise the local machine username</summary>
        protected string GetUserName() => string.IsNullOrWhiteSpace(connection.User) ? Environment.UserName : connection.User;

        private void RecreateContext()
        {
            Log.Information($"{GetType().Name}.{nameof(RecreateContext)}");
            _context?.Dispose();
            _context = ShowContext.Open(connection);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            windowWithEventsAttached = Window.GetWindow(this);
            windowWithEventsAttached.KeyDown += Window_KeyDown;
            windowWithEventsAttached.Closing += Window_Closing;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (windowWithEventsAttached != null)
            {
                windowWithEventsAttached.KeyDown -= Window_KeyDown;
                windowWithEventsAttached.Closing -= Window_Closing;
            }
        }

        protected virtual async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.S && Properties.Settings.Default.SaveOnCtrlS)
                {
                    await SaveChanges();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter && Properties.Settings.Default.SaveAndExitOnCtrlEnter)
                {
                    await SaveChangesAndReturn();
                    e.Handled = true;
                }
            }
        }

        /// <summary>If a text box is focussed, this commits its value.</summary>
        protected static void CommitTextboxValue()
        {
            if (Keyboard.FocusedElement is TextBox text_box
                && text_box.IsEnabled && !text_box.IsReadOnly
                && text_box.GetBindingExpression(TextBox.TextProperty) is BindingExpression binding)
            {
                binding.UpdateSource();
            }
        }

        /// <summary>Save changes and exit to main menu if succeeded</summary>
        protected async Task SaveChangesAndReturn(bool user_initiated = true)
        {
            if (await SaveChanges(user_initiated))
                OnReturn(new ReturnEventArgs<DataObjects>(saved_changes));
        }

        /// <summary>Save changes to the database and return true if succeeded</summary>
        protected async Task<bool> SaveChanges(bool user_initiated = true)
        {
            Log.Information($"{GetType().Name}.{nameof(SaveChanges)}({user_initiated})");
            CommitTextboxValue();
            if (!await PreSaveChecks())
                return false;
            if (!context.ChangeTracker.HasChanges())
            {
                if (user_initiated)
                    MessageBox.Show("No unsaved changes have been made.", WindowTitle);
                return true;
            }
            using var saving = new LoadingOverlay(this) { MainText = "Saving..." };
            var changes = context.DataChanges();
            var saved = false;
            while (!saved)
            {
                try
                {
                    await context.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Log.Error(ex, $"{GetType().Name}.{nameof(SaveChanges)}({user_initiated})");
                    LogChanges();
                    foreach (var entry in ex.Entries)
                    {
                        var proposed_values = entry.CurrentValues;
                        if (entry.GetDatabaseValues() is PropertyValues database_values)
                        {
                            foreach (var property in proposed_values.Properties)
                            {
                                var proposed = proposed_values[property];
                                var database = database_values[property];
                                var msg = $"A conflicting change has been made to {entry.Metadata.Name} (possibly caused by another user). " +
                                    $"Would you like to overwrite their {property.Name} value of '{database}' with your value of '{proposed}'?";
                                var result = MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNoCancel);
                                if (result == MessageBoxResult.Cancel)
                                    return false;
                                if (result == MessageBoxResult.No)
                                    proposed_values[property] = database;
                            }
                            entry.OriginalValues.SetValues(database_values); // clear conflict for next time
                        }
                        else
                        {
                            var msg = $"A conflicting change has been made to {entry.Metadata.Name} (possibly caused by another user). " +
                                $"Would you like to skip this object and save all other changes?";
                            var result = MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNoCancel);
                            if (result == MessageBoxResult.Cancel)
                                return false;
                            if (result == MessageBoxResult.Yes)
                                entry.State = EntityState.Detached; // force skip this entry so other changes can succeed
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"{GetType().Name}.{nameof(SaveChanges)}({user_initiated})");
                    MessageBox.Show($"Error while saving changes: {ex.InnermostException().Message}\nChanges not saved.");
                    return false;
                }
            }
            saved_changes |= changes;
            Log.Information($"{GetType().Name}.{nameof(SaveChanges)}({user_initiated}) SUCCESS");
            return true;
        }

        /// <summary>Check any data consistency constraints before allowing save to be processed.
        /// This may contain user interaction. Return true to continue or false to cancel saving.</summary>
        protected virtual Task<bool> PreSaveChecks() => Task.FromResult(true);

        /// <summary>Confirm cancel with the user and exit to main menu</summary>
        protected void CancelChangesAndReturn()
        {
            if (CancelChanges())
                OnReturn(new ReturnEventArgs<DataObjects>(saved_changes));
        }

        /// <summary>Confirms cancel with the user (if any changes have been made) and returns true if its okay to cancel</summary>
        protected bool CancelChanges()
        {
            Log.Error($"{GetType().Name}.{nameof(CancelChanges)}");
            CommitTextboxValue();
            if (!context.ChangeTracker.HasChanges())
                return true; // no changes, always okay to cancel
            if (MessageBox.Show("Are you sure you want to cancel?\nAny unsaved changes will be lost.", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                return false;
            LogChanges();
            return true;
        }

        /// <summary>Reverts changes by refreshing the context.
        /// Returns true if a full context refresh is required.</summary>
        protected bool RevertChanges()
        {
            Log.Error($"{GetType().Name}.{nameof(RevertChanges)}");
            if (!context.ChangeTracker.HasChanges())
                return false; // no changes to revert
            using var reverting = new LoadingOverlay(this);
            reverting.MainText = "Reverting...";
            RecreateContext();
            return true;
        }

        protected static string ParseAuditionEngine()
        {
            var name = Properties.Settings.Default.AuditionEngine;
            if (AuditionEngine.Implementations.Any(t => t.Name == name))
                return name;
            return AuditionEngine.Implementations.First().Name;
        }

        protected static string ParseSelectionEngine()
        {
            var name = Properties.Settings.Default.SelectionEngine;
            if (SelectionEngine.Implementations.Any(t => t.Name == name))
                return name;
            return SelectionEngine.Implementations.First().Name;
        }

        protected static string ParseAllocationEngine()
        {
            var name = Properties.Settings.Default.AllocationEngine;
            if (AllocationEngine.Implementations.Any(t => t.Name == name))
                return name;
            return AllocationEngine.Implementations.First().Name;
        }

        protected bool NeuralEngineConfirm(string message)
        {
            var result = MessageBox.Show(message, WindowTitle, MessageBoxButton.YesNo);
            Log.Error($"{GetType().Name}.{nameof(NeuralEngineConfirm)}={result}");
            return result == MessageBoxResult.Yes;
        }

        protected bool Confirm(string msg)
        {
            var result = MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNo);
            Log.Error($"{GetType().Name}.{nameof(Confirm)}={result}");
            return result == MessageBoxResult.Yes;
        }

        protected void EditableImage_ImageChanged(object sender, ImageChangedEventArgs e)
        {
            if (e.OldImage != null)
                context.Images.Remove(e.OldImage); // this assumes images are only ever used once
        }

        /// <summary>Actually dispose change handlers, etc.
        /// This will only be called once.</summary>
        protected virtual void DisposeInternal()
        {
            context.Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                DisposeInternal();
                disposed = true;
            }
        }
    }
}
