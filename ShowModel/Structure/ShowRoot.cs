using ShowModel.Applicants;
using System;
using System.Linq;

namespace ShowModel.Structure
{
    /// <summary>
    /// The (singular) root node of the item tree, including various details about the show
    /// </summary>
    public class ShowRoot : InnerNode //LATER implement INotifyPropertyChanged for completeness
    {
        public override string Name { get; set; } = "Show";
        public DateTime? ShowDate { get; set; }
        public virtual Image? Logo { get; set; }
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
