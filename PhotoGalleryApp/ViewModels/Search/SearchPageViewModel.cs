using Microsoft.CSharp.RuntimeBinder;
using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels.Search
{
    /// <summary>
    /// ViewModel for the Search Page. This is where users select all kinds of properties to filter by in a search.
    /// </summary>
    class SearchPageViewModel : ViewModelBase
    {
        public SearchPageViewModel(NavigatorViewModel nav, MediaCollection coll, FilterSet initialFilters = null)
        {
            _searchCommand = new RelayCommand(InitiateSearch);

            _navigator = nav;
            _mediaCollection = coll;

            if (initialFilters == null)
                _filters = new FilterSet(coll);
            else
            {
                if (!ReferenceEquals(initialFilters.MediaCollection, coll))
                    throw new ArgumentException("FilterSet passed to SearchPageViewModel has different MediaCollection than the MediaCollection passed to SearchPageViewModel");

                _filters = initialFilters;
            }
        }

        public override void Cleanup() 
        { 
            foreach(ViewModelBase vm in _criteriaViewModels)
                vm.Cleanup();
        }


        private NavigatorViewModel _navigator;
        private MediaCollection _mediaCollection;

        /**
         * Contains filters for all filterable properties
         */
        private FilterSet _filters;

        private List<ViewModelBase> _criteriaViewModels = new List<ViewModelBase>();




        private RelayCommand _searchCommand;
        public ICommand SearchCommand => _searchCommand;

        /**
         * Take the current filter parameters and show the filtered Media
         */
        public void InitiateSearch()
        {
            _navigator.NewPage(new SearchResultsViewModel(_navigator, _mediaCollection, _filters));
        }



        #region Filter Types
        /**
         * Exposes a bunch of FilterCriteria View Models for each in FilterSet. Because of FilterSet's lazy loading, must have the getter
         * function at the bottom to lazily create the VMs.
         */

        public TagFilterViewModel TagFilter
        {
            get
            {
                ViewModelBase vm = GetFilterCriteriaViewModel(typeof(TagFilterViewModel), typeof(TagFilter));
                if (vm is not TagFilterViewModel)
                    throw new Exception("TagFilterViewModel is not of the right type");
                return (TagFilterViewModel)vm;
            }
        }

        public TimeRangeFilterViewModel TimeRangeFilter
        {
            get
            {
                ViewModelBase vm = GetFilterCriteriaViewModel(typeof(TimeRangeFilterViewModel), typeof(TimeRangeFilter));
                if(vm is not TimeRangeFilterViewModel)
                    throw new Exception("TimeRangeFilterViewModel is not of the right type");
                return (TimeRangeFilterViewModel)vm;
            }
        }


        /**
         * Return the ViewModel for a specific FilterCriteria type. Need to pass both the ViewModel type and the FilterCriteria type.
         * This is lazy constructing, so if the ViewModel does not originally exist, it will create it here.
         */
        public ViewModelBase GetFilterCriteriaViewModel(Type filterVMType, Type filterCriteriaType) 
        {
            // The types need to be of the right superclass
            if (!filterVMType.IsSubclassOf(typeof(ViewModelBase)))
                throw new ArgumentException("GetFilterCriteriaViewModel was given a Type not of FilterCriteriaViewModel");

            if(!filterCriteriaType.IsSubclassOf(typeof(FilterCriteria)))
                throw new ArgumentException("GetFilterCriteriaViewModel was given a Type not of FilterCriteria");


            // Return it if it exists already
            foreach(ViewModelBase cvm in _criteriaViewModels)
            {
                if (cvm.GetType() == filterVMType)
                    return cvm;
            }

            // If not, create a new one. Seems hacky!
            FilterCriteria crit = _filters.GetCriteriaByType(filterCriteriaType);

            object? obj = Activator.CreateInstance(filterVMType, crit);
            if (obj == null)
                throw new Exception("Failed to create FilterCriteriaViewModel instance");
            return (ViewModelBase)obj;
        }

        #endregion Filter Types
    }
}
