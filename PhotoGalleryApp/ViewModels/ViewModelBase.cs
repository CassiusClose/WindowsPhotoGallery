using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A base View Model class that supports property changes (with INotifyPropertyChanged)
    /// </summary>
    /// <remarks>
    /// Implementation taken from
    /// <see href="https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern"/>
    /// and
    /// <see href="https://intellitect.com/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/"/>
    /// </remarks>
    public abstract class ViewModelBase : NotifyPropertyChanged
    {
        /// <summary>
        /// Called by NavigatorViewModel when this ViewModel page is popped from the history stack.
        /// In other words, when the page is on top and the 'back' button is pressed. Used to clean
        /// up anything that should end when a page is destroyed, such as cancelling asynchronous tasks.
        /// </summary>
        public virtual void NavigatorLostFocus() { }

        public abstract void Cleanup();
    }
}
