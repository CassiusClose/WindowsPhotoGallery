using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public MapViewModel(NavigatorViewModel nav, PhotoGalleryApp.Models.Map map, bool disableEditMode = false, bool disablePreviews = false, bool displayToolbar = true)
        {
            _nav = nav;
            _disableEditMode = disableEditMode;
            _disablePreviews = disablePreviews;
            _displayToolbar = displayToolbar;

            _map = map;
            _map.CollectionChanged += MapItems_CollectionChanged;

            _addLocationCommand = new RelayCommand(AddLocation);
            _addPathCommand = new RelayCommand(AddPath);
            _finishEditingCommand = new RelayCommand(FinishEditing);
            _deleteItemCommand = new RelayCommand(DeleteMapItem);
            _editItemCommand = new RelayCommand(EditMapItem);
            _newTrackFromSelectedCommand = new RelayCommand(NewTrackFromSelected);
            _splitTrackAtSelectedCommand = new RelayCommand(SplitTrackAtSelected);


            _mapItems = new ObservableCollection<MapItemViewModel>();
            MapItems = CollectionViewSource.GetDefaultView(_mapItems);
            MapItems_Reset();
        }

        public override void Cleanup()
        {
            _map.CollectionChanged -= MapItems_CollectionChanged;
        }


        #region Fields and Properties

        private NavigatorViewModel _nav;

        private PhotoGalleryApp.Models.Map _map;


        private ObservableCollection<MapItemViewModel> _mapItems;
        public ICollectionView MapItems { get; }



        private bool _displayToolbar = true;
        /// <summary>
        /// Whether the toolbar above the map should be displayed
        /// </summary>
        public bool DisplayToolbar
        {
            get { return _displayToolbar; }
        }


        #endregion Fields and Properties



        #region Create MapItem View Model

        /**
         * When creating a MapItemViewModel, call this, and it will handle what
         * method to actually call
         */
        private MapItemViewModel CreateMapItemViewModel(MapItem item)
        {
            // If the user provides an override, call that instead of the default
            if (MapItemViewModelGenerator != null)
                return MapItemViewModelGenerator(item);

            return CreateMapItemViewModelDefault(item);
        }


        /// <summary>
        /// Creates and returns a MapItemViewModel around the given MapItem.
        /// </summary>
        public delegate MapItemViewModel? CreateMapItemViewModelDelegate(MapItem item);
        private CreateMapItemViewModelDelegate? MapItemViewModelGenerator = null;


        /// <summary>
        /// Outside users can specify their own generator function to create
        /// custom MapItemViewModels. If the function is null, the default
        /// generator will be use
        /// </summary>
        /// <param name="func"></param>
        public void SetMapItemViewModelGenerator(CreateMapItemViewModelDelegate? func)
        {
            MapItemViewModelGenerator = func;
        }


        /**
         * The default generator creates MapPathViewModels and MapLocationViewModels
         */
        private MapItemViewModel? CreateMapItemViewModelDefault(MapItem item)
        {
            if (item is MapPath)
                return new MapPathViewModel(_nav, (MapPath)item);

            if (item is MapLocation)
                return new MapLocationViewModel(_nav, (MapLocation)item);

            return null;
        }

        #endregion Create MapItem View Model




        #region Add/Delete Commands


        private RelayCommand _addLocationCommand;
        public ICommand AddLocationCommand => _addLocationCommand;

        private void AddLocation(object parameter)
        {
            if (parameter is not Location)
                throw new ArgumentException("AddLocation argument must be the location to add at");

            // Show the user the popup to create a location
            CreateLocationPopupViewModel popup = new CreateLocationPopupViewModel((Location)parameter);
            CreateLocationPopupReturnArgs args = (CreateLocationPopupReturnArgs)_nav.OpenPopup(popup);

            // If not cancelled, then create the location
            if (args.PopupAccepted)
            {
                MapLocation loc = new MapLocation(args.Name, args.Location);
                _map.Add(loc);
            }
        }


        private RelayCommand _deleteItemCommand;
        public ICommand DeleteItemCommand => _deleteItemCommand;

        /**
         * Remove the given MapItemViewModel from the list
         */
        public void DeleteMapItem(object parameter)
        {
            if (parameter is not MapItemViewModel)
                throw new ArgumentException("Argument to Delete Map Item command must be MapObjectViewModel");

            _map.Remove(((MapItemViewModel)parameter).GetModel());
        }


        private RelayCommand _addPathCommand;
        public ICommand AddPathCommand => _addPathCommand;

        private void AddPath(object parameter)
        {
            // Show the user the popup to create a path
            CreatePathPopupViewModel popup = new CreatePathPopupViewModel();
            CreatePathPopupReturnArgs args = (CreatePathPopupReturnArgs)_nav.OpenPopup(popup);

            // If not cancelled, then create the path
            if (args.PopupAccepted)
            {
                MapPath path = new MapPath(args.Name);

                if (args.LoadFromFile)
                {
                    LocationCollection coll = PathFileFormats.LoadFromTxtFile(args.Filename);
                    path.Locations = coll;
                }

                _map.Add(path);

                // Only put into edit mode if the user didn't provide path data
                if (!args.LoadFromFile)
                {
                    foreach (MapItemViewModel vm in _mapItems)
                    {
                        if (vm.GetModel() == path)
                            EditableMapItem = vm;
                    }
                }
            }
        }


        #endregion Add/Delete Commands


        #region MapItem Click

        public delegate void MapItemClicked(MapItemViewModel vm);
        /// <summary>
        /// Called when the user clicks on MapItem
        /// </summary>
        public MapItemClicked? MapItemClickEvent = null;


        /**
         * Toggle the preview box of the item if enabled, and call the event
         */
        public void MapItemClick(object parameter)
        {
            if (parameter is not MapItemViewModel)
                throw new ArgumentException("TogglePreview() needs a MapItemViewModel");

            MapItemViewModel vm = (MapItemViewModel)parameter;

            if (!_disablePreviews)
                TogglePreview(vm);

            if (MapItemClickEvent != null)
                MapItemClickEvent(vm);
        }

        #endregion MapItem Click



        #region Preview Boxes

        // Whether to show the previews or not
        private bool _disablePreviews;


        /**
         * Close all preview boxes
         */
        private void CloseAllPreviews(MapItemViewModel? exception = null)
        {
            foreach (MapItemViewModel vm in _mapItems)
            {
                if (exception == null || !ReferenceEquals(exception, vm))
                    vm.PreviewOpen = false;
            }
        }


        /**
         * Open or close the preview box for the given MapItemViewModel. If
         * EditMode is enabled, then the preview will not open.
         */
        public void TogglePreview(MapItemViewModel vm)
        {
            if (EditMode)
            {
                vm.PreviewOpen = false;
            }
            else
            {
                foreach (MapItemViewModel o in _mapItems)
                {
                    if (ReferenceEquals(vm, o))
                        o.PreviewOpen = !o.PreviewOpen;
                    else if (o.PreviewOpen)
                        o.PreviewOpen = false;
                }
            }
        }

        #endregion Preview Boxes



        #region Item Editing

        /**
         * Disables putting an item into EditMode. Note that if the
         * MapItemViewModel sets its own EditMode, this cannot prevent that.
         * But if it follows the expected architecture where the MapViewModel
         * sets EditMode of its children, then this will work.
         */
        private bool _disableEditMode = false;


        private MapItemViewModel? _editableMapItem = null;
        /**
         * The MapItemViewModel that is currently being edited, or null if none
         * are being edited. When this is set, it takes care of setting the EditMode
         * property on the view model, and also handles closing all preview windows.
         */
        public MapItemViewModel? EditableMapItem
        {
            get { return _editableMapItem; }
            set
            {
                if (value == _editableMapItem)
                    return;

                if (_disableEditMode)
                    return;

                if (_editableMapItem != null)
                {
                    _editableMapItem.PropertyChanged -= _editableMapItem_PropertyChanged;
                    _editableMapItem.EditMode = false;
                }

                _editableMapItem = value;

                if (_editableMapItem != null)
                {
                    _editableMapItem.EditMode = true;
                    _editableMapItem.PropertyChanged += _editableMapItem_PropertyChanged;
                    CloseAllPreviews();

                    // All items should be faded except for the editable one
                    foreach(MapItemViewModel vm in _mapItems)
                    {
                        if (vm == _editableMapItem)
                            vm.FadedColor = false;
                        else
                            vm.FadedColor = true;
                    }
                }
                else
                {
                    // Unfade all items
                    foreach(MapItemViewModel vm in _mapItems)
                        vm.FadedColor = false;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(EditMode));
                OnPropertyChanged(nameof(PathSinglePointSelection));
                OnPropertyChanged(nameof(PathMultiplePointsSelection));
            }
        }


        /**
         * Is an item currently being edited
         */
        public bool EditMode
        {
            get { return EditableMapItem != null; }
        }



        private RelayCommand _editItemCommand;
        public ICommand EditItemCommand => _editItemCommand;

        /**
         * Enable edit mode for the given MapItemViewModel
         */
        public void EditMapItem(object parameter)
        {
            if (parameter is not MapItemViewModel)
                throw new ArgumentException("Argument to Delete Map Item command must be MapObjectViewModel");

            EditableMapItem = (MapItemViewModel)parameter;
        }



        private RelayCommand _finishEditingCommand;
        public ICommand FinishEditingCommand => _finishEditingCommand;

        /**
         * Take the item being currently edited out of edit mode
         */
        private void FinishEditing()
        {
            // If the user is editing a path
            if (EditableMapItem is MapPathViewModel)
            {
                MapPathViewModel vm = (MapPathViewModel)EditableMapItem;
                // If the path has no points, then prompt user to either delete
                // the path or stay in edit mode
                if (vm.Points.Count == 0)
                {
                    YNPopupViewModel popup = new YNPopupViewModel("The path you are editing has no points. It will be deleted.");
                    PopupReturnArgs args = _nav.OpenPopup(popup);

                    if (args.PopupAccepted)
                    {
                        _map.Remove(EditableMapItem.GetModel());
                        EditableMapItem = null;
                    }
                }
                else
                    EditableMapItem = null;
            }
            else
            {
                EditableMapItem = null;
            }
        }



        /**
         * When the MapPath item being edited changes what points are selected,
         * update properties here
         */
        private void _editableMapItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is MapPathViewModel)
            {
                if(e.PropertyName == nameof(MapPathViewModel.SelectionRange))
                {
                    OnPropertyChanged(nameof(PathSinglePointSelection));
                    OnPropertyChanged(nameof(PathMultiplePointsSelection));
                }
            }
        }


        /// <summary>
        /// Returns true if a MapPath item is being edited and a single point
        /// is selected
        /// </summary>
        public bool PathSinglePointSelection
        {
            get
            {
                if (EditableMapItem is not MapPathViewModel)
                    return false;

                return ((MapPathViewModel)EditableMapItem).SinglePointSelected;
            }
        }

        /// <summary>
        /// Returns true if a MapPath item is being edited and multiple points
        /// are selected
        /// </summary>
        public bool PathMultiplePointsSelection
        {
            get
            {
                if (EditableMapItem is not MapPathViewModel)
                    return false;

                MapPathViewModel vm = (MapPathViewModel)EditableMapItem;
                return vm.IsSelection && !vm.SinglePointSelected;
            }
        }


        private RelayCommand _splitTrackAtSelectedCommand;
        public ICommand SplitTrackAtSelectedCommand => _splitTrackAtSelectedCommand;

        /**
         * Split the currently-edited path into two paths that both include the
         * selected point. Renames the paths.
         */
        public void SplitTrackAtSelected()
        {
            if (EditableMapItem is not MapPathViewModel)
                throw new ArgumentException();

            MapPathViewModel vm = (MapPathViewModel)EditableMapItem;

            MapPath newPath = new MapPath(((MapPath)vm.GetModel()).Name + " (2)");

            for(int i = vm.SelectionRange.X; i < vm.Points.Count; i++) 
            {
                newPath.Locations.Add(vm.Points[i]);
            }

            for(int i = vm.SelectionRange.X+1; i < vm.Points.Count; i++)
            {
                vm.Points.RemoveAt(i--);
            }

            _map.Add(newPath);

            vm.Name = vm.Name + " (1)";
            vm.ClearSelection();
        }


        private RelayCommand _newTrackFromSelectedCommand;
        public ICommand NewTrackFromSelectedCommand => _newTrackFromSelectedCommand;

        /**
         * Split the currently-selected path into multiple paths, so that the
         * currently selected set of points is its own track. Points on either
         * end of the path will put in their own path. Renames the paths.
         */
        public void NewTrackFromSelected()
        {
            if (EditableMapItem is not MapPathViewModel)
                throw new ArgumentException();

            MapPathViewModel vm = (MapPathViewModel)EditableMapItem;

            int nameInd = 1;

            if(vm.SelectionRange.X > 0)
            {
                MapPath beforePath = new MapPath(vm.Name + " (" + nameInd++ + ")");
                for(int i = 0; i < vm.SelectionRange.X+1; i++)
                    beforePath.Locations.Add(vm.Points[i]);

                _map.Add(beforePath);
            }

            while(vm.SelectionRange.X != 0)
                vm.Points.RemoveAt(0);

            if(vm.SelectionRange.Y < vm.Points.Count)
            {
                MapPath afterPath = new MapPath(vm.Name + " (" + (nameInd+1) + ")");
                for(int i = vm.SelectionRange.Y-1; i < vm.Points.Count; i++)
                    afterPath.Locations.Add(vm.Points[i]);

                _map.Add(afterPath);
            }

            for(int i = vm.SelectionRange.Y; i < vm.Points.Count; i++)
                vm.Points.RemoveAt(i--);

            vm.Name += " (" + nameInd + ")";

            vm.ClearSelection();
        }


        #endregion Item Editing




        /**
         * When the user clicks on the map, hide all the previews
         */
        public void MapClickEvent()
        {
            foreach (MapItemViewModel vm in _mapItems)
                vm.PreviewOpen = false;
        }



        #region MapItems Collection Changed

        private void MapItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null)
                        throw new ArgumentException("MapViewModel Items - tried to add items, but NewItems was null.");

                    MapItems_Add(e.NewItems);
                    break;


                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null)
                        throw new ArgumentException("MapViewModel Items - tried to remove items, but OldItems was null.");

                    MapItems_Remove(e.OldItems);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null)
                        throw new ArgumentException("MapViewModel Items - tried to add items, but NewItems was null.");
                    if (e.OldItems == null)
                        throw new ArgumentException("MapViewModel Items - tried to remove items, but OldItems was null.");

                    MapItems_Add(e.NewItems);
                    MapItems_Remove(e.OldItems);
                    break;

                default:
                    MapItems_Reset();
                    break;
            }
        }

        private void MapItems_Add(IList newItems)
        {
            foreach (MapItem item in newItems)
                _mapItems.Add(CreateMapItemViewModel(item));
        }

        private void MapItems_Remove(IList oldItems)
        {
            foreach(MapItem item in oldItems)
            {
                for(int i = 0; i < _mapItems.Count; i++)
                {
                    if (_mapItems[i].GetModel() == item)
                    {
                        _mapItems.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void MapItems_Reset()
        {
            _mapItems.Clear();
            foreach (MapItem item in _map)
                _mapItems.Add(CreateMapItemViewModel(item));
        }

        #endregion MapItems Collection Changed


    }
}
