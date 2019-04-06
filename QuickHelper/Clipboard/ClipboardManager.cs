using System;
using System.Windows;
using System.Windows.Interop;

namespace QuickHelper
{
    /// <summary>
    /// Class for interaction with clipboard.
    /// </summary>
    public class ClipboardManager
    {
        private static readonly IntPtr WndProcSuccess = IntPtr.Zero;

        public event EventHandler ClipboardChanged;


        /// <summary>
        /// A basic constructor.
        /// </summary>
        /// <param name="windowSource">Current window for checking initialization.</param>
        public ClipboardManager(Window windowSource)
        {
            if (!(PresentationSource.FromVisual(windowSource) is HwndSource source))
            {
                const string MESSAGE = "Window source MUST be initialized first, such as in the " +
                                       "Window's OnSourceInitialized handler.";
                throw new ArgumentException(MESSAGE, nameof(windowSource));
            }

            source.AddHook(WndProc);

            // Get window handle for interop.
            var windowHandle = new WindowInteropHelper(windowSource).Handle;

            // Register for clipboard events.
            NativeMethods.AddClipboardFormatListener(windowHandle);
        }

        /// <summary>
        /// Event that triggered when clipboard changed.
        /// </summary>
        private void OnClipboardChanged()
        {
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != NativeMethods.WM_CLIPBOARDUPDATE) 
                return WndProcSuccess;

            OnClipboardChanged();
            handled = true;

            return WndProcSuccess;
        }
    }
}
