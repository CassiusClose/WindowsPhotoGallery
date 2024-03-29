﻿using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using PhotoGalleryApp.Views.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels.Search
{
    class MapItemFilterViewModel : ViewModelBase
    {
        /// <summary>
        /// ViewModel for the MapItemFilter class. The view will be used to display & set the filter.
        /// </summary>
        /// <param name="filter"></param>
        public MapItemFilterViewModel(MapItemFilter filter)
        {
            _clearMapItemCommand = new RelayCommand(ClearMapItem);
            _chooseMapItemCommand = new RelayCommand(ChooseMapItemForFilter);

            _filter = filter;
            _filter.PropertyChanged += _filter_PropertyChanged;

            _mapItemNameCollection = new MaintainedParentCollection<MapItem, string>(MainWindow.GetCurrentSession().Map,
                                                                                     _mapItemNames_IsItemCollection,
                                                                                     _mapItemNames_GetItemCollection,
                                                                                     _mapItemNames_GetItem,
                                                                                     _mapItemNames_GetPropertyName);


            OnPropertyChanged(nameof(MapItemNames));
        }

        public override void Cleanup() 
        {
            _filter.PropertyChanged -= _filter_PropertyChanged;
        }



        private MapItemFilter _filter;

        // When the filter changes, update the PropertyChanged listeners here
        private void _filter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_filter.MapItemCriteria))
                OnPropertyChanged(nameof(SelectedItemName));
        }



        /// <summary>
        /// A list of the names of all of the MapItems (for display in the drop-down)
        /// </summary>
        public ObservableCollection<string> MapItemNames
        {
            get { return _mapItemNameCollection.Items; }
        }


        // Maintain the list of MapItem names
        private MaintainedParentCollection<MapItem, string> _mapItemNameCollection;

        private bool _mapItemNames_IsItemCollection(MapItem i) { return false; }
        private ObservableCollection<string> _mapItemNames_GetItemCollection(MapItem i) { throw new ArgumentException(); }
        private string _mapItemNames_GetItem(MapItem i) { return i.Name; }
        private string _mapItemNames_GetPropertyName(MapItem i) { return nameof(i.Name); }





        /// <summary>
        /// The name of the current filter MapItem
        /// </summary>
        public string? SelectedItemName
        {
            get
            {
                if (_filter.MapItemCriteria == null)
                    return null;

                return _filter.MapItemCriteria.Name;
            }
        }



        private RelayCommand _clearMapItemCommand;
        public ICommand ClearMapItemCommand => _clearMapItemCommand;

        /**
         * Removes any MapItem from the filter
         */
        public void ClearMapItem()
        {
            _filter.ClearFilter();
        }



        private RelayCommand _chooseMapItemCommand;
        public ICommand ChooseMapItemCommand => _chooseMapItemCommand;

        public void ChooseMapItemForFilter()
        {
            NavigatorViewModel nav = MainWindow.GetNavigator();
            PickMapItemPopupViewModel popup = new PickMapItemPopupViewModel(nav);
            PickMapItemPopupReturnArgs args = (PickMapItemPopupReturnArgs)nav.OpenPopup(popup);

            if(args.PopupAccepted)
                _filter.MapItemCriteria = args.ChosenMapItem;
        }
    }
}
