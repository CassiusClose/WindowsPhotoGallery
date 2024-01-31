using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for a single path entry in the LoadPathsFileResultsPopup.
    /// The user can mark whether the path should be added to the map or not
    /// and edit the name. The popup will mark whether there is an overlap with
    /// an existing path, and if there is, the user can choose how to deal with
    /// that overlap. Lastly, the user can preview each path in a separate
    /// popup.
    /// </summary>
    public class PathFileResultsPathViewModel : ViewModelBase
    {
        public PathFileResultsPathViewModel(MapPath path)
        {
            _previewPathCommand = new RelayCommand(PreviewPath);

            Path = path;
        }
        public override void Cleanup() { }


        public MapPath Path
        {
            get; internal set;
        }


        public string Name
        {
            get { return Path.Name; }
            set
            {
                Path.Name = value;
                OnPropertyChanged();
            }
        }


        private bool _addToMap = true;
        public bool AddToMap
        {
            get { return _addToMap; }
            set
            {
                _addToMap = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowReplaceChoices));
            }
        }



        #region Overlapping Path

        /// <summary>
        /// Whether the path overlaps with an existing path. Full overlap means
        /// the list of points is exactly the same. Partial overlap means some
        /// of the points match, but not all. To be considered partial overlap,
        /// there must be at least some number of points overlapping.
        /// </summary>
        public enum IsOverlap { NoOverlap, PartialOverlap, FullOverlap };


        private IsOverlap _overlapFound = IsOverlap.NoOverlap;
        public IsOverlap OverlapFound
        {
            get { return _overlapFound; }
            set 
            { 
                _overlapFound = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(ShowReplaceChoices)); 
            }
        }

        // The existing path that this overlaps with
        public MapPath? OverlapPath = null;

        #endregion Overlapping Path




        #region Replace Choices

        /// <summary>
        /// What to do about the overlap. AddNormally will simply add the new
        /// path to the map. ReplaceOverlap will remove the existing
        /// overlapping path and add the new one. MergeOverlap will merge the
        /// new & existing path together and add that.
        /// </summary>
        public enum ReplaceChoices
        {
            AddNormally,
            ReplaceOverlap,
            MergeOverlap
        }


        private ReplaceChoices _replaceSelection = ReplaceChoices.AddNormally;
        /// <summary>
        /// Which choice the user has selected to handle overlap
        /// </summary>
        public ReplaceChoices ReplaceSelection
        {
            get { return _replaceSelection; }
            set { _replaceSelection = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// Names of each choice for display in the Combobox.
        /// </summary>
        public static Dictionary<ReplaceChoices, string> ReplaceChoiceNames { get; } =
            new Dictionary<ReplaceChoices, string>()
            {
                {ReplaceChoices.AddNormally, "Add Normally"},
                {ReplaceChoices.ReplaceOverlap, "Replace"},
                {ReplaceChoices.MergeOverlap, "Merge"}
            };



        /**
         * Only show the Combobox if the user has marked the path to be added
         * to the map and there is an overlap.
         */
        public bool ShowReplaceChoices
        {
            get { return AddToMap && OverlapFound != IsOverlap.NoOverlap; }
        }

        #endregion Replace Choices





        private RelayCommand _previewPathCommand;
        public ICommand PreviewPathCommand => _previewPathCommand;

        /**
         * Open a popup to look at the path. Also add the existing overlapping
         * path with a different color & thickness to be shown.
         */
        public void PreviewPath()
        {
            Map m = new Map();
            if(OverlapPath != null)
                m.Add(OverlapPath);
            m.Add(Path);

            PreviewMapPopupViewModel popup = new PreviewMapPopupViewModel(m);

            // Find the overlapping path and make it a different color & thickness
            if(OverlapPath != null)
            {
                foreach(MapItemViewModel item in popup.Map.MapItems)
                {
                    if(item.GetModel() == OverlapPath)
                    {
                        MapPathViewModel vm = (MapPathViewModel)item;
                        vm.OverridePathColor = Colors.Blue;
                        vm.OverrideStrokeThickness = 10;
                    }
                }
            }

            MainWindow.GetNavigator().OpenPopup(popup);
        }
    }
}