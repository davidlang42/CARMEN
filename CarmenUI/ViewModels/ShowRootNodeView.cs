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
    public class ShowRootNodeView : NodeView
    {
        public ShowRoot ShowRoot { get; init; }

        public override string Name => ShowRoot.Name;

        protected override async Task<(double progress, bool has_errors)> CalculateAsync(double child_progress, bool child_errors)
        {
            child_errors |= !await Task.Run(() => ShowRoot.VerifyConsecutiveItems());
            return (child_progress, child_errors);
        }

        public ShowRootNodeView(ShowRoot show_root, uint total_cast, AlternativeCast[] alternative_casts)
            : base(show_root.Children.InOrder().Select(n => CreateView(n, total_cast, alternative_casts)))
        {
            ShowRoot = show_root;
        }
    }
}
