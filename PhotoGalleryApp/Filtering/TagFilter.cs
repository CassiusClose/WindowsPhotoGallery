using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Filtering
{
    /// <summary>
    /// A type of FilterCriteria that filters based on media tags. This stores a list of tags and will only let an item through if it has all of those tags.
    /// </summary>
    public class TagFilter : FilterCriteria
    {
        public TagFilter(MediaCollection coll) : base(coll)
        {
            coll.ItemTagsChanged += CollectionItem_TagsChanged;

            _filterTags.CollectionChanged += FilterTagsChanged;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _mediaCollection.ItemTagsChanged -= CollectionItem_TagsChanged;
            _filterTags.CollectionChanged -= FilterTagsChanged;
        }


        /**
         * A list of tags to filter with. Currently, all tags must exist in an item for it to pass the filter
         */
        protected ObservableCollection<string> _filterTags = new ObservableCollection<string>();
        public ObservableCollection<string> FilterTags { get { return _filterTags; } }


        /**
         * If ICollectable has all the tags in the filter, pass it.
         */
        public override bool Filter(ICollectable c)
        {
            if(c is Media)
            {
                Media m = (Media)c;
                foreach(string tag in _filterTags)
                {
                    if (!m.Tags.Contains(tag))
                        return false;
                }
                return true;

            }     
            else if(c is Event)
            {
                Event e = (Event)c;
                foreach (string tag in _filterTags)
                {
                    if (!e.Collection.Tags.Contains(tag))
                        return false;
                }

                return true;
            }

            return false;
        }

        public override bool IsFilterActive()
        {
            return (_filterTags.Count > 0);
        }

        public override void ClearFilter()
        {
            _filterTags.Clear();
            if(FilterCriteriaLoosened != null)
                FilterCriteriaLoosened();
        }


        /**
         * If an item in the MediaCollection changes its tags, send notification to refilter it
         */
        public void CollectionItem_TagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if(sender is ICollectable)
            {
                if (FilteredPropertyChanged != null)
                    FilteredPropertyChanged((ICollectable)sender);
            }
        }

        public override void CollectionItem_PropertyChanged(object? sender, PropertyChangedEventArgs e) { } 

        /**
         * When the filter tags change, update listeners to refilter items
         */
        private void FilterTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (FilterCriteriaTightened != null)
                        FilterCriteriaTightened();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (FilterCriteriaLoosened != null)
                        FilterCriteriaLoosened();
                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                    if (FilterCriteriaLoosened != null)
                        FilterCriteriaLoosened();
                    if (FilterCriteriaTightened != null)
                        FilterCriteriaTightened();
                    break;
            }
        }

        public override void CopyTo(FilterCriteria c)
        {
            if(c.GetType() != typeof(TagFilter))
                throw new ArgumentException("FilterCriteria CopyTo() was given a filter criteria of a different type");

            TagFilter f = (TagFilter)c;

            f.FilterTags.Clear();
            foreach(string tag in _filterTags)
                f.FilterTags.Add(tag);
        }

    }
}
