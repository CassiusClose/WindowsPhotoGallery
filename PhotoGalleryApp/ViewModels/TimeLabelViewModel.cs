using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Represents a label used separate media in MediaCollection by year, month, day, etc.
    /// </summary>
    public class TimeLabelViewModel : ICollectableViewModel
    {
        /// <summary>
        /// Creates a new TimeLabelViewModel. The time period category is specified by category, either year or month.
        /// The timestamp fields day, time are ignored. If the category is year, then the month field will be ignored as well.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="category"></param>
        public TimeLabelViewModel(DateTime timestamp, TimeRange category) : this(timestamp.Year, timestamp.Month, category) { }

        /// <summary>
        /// Creates a new TimeLabelViewModel. The time period category is specified by category, either year or month.
        /// If the category is year, then the month argument is ignored.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="category"></param>
        public TimeLabelViewModel(int year, int month, TimeRange category)
        {
            _category = category;

            if (category == TimeRange.Year)
            {
                _timestamp = new DateTime(year, 1, 1, 0, 0, 0);
                Label = year.ToString();
            }
            else
            {
                // Make the month timestamp 1 second ahead of the year, so it comes later if ever sorted
                _timestamp = new DateTime(year, month, 1, 0, 0, 1);
                Label = _timestamp.ToString("MMMM " + year);
            }
        }

        /// <summary>
        /// Creates a new TimeLabelViewModel of the year category.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="category"></param>
        /// <exception cref="ArgumentException"></exception>
        public TimeLabelViewModel(int year)
        {
            _category = TimeRange.Year;

            _timestamp = new DateTime(year, 1, 1, 0, 0, 0);
            Label = year.ToString();
        }


        /// <summary>
        /// The display string
        /// </summary>
        public string Label
        {
            get; internal set;
        }

        /**
         * Category is what period of time this refers to (year, month, day)
         * _timestamp should be the very begining of the time range
         */
        private TimeRange _category;
        private DateTime _timestamp;


        protected override DateTime _getTimestamp()
        {
            return _timestamp;
        }

        public override ICollectable GetModel() { return null; }
    }
}
