using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class SectionNodeView : NodeView
    {
        private Section section;
        private uint totalCast;

        public override string Name => section.Name;

        protected override async Task<(double progress, bool has_errors)> CalculateAsync(double child_progress, bool child_errors)
        {
            child_errors |= !await Task.Run(() => section.CastingMeetsSectionTypeRules(totalCast, out _, out _));
            child_errors |= !await Task.Run(() => section.VerifyConsecutiveItems());
            return (child_progress, child_errors);
        }

        public SectionNodeView(Section section, uint total_cast, AlternativeCast[] alternative_casts)
            : base(section.Children.InOrder().Select(n => CreateView(n, total_cast, alternative_casts)))
        {
            this.section = section;
            this.totalCast = total_cast;
        }
    }
}
