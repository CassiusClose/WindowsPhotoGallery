using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PhotoGalleryApp.Converters
{
    /// <summary>
    /// Converts an ObservableCollection of EventViewModel objects to an ObservableCollection of strings of their names.
    /// This is used to display the event names in a ChooserDropDown list.
    /// </summary>
    class EventToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<EventViewModel> e = (ObservableCollection<EventViewModel>)value;
            if(e == null)
                throw new ArgumentNullException(nameof(value));

            ObservableCollection<string> names = new ObservableCollection<string>();
            foreach(EventViewModel vm in e)
                names.Add(vm.Name);

            return names;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
