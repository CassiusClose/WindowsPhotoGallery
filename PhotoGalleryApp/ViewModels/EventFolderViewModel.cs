using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A FolderViewModel representing one Event (either user created or
    /// default year/month) in a tree.
    /// </summary>
    public class EventFolderViewModel : FolderViewModel
    {
        public EventFolderViewModel(string label, PrecisionDateTime timestamp)
        {
            Label = label;
            Timestamp = timestamp;

            Children = new ObservableCollection<FolderViewModel>();
            Children.CollectionChanged += Children_CollectionChanged;
            Children_CollectionChanged(null, null);
        }

        public override void Cleanup()
        {
            Children.CollectionChanged -= Children_CollectionChanged;
            foreach (EventFolderViewModel vm in Children)
                vm.Cleanup();
        }


        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasChildren = Children.Count > 0;
        }


        /// <summary>
        /// The timestamp associated with this folder
        /// </summary>
        public PrecisionDateTime Timestamp { get; }
    }
}
