using PhotoGalleryApp.Filtering;
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
        }

        private TimeRangeFilter _filter;


        public override void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
