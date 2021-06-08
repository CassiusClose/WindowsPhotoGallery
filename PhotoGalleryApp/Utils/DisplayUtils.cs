using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A set of utility functions that have to do with how content is displayed to the user.
    /// </summary>
    class DisplayUtils
    {
        /// <summary>
        /// Returns a list of the items in a given ListBox that are visible within the given container.
        /// </summary>
        /// <param name="listbox">The ListBox with which to find visible items.</param>
        /// <param name="container">The container that the listbox items must be visible in.</param>
        /// <returns>A list of the ListBox's items that are visible within the container.</returns>
        public static List<object> GetVisibleItemsFromListBox(ListBox listbox, FrameworkElement container)
        {
            List<object> items = new List<object>();

            foreach (var item in listbox.Items)
            {
                // Add element if it's visible
                if (IsElementVisible((ListBoxItem)listbox.ItemContainerGenerator.ContainerFromItem(item), container))
                {
                    items.Add(item);
                }
                // Items are displayed sequentially, so if we've already reached the visible items (i.e. the list is 
                // not empty), then when we reach an image that isn't visible, we can stop the process, since no
                // images after that will be visible either.
                else if (items.Any())
                {
                    break;
                }
            }

            return items;
        }


        /// <summary>
        /// Returns whether or not the given element is visible within the given container.
        /// </summary>
        /// <param name="element">The element to determine visibility on.</param>
        /// <param name="container">The container which the element must be visible in.</param>
        /// <returns>Whether or not the element is visible in the container.</returns>
        public static bool IsElementVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect elementBounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            // Return true if any of the element is within the visible bounds of the container
            return containerBounds.Contains(elementBounds.TopLeft) || containerBounds.Contains(elementBounds.BottomRight);
        }
    }
}
