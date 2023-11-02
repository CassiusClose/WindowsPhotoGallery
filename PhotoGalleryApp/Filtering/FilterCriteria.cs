using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace PhotoGalleryApp.Filtering
{
    /// <summary>
    /// A general class to do filtering of a single property. 
    /// 
    /// Subclasses should implement storage of specific filter parameters, as well as
    /// detecting when those properties being filtered change in the MediaCollection items. Do this using CollectionItem_PropertyChanged or 
    /// other event handlers. When those properties change, call the FilteredPropertyChanged event. 

    /// When the filter parameters change, call either the FilterCriteriaTightened or FilterCriteriaLoosened events. 
    /// 
    /// Make sure to implement Cleanup() if you need to remove any external change handlers you created in the subclass.
    /// </summary>
    public abstract class FilterCriteria
    {
        public FilterCriteria(MediaCollection mediaCollection)
        {
            _mediaCollection = mediaCollection;
            _mediaCollection.ItemPropertyChanged += CollectionItem_PropertyChanged;
        }

        /**
         * Removes change listeners from MediaCollection. Subclasses should call this base class version.
         */
        public virtual void Cleanup()
        {
            _mediaCollection.ItemPropertyChanged -= CollectionItem_PropertyChanged;
        }



        protected MediaCollection _mediaCollection;
        public MediaCollection MediaCollection { get { return _mediaCollection; } }

        /**
         * Should be called when the filter parameters change to potentially let more items pass
         */
        public FilterSet.FilterCriteriaTightenedEvent? FilterCriteriaTightened = null;

        /**
         * Should be called when the filter parameters change to potentially let fewer items pass
         */
        public FilterSet.FilterCriteriaLoosenedEvent? FilterCriteriaLoosened = null;

        /**
         * Should be called when a MediaCollection item property filtered by this class changes
         */
        public FilterSet.FilteredPropertyChangedEvent? FilteredPropertyChanged = null;


        /**
         * Returns whether the given ICollectable passes the filter requirements
         */
        public abstract bool Filter(ICollectable c);


        /**
         * Remove all parameters from the filter type. Filter() should return true for every input after this.
         */
        public abstract void ClearFilter();


        /// <summary>
        /// Returns whether there are currently any filter restrictions
        /// </summary>
        /// <returns></returns>
        public abstract bool IsFilterActive();


        /**
         * Called when a property changes in an item in the MediaCollection. Use to notify listeners that the item should be refiltered
         */
        public abstract void CollectionItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e);




        /**
         * Copies the filter parameters from this FilterCriteria to another instance.
         */
        public abstract void CopyTo(FilterCriteria crit);

    }
}
