using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A utility class for the architecture when a Collection of items wants
    /// to keep an totalled list of a certain field in its children. So for
    /// example, a collection of ICollectable objects wants to maintain a list
    /// that contains all of the Tags in all of its children.
    /// 
    /// This class maintains that list through changes in the parent collection
    /// and changes in each child's field.
    /// 
    /// The children's field to be collected in the list is given by ItemType.
    /// The field in the child may either be a list of objects or a single
    /// object. This can change per child, and this is why this class'
    /// constructor accepts so many functions as arguments. These functions are
    /// used to determine whether a child has a single object or collection of
    /// objects, and to retreive whichever is the case.
    /// </summary>
    /// <typeparam name="CollType">The type of the child, the type stored in the parent collection</typeparam>
    /// <typeparam name="ItemType">The type of the field stored in the child, the field to be maintained in a list</typeparam>
    public class MaintainedParentCollection<CollType, ItemType> where CollType : NotifyPropertyChanged
    {
        /// <summary>
        /// Maintains a totaled list for a certain field in the children of the
        /// given parent collection.
        /// 
        /// See the docs for each function delegate for more details on when
        /// they will be used.
        /// </summary>
        /// <param name="coll">The parent collection from which to maintain a
        /// totaled list</param>
        /// <param name="isItemCollection">A function which returns whether
        /// a child object has a single instance of the desired field or a
        /// collection</param>
        /// <param name="getItemCollection">A function which returns a
        /// collection of the desired field from a given child.</param>
        /// <param name="getItem">A function which returns the instance of the
        /// desired field from a given child</param>
        /// <param name="getItemPropertyName">A function which returns the name
        /// of the desired field in a given child.</param>
        public MaintainedParentCollection(ObservableCollection<CollType> coll, 
                                          IsItemCollection isItemCollection, 
                                          GetItemCollection getItemCollection, 
                                          GetItem getItem,
                                          GetItemPropertyName getItemPropertyName)
        {
            Items = new RangeObservableCollection<ItemType>();

            _isItemCollection = isItemCollection;
            _getItemCollection = getItemCollection;
            _getItem = getItem;
            _getItemPropertyName = getItemPropertyName;

            _coll = coll;
            _coll.CollectionChanged += Parent_CollectionChanged;

            ParentCollectionChanged_Reset();
        }

        // The parent collection to monitor
        private ObservableCollection<CollType> _coll;


        /// <summary>
        /// The complete list of all the items in the children's desired field
        /// </summary>
        public RangeObservableCollection<ItemType> Items { get; protected set; }



        /// <summary>
        /// Given a child item, returns whether that item stores a single
        /// instance of the desired field or a collection.
        /// </summary>
        /// <param name="item">A child item</param>
        /// <returns>Whether the field is a singleton or a collection</returns>
        public delegate bool IsItemCollection(CollType item);
        private IsItemCollection _isItemCollection;

        /// <summary>
        /// Given a child item, returns the item's collection of the desired
        /// field. This will only be called when IsItemCollection() returns
        /// true for this item.
        /// </summary>
        /// <param name="item">A child item</param>
        /// <returns>The child's collection of the desired field</returns>
        public delegate ObservableCollection<ItemType> GetItemCollection(CollType item);
        private GetItemCollection _getItemCollection;

        /// <summary>
        /// Given a child item, returns the item's instance of the desired
        /// field. This will only be called when IsItemCollection() returns
        /// false for this item.
        /// </summary>
        /// <param name="item">A child item</param>
        /// <returns>The child's instance of the desired field</returns>
        public delegate ItemType? GetItem(CollType item);
        private GetItem _getItem;

        /// <summary>
        /// Given a child item, returns the name of the item's desired field.
        /// This will only be called when IsItemCollection() returns false for
        /// this item.
        /// </summary>
        /// <param name="item">A child item</param>
        /// <returns>The name of the item's desired field</returns>
        public delegate string GetItemPropertyName(CollType item);
        private GetItemPropertyName _getItemPropertyName;


        #region Parent Collection Changed

        /**
         * If the parent's collection changes, maintain listeners to all of the
         * childrens' change events, and also update the total list of the
         * field based on what changed.
         */
        private void Parent_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) 
            { 
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding tags to child of MediaCollection, but NewItems is null");

                    ParentCollectionChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing tags from a child of MediaCollection, but OldItems is null");

                    ParentCollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing tags from a child of MediaCollection, but OldItems or NewItems is null");

                    ParentCollectionChanged_Remove(e.OldItems);
                    ParentCollectionChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ParentCollectionChanged_Reset();
                    break;
            }
        }

        private void ParentCollectionChanged_Add(IList newItems)
        {
            foreach(CollType item in newItems)
            {
                item.PropertyChanged += Child_PropertyChanged;
                if (_isItemCollection(item))
                {
                    _getItemCollection(item).CollectionChanged += Child_CollectionChanged;

                    ChildCollectionChanged_Add(_getItemCollection(item));
                }
                else
                {
                    ItemType? i = _getItem(item);
                    if(i != null)
                    {
                        List<ItemType> list = new List<ItemType>();
                        list.Add(i);
                        ChildCollectionChanged_Add(list);
                    }

                }

            }
        }
        private void ParentCollectionChanged_Remove(IList oldItems)
        {
            foreach(CollType item in oldItems)
            {
                item.PropertyChanged -= Child_PropertyChanged;
                if(_isItemCollection(item))
                {
                    _getItemCollection(item).CollectionChanged -= Child_CollectionChanged;
                    ChildCollectionChanged_Remove(_getItemCollection(item));
                }
                else
                {
                    ItemType? i = _getItem(item);
                    if (i != null)
                    {
                        List<ItemType> list = new List<ItemType>();
                        list.Add(i);
                        ChildCollectionChanged_Remove(list);
                    }
                }
            }
        }

        private void ParentCollectionChanged_Reset()
        {
            foreach(CollType item in _coll)
            {
                item.PropertyChanged -= Child_PropertyChanged;
                item.PropertyChanged += Child_PropertyChanged;

                if(_isItemCollection(item))
                {
                    _getItemCollection(item).CollectionChanged -= Child_CollectionChanged;
                    _getItemCollection(item).CollectionChanged += Child_CollectionChanged;
                }
            }

            ChildCollectionChanged_Reset();
        }

        #endregion Parent Collection Changed


        /**
         * When a child's singleton field changes, update the list
         */
        private void Child_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not CollType)
                throw new ArgumentException("PropertyChanged in MaintainedParentCollection must be of CollType");

            // No way to tell what the old value was, so just reset the list
            if(!_isItemCollection((CollType)sender) && e.PropertyName == _getItemPropertyName((CollType)sender))
                ChildCollectionChanged_Reset();
        }


        #region Child Collection Changed

        /**
         * When a child has a collection of the desired field & it changes, update the list
         */
        private void Child_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding tags to child of MediaCollection, but NewItems is null");

                    ChildCollectionChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("Removing tags from a child of MediaCollection, but OldItems is null");

                    ChildCollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing tags from a child of MediaCollection, but OldItems or NewItems is null");

                    ChildCollectionChanged_Add(e.NewItems);
                    ChildCollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ChildCollectionChanged_Reset();
                    break;
            }
        }

        private void ChildCollectionChanged_Add(IList newItems)
        {
            foreach(ItemType item in newItems)
            {
                if (!Items.Contains(item))
                    Items.Add(item);
            }
        }

        private void ChildCollectionChanged_Remove(IList oldI)
        {
            List<ItemType> oldItems = oldI.Cast<ItemType>().ToList();

            // Go through existing children, remove any items from oldItems
            // that still exist as children's locations. Whatever is left
            // is what should be removed
            foreach(CollType child in _coll)
            {
                for (int i = 0; i < oldItems.Count; i++)
                {
                    if(_isItemCollection(child))
                    {
                        if (_getItemCollection(child).Contains(oldItems[i]))
                            oldItems.RemoveAt(i--);
                    }
                    else
                    {
                        if (ReferenceEquals(_getItem(child), oldItems[i]))
                            oldItems.RemoveAt(i--);
                    }
                }
            }

            // Remove the items that are no longer in the children.
            foreach(ItemType item in oldItems)
            {
                Items.Remove(item);
            }
        }


        private void ChildCollectionChanged_Reset()
        {
            List<ItemType> newItems = new List<ItemType>();

            // Go through each child, add the field/collection to the list if not already added
            foreach(CollType child in _coll)
            {
                if (_isItemCollection(child))
                {
                    foreach(ItemType item in _getItemCollection(child))
                    {
                        if (!newItems.Contains(item))
                            newItems.Add(item);
                    }
                }
                else
                {
                    ItemType? childItem = _getItem(child);
                    if (childItem != null && !Items.Contains(childItem))
                        newItems.Add(childItem);
                }
            }

            // Replace the list & send out one "reset" notification. It's likely more efficient than
            // sending out a "reset" when calling Clear() and then sending out an "add" for each tag
            Items.ReplaceItems(newItems);
        }

        #endregion  Child Collection Changed
    }
}
