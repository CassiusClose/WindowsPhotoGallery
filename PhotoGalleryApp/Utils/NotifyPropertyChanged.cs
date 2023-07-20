using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// Implementation of INotifyPropertyChanged
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Signals that a property of the class has been changed.
        /// </summary>
        /// <remarks>
        /// Subclasses should call this function on a property whenever that property is changed.
        /// <para>
        /// <c>CallerMemberName</c> automatically provides the propertyName argument when called from
        /// inside a property's setter. If called outside of that property's getter & setter,
        /// the caller will have to explicitly pass the property's name.
        /// </para>
        /// </remarks>
        /// <param name="propertyName">
        /// The name of the property that has been changed. If no name is provided and the function is
        /// called within a member, the name will be automatically provided.
        /// </param>
        /// <returns></returns>
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
