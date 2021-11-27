using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class AutoSelectSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool? selectCastGroups;
        public bool? SelectCastGroups
        {
            get => selectCastGroups;
            set
            {
                if (selectCastGroups == value)
                    return;
                selectCastGroups = value;
                OnPropertyChanged();
            }
        }

        private bool? balanceAlternativeCasts;
        public bool? BalanceAlternativeCasts
        {
            get => balanceAlternativeCasts;
            set
            {
                if (balanceAlternativeCasts == value)
                    return;
                balanceAlternativeCasts = value;
                if (value == false)
                {
                    AllocateCastNumbers = false;
                    ApplyTags = false;
                }
                OnPropertyChanged();
            }
        }

        private bool? applyTags;
        public bool? ApplyTags
        {
            get => applyTags;
            set
            {
                if (applyTags == value)
                    return;
                applyTags = value;
                if (value != false && BalanceAlternativeCasts == false)
                    BalanceAlternativeCasts = null;
                OnPropertyChanged();
            }
        }

        private bool? allocateCastNumbers;
        public bool? AllocateCastNumbers
        {
            get => allocateCastNumbers;
            set
            {
                if (allocateCastNumbers == value)
                    return;
                allocateCastNumbers = value;
                if (value != false && BalanceAlternativeCasts == false)
                    BalanceAlternativeCasts = null;
                OnPropertyChanged();
            }
        }

        public string CastGroupNames { get; }
        public string AlternativeCastNames { get; }
        public string TagNames { get; }
        public string CastNumberOrderName { get; }

        public AutoSelectSettings(CastGroup[] cast_groups, AlternativeCast[] alternative_casts, Tag[] tags, ShowRoot show_root)
        {
            CastGroupNames = string.Join(", ", cast_groups.Select(cg => cg.Name));
            AlternativeCastNames = string.Join(", ", alternative_casts.Select(cg => cg.Name));
            TagNames = string.Join(", ", tags.Select(cg => cg.Name));
            CastNumberOrderName = $"by {show_root.CastNumberOrderBy?.Name ?? "Overall Ability"} {show_root.CastNumberOrderDirection.ToString().ToLower()}";
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
