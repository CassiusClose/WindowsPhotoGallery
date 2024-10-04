using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Debug
{
    public class VMEntryViewModel
    {
        public VMEntryViewModel(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public int ID { get; internal set; }
        public string Name { get; internal set; }
    }
}
