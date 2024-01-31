using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the popup window, where the user can choose how to load in each track.
    /// </summary>
    public class LoadPathsFileResultsPopupViewModel : PopupViewModel
    {
        public LoadPathsFileResultsPopupViewModel(List<MapPath> paths)
        {
            _checkAllCommand = new RelayCommand(CheckAllClicked);

            _paths = new ObservableCollection<MapPath>(paths);

            _pathsView = new ModelVMView<MapPath, PathFileResultsPathViewModel>(_paths, _createPathViewModel, _getPathFromViewModel);


            // Find overlaps with existing paths
            foreach(PathFileResultsPathViewModel vm in MapPaths)
            {
                bool overlapFound = false;

                foreach(MapItem item in MainWindow.GetCurrentSession().Map)
                {
                    if (item is not MapPath)
                        continue;

                    MapPath path = (MapPath)item;

                    if (path.Locations.Count == 1)
                        throw new Exception();

                    // How many points match between the two paths
                    int matches = 0;
                    foreach(Location l1 in vm.Path.Locations)
                    {
                        bool found = false;
                        foreach(Location l2 in path.Locations)
                        {
                            if (l1.Latitude == l2.Latitude && l1.Longitude == l2.Longitude)
                            {
                                found = true;
                                break;
                            }
                        }

                        if(found)
                        {
                            matches++;
                        }
                    }

                    // Full overlap - if the points match exactly
                    if (matches == vm.Path.Locations.Count)
                    {
                        vm.OverlapFound = PathFileResultsPathViewModel.IsOverlap.FullOverlap;
                        vm.ReplaceSelection = PathFileResultsPathViewModel.ReplaceChoices.ReplaceOverlap;
                        vm.AddToMap = false;
                        vm.OverlapPath = path;
                    }
                    // Partial overlap - matches at least a 1/4 of the path, or at least 50 points
                    else if (matches > 50 || matches > 0.25*vm.Path.Locations.Count)
                    {
                        vm.OverlapFound = PathFileResultsPathViewModel.IsOverlap.PartialOverlap;
                        vm.ReplaceSelection = PathFileResultsPathViewModel.ReplaceChoices.MergeOverlap;
                        vm.AddToMap = false;
                        vm.OverlapPath = path;
                        break;
                    }
                }
            }

        }
        public override void Cleanup() { }




        private ObservableCollection<MapPath> _paths;


        ModelVMView<MapPath, PathFileResultsPathViewModel> _pathsView;
        private PathFileResultsPathViewModel _createPathViewModel(MapPath path) 
        { 
            PathFileResultsPathViewModel vm = new PathFileResultsPathViewModel(path);
            vm.PropertyChanged += PathVM_PropertyChanged;
            return vm;
        }

        private MapPath _getPathFromViewModel(PathFileResultsPathViewModel vm) { return vm.Path; }


        // A VM for each track that might be loaded into the map
        public ObservableCollection<PathFileResultsPathViewModel> MapPaths
        {
            get { return _pathsView.View; }
        }



        private void PathVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PathFileResultsPathViewModel.AddToMap))
                OnPropertyChanged(nameof(SomeChecked));
        }

        /// <summary>
        /// True if any of the childen paths are checked
        /// </summary>
        public bool SomeChecked 
        {
            get 
            { 
                foreach(PathFileResultsPathViewModel path in MapPaths)
                    if(path.AddToMap)
                        return true;

                return false; 
            }
        }


        private RelayCommand _checkAllCommand;
        public ICommand CheckAllCommand => _checkAllCommand;

        /*
         * Toggle the CheckAll button. So if any paths are checked, uncheck all
         * of them. If none are checked, check all of them.
         */
        public void CheckAllClicked()
        {
            if(SomeChecked)
            {
                foreach (PathFileResultsPathViewModel vm in MapPaths)
                    vm.AddToMap = false;
            }
            else
            {
                foreach (PathFileResultsPathViewModel vm in MapPaths)
                    vm.AddToMap = true;
            }
        }




        public override PopupReturnArgs GetPopupResults()
        {
            return new LoadPathsFileResultsPopupReturnArgs(MapPaths.ToList());
        }

        protected override bool ValidateData()
        {
            return true;
        }
    }

    public class LoadPathsFileResultsPopupReturnArgs : PopupReturnArgs
    {
        public LoadPathsFileResultsPopupReturnArgs(List<PathFileResultsPathViewModel> paths)
        {
            Paths = paths;
        }

        public List<PathFileResultsPathViewModel> Paths;
    }
}
