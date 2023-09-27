using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Extensions
{
    internal static class MauiExtensions
    {
        /// <summary>This is primarily required because ImageButton doesn't work.
        /// It completely ignores the Aspect setting and displays like junk, it is unusable.
        /// This extension allows you to add a Tapped event handler to a non-tappable Image, which DOES work.</summary>
        public static void AddTapHandler(this View view, EventHandler<TappedEventArgs> handler, int number_of_taps_required = 1)
        {
            var tapped = new TapGestureRecognizer
            {
                NumberOfTapsRequired = number_of_taps_required
            };
            tapped.Tapped += handler;
            view.GestureRecognizers.Add(tapped);
        }
    }
}
