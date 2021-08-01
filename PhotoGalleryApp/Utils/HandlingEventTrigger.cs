using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PhotoGalleryApp.Utils
{
    class HandlingEventTrigger : System.Windows.Interactivity.EventTrigger
    {

        /**
         * Mark the event as handled so that it doesn't propogate any more.
         */
        protected override void OnEvent(EventArgs eventArgs)
        {
            RoutedEventArgs args = eventArgs as RoutedEventArgs;
            if (args != null)
                args.Handled = true;

            base.OnEvent(eventArgs);
        }
    }
}
