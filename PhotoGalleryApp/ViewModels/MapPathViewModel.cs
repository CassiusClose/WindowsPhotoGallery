using Microsoft.Maps.MapControl.WPF;
using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// ViewModel for the MapPath model
    /// </summary>
    public class MapPathViewModel : MapItemViewModel
    {
        public MapPathViewModel(MapPath path)
        {
            _path = path;
        }

        public override void Cleanup() {}


        
        private MapPath _path;

        public LocationCollection Points { get { return _path.Points; } }

        public string Name { get { return _path.Name; } }


        /// <summary>
        /// Removes the given point from the path
        /// </summary>
        /// <param name="location"></param>
        public void RemovePoint(Location location)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                if (Points[i] == location)
                {
                    Points.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Adds the given point to the end of the path
        /// </summary>
        /// <param name="location"></param>
        public void AddPoint(Location location)
        {
            Points.Add(location);
        }


        public override MapItem GetModel()
        {
            return _path;
        }
    }
}
