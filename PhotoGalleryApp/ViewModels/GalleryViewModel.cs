﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using PhotoGalleryApp.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using PhotoGalleryApp.Utils;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using PhotoGalleryApp.Filtering;
using PhotoGalleryApp.ViewModels.Search;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the Gallery view. Displays a MediaCollection to the user and lets users view, filter,
    /// and edit them. Holds a MediaCollectionViewModel, which contains the MediaCollection.
    /// </summary>
    class GalleryViewModel : ViewModelBase
    {
        #region Constructors
        public GalleryViewModel(string name, NavigatorViewModel navigator, MediaCollection coll, TimeRange? maxViewLabel=TimeRange.Year, FilterSet filters=null)
        {
            _navigator = navigator;
            _name = name;

            // Init commands
            _addFilesCommand = new RelayCommand(AddFiles);
            _saveGalleryCommand = new RelayCommand(SaveGallery);
            _escapePressedCommand = new RelayCommand(EscapePressed);
            _searchCommand = new RelayCommand(OpenSearch);

            // Init the media collection
            _mediaCollection = new MediaCollectionViewModel(navigator, coll, false, maxViewLabel, filters);
        }
        
        public override void Cleanup()
        {
            _mediaCollection.Cleanup();
        }


        #endregion Constructors



        #region Fields and Properties

        // A reference to the navigator so we can add pages to it
        private NavigatorViewModel _navigator;

        // A reference to the MediaCollectionViewModel so the gallery controls and the collection can interact
        private MediaCollectionViewModel _mediaCollection;
        public MediaCollectionViewModel MediaCollection { get { return _mediaCollection; } }


        private string _name;
        /// <summary>
        /// The Gallery's name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        #endregion Fields and Properties



        #region Commands

        #region AddFiles

        private RelayCommand _addFilesCommand;
        /// <summary>
        /// A command that opens a dialog box and adds the selected image files to the gallery.
        /// </summary>
        public ICommand AddFilesCommand => _addFilesCommand;

        /// <summary>
        /// Opens a dialog box and adds the selected image files to the gallery.
        /// </summary>
        public void AddFiles()
        {
            List<ICollectable> media = new List<ICollectable>();

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Media files (*.png;*.jpeg;*.jpg,*.mp4)|*.png;*.jpeg;*.jpg;*.mp4";
            if(fileDialog.ShowDialog() == true)
            {
                foreach (string filename in fileDialog.FileNames)
                {
                    if (!Path.HasExtension(filename))
                        continue;

                    Media m;

                    string ext = Path.GetExtension(filename).ToLower();
                    if (ext == ".png" || ext == ".jpeg" || ext == ".jpg")
                        m = new Image(filename);
                    else
                        m = new Video(filename);

                    MediaCollection.DisableImageLoad();
                    MediaCollection.MediaCollectionModel.Add(m);
                    MediaCollection.EnableImageLoad();
                }
            }
        }

        #endregion AddFiles


        #region SaveGallery

        private RelayCommand _saveGalleryCommand;
        /// <summary>
        /// A command which saves the gallery's state to disk.
        /// </summary>
        public ICommand SaveGalleryCommand => _saveGalleryCommand;

        /// <summary>
        /// Saves the gallery's state to disk.
        /// </summary>
        public void SaveGallery()
        {
            //TODO Move to top bar
            ((MainWindow)System.Windows.Application.Current.MainWindow).SaveGallery();
        }

        #endregion SaveGallery


        #region KeyCommands

        private RelayCommand _escapePressedCommand;
        /// <summary>
        /// A command which handles the escape key being pressed.
        /// </summary>
        public ICommand EscapePressedCommand => _escapePressedCommand;

        /// <summary>
        /// Handle the escape key being pressed. This deselects any selected media.
        /// </summary>
        public void EscapePressed(object parameter)
        {
            if (MediaCollection.MediaSelected)
            {
                MediaCollection.DeselectAllMedia();
            }
        }

        #endregion KeyCommands


        private RelayCommand _searchCommand;
        public ICommand SearchCommand => _searchCommand;

        /// <summary>
        /// Opens the page for advanced sorting
        /// </summary>
        private void OpenSearch()
        {
            _navigator.NewPage(new SearchPageViewModel(_navigator, _mediaCollection.MediaCollectionModel, _mediaCollection.MediaViewClass.ViewFilters.Clone()));
        }

        #endregion Commands
    }
}
