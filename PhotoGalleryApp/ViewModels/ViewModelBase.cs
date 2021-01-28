using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /*
         * Notifies listeners that a property in the class has changed. Subclasses should
         * call this on a property whenever changing it.
         * 
         * [CallerMemberName] automatically provides the propertyName argument when called
         * from inside a property's setter. If called outside that property's getters & setters,
         * will have to explicitly pass the property's name.
         * 
         * Implementation from
         * https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern
         * and
         * https://intellitect.com/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
         */
        protected bool OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }
    }
}
