using PhotoGalleryApp.Filtering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels.Search
{
    /// <summary>
    /// A ViewModel for viewing & selecting the tags to filter with in a search.
    /// </summary>
    class TagFilterViewModel : ViewModelBase
    {
        public TagFilterViewModel(TagFilter filter)
        {
            _filter = filter;

            _removeTagFromFilterCommand = new RelayCommand(RemoveTagFromFilter);
        }

        public override void Cleanup() { }


        private TagFilter _filter;


        /**
         * The tags selected for the filter
         */
        public ObservableCollection<string> SelectedTags { get { return _filter.FilterTags; } }


        /**
         * All the tags possible to select
         */
        public ObservableCollection<string> AllTags
        {
            get { return _filter.MediaCollection.Tags; }
        }




        private RelayCommand _removeTagFromFilterCommand;
        public ICommand RemoveTagFromFilterCommand => _removeTagFromFilterCommand;

        /**
         * Remove a tag from the filter list
         */
        public void RemoveTagFromFilter(object obj)
        {
            if (obj is not string)
            {
                throw new ArgumentException("Tried to pass non-string to RemoveTagFromFilter");
            }

            string tag = (string)obj;
            _filter.FilterTags.Remove(tag);
        }


        /**
         * Add a tag to the filter list
         */
        public void AddTagToFilter(object sender, Views.ItemChosenEventArgs e)
        {
            string tag = e.Item;
            if (!_filter.FilterTags.Contains(tag))
                _filter.FilterTags.Add(tag);
        }
    }
}
