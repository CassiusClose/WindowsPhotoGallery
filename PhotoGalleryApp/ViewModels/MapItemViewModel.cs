using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// Abstract class representing items that can be displayed on a map. Currently, these items are 
    /// Locations and Paths. These items should be able to be edited, and should have some sort of
    /// preview box that pops up when clicked on. 
    /// </summary>
    public abstract class MapItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Returns the model that the ViewModel is associated with
        /// </summary>
        /// <returns></returns>
        public abstract MapItem GetModel();


        /// <summary>
        /// Returns the Type of the UserControl that is the Preview for this
        /// item. Subclasses should set this to not-null if previews are
        /// enabled.
        /// </summary>
        public Type? PreviewType { get; protected set; }


        protected bool _editMode;
        /// <summary>
        /// Whether the MapItem is able to be edited
        /// </summary>
        public virtual bool EditMode
        {
            get { return _editMode; }
            set
            {
                _editMode = value;
                OnPropertyChanged();
                EditModeChanged();
            }
        }

        protected virtual void EditModeChanged() {}


        protected bool _previewOpen;
        /// <summary>
        /// Whether the MapItem's preview box should be open
        /// </summary>
        public virtual bool PreviewOpen
        {
            get { return _previewOpen; }
            set
            {
                _previewOpen = value; 
                OnPropertyChanged();
            }
        }


        public string Name { 
            get { return GetModel().Name; } 
            set
            {
                GetModel().Name = value;
                OnPropertyChanged();
            }
        }
    }
}
