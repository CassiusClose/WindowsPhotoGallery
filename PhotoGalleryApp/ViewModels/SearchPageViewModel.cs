using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the Search Page. This is where users select all kinds of properties to filter by in a search.
    /// </summary>
    class SearchPageViewModel : ViewModelBase
    {
        public SearchPageViewModel(NavigatorViewModel nav, MediaCollection coll, FilterSet initialFilters = null)
        {
            _searchCommand = new RelayCommand(InitiateSearch);
            _removeTagFromFilterCommand = new RelayCommand(RemoveTagFromFilter);

            _navigator = nav;
            _mediaCollection = coll;

            if(initialFilters == null)
                _filters = new FilterSet(coll);
            else
            {
                if (!ReferenceEquals(initialFilters.MediaCollection, coll))
                    throw new ArgumentException("FilterSet passed to SearchPageViewModel has different MediaCollection than the MediaCollection passed to SearchPageViewModel");

                _filters = initialFilters;
            }
        }

        public override void Cleanup() { }


        private NavigatorViewModel _navigator;
        private MediaCollection _mediaCollection;

        /**
         * Contains filters for all filterable properties
         */
        private FilterSet _filters;



        #region Filter Parameters
        /**
         * Exposes a bunch of filter parameters from FilterSet to be displayed & changed by the user
         */

        //TODO When the filter parameters change, should update the property here.
        // Really, make a ViewModel class for each filter type, and then bind it to a display element
        // for each filter type. That puts most of the property exposing away in other classes.

        public ObservableCollection<string> SelectedTags
        {
            get 
            {
                TagFilter filt = (TagFilter)_filters.GetCriteriaByType(typeof(TagFilter));
                return filt.FilterTags;
            }
        }

        private RelayCommand _removeTagFromFilterCommand;
        public ICommand RemoveTagFromFilterCommand => _removeTagFromFilterCommand;
        
        public void RemoveTagFromFilter(object obj)
        {
            if(obj is not string)
            {
                throw new ArgumentException("Tried to pass non-string to RemoveTagFromFilter");
            }

            string tag = (string)obj;
            TagFilter filt = (TagFilter)_filters.GetCriteriaByType(typeof(TagFilter));
            filt.FilterTags.Remove(tag);
        }

        public void AddTagToFilter(object sender, Views.ItemChosenEventArgs e)
        {
            string tag = e.Item;
            TagFilter filt = (TagFilter)_filters.GetCriteriaByType(typeof(TagFilter));
            if(!filt.FilterTags.Contains(tag))
                filt.FilterTags.Add(tag);
        }

        public ObservableCollection<string> AllTags
        {
            get { return _mediaCollection.Tags; }
        }

        #endregion Filter Parameters



        private RelayCommand _searchCommand;
        public ICommand SearchCommand => _searchCommand;

        /**
         * Take the current filter parameters and show the filtered Media
         */
        public void InitiateSearch()
        {
            _navigator.NewPage(new SearchResultsViewModel(_navigator, _mediaCollection, _filters));
        }


    }
}
