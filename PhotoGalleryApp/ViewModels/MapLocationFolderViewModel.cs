using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A FolderViewModel that represents a MapLocation object.
    /// </summary>
    public class MapLocationFolderViewModel : FolderViewModel
    {
        public MapLocationFolderViewModel(MapLocation model) : base()
        {
            _model = model;
            _model.PropertyChanged += _model_PropertyChanged;

            _childrenView = new MapLocationFolderView(model.Children);
            _childrenView.FolderClicked += _clickFolder;

            Children = _childrenView.View;
            Children.CollectionChanged += Children_CollectionChanged;
            Children_CollectionChanged(null, null);
        }

        public override void Cleanup()
        {
            _model.PropertyChanged -= _model_PropertyChanged;
            _childrenView.FolderClicked -= _clickFolder;
            _childrenView.Cleanup();
            Children.CollectionChanged -= Children_CollectionChanged;
        }

        private MapLocation _model;

        public MapLocation GetModel()
        {
            return _model;
        }

        public override string Label
        {
            get { return _model.Name; }
            set {  
                _model.Name = value;
                OnPropertyChanged();
            }
        }


        private MapLocationFolderView _childrenView;


        private void _model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_model.Name)) 
                OnPropertyChanged(nameof(Label));
        }


        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HasChildren = Children.Count > 0;
        }
    }
}
