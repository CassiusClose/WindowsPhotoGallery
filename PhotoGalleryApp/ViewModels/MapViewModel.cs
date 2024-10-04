using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.Views;
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

            _addLocationCommand = new RelayCommand(AddLocation);
            _addPathCommand = new RelayCommand(AddPath);
            _finishEditingCommand = new RelayCommand(FinishEditing);
            _deleteItemCommand = new RelayCommand(DeleteMapItem);
            _editItemCommand = new RelayCommand(EditMapItem);
            _newTrackFromSelectedCommand = new RelayCommand(NewTrackFromSelected);
            _splitTrackAtSelectedCommand = new RelayCommand(SplitTrackAtSelected);

            _mapItems = new MapItemView(_map, this);
            MapItems.CollectionChanged += MapItems_CollectionChanged;
        }

        public override void Cleanup()
        {
            MapItems.CollectionChanged -= MapItems_CollectionChanged;
            _mapItems.Cleanup();
        }




        #region Fields and Properties

        private NavigatorViewModel _nav;

        private PhotoGalleryApp.Models.Map _map;





        // Maintains a list of ViewModels
        private MapItemView _mapItems;

        // Expose the View list
        public ObservableCollection<MapItemViewModel> MapItems { 
            get { return _mapItems.View; }
        }

        private void MapItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // When an item is added & an item is being edited, make all new items faded too
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    if (e.NewItems == null)
                        throw new ArgumentException();

                    if(EditableMapItem != null)
                    {
                        foreach(MapItemViewModel vm in e.NewItems)
                        {
                            if (!ReferenceEquals(vm.GetModel(), EditableMapItem))
                                vm.FadedColor = true;
                        }
                    }

                    break;

                default:
                    if(EditableMapItem != null)
                    {
                        foreach(MapItemViewModel vm in MapItems)
                        {
                            if (ReferenceEquals(vm.GetModel(), EditableMapItem))
                                vm.FadedColor = false;
                            else
                                vm.FadedColor = true;
                        }
                    }
                    break;
            }
        }



        private bool _displayToolbar = true;
        /// <summary>
        /// Whether the toolbar above the map should be displayed
        /// </summary>
        public bool DisplayToolbar
        {
            get { return _displayToolbar; }
        }


        private double _zoomLevel;
        /// <summary>
        /// The zoom level of the map.
        /// </summary>
        public double ZoomLevel 
        {
            get { return _zoomLevel; }
            set
            {
                _zoomLevel = value;
                OnPropertyChanged();
            }
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

            return MapItemViewModel.CreateMapItemViewModel(item, this);
        }


        /// <summary>
        /// Creates and returns a MapItemViewModel around the given MapItem.
        /// </summary>
        public delegate MapItemViewModel CreateMapItemViewModelDelegate(MapItem item);
        private CreateMapItemViewModelDelegate MapItemViewModelGenerator = null;


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

        private MapItem _getMapItemModel(MapItemViewModel vm) { return vm.GetModel(); }

        #endregion Create MapItem View Model




        #region Add/Delete Commands


        private RelayCommand _addLocationCommand;
        public ICommand AddLocationCommand => _addLocationCommand;

        private void AddLocation(object parameter)
        {
            if (parameter is not Location)
                throw new ArgumentException("AddLocation argument must be the location to add at");

            // Show the user the popup to create a location
            CreateLocationPopupViewModel popup = new CreateLocationPopupViewModel((Location)parameter, _map);
            CreateLocationPopupReturnArgs args = (CreateLocationPopupReturnArgs)_nav.OpenPopup(popup);

            // If not cancelled, then create the location
            if (args.PopupAccepted)
            {
                MapLocation loc = new MapLocation(args.Name, args.Location);
                if (args.Parent == null) 
                    _map.Add(loc);
                else
                    args.Parent.Children.Add(loc);

                EditableMapItem = loc;
            }

            popup.Cleanup();
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
            TextEntryPopupViewModel popup = new TextEntryPopupViewModel();
            TextEntryPopupReturnArgs args = (TextEntryPopupReturnArgs)_nav.OpenPopup(popup);

            // If not cancelled, then create the path
            if (args.PopupAccepted)
            {
                MapPath path = new MapPath(args.Text);
                _map.Add(path);
                EditableMapItem = path;
            }

            popup.Cleanup();
        }



        /**
         * Attempts to load path data from the file at the given path. If the
         * parsing is successful, this will open a popup where the user can
         * edit details of each track.
         */
        public void LoadPathsFromFile(string filename)
        {
            List<MapPath> paths = PathFileFormats.LoadFromTxtFile(filename);

            // Show a ProgessBar while calculating overlaps of each path
            List<object> workArgs = new List<object>();
            workArgs.Add(paths);
            workArgs.Add(_map);
            ProgressBarPopupViewModel progBar = new ProgressBarPopupViewModel((s, e) =>
            {
                if (s is null || e.Argument is null)
                    throw new ArgumentException();

                BackgroundWorker worker = (BackgroundWorker)s;
                List<object> args = (List<object>)e.Argument;
                List<MapPath> paths = (List<MapPath>)args[0];
                PhotoGalleryApp.Models.Map map = (PhotoGalleryApp.Models.Map)args[1];

                LoadPathsFileResultsPopupViewModel popup = new LoadPathsFileResultsPopupViewModel(paths, map, worker);

                e.Result = popup;
            }, workArgs);
            ProgressBarPopupReturnArgs progArgs = (ProgressBarPopupReturnArgs)_nav.OpenPopup(progBar);

            if (progArgs.Result == null)
                throw new Exception();

            // Show the loaded paths & their options
            LoadPathsFileResultsPopupViewModel popup = (LoadPathsFileResultsPopupViewModel)progArgs.Result;
            LoadPathsFileResultsPopupReturnArgs args = (LoadPathsFileResultsPopupReturnArgs)_nav.OpenPopup(popup);
            if(args.PopupAccepted)
            {
                foreach (PathFileResultsPathViewModel vm in args.Paths)
                {
                    if(vm.AddToMap)
                    {
                        // If the path overlaps with another path, different ways to handle it
                        switch (vm.ReplaceSelection)
                        {
                            case PathFileResultsPathViewModel.ReplaceChoices.AddNormally:
                                _map.Add(vm.Path);
                                break;

                            case PathFileResultsPathViewModel.ReplaceChoices.ReplaceOverlap:
                                if (vm.OverlapPath != null)
                                    _map.Remove(vm.OverlapPath);
                                else
                                    throw new ArgumentException();
                                _map.Add(vm.Path);
                                break;

                            case PathFileResultsPathViewModel.ReplaceChoices.MergeOverlap:
                                MapPath p;
                                if (vm.OverlapPath != null)
                                {
                                    p = MapPath.MergeOverlappingPaths(vm.Path, vm.OverlapPath, vm.Name);

                                    /**
                                     * Transfer Media belonging to orignal path to the new merged path.
                                     */
                                    if(MainWindow.GetCurrentSession().Map == _map)
                                    {
                                        foreach(ICollectable coll in MainWindow.GetCurrentSession().Gallery.Collection)
                                        {
                                            if (coll is not Media)
                                                continue;

                                            Media m = (Media)coll;
                                            if (m.MapItem == vm.OverlapPath)
                                                m.MapItem = p;
                                        }

                                    }

                                    _map.Remove(vm.OverlapPath);
                                }
                                else
                                    throw new ArgumentException();
                                _map.Add(p);
                                break;
                        }
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
            foreach (MapItemViewModel vm in MapItems)
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
                foreach (MapItemViewModel o in MapItems)
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


        private MapItem? _editableMapItem = null;
        /**
         * The MapItemViewModel that is currently being edited, or null if none
         * are being edited. When this is set, it takes care of setting the EditMode
         * property on the view model, and also handles closing all preview windows.
         */
        public MapItem? EditableMapItem
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
                    MapItemViewModel? vm = GetViewModel(_editableMapItem);
                    if (vm == null)
                        throw new Exception("Old EditableMapItem not found in VM list");

                    vm.PropertyChanged -= _editableMapItem_PropertyChanged;
                    vm.EditMode = false;
                }

                _editableMapItem = value;

                if (_editableMapItem != null)
                {
                    // Mark any currently displayed MapItems that are in the
                    // same tree as the edited item as so. This prevents
                    // zooming from collapsing & thus hiding the currently
                    // edited location
                    if(_editableMapItem is MapLocation)
                    {
                        _mapItems.ClearFrozen();
                        _mapItems.FreezeMapLocationLevel((MapLocation)_editableMapItem);
                    }

                    MapItemViewModel? vm = GetViewModel(_editableMapItem);
                    if (vm == null)
                        throw new Exception("New EditableMapItem not found in VM list");

                    vm.EditMode = true;
                    vm.PropertyChanged += _editableMapItem_PropertyChanged;
                    CloseAllPreviews();

                    // All items should be faded except for the editable one
                    foreach(MapItemViewModel i in MapItems)
                    {
                        if (ReferenceEquals(i.GetModel(), _editableMapItem))
                            i.FadedColor = false;
                        else
                            i.FadedColor = true;
                    }
                }
                else
                {
                    _mapItems.ClearFrozen();
                    // Unfade all items
                    foreach(MapItemViewModel vm in MapItems)
                        vm.FadedColor = false;

                    // If editing has finished, then refresh the list for
                    // accurate MapLocation expansions
                    _mapItems.Refresh();
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

            EditableMapItem = ((MapItemViewModel)parameter).GetModel();
        }



        private RelayCommand _finishEditingCommand;
        public ICommand FinishEditingCommand => _finishEditingCommand;

        /**
         * Take the item being currently edited out of edit mode
         */
        private void FinishEditing()
        {
            // If the user is editing a path
            if (EditableMapItem is MapPath)
            {
                MapPathViewModel? vm = (MapPathViewModel?)GetViewModel(EditableMapItem);
                if (vm == null)
                    throw new Exception("Currently edited VM not found");

                // If the path has no points, then prompt user to either delete
                // the path or stay in edit mode
                /*if (vm.Points.Count == 0)
                {
                    YNPopupViewModel popup = new YNPopupViewModel("The path you are editing has no points. It will be deleted.");
                    PopupReturnArgs args = _nav.OpenPopup(popup);

                    if (args.PopupAccepted)
                    {
                        _map.Remove(EditableMapItem);
                        EditableMapItem = null;
                    }
                }
                else*/
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
                if (EditableMapItem is not MapPath)
                    return false;

                MapPathViewModel? vm = (MapPathViewModel?)GetViewModel(EditableMapItem);
                if(vm == null)
                    throw new Exception("Currently edited VM not found");
                return vm.SinglePointSelected;
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
                if (EditableMapItem is not MapPath)
                    return false;

                MapPathViewModel? vm = (MapPathViewModel?)GetViewModel(EditableMapItem);
                if(vm == null)
                    throw new Exception("Currently edited VM not found");
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
            if (EditableMapItem is not MapPath)
                throw new ArgumentException();

            MapPathViewModel? vm = (MapPathViewModel?)GetViewModel(EditableMapItem);
            if(vm == null)
                throw new Exception("Currently edited VM not found");

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
            if (EditableMapItem is not MapPath)
                throw new ArgumentException();

            MapPathViewModel? vm = (MapPathViewModel?)GetViewModel(EditableMapItem);
            if(vm == null)
                throw new Exception("Currently edited VM not found");

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
            foreach (MapItemViewModel vm in MapItems)
                vm.PreviewOpen = false;
        }


        private MapItemViewModel? GetViewModel(MapItem model)
        {
            foreach (MapItemViewModel vm in MapItems)
            {
                if (ReferenceEquals(vm.GetModel(), model))
                    return vm;
            }
            return null;
        }
    }
}
