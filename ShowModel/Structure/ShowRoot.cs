using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Carmen.ShowModel.Structure
{
    /// <summary>
    /// The (singular) root node of the item tree, including various details about the show
    /// </summary>
    public class ShowRoot : InnerNode //LATER implement INotifyPropertyChanged for completeness
    {
        public DateTime? ShowDate { get; set; }
        public virtual Image? Logo { get; set; }
        /// <summary>The criteria which will be used to determine the order for Cast Numbers (if not set, OverallAbility will be used)</summary>
        public virtual Criteria? CastNumberOrderBy { get; set; }
        public ListSortDirection CastNumberOrderDirection { get; set; }
        public bool AllowConsecutiveItems { get; set; }
        public override InnerNode? Parent
        {
            get => null;
            set
            {
                if (value != null)
                    throw new InvalidOperationException("Parent of ShowRoot must be null.");
            }
        }

        protected override bool GetAllowConsecutiveItems() => AllowConsecutiveItems;
    }
}
