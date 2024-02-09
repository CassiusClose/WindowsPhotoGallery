using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// There's a RectangleF, but not a RectangleD, apparently, so this is that.
    /// </summary>
    public class RectangleD
    {
        public RectangleD(double bottom, double left, double top, double right)
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }

        public RectangleD(RectangleD rect)
        {
            Top = rect.Top;
            Left = rect.Left;
            Right = rect.Right;
            Bottom = rect.Bottom;
        }

        public double Top;
        public double Left;
        public double Right;
        public double Bottom;
    }
}
