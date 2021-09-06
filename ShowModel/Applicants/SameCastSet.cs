using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Applicants
{
    /// <summary>
    /// A set of Applicants which must be kept in the same AlternativeCast
    /// </summary>
    public class SameCastSet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int SameCastSetId { get; private set; }

        private ObservableCollection<Applicant> applicants = new();
        public virtual ICollection<Applicant> Applicants => applicants;

        public string Description
        {
            get
            {
                var total_count = Applicants.Count;
                if (total_count == 0)
                    return "Empty set";
                var families = applicants.OrderBy(a => a.LastName).GroupBy(a => a.LastName).ToDictionary(g => g.Key, g => g.ToArray());
                var missing_last_name = families.ContainsKey("");
                var all_families_have_mulitple_members = families.Values.All(arr => arr.Length > 1);
                string summary;
                if (!missing_last_name && all_families_have_mulitple_members && families.Count <= 3)
                    // describe as families
                    summary = families.Keys.Select(n => n.Plural()).JoinWithCommas();
                else
                    // describe with initials
                    summary = families.Values.SelectMany(arr => arr
                        .OrderBy(a => a.FirstName)
                        .Select(a => a.FirstName.Initial() + a.LastName.Initial()))
                        .JoinWithCommas();
                return $"Set of {total_count} ({summary})";
            }
        }

        public SameCastSet()
        {
            applicants.CollectionChanged += Applicants_CollectionChanged;
        }

        private void Applicants_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Description));

        public bool VerifyAlternativeCasts(out AlternativeCast? common_alternative_cast)
        {
            common_alternative_cast = null;
            foreach (var applicant in Applicants)
            {
                if (applicant.CastGroup is not CastGroup cg)
                    continue; // skip not accepted applicants
                if (!cg.AlternateCasts)
                    continue; // skip if CastGroup doesn't alternate casts
                if (applicant.AlternativeCast is not AlternativeCast ac)
                    continue; // skip if AlternativeCast not set yet
                if (common_alternative_cast == null)
                    common_alternative_cast = ac;
                else if (common_alternative_cast != ac)
                    return false;
            }
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
