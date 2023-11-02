using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels.Search
{
    /// <summary>
    /// ViewModel for the result of a search. Right now, just shows the media, but later on, this will show details about the search parameters.
    /// </summary>
    class SearchResultsViewModel : ViewModelBase
    {
        public SearchResultsViewModel(NavigatorViewModel navigator, MediaCollection coll, FilterSet searchFilter)
        {
            _nav = navigator;

            _mediaCollection = new MediaCollectionViewModel(navigator, coll, false, Utils.TimeRange.Year, searchFilter, true);
        }

        public override void Cleanup()
        {
            _mediaCollection.Cleanup();
        }


        private NavigatorViewModel _nav;

        private MediaCollectionViewModel _mediaCollection;
        public MediaCollectionViewModel MediaCollection { get { return _mediaCollection; } }
    }
}
