using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace PhotoGalleryApp.Filtering
{
    /// <summary>
    /// A type of FilterCriteria that filters based on timestamp. This stores a start & stop timestamp, and only allows media within that range.
    /// </summary>
    public class TimeRangeFilter : FilterCriteria
    {
        public TimeRangeFilter(MediaCollection mediaCollection) : base(mediaCollection) { }

        
        public PrecisionDateTime? StartTime = null;
        public PrecisionDateTime? EndTime = null;


        /**
         * Set the time range of the filter
         */
        public void SetTimeRange(PrecisionDateTime start, PrecisionDateTime end)
        {
            if (start > end)
                throw new ArgumentException("TimeRangeFilter needs the start timestamp to be before or equal to the end timestamp");

            bool tightened = false;
            bool loosened = false;
            // If the new start time increases the time range, filter is loosened
            if (StartTime == null || StartTime < start)
                tightened = true;
            else
                loosened = true;

            // If the new end time increases the time range, filter is loosened
            if (EndTime == null || EndTime > end)
                tightened = true;
            else
                loosened = true;

            StartTime = start;
            EndTime = end;


            if(tightened && loosened)
            {
                if(FilterCriteriaChanged != null)
                    FilterCriteriaChanged();
            }
            else if(tightened)
            {
                if(FilterCriteriaTightened != null)
                    FilterCriteriaTightened();
            }
            else if(loosened)
            {
                if (FilterCriteriaLoosened != null)
                    FilterCriteriaLoosened();
            }

        }

        public override bool Filter(ICollectable c)
        {
            if (!IsFilterActive())
                return true;

            PrecisionDateTime itemTime;
            if (c is Event)
                itemTime = ((Event)c).StartTimestamp;
            else if (c is Media)
                itemTime = ((Media)c).Timestamp;
            else
                return false;

            return (itemTime >= StartTime && itemTime <= EndTime);
        }


        public override bool IsFilterActive()
        {
            return (StartTime != null && EndTime != null);
        }

        public override void ClearFilter()
        {
            StartTime = null;
            EndTime = null;

            if(FilterCriteriaLoosened != null)
                FilterCriteriaLoosened();
        }


        /**
         * When an item in the MediaCollection changes its timestamp, send the notification that it should
         * be refiltered.
         */
        public override void CollectionItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!IsFilterActive())
                return;

            if(sender is Event)
            {
                Event ev = (Event) sender;
                if (e.PropertyName == nameof(ev.StartTimestamp) && FilteredPropertyChanged != null)
                    FilteredPropertyChanged(ev);
                    
            }
            else if(sender is Media) 
            { 
                Media m = (Media)sender;
                if (e.PropertyName == nameof(m.Timestamp) && FilteredPropertyChanged != null)
                    FilteredPropertyChanged(m);
            }
        }

        public override void CopyTo(FilterCriteria c)
        {
            if(c.GetType() != typeof(TimeRangeFilter))
                throw new ArgumentException("FilterCriteria CopyTo() was given a filter criteria of a different type");


            TimeRangeFilter f = (TimeRangeFilter)c;

            if (!IsFilterActive())
                f.ClearFilter();
            else
                f.SetTimeRange(StartTime, EndTime);
        }
    }
}
