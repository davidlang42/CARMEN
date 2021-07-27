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
        /// <summary>There appears to be a significant delay the first time a DbSet<typeparamref name="T"/>
        /// property is accessed on the DbContext, therefore this extension method has been added to
        /// perform the DbSet__get and DbSet.Load() asyncronously. The result of the returned task
        /// is the DbSet itself, to assist in chaining.</summary>
        public static Task<DbSet<U>> ColdLoadAsync<T, U>(this T context, Func<T, DbSet<U>> db_set_getter) where T : DbContext where U : class
            => Task.Run<DbSet<U>>(() =>
            {
                var db_set = db_set_getter(context);
                db_set.Load();
                return db_set;
            });
    }
}
