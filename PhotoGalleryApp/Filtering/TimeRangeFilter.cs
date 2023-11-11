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

            StartTime = start;
            EndTime = end;

            if(FilterCriteriaTightened != null)
                FilterCriteriaTightened();
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

            if (itemTime.Year == 2019)
                Trace.WriteLine("2019");

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
