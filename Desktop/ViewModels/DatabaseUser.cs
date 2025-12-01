using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Desktop.ViewModels
{
    internal class DatabaseUser
    {
        public string Name { get; }
        
        public string Host { get; }

        /// <summary>Null means exactly the current database name</summary>
        public string? Database { get; }

        public bool IsAdmin { get; }

        public bool CanWrite { get;  }

        public bool CanRead { get; }

        public DatabaseUser(string name, string host, string database, string current_database, string[] grants)
        {
            Name = name;
            Host = host;
            Database = database == current_database ? null : database;
            //TODO set IsAdmin/CanWrite/CanRead from grants
        }

        public string Description
        {
            get
            {
                var sb = new StringBuilder();
                if (Host == "%") {
                    sb.Append(Name);
                } else {
                    sb.Append($"'{Name}'@'{Host}'");
                }
                if (Database != null) {
                    sb.Append($" ({Database})");
                }
                if (IsAdmin) {
                    sb.Append(" ADMIN");
                }
                if (CanWrite) {
                    sb.Append(" WRITE");
                }
                if (CanRead) {
                    sb.Append(" READ");
                }
                return sb.ToString();
            }
        }

        public void GrantRead()
        {
            //TODO GRANT SELECT ON `cgs_2026`.* TO 'USERNAME'@'%';
        }

        public void GrantWrite()
        {
            //TODO GRANT ALL ON `cgs_2026`.* TO 'USERNAME'@'%';
        }

        public void GrantAdmin()
        {
            //TODO GRANT CREATE USER ON *.* TO 'USERNAME'@'%' IDENTIFIED BY 'PASSWORD' REQUIRE SSL WITH GRANT OPTION;
            //TODO GRANT SELECT ON mysql.* TO 'USERNAME'@'%';
        }
    }
}
