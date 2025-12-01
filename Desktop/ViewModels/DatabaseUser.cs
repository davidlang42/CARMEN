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

        public string Database { get; }

        public bool HideDatabase { get; }

        public bool IsAdmin { get; }

        public bool CanWrite { get;  }

        public bool CanRead { get; }

        public DatabaseUser(string name, string host, string database, string current_database, string[] grants)
        {
            Name = name;
            Host = host;
            Database = database;
            HideDatabase = database == current_database;
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
                    sb.Append($"{Name}@{Host}");
                }
                if (!HideDatabase) {
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

        public string SqlToDeleteUser() => $"DROP USER '{Name}'@'{Host}';";

        public string SqlToGrantRead() => $"GRANT SELECT ON `{Database}`.* TO '{Name}'@'{Host}';";

        public string SqlToGrantWrite() => $"GRANT ALL ON `{Database}`.* TO '{Name}'@'{Host}';";

        public IEnumerable<string> SqlToGrantAdmin()
        {
            yield return $"GRANT SELECT ON mysql.* TO '{Name}'@'{Host}';";
            yield return $"GRANT CREATE USER ON *.* TO '{Name}'@'{Host}' WITH GRANT OPTION;"; //TODO this grant option doesn't look right
        }
    }
}
