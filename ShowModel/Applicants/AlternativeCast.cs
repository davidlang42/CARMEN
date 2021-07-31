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

namespace ShowModel.Applicants
{
    /// <summary>
    /// An alternating cast within a cast group for which casting should be duplicated.
    /// </summary>
    public class AlternativeCast : INamed, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int AlternativeCastId { get; private set; }

        private string name = "";
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
                if (name != "")
                    Initial = name.Abbreviate(1, 1).First();
            }
        }

        private char initial = 'A';
        public char Initial
        {
            get => initial;
            set
            {
                if (initial == value)
                    return;
                initial = value;
                OnPropertyChanged();
            }
        }

        private readonly ObservableCollection<Applicant> members = new();
        public virtual ICollection<Applicant> Members => members;

        public AlternativeCast()
        {
            members.CollectionChanged += Members_CollectionChanged;
        }

        private void Members_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Members));

        protected void OnPropertyChanged([CallerMemberName]string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
