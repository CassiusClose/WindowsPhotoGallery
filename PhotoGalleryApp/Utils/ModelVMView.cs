using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A View-type class that maintains a list of ViewModel objects based on a changing list of Model objects.
    /// </summary>
    public class ModelVMView<ModelType, ViewModelType> where ViewModelType : ViewModelBase
    {
        /// <summary>
        /// </summary>
        /// <param name="modelColl">The collection of model items to reference</param>
        /// <param name="createVM">A function that creates a ViewModel object from a given Model object</param>
        /// <param name="getModel">A function that returns the Model object from an associated ViewModel</param>
        public ModelVMView(ObservableCollection<ModelType> modelColl, CreateViewModelDelegate createVM, GetModelDelegate getModel)
        {
            CreateViewModel = createVM;
            GetModel = getModel;

            View = new ObservableCollection<ViewModelType>();

            _modelColl = modelColl;
            _modelColl.CollectionChanged += _modelColl_CollectionChanged;

            CollectionChanged_Reset();
        }

        public void Cleanup()
        {
            _modelColl.CollectionChanged -= _modelColl_CollectionChanged;
            foreach(ViewModelType vm in View)
                vm.Cleanup();
        }


        // The list of Model items to base the ViewModel list on
        private ObservableCollection<ModelType> _modelColl;

        // The list of ViewModels
        public ObservableCollection<ViewModelType> View { get; }



        /// <summary>
        /// Given a Model object, returns a new ViewModel object associated with it.
        /// </summary>
        /// <param name="m">The Model to create a ViewModel for</param>
        /// <returns>The associated ViewModel</returns>
        public delegate ViewModelType CreateViewModelDelegate(ModelType m);
        private CreateViewModelDelegate CreateViewModel;

        private ViewModelType _createViewModel(ModelType m)
        {
            ViewModelType vm = CreateViewModel(m);
            return vm;
        }


        /// <summary>
        /// Given a ViewModel object, returns the associated Model object
        /// </summary>
        /// <param name="vm">A ViewModelType object</param>
        /// <returns>The associated ModelType object</returns>
        public delegate ModelType GetModelDelegate(ViewModelType vm);
        private GetModelDelegate GetModel;


        /** Adds an item to the view */
        private void AddItem(ViewModelType vm)
        {
            View.Add(vm);
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
                AddItem(_createViewModel(m));
            }
        }

        private void CollectionChanged_Remove(IList oldItems)
        {
            foreach(ModelType c in oldItems)
            {
                int i;
                for(i = 0; i < View.Count; i++)
                {
                    ModelType m = GetModel(View[i]);
                    if (m.Equals(c))
                    {
                        View.RemoveAt(i);
                        return;
                    }
                }
                if(i == View.Count)
                    throw new Exception("Error: Can't find item when removing from MediaView");
            }
        }


        private void CollectionChanged_Replace(IList newItems, IList oldItems)
        {
            CollectionChanged_Remove(oldItems);
            CollectionChanged_Add(newItems);
        }


        private void CollectionChanged_Reset()
        {
            View.Clear();
            foreach (ModelType m in _modelColl)
                View.Add(CreateViewModel(m));
        }
    }
}
