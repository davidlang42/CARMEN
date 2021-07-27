using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI
{
    public static class Extensions
    {
        /// <summary>DbSet<typeparamref name="T"/>.LoadAsync() does not appear to run asynchronously when awaited,
        /// so this extension method has been added to implement a workaround at a single point in the code.</summary>
        public static Task LoadAsyncAwaitable<T>(this DbSet<T> db_set) where T : class
            => Task.Run(() => db_set.Load());
    }
}
