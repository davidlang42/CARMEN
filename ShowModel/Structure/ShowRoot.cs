using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
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
    public class ShowRoot : InnerNode
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

        private double overallSuitabilityWeight = 1;
        /// <summary>The weighting of the OverallAbility compared to other requirement weights in
        /// calculating the suitability of an applicant for a role.</summary>
        public double OverallSuitabilityWeight
        {
            get => overallSuitabilityWeight;
            set
            {
                if (overallSuitabilityWeight == value)
                    return;
                overallSuitabilityWeight = value;
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
    }
}
