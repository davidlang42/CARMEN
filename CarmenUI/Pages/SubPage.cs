using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace CarmenUI.Pages
{
    /// <summary>
    /// The base class of all sub pages, which handles the database context and returns a flagged enum of what objects have changed.
    /// </summary>
    public class SubPage : PageFunction<DataObjects>, IDisposable
    {
        bool disposed = false;
        private ShowContext? _context;

        protected ShowContext context => _context
            ?? throw new ApplicationException("Tried to use context after it was disposed.");

        public SubPage(DbContextOptions<ShowContext> context_options)
        {
            _context = new ShowContext(context_options);
        }

        protected void OnReturn(DataObjects changes)
            => OnReturn(new ReturnEventArgs<DataObjects>(changes));

        /// <summary>Save changes to the database and return true if succeeded</summary>
        protected bool SaveChanges()
        {
            context.SaveChanges(); //LATER handle db errors
            return true;
        }

        /// <summary>Confirms cancel with the user (if any changes have been made) and returns true if its okay to cancel</summary>
        protected bool CancelChanges()
        {
            if (!context.ChangeTracker.HasChanges())
                return true; // no changes, always okay to cancel
            return MessageBox.Show("Are you sure you want to cancel?\nAny unsaved changes will be lost.", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
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
