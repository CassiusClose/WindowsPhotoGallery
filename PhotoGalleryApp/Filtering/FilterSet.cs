using PhotoGalleryApp.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Filtering
{
    /// <summary>
    /// Holds filtering information for a MediaView. Filtering might be by many properties (tag, date, location, etc.) This contains an instance
    /// of one subclass of FilterCriteria for each type of filtering, which can be gotten by calling GetCriteriaByType(). MediaView will call
    /// Filter() to filter items.
    /// </summary>
    public class FilterSet
    {
        public FilterSet(MediaCollection mediaCollection)
        {
            _mediaCollection = mediaCollection;
        }

        public void Cleanup()
        {
            foreach (FilterCriteria c in _criteria)
                c.Cleanup();
        }



        private MediaCollection _mediaCollection;
        public MediaCollection MediaCollection { get { return _mediaCollection; } }

        /**
         * Should contain at most one item for each kind of filtering (each subclass of FilterCriteria). Added as needed.
         */
        private List<FilterCriteria> _criteria = new List<FilterCriteria>();



        #region Methods

        /**
         * Returns whether the given ICollectable meets the filter criteria
         */
        public bool Filter(ICollectable c)
        {
            foreach (FilterCriteria crit in _criteria)
            {
                if (!crit.Filter(c))
                    return false;
            }

            return true;
        }


        /**
         * Returns whether there are any filters currently set
         */
        public bool AreFiltersActive()
        {
            foreach (FilterCriteria crit in _criteria)
            {
                if (crit.IsFilterActive())
                    return true;
            }

            return false;
        }

        /**
         * Returns the FilterCriteria instance for the given type. Lazy loading, so creates it if it doesn't exist.
         */
        public FilterCriteria GetCriteriaByType(Type criteriaType)
        {
            if (criteriaType.IsSubclassOf(typeof(FilterCriteria)))
            {
                // If the criteria exists, return it
                foreach (FilterCriteria crit in _criteria)
                {
                    if (crit.GetType() == criteriaType)
                        return crit;
                }

                // Otherwise, create new instance
                object? obj = Activator.CreateInstance(criteriaType, _mediaCollection);
                if (obj == null)
                    throw new Exception("Failed to create instance in GetCriteriaByType");
                else
                {
                    FilterCriteria crit = (FilterCriteria)obj;
                    crit.FilterCriteriaLoosened += Criteria_FilterLoosened;
                    crit.FilterCriteriaTightened += Criteria_FilterTightened;
                    crit.FilterCriteriaChanged += Criteria_FilterChanged;
                    crit.FilteredPropertyChanged += Criteria_FilteredPropertyChanged;
                    _criteria.Add(crit);
                    return crit;
                }
            }
            else
            {
                throw new ArgumentException("Type passed to GetCriteriaByType is not a subclass of FilterCriteria");
            }
        }

        /**
         * Returns a new FilterSet objects with the same criteria as this one
         */
        public FilterSet Clone()
        {
            FilterSet newSet = new FilterSet(_mediaCollection);

            foreach (FilterCriteria c in _criteria)
            {
                FilterCriteria crit = newSet.GetCriteriaByType(c.GetType());
                c.CopyTo(crit);
            }

            return newSet;
        }

        #endregion Methods



        #region Events

        /**
         * When an item in the MediaCollection changes a property read by one of the FilterCriteria, that 
         * FilterCriteria will notify its FilterSet, and the FilterSet will notify any external listeners
         * through this event.
         */
        public delegate void FilteredPropertyChangedEvent(ICollectable c);
        public FilteredPropertyChangedEvent? FilteredPropertyChanged = null;

        /**
         * When one of the FilterCriteria changes its criteria to restrict more than previously, that
         * FilterCriteria will notify its FilterSet, and the FilterSet will notify any external listeners
         * through this event.
         */
        public delegate void FilterCriteriaTightenedEvent();
        public FilterCriteriaTightenedEvent? FilterCriteriaTightened = null;

        /**
         * When one of the FilterCriteria changes its criteria to restrict less than previously, that
         * FilterCriteria will notify its FilterSet, and the FilterSet will notify any external listeners
         * through this event.
         */
        public delegate void FilterCriteriaLoosenedEvent();
        public FilterCriteriaLoosenedEvent? FilterCriteriaLoosened = null;

        /**
         * If one of the FilterCriteria changes its criteria in such a way that
         * tightened or loosened cannot be determined, that FilterCriteria will
         * notify its FilterSet, and the FilterSet will notify any external
         * listeners through this event.
         */
        public delegate void FilterCriteriaChangedEvent();
        public FilterCriteriaChangedEvent? FilterCriteriaChanged = null;


        public void Criteria_FilterLoosened()
        {
            if (FilterCriteriaLoosened != null)
                FilterCriteriaLoosened();
        }

        public void Criteria_FilterTightened()
        {
            if (FilterCriteriaTightened != null)
                FilterCriteriaTightened();
        }

        public void Criteria_FilterChanged()
        {
            if (FilterCriteriaChanged != null)
                FilterCriteriaChanged();
        }

        public void Criteria_FilteredPropertyChanged(ICollectable c)
        {
            if (FilteredPropertyChanged != null)
                FilteredPropertyChanged(c);
        }

        #endregion Events
    }
}
