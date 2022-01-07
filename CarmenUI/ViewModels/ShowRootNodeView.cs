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

        /// <summary>Updates any node which may need updating due to casting changes of the given role</summary>
        public void RoleCastingChanged(Role role)
        {
            ItemNodeView? current_item = null;
            ItemNodeView? previous_item = null;
            bool update_next_item = false;
            foreach (var node_view in Recurse())
            {
                if (node_view is ItemNodeView next_item_view)
                {
                    if (update_next_item)
                    {
                        _ = next_item_view.UpdateAsync();
                        update_next_item = false;
                    }
                    previous_item = current_item;
                    current_item = next_item_view;
                }
                else if (node_view is RoleNodeView role_view && role_view.Role == role)
                {
                    _ = role_view.UpdateAsync(); // this is the role that changed
                    if (previous_item != null)
                        _ = previous_item.UpdateAsync(); // previous item might have consecutive item errors
                    update_next_item = true; // next item might have consecutive item errors
                }
            }
        }

        /// <summary>Updates the given item node</summary>
        public void ItemCastingChanged(Item item)
        {
            foreach (var node_view in Recurse())
            {
                if (node_view is ItemNodeView item_view && item_view.Item == item)
                {
                    _ = item_view.UpdateAsync(); // this is the item that changed
                    break;
                }
            }
        }
    }
}
