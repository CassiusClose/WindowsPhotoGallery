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
        public MapViewModel(NavigatorViewModel nav, PhotoGalleryApp.Models.Map map)
        {
            _nav = nav;
            _map = map;
            _map.Items.CollectionChanged += MapItems_CollectionChanged;

            _addLocationCommand = new RelayCommand(AddLocation);
            _addPathCommand = new RelayCommand(AddPath);
            _finishEditingCommand = new RelayCommand(FinishEditing);
            _deleteItemCommand = new RelayCommand(DeleteMapItem);
            _editItemCommand = new RelayCommand(EditMapItem);


            _mapItems = new ObservableCollection<MapItemViewModel>();
            MapItems = CollectionViewSource.GetDefaultView(_mapItems);
            MapItems_Reset();
        }

        public override void Cleanup() 
        {
            _map.Items.CollectionChanged -= MapItems_CollectionChanged;
        }



        private NavigatorViewModel _nav;

        private PhotoGalleryApp.Models.Map _map;


        private ObservableCollection<MapItemViewModel> _mapItems;
        public ICollectionView MapItems { get; }




        #region Methods

        private MapItemViewModel? CreateMapItemViewModel(MapItem item)
        {
            if(item is MapPath)
                return new MapPathViewModel((MapPath)item);

            if (item is MapLocation)
                return new MapLocationViewModel((MapLocation)item);

            return null;
        }


        #endregion Methods



        #region Add/Delete Commands


        private RelayCommand _addLocationCommand;
        public ICommand AddLocationCommand => _addLocationCommand;

        private void AddLocation(object parameter)
        {
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

            _mapItems.Remove((MapItemViewModel)parameter);
        }


        private RelayCommand _addPathCommand;
        public ICommand AddPathCommand => _addPathCommand;

        private void AddPath(object parameter)
        {
            /*MapPath path = new MapPath("Path 1");
            _map.Items.Add(path);
            foreach (MapItemViewModel vm in _mapItems)
            {
                if (vm.GetModel() == path)
                    EditableMapItem = vm;
            }*/
        }


        #endregion Add/Delete Commands



        #region Preview Boxes


        /**
         * Close all preview boxes
         */
        private void CloseAllPreviews(MapItemViewModel? exception=null)
        {
            foreach (MapItemViewModel vm in _mapItems)
            {
                if(exception == null || !ReferenceEquals(exception, vm))
                    vm.PreviewOpen = false;
            }
        }


        /**
         * Open or close the preview box for the given MapItemViewModel. If
         * EditMode is enabled, then the preview will not open.
         */
        public void TogglePreview(object parameter)
        {
            if (parameter is not MapItemViewModel)
                throw new ArgumentException("TogglePreview() needs a MapItemViewModel");

            MapItemViewModel vm = (MapItemViewModel)parameter;

            if(EditMode)
            {
                vm.PreviewOpen = false;
            }
            else
            {
                foreach(MapItemViewModel o in _mapItems)
                {
                    if (ReferenceEquals(vm, o))
                        o.PreviewOpen = !o.PreviewOpen;
                    else if(o.PreviewOpen)
                        o.PreviewOpen = false;
                }
            }
        }

        #endregion Preview Boxes



        #region Item Editing


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

                if (_editableMapItem != null)
                    _editableMapItem.EditMode = false;

                _editableMapItem = value;

                if (_editableMapItem != null)
                {
                    _editableMapItem.EditMode = true;
                    CloseAllPreviews();
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(EditMode));
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
            EditableMapItem = null;
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
            foreach (MapItem item in _map.Items)
                _mapItems.Add(CreateMapItemViewModel(item));
        }

        #endregion MapItems Collection Changed


    }
}
