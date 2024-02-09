using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public LoadPathsFileResultsPopupViewModel(List<MapPath> paths, BackgroundWorker worker)
        {
            _checkAllCommand = new RelayCommand(CheckAllClicked);

            _paths = new ObservableCollection<MapPath>(paths);

            _pathsView = new ModelVMView<MapPath, PathFileResultsPathViewModel>(_paths, _createPathViewModel, _getPathFromViewModel);

            FindOverlaps(worker);
        }

        public override void Cleanup() { }




        #region Potential Paths

        // Each potential path
        private ObservableCollection<MapPath> _paths;


        // A VM for each potential path
        public ObservableCollection<PathFileResultsPathViewModel> MapPaths
        {
            get { return _pathsView.View; }
        }


        ModelVMView<MapPath, PathFileResultsPathViewModel> _pathsView;
        private PathFileResultsPathViewModel _createPathViewModel(MapPath path) 
        { 
            PathFileResultsPathViewModel vm = new PathFileResultsPathViewModel(path);
            vm.PropertyChanged += PathVM_PropertyChanged;
            return vm;
        }

        private MapPath _getPathFromViewModel(PathFileResultsPathViewModel vm) { return vm.Path; }

        #endregion Potential Paths




        #region Checked Paths

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

        private void PathVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PathFileResultsPathViewModel.AddToMap))
                OnPropertyChanged(nameof(SomeChecked));
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

        #endregion Checked Paths



        public override PopupReturnArgs GetPopupResults()
        {
            return new LoadPathsFileResultsPopupReturnArgs(MapPaths.ToList());
        }

        protected override bool ValidateData()
        {
            return true;
        }



        #region Overlap Algorithm

        /**
         * This is an async task, so report progress with the BackgroundWorker
         */
        private void FindOverlaps(BackgroundWorker worker)
        {
            float tot = MapPaths.Count;
            int count = 0;
            // Find overlaps with existing paths
            foreach(PathFileResultsPathViewModel vm in MapPaths)
            {
                int div = 8;
                // A "partially overlapping region" must be at least the length
                // of 1/8 of the path, or 50 points, whichever is smaller. At
                // the risk of missing regions with just a couple overlapping
                // points, this greatly speeds up the calculations.
                //
                // 50 is arbitrarily chosen, with the idea that if the path is
                // really long, it should be able to have a reasonably sized
                // overlap be detected, even if the overlap is not super large
                // (enough to reach 1/8 length)
                //
                // 1/8 of the path length is made to be at least 1 point, since
                // 0 points of overlap should not be treated as an overlap
                int minRegionSize = Math.Min(Math.Max(vm.Path.Locations.Count / 8, 1), 50);

                worker.ReportProgress((int)(100*count++ / tot));


                // Make a list of all the existing MapPaths in the map
                List<MapItem> mapPaths = MainWindow.GetCurrentSession().Map.ToList();
                for(int i = 0; i < mapPaths.Count; i++)
                {
                    if (mapPaths[i] is not MapPath)
                        mapPaths.RemoveAt(i--);
                }


                // If any start with the same location as the current path, put
                // it at the front to prioritize it. This will make full
                // overlaps detected very quickly.
                for(int i = 1; i < mapPaths.Count; i++)
                {
                    MapPath p = (MapPath)mapPaths[i];
                    if (p.Locations[0] == vm.Path.Locations[0])
                    {
                        mapPaths.RemoveAt(i);
                        mapPaths.Insert(0, p);
                    }
                }


                // Compare with each map path for overlap
                foreach(MapItem item in mapPaths)
                {
                    MapPath path = (MapPath)item;

                    if (path.Locations.Count == 1)
                        throw new Exception();

                    int matches = 0;
                    // Increment by the region size to try to find a single
                    // matching point. From there, look forward and backward
                    // and figure out how big the matching region is
                    for(int i = 0; i < vm.Path.Locations.Count; i+=minRegionSize)
                    {
                        // For the current point, see if any locations in the compared path match
                        for(int j = 0; j < path.Locations.Count; j++)
                        {
                            // If we have found a match, figure out how big the overlapping region is
                            if (vm.Path.Locations[i].Latitude == path.Locations[j].Latitude && vm.Path.Locations[i].Longitude == path.Locations[j].Longitude)
                            {
                                // Go backward from the matching point, and see how many previous consecutive points are also matching
                                for (int sub = 0; sub < Math.Min(i, j); sub++)
                                {
                                    if (vm.Path.Locations[i - sub].Latitude == path.Locations[j - sub].Latitude && vm.Path.Locations[i - sub].Longitude == path.Locations[j - sub].Longitude)
                                    {
                                        matches++;
                                    }
                                    else
                                        break;
                                }

                                // Go forward from the matching point, and see how many consecutive points are also matching
                                int add;
                                for(add = 0; add < Math.Min(vm.Path.Locations.Count - i, path.Locations.Count - j); add++)
                                {
                                    if (vm.Path.Locations[i + add].Latitude == path.Locations[j + add].Latitude && vm.Path.Locations[i + add].Longitude == path.Locations[j + add].Longitude)
                                    {
                                        matches++;
                                    }
                                    else 
                                        break;

                                }

                                // If the matches are enough to count as a
                                // "partially overlapping region", then break
                                // out
                                if (matches >= minRegionSize)
                                    break;


                                // If not enough matches to make a partially
                                // overlapping region, then reset the number of
                                // matches and keep searching
                                if(matches > 0)
                                {
                                    matches = 0;
                                    // Update i to start on the next location that wasn't looked at by this search
                                    i = i + add + 2 - minRegionSize;
                                }

                                break;
                            }
                        }

                        // If the matches are enough to count as a "partially
                        // overlapping region", then stop searching
                        if (matches >= minRegionSize)
                            break;
                    }

                    // Full overlap - if the points match exactly
                    if (matches == vm.Path.Locations.Count)
                    {
                        vm.OverlapFound = PathFileResultsPathViewModel.IsOverlap.FullOverlap;
                        vm.ReplaceSelection = PathFileResultsPathViewModel.ReplaceChoices.ReplaceOverlap;
                        vm.AddToMap = false;
                        vm.OverlapPath = path;
                        break;
                    }
                    // Partial overlap - matches at least a 1/8 of the path, or at least 50 points
                    else if (matches >= minRegionSize)
                    {
                        vm.OverlapFound = PathFileResultsPathViewModel.IsOverlap.PartialOverlap;
                        vm.ReplaceSelection = PathFileResultsPathViewModel.ReplaceChoices.MergeOverlap;
                        vm.AddToMap = true;
                        vm.OverlapPath = path;
                        break;
                    }
                }
            }
        }

        #endregion Overlap Algorithm

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
