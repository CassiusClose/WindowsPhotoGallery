using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels.Search
{
    /// <summary>
    /// A ViewModel for viewing & selecting the timerange to filter with in a search.
    /// </summary>
    public class TimeRangeFilterViewModel : ViewModelBase
    {
        public TimeRangeFilterViewModel(TimeRangeFilter filter)
        {
            _filter = filter;
            _filter.PropertyChanged += _filter_PropertyChanged;
        }

        public override void Cleanup()
        {
            _filter.PropertyChanged -= _filter_PropertyChanged;
        }



        private TimeRangeFilter _filter;

        /**
         * If the filter changes, notify the PropertyChanged listeners
         */
        private void _filter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_filter.StartTime))
                OnPropertyChanged(nameof(StartTimestamp));

            if (e.PropertyName == nameof(_filter.EndTime))
                OnPropertyChanged(nameof(EndTimestamp));
        }



        public PrecisionDateTime? StartTimestamp
        {
            get { return _filter.StartTime; }
            set
            {
                _filter.StartTime = value;
                OnPropertyChanged();
            }
        }

        public PrecisionDateTime? EndTimestamp
        {
            get { return _filter.EndTime; }
            set
            {
                _filter.EndTime = value;
                OnPropertyChanged();
            }
        }
    }
}
