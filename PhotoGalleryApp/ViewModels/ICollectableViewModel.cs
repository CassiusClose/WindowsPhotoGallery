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
    /// Abstract view model for the ICollectable model
    /// </summary>
    public abstract class ICollectableViewModel : ViewModelBase
    {
        /// <summary>
        /// Returns the ICollectable model associated with this viewmodel
        /// </summary>
        /// <returns>The ICollectable model associated with this viewmodel</returns>
        public abstract ICollectable GetModel();



        protected abstract PrecisionDateTime _getTimestamp();
        public PrecisionDateTime Timestamp
        {
            get { return _getTimestamp(); }
        }


        
        private bool _isSelected;
        /// <summary>
        /// Whether the media is selected in the containing MediaCollection. Currently, it's
        /// not possible to select events, but it might be in the future.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; } 
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool _isInView = false;
        /// <summary>
        /// Whether the element is currently in view of its containing MediaCollection.
        /// </summary>
        public bool IsInView
        {
            get { return _isInView; }
            set
            {
                _isInView = value;
                OnPropertyChanged();
            }
        }
    }
}
