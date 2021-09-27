using Microsoft.EntityFrameworkCore;
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
using Carmen.CastingEngine;
using Carmen.CastingEngine.Base;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CarmenUI.Pages
{
    /// <summary>
    /// The base class of all sub pages, which handles the database context and returns a flagged enum of what objects have changed.
    /// </summary>
    public class SubPage : PageFunction<DataObjects>, IDisposable
    {
        bool disposed = false;
        private ShowContext? _context;
        private DbContextOptions<ShowContext> contextOptions;
        Window? windowWithEventsAttached;
        DataObjects saved_changes = DataObjects.None;

        protected ShowContext context => _context
            ?? throw new ApplicationException("Tried to use context after it was disposed.");

        public SubPage(DbContextOptions<ShowContext> context_options)
        {
            this.Loaded += Page_Loaded;
            this.Unloaded += Page_Unloaded;
            this.contextOptions = context_options;
            RecreateContext();
        }

        private void RecreateContext()
        {
            _context?.Dispose();
            _context = new ShowContext(contextOptions);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            windowWithEventsAttached = Window.GetWindow(this);
            windowWithEventsAttached.KeyDown += Window_KeyDown;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (windowWithEventsAttached != null)
                windowWithEventsAttached.KeyDown -= Window_KeyDown;
        }

        protected virtual void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.S && Properties.Settings.Default.SaveOnCtrlS)
                {
                    SaveChanges();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter && Properties.Settings.Default.SaveAndExitOnCtrlEnter)
                {
                    SaveChangesAndReturn();
                    e.Handled = true;
                }
            }
        }

        /// <summary>If a text box is focussed, this commits its value.</summary>
        private void CommitTextboxValue()
        {
            if (Keyboard.FocusedElement is TextBox text_box
                && text_box.IsEnabled && !text_box.IsReadOnly
                && text_box.GetBindingExpression(TextBox.TextProperty) is BindingExpression binding)
            {
                binding.UpdateSource();
            }
        }

        /// <summary>Save changes and exit to main menu if succeeded</summary>
        protected void SaveChangesAndReturn()
        {
            if (SaveChanges())
                OnReturn(new ReturnEventArgs<DataObjects>(saved_changes));
        }

        /// <summary>Save changes to the database and return true if succeeded</summary>
        protected bool SaveChanges(bool user_initiated = true)
        {
            CommitTextboxValue();
            if (!PreSaveChecks())
                return false;
            if (!context.ChangeTracker.HasChanges())
            {
                if (user_initiated)
                    MessageBox.Show("No unsaved changes have been made.", WindowTitle);
                return true;
            }
            using var saving = new LoadingOverlay(this);
            saving.MainText = "Saving...";
            var changes = context.DataChanges();
            context.SaveChanges(); //LATER handle db errors, could this be async?
            saved_changes |= changes;
            return true;
        }

        /// <summary>Check any data consistency constraints before allowing save to be processed.
        /// This may contain user interaction. Return true to continue or false to cancel saving.</summary>
        protected virtual bool PreSaveChecks() => true;

        /// <summary>Confirm cancel with the user and exit to main menu</summary>
        protected void CancelChangesAndReturn()
        {
            if (CancelChanges())
                OnReturn(new ReturnEventArgs<DataObjects>(saved_changes));
        }

        /// <summary>Confirms cancel with the user (if any changes have been made) and returns true if its okay to cancel</summary>
        protected bool CancelChanges()
        {
            CommitTextboxValue();
            if (!context.ChangeTracker.HasChanges())
                return true; // no changes, always okay to cancel
            if (MessageBox.Show("Are you sure you want to cancel?\nAny unsaved changes will be lost.", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                return false;
            return true;
        }

        /// <summary>Reverts changes by refreshing the context.
        /// Returns true if a full context refresh is required.</summary>
        protected bool RevertChanges()
        {
            if (!context.ChangeTracker.HasChanges())
                return false; // no changes to revert
            using var reverting = new LoadingOverlay(this);
            reverting.MainText = "Reverting...";
            RecreateContext(); //LATER there has to be a better way than this
            return true;
        }

        protected string ParseApplicantEngine()
        {
            var name = Properties.Settings.Default.ApplicantEngine;
            if (ApplicantEngine.Implementations.Any(t => t.Name == name))
                return name;
            return ApplicantEngine.Implementations.First().Name;
        }

        protected string ParseSelectionEngine()
        {
            var name = Properties.Settings.Default.SelectionEngine;
            if (SelectionEngine.Implementations.Any(t => t.Name == name))
                return name;
            return SelectionEngine.Implementations.First().Name;
        }

        protected string ParseAllocationEngine()
        {
            var name = Properties.Settings.Default.AllocationEngine;
            if (AllocationEngine.Implementations.Any(t => t.Name == name))
                return name;
            return AllocationEngine.Implementations.First().Name;
        }

        protected bool NeuralEngineConfirm(string message)
            => MessageBox.Show(message, WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

        protected bool Confirm(string msg)
            => MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

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
