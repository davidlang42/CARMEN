using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
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
