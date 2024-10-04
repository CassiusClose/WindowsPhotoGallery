using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Debug
{
    public class ViewModelList
    {
        public ViewModelList()
        {
            ViewModels = new ObservableCollection<VMEntryViewModel>();
        }

        public ObservableCollection<VMEntryViewModel> ViewModels
        {
            get; internal set;
        }


        public int RegisterViewModel(string name)
        {
            int id = _nextID++;
            ViewModels.Add(new VMEntryViewModel(id, name));
            return id;
        }

        public void RemoveViewModel(int id)
        {
            ViewModels.Remove(ViewModels.Single(vm => vm.ID == id));
        }

        private int _nextID = 0;
    }
}
