using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        static Regex readRegex = new("GRANT SELECT ON `([^`]+)`", RegexOptions.Compiled);
        static Regex writeRegex = new("GRANT ALL PRIVILEGES ON `([^`]+)`", RegexOptions.Compiled);
        static string adminStart = "GRANT CREATE USER ON *.* ";
        static string adminEnd = "WITH GRANT OPTION";

        public DatabaseUser(string name, string host, string database, string current_database, string[] grants)
        {
            Name = name;
            Host = host;
            Database = database;
            HideDatabase = database == current_database;
            IsAdmin = grants.Any(g => g.StartsWith(adminStart) && g.EndsWith(adminEnd));
            CanWrite = FindMatch(grants, writeRegex, current_database);
            CanRead = FindMatch(grants, readRegex, current_database) || CanWrite;
        }

        bool FindMatch(string[] grants, Regex db_getter, string db_match)
        {
            foreach (var g in grants) {
                var m = db_getter.Match(g);
                if (m.Success && SqlLike(m.Groups[1].Value, db_match)) {
                    return true;
                }
            }
            return false;
        }

        bool SqlLike(string pattern, string match)
        {
            if (!pattern.Contains("%")) {
                return pattern == match;
            }
            var like_regex = new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(pattern, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline);
            return like_regex.IsMatch(match);
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
            yield return $"GRANT CREATE USER ON *.* TO '{Name}'@'{Host}' WITH GRANT OPTION;";
        }
    }
}
