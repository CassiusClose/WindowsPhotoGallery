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
    /// A ViewModel representing one folder in a folder tree. 
    /// </summary>
    public class FolderLabelViewModel : ViewModelBase
    {
        public FolderLabelViewModel(string label, PrecisionDateTime timestamp)
        {
            _label = label;
            Timestamp = timestamp;

            _toggleExpandCommand = new RelayCommand(ToggleExpand);

            Children = new ObservableCollection<FolderLabelViewModel>();
            Children.CollectionChanged += Children_CollectionChanged;
            Children_CollectionChanged(null, null);
        }

        public override void Cleanup()
        {
            Children.CollectionChanged -= Children_CollectionChanged;
            foreach (FolderLabelViewModel vm in Children)
                vm.Cleanup();
        }



        /// <summary>
        /// The folders contained within this one
        /// </summary>
        public ObservableCollection<FolderLabelViewModel> Children
        {
            get; internal set;
        } 
        
        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasChildren = Children.Count > 0;
        }

        private bool _hasChildren;
        /// <summary>
        /// Whether the folder contains any child folders
        /// </summary>
        public bool HasChildren
        {
            get { return _hasChildren; }
            internal set
            {
                _hasChildren = value;
                if (_hasChildren == false)
                    IsExpanded = false;
                OnPropertyChanged();
            }
        }



        private string _label;
        /// <summary>
        /// The display name of this folder
        /// </summary>
        public string Label { 
            get { return _label; } 
            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// The timestamp associated with this folder
        /// </summary>
        public PrecisionDateTime Timestamp { get; }


        /// <summary>
        /// Whether the folder is expanded to show its children in the view
        /// </summary>
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { 
                _isExpanded = value; 
                OnPropertyChanged(); 
            }
        }


        public delegate void FolderOpenedDelegate(FolderLabelViewModel folder);
        public FolderOpenedDelegate? FolderOpened;
        /** Send the Open event to any listeners */
        public void OpenFolder(object sender, EventArgs eArgs)
        {
            if (FolderOpened != null)
                FolderOpened(this);
        }


        private RelayCommand _toggleExpandCommand;
        public ICommand ToggleExpandCommand => _toggleExpandCommand;
        
        public void ToggleExpand()
        {
            IsExpanded = !IsExpanded && HasChildren;
        }


    }
}
