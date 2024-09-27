using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{

    /// <summary>
    /// A type of ModelVMView meant to convert models into FolderViewModels,
    /// for display in a folder tree. This class also handles click
    /// notifications for all children in the tree, so a user simply has to add
    /// an event handler to FolderClicked.
    /// </summary>
    public abstract class FolderView<ModelType> : ModelVMView<ModelType, FolderViewModel>
    {
        protected FolderView(ObservableCollection<ModelType> modelColl) : base(modelColl) {}


        /* Fired whenever the FolderClicked event from any child folder is fired. */
        public FolderViewModel.FolderClickedDelegate? FolderClicked = null;

        protected void _folderClicked(FolderViewModel vm)
        {
            if (FolderClicked != null)
                FolderClicked(vm);
        }

        protected override void PostCreation(FolderViewModel vm)
        {
            vm.FolderClicked += _folderClicked;
        }

        protected override void PrepareForRemoval(FolderViewModel vm)
        {
            vm.FolderClicked -= _folderClicked;
        }
    }
}
