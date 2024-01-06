using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// Functions for finding ancestors of UIElements
    /// Taken from: https://stackoverflow.com/questions/53201935/find-controls-ancestor-by-type
    /// </summary>
    public class ViewAncestor
    {
        public static T? FindAncestor<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                var dependObj = obj;
                do
                {
                    dependObj = GetParent(dependObj);
                    if (dependObj is T)
                        return dependObj as T;
                }
                while (dependObj != null);
            }

            return null;
        }

        public static DependencyObject? GetParent(DependencyObject obj)
        {
            if (obj == null)
                return null;
            if (obj is ContentElement)
            {
                var parent = ContentOperations.GetParent(obj as ContentElement);
                if (parent != null)
                    return parent;
                if (obj is FrameworkContentElement)
                    return ((FrameworkContentElement)obj).Parent;
                return null;
            }

            return VisualTreeHelper.GetParent(obj);
        }
    }
}
