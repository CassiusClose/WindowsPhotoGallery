using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace PhotoGalleryApp.Views.Behavior
{
    /// <summary>
    /// Provides methods for enabling and disabling the close button of a
    /// window. Not a behavior because it provides functions that can be called
    /// from the code-behind.
    /// 
    /// Code adapted from: https://blog.magnusmontin.net/2014/11/30/disabling-or-hiding-the-minimize-maximize-or-close-button-of-a-wpf-window/
    /// </summary>
    class CloseButtonVisibility
    {
        public CloseButtonVisibility(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;

            window.Closed += Window_Closed;
        }


        public void DisableCloseButton()
        {
            if (_windowHandle == null)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            if(menuHandle == IntPtr.Zero)
                menuHandle = GetSystemMenu((IntPtr)_windowHandle, false);

            if (menuHandle != IntPtr.Zero)
                EnableMenuItem(menuHandle, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
        }

        public void EnableCloseButton()
        {
            if (_windowHandle == null)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            if(menuHandle == IntPtr.Zero)
                menuHandle = GetSystemMenu((IntPtr)_windowHandle, false);

            if (menuHandle != IntPtr.Zero)
                EnableMenuItem(menuHandle, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
        }



        /*
         * Remove any handles that would prevent destruction. I think the
         * windowHandle doesn't need this, though?
         */
        private void Window_Closed(object? sender, EventArgs e)
        {
            if (menuHandle != IntPtr.Zero)
                DestroyMenu(menuHandle);
        }




        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        private static extern IntPtr DestroyMenu(IntPtr hWnd);

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_ENABLED = 0x00000000;
        private const uint SC_CLOSE = 0xF060;

        private IntPtr? _windowHandle;
        private IntPtr menuHandle = IntPtr.Zero;

    }
}
