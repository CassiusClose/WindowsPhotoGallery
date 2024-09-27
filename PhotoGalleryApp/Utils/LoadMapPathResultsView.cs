using PhotoGalleryApp.Models;
using PhotoGalleryApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// Maintains a list of ViewModels for each path found when loading a track file
    /// </summary>
    class LoadMapPathResultsView : ModelVMView<MapPath, PathFileResultsPathViewModel>
    {
        public LoadMapPathResultsView(ObservableCollection<MapPath> modelColl) : base(modelColl) { } 

        protected override void AddCollectionChangedListener(MapPath model, NotifyCollectionChangedEventHandler func)
        {
            // There are no collections in paths
            throw new ArgumentException();
        }

        protected override PathFileResultsPathViewModel CreateViewModel(MapPath item)
        {
            return new PathFileResultsPathViewModel(item);
        }

        protected override IList GetCollection(MapPath item)
        {
            throw new ArgumentException();
        }

        protected override MapPath GetModel(PathFileResultsPathViewModel vm)
        {
            return vm.Path;
        }

        protected override bool IsCollection(MapPath item)
        {
            return false;
        }

        protected override void RemoveCollectionChangedListener(MapPath model, NotifyCollectionChangedEventHandler func)
        {
            throw new ArgumentException();
        }
    }
}
