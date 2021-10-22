using Carmen.ShowModel.Applicants;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// The count of applicants required for a role from a certain group.
    /// </summary>
    [Owned]
    public class CountByGroup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private CastGroup castGroup = null!;
        public virtual CastGroup CastGroup
        {
            get => castGroup;
            set
            {
                if (castGroup == value)
                    return;
                castGroup = value;
                OnPropertyChanged();
            }
        }

        private uint count;
        /// <summary>The number of applicants required of this CastGroup</summary>
        public uint Count
        {
            get => count;
            set
            {
                if (count == value)
                    return;
                count = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
