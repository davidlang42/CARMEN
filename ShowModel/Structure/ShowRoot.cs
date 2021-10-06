using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// The (singular) root node of the item tree, including various details about the show
    /// </summary>
    public class ShowRoot : InnerNode, IOverallWeighting
    {
        private DateTime? showDate;
        public DateTime? ShowDate
        {
            get => showDate;
            set
            {
                if (showDate == value)
                    return;
                showDate = value;
                OnPropertyChanged();
            }
        }

        private Image? logo;
        public virtual Image? Logo
        {
            get => logo;
            set
            {
                if (logo == value)
                    return;
                logo = value;
                OnPropertyChanged();
            }
        }

        private Criteria? castNumberOrderBy = null;
        /// <summary>The criteria which will be used to determine the order for Cast Numbers (if not set, OverallAbility will be used)</summary>
        public virtual Criteria? CastNumberOrderBy
        {
            get => castNumberOrderBy;
            set
            {
                if (castNumberOrderBy == value)
                    return;
                castNumberOrderBy = value;
                OnPropertyChanged();
            }
        }

        private ListSortDirection castNumberOrderDirection = ListSortDirection.Ascending;
        public ListSortDirection CastNumberOrderDirection
        {
            get => castNumberOrderDirection;
            set
            {
                if (castNumberOrderDirection == value)
                    return;
                castNumberOrderDirection = value;
                OnPropertyChanged();
            }
        }

        protected override bool _allowConsecutiveItems => allowConsecutiveItems;
        private bool allowConsecutiveItems = false;
        /// <summary>AllowConsecutiveItems defaults to true in SectionType, but false in ShowRoot,
        /// so that the consecutive item check does not run multiple times by default</summary>
        public new bool AllowConsecutiveItems
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

        private double? commonOverallWeight = 1;
        /// <summary>If set, this is the common weighting of the OverallAbility compared to the requirement weights,
        /// used in calculating the suitability of an applicant for a role. This weighting will be applied to the
        /// OverallSuitability only once in the weighted average calculation, regardless of how many other requirements
        /// are involved.
        /// If not set, the OverallWeight of each individual requirement will be used, but only included in the weighted
        /// average calculation if that requirement is required by the role.
        /// NOTE: This means that having a CommonOverallWeight is different to each requirement having the same value
        /// for OverallWeight if there is more than one requirement for a role.</summary>
        public double? CommonOverallWeight
        {
            get => commonOverallWeight;
            set
            {
                if (value.HasValue && value.Value < 0)
                    value = 0;
                if (commonOverallWeight == value)
                    return;
                commonOverallWeight = value;
                OnPropertyChanged();
            }
        }

        public override InnerNode? Parent
        {
            get => null;
            set
            {
                if (value != null)
                    throw new InvalidOperationException("Parent of ShowRoot must be null.");
            }
        }

        double IOverallWeighting.OverallWeight
        {
            get => CommonOverallWeight ?? throw new InvalidOperationException("Cannot use ShowRoot as IOverallWeighting if CommonOverallWeight is not set");
            set
            {
                if (!CommonOverallWeight.HasValue)
                    throw new InvalidOperationException("Cannot use ShowRoot as IOverallWeighting if CommonOverallWeight is not set");
                CommonOverallWeight = value;
            }
        }
    }
}
