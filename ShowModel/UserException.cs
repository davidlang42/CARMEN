using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel
{
    /// <summary>
    /// A type of exception which should be shown to the user, and is guarenteed to have an acceptable user facing Message
    /// </summary>
    public class UserException : Exception
    {
        public UserException(string user_facing_message)
            : base(user_facing_message)
        { }

        /// <summary>Create a UserException containing the Exception message, and Log it</summary>
        public UserException(Exception exception_to_log, string user_facing_message)
            : base(user_facing_message, exception_to_log)
        {
            Log.Warning(exception_to_log, $"{user_facing_message}\n({exception_to_log.Message})");
        }

        public static void Handle(Action action, string user_facing_message)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new UserException(ex, user_facing_message);
            }
        }

        public static T Handle<T>(Func<T> action, string user_facing_message)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                throw new UserException(ex, user_facing_message);
            }
        }
    }
}
