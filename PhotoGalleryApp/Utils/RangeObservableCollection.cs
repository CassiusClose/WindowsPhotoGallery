using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// An ObservableCollection subclass that supports, among others, AddRange functionality, which allows
    /// multiple items to be added with only one notification to any observers.
    /// </summary>
    /// <typeparam name="T">The object held in the collection.</typeparam>
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        /**
         * Whether or not to send notifications to observers. This is set to false
         * in the middle of ranged operations so that each Add/Remove, etc. doesn't
         * send a notification.
         */
        private bool notificationsEnabled = true;

        /// <summary>
        /// Adds a list of items to this collection. The same as calling Add on each
        /// item in the list, but only one notification is send out to observers.
        /// </summary>
        /// <param name="items">The list of items to add to the collection.</param>
        public void AddRange(IEnumerable<T> items)
        {
            notificationsEnabled = false;
            foreach(T item in items)
                Add(item);

            notificationsEnabled = true;

            // Send one change notification
            // The 2nd argument of NotifyCollectionChangedEventArgs must contain only one item
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.First()));
        }

        /// <summary>
        /// Sets the collection's items to be those from the given list. Any previous items
        /// in the collection are removed. Only one notification is sned out to observers.
        /// </summary>
        /// <param name="items">The items that will replace the current collection of items.</param>
        public void ReplaceWith(IEnumerable<T> items)
        {
            notificationsEnabled = false;

            Clear();
            foreach (T item in items)
                Add(item);

            notificationsEnabled = true;

            // Send one change notification
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.First()));

        }

        /**
         * This is the same as the ObservableCollection's method, but it's only called when
         * notifications are enabled.
         */
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (notificationsEnabled)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
