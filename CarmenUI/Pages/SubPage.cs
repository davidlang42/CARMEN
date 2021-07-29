using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace CarmenUI.Pages
{
    /// <summary>
    /// The base class of all sub pages, which handles the database context and returns a flagged enum of what objects have changed.
    /// </summary>
    public class SubPage : PageFunction<DataObjects>, IDisposable
    {
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

        //LATER this will crash if still running when the page is cancelled, maybe I need to wrap this in a LoadingOverlay afterall
        protected Task<IList> TaskToLoad<T>(Func<ShowContext, DbSet<T>> db_set_getter) where T : class
            => new Task<IList>(() =>
            {
                var db_set = db_set_getter(context);
                db_set.Load();
                return db_set.Local.ToObservableCollection();
            });

        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
    }
}
