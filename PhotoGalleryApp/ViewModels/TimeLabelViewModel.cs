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
        public TimeLabelViewModel(PrecisionDateTime dt)
        {
            switch(dt.Precision)
            {
                case TimeRange.Year:
                    Label = dt.Year.ToString();
                    break;

                case TimeRange.Month:
                    Label = dt.ToString("MMMM");
                    break;

                case TimeRange.Day:
                    Label = dt.ToString("D");
                    break;
            }

            _timestamp = dt;
        }

        public override void Cleanup() { }


        /// <summary>
        /// The display string
        /// </summary>
        public string Label
        {
            get; internal set;
        }
        public object PrecisionDateTime { get; internal set; }

        private PrecisionDateTime _timestamp;
        protected override PrecisionDateTime _getTimestamp()
        {
            return _timestamp;
        }

        public override ICollectable GetModel() { return null; }
    }
}
