using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Carmen.ShowModel.Structure
{
    public class SectionType : INameOrdered, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int SectionTypeId { get; private set; }

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
            }
        }

        private bool allowMultipleRoles = false;
        public bool AllowMultipleRoles
        {
            get => allowMultipleRoles;
            set
            {
                if (allowMultipleRoles == value)
                    return;
                allowMultipleRoles = value;
                OnPropertyChanged();
            }
        }

        private bool allowNoRoles = false;
        public bool AllowNoRoles
        {
            get => allowNoRoles;
            set
            {
                if (allowNoRoles == value)
                    return;
                allowNoRoles = value;
                OnPropertyChanged();
            }
        }

        private bool allowConsecutiveItems = true;
        /// <summary>AllowConsecutiveItems defaults to true in SectionType, but false in ShowRoot,
        /// so that the consecutive item check does not run multiple times by default</summary>
        public bool AllowConsecutiveItems
        {
            get => allowConsecutiveItems;
            set
            {
                if (allowConsecutiveItems == value)
                    return;
                allowConsecutiveItems = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Section> sections = new();
        public virtual ICollection<Section> Sections => sections;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
