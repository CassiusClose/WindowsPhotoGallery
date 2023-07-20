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

        public void ReplaceItems(IEnumerable<T> items)
        {
            notificationsEnabled = false;

            Clear();
            foreach (T item in items)
                Add(item);

            notificationsEnabled = true;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Adds a list of items to this collection. The same as calling Add on each
        /// item in the collection, but only one notification is sent out to observers.
        /// </summary>
        /// <param name="items">The list of items to add to the collection.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items.Count<T>() == 0)
                return;

            notificationsEnabled = false;
            foreach (T item in items)
                Add(item);

            notificationsEnabled = true;

            // Send one change notification - Reset means collection has "dramatically changed".
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// Removes a list of items from this collection. The same as calling Remove on
        /// each item in the collection, but only one notification is sent out to observers.
        /// </summary>
        /// <param name="items"></param>
        public void RemoveRange(IEnumerable<T> items)
        {
            if (items.Count<T>() == 0)
                return;

            notificationsEnabled = false;
            foreach (T item in items)
                Remove(item);
            notificationsEnabled = true;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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