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

namespace CarmenUI.Pages
{
    /// <summary>
    /// The base class of all sub pages, which handles the database context and returns a flagged enum of what objects have changed.
    /// </summary>
    public class SubPage : PageFunction<DataObjects>, IDisposable
    {
        bool disposed = false;
        private ShowContext? _context;
        private DbContextOptions<ShowContext> context_options;

        protected ShowContext context => _context
            ?? throw new ApplicationException("Tried to use context after it was disposed.");

        public SubPage(DbContextOptions<ShowContext> context_options)
        {
            this.context_options = context_options;
            _context = new ShowContext(context_options);
        }

        protected void OnReturn(DataObjects changes)
            => OnReturn(new ReturnEventArgs<DataObjects>(changes));

        /// <summary>Save changes to the database and return true if succeeded</summary>
        protected bool SaveChanges()
        {
            using var saving = new LoadingOverlay(this);
            saving.MainText = "Saving...";
            context.SaveChanges(); //LATER handle db errors, could this be async?
            return true;
        }

        /// <summary>Confirms cancel with the user (if any changes have been made) and returns true if its okay to cancel</summary>
        protected bool CancelChanges()
        {
            if (!context.ChangeTracker.HasChanges())
                return true; // no changes, always okay to cancel
            return MessageBox.Show("Are you sure you want to cancel?\nAny unsaved changes will be lost.", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        /// <summary>Reverts changes by refreshing the context.
        /// Returns true if a full context refresh is required.</summary>
        protected bool RevertChanges()
        {
            if (!context.ChangeTracker.HasChanges())
                return false; // no changes to revert
            using var reverting = new LoadingOverlay(this);
            reverting.MainText = "Reverting...";
            _context?.Dispose();
            _context = new ShowContext(context_options); //LATER there has to be a better way than this
            return true;
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
