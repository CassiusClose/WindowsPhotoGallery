using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A View-type class that maintains a list of ViewModel objects based on a
    /// changing list of Model objects. This class is abstract. Subclasses
    /// should be created for each pair of model & view model and should
    /// implement functions that deal with model & view-model specific
    /// implementations.
    /// </summary>
    public abstract class ModelVMView<ModelType, ViewModelType> where ViewModelType : ViewModelBase
    {
        /// <summary>
        /// </summary>
        /// <param name="modelColl">The collection of model items to reference</param>
        /// <param name="expand">Whether to expand collections of items</param>
        /// <param name="refresh">Whether to initially refresh the collection. Subclasses can pass false if they need to do more initialization before refreshing</param>
        public ModelVMView(ObservableCollection<ModelType> modelColl, bool expand=false, bool refresh=true)
        {
            _expand = expand;

            View = new ObservableCollection<ViewModelType>();

            _modelColl = modelColl;
            _modelColl.CollectionChanged += _modelColl_CollectionChanged;

            if (refresh)
                Refresh();
        }


        public void Cleanup()
        {
            _modelColl.CollectionChanged -= _modelColl_CollectionChanged;
            foreach(ViewModelType vm in View)
                vm.Cleanup();
        }


        // The list of Model items to base the ViewModel list on
        protected ObservableCollection<ModelType> _modelColl;

        // The list of ViewModels
        public ObservableCollection<ViewModelType> View { get; }



        // If Model items are collections, whether or not to create a single
        // ViewModel for the collection or two create a ViewModel for each item
        // within the collection.
        private bool _expand;




        /**
         * Completely rebuilds the list of ViewModels
         */
        public void Refresh()
        {
            CollectionChanged_Reset();
        }




        /// <summary>
        /// Given a Model object, returns a new ViewModel object associated with it.
        /// </summary>
        protected abstract ViewModelType? CreateViewModel(ModelType item);

        protected virtual void PostCreation(ViewModelType vm) {}


        /// <summary>
        /// Given a ViewModel object, returns the associated Model object
        /// </summary>
        protected abstract ModelType GetModel(ViewModelType vm);




        /// <summary>
        /// Given a Model object, returns whether it is a collection
        /// </summary>
        protected abstract bool IsCollection(ModelType item);

        /// <summary>
        /// Given a collection Model object, returns a List of the collection.
        /// This will only be called on Model objects that return true when
        /// passed to IsCollection().
        /// </summary>
        protected abstract IList GetCollection(ModelType item);

        /// <summary>
        /// Adds the given CollectionChanged handler to the given Model's
        /// collection. This should not attach the CollectionChanged handler to
        /// any children of the collection - that will be handled by this
        /// class.
        /// </summary>
        protected abstract void AddCollectionChangedListener(ModelType model, NotifyCollectionChangedEventHandler func);


        /**
         * Attaches this class' CollectionChanged handler to the given Model
         * object. If collections should be expanded, add to all its children
         * too.
         */
        private void _addCollectionChangedListener(ModelType model)
        {
            AddCollectionChangedListener(model, _modelColl_CollectionChanged);

            if(_expand && IsCollection(model))
            {
                foreach(ModelType i in GetCollection(model))
                {
                    if(IsCollection(i))
                        _addCollectionChangedListener(i);
                }
            }
        }


        /// <summary>
        /// Removes the given CollectionChanged handler from the given Model's
        /// collection. This should not remove the CollectionChanged handler
        /// from any children of the collection - that will be handled by this
        /// class.
        /// </summary>
        protected abstract void RemoveCollectionChangedListener(ModelType model, NotifyCollectionChangedEventHandler func);

        /**
         * Removes this class' CollectionChanged handler from the given Model
         * object. If collections should be expanded, remove from all its
         * children too.
         */
        private void _removeCollectionChangedListener(ModelType model)
        {
            RemoveCollectionChangedListener(model, _modelColl_CollectionChanged);

            if(_expand && IsCollection(model))
            {
                foreach(ModelType i in GetCollection(model))
                {
                    if(IsCollection(i))
                        _removeCollectionChangedListener(i);
                }
            }
        }



        /// <summary>
        /// Cleans up the given ViewModel object before it is removed from the
        /// View. This lets subclasses remove any event handlers that they
        /// installed.
        /// </summary>
        protected virtual void PrepareForRemoval(ViewModelType vm) {}



        /** Adds an item to the view */
        protected void AddItem(ModelType item)
        {
            if (_expand && IsCollection(item))
            {
                foreach (ModelType i in GetCollection(item))
                    AddItem(i);
            }
            else 
            {
                ViewModelType? vm = CreateViewModel(item);
                if (vm != null)
                {
                    PostCreation(vm);
                    View.Add((ViewModelType)vm);
                }
            }
        }


        /** When the Model list changes, update the ViewModel list */
        private void _modelColl_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("Adding items to MediaCollection, but NewItems is null");
                    CollectionChanged_Add(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if(e.OldItems == null)
                        throw new ArgumentException("Removing items from MediaCollection, but OldItems is null");
                    CollectionChanged_Remove(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if(e.NewItems == null || e.OldItems == null)
                        throw new ArgumentException("Replacing items in MediaCollection, but NewItems or OldItems is null");
                    CollectionChanged_Replace(e.NewItems, e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    CollectionChanged_Reset();
                    break;
            }
        }

        private void CollectionChanged_Add(IList newItems)
        {
            foreach(ModelType m in newItems)
            {
                if(_expand && IsCollection(m))
                    _addCollectionChangedListener(m);
                AddItem(m);
            }
        }

        private void CollectionChanged_Remove(IList oldItems)
        {
            foreach(ModelType c in oldItems)
            {
                if(_expand && IsCollection(c))
                    _removeCollectionChangedListener(c);

                int i;
                for(i = 0; i < View.Count; i++)
                {
                    ModelType m = GetModel(View[i]);
                    if (m.Equals(c))
                    {
                        PrepareForRemoval(View[i]);
                        View[i].Cleanup();
                        View.RemoveAt(i);
                        return;
                    }
                }
                if(i == View.Count)
                    throw new Exception("Error: Can't find item when removing from MediaView");
            }
        }


        protected void RemoveItem(ModelType item, bool removeChildren=true)
        {
            int i;
            for(i = 0; i < View.Count; i++)
            {
                ModelType m = GetModel(View[i]);
                if (ReferenceEquals(m, item))
                {
                    PrepareForRemoval(View[i]);
                    View[i].Cleanup();
                    View.RemoveAt(i);
                    break;
                }
            }
            if(i == View.Count+1)
                throw new Exception("Error: Can't find item when removing from MediaView");

            if(removeChildren && _expand && IsCollection(item))
            {
                foreach(ModelType child in GetCollection(item))
                    RemoveItem(child);
            }
        }



        private void CollectionChanged_Replace(IList newItems, IList oldItems)
        {
            CollectionChanged_Remove(oldItems);
            CollectionChanged_Add(newItems);
        }


        private void CollectionChanged_Reset()
        {
            foreach(ViewModelType vm in View)
            {
                PrepareForRemoval(vm);
                vm.Cleanup();
            }
            View.Clear();
            foreach (ModelType m in _modelColl)
            {
                if(_expand && IsCollection(m))
                    _addCollectionChangedListener(m);
                AddItem(m);
            }
        }
    }
}
