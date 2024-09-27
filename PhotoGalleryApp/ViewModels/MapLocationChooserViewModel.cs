using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel which provides a tree of FolderViewModel objects of all the
    /// MapLocations in the given Map. One or none of the MapLocations can be
    /// chosen.
    /// </summary>
    public class MapLocationChooserViewModel : ViewModelBase
    {
        public MapLocationChooserViewModel(Map map)
        {
            _view = new MapItemToLocationFolderView(map);
            _view.FolderClicked += FolderClicked;
            Folders = _view.View;
            OnPropertyChanged(nameof(SelectionText));
        }
        public override void Cleanup()
        {
            _view.Cleanup();
        }


        private MapItemToLocationFolderView _view;

        public ObservableCollection<FolderViewModel> Folders
        {
            get; internal set;
        }

        public string? SelectionText
        {
            get {
                if (_selectedFolder == null)
                    return null;

                if (_selectedFolder is not MapLocationFolderViewModel)
                    return null;

                return ((MapLocationFolderViewModel)_selectedFolder).GetModel().DisplayString();
            }
        }


        private FolderViewModel? _selectedFolder = null;
        public FolderViewModel? SelectedFolder
        {
            get { return _selectedFolder; }
            internal set
            {
                _selectedFolder = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectionText));
            }
        }

        void FolderClicked(FolderViewModel vm)
        {
            if (vm.IsSelected) 
            { 
                vm.IsSelected = false;
                SelectedFolder = null;
            }
            else
            {
                foreach (FolderViewModel f in Folders)
                    f.DeselectAll();

                vm.IsSelected = true;
                SelectedFolder = vm;
            }
        }
    }
}
