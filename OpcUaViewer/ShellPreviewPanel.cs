using Microsoft.Win32;

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpcUaViewer
{
    /// <summary>
    /// Hosts the Windows Shell preview handler registered for a file extension,
    /// embedding it directly into a WinForms panel using the IPreviewHandler COM interface.
    /// On machines with Foxit (or any other PDF reader that registers a preview handler)
    /// this renders PDFs with no additional dependencies.
    /// </summary>
    internal class ShellPreviewPanel : Panel
    {
        private static readonly Guid ShellExPreviewHandlerGuid =
            new Guid("{8895b1c6-b41f-4c1c-a562-0d564250836f}");

        private object _handler;
        private string _message; // shown via OnPaint when no handler is available

        public ShellPreviewPanel()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            BackColor = System.Drawing.Color.White;
        }

        /// <summary>Loads and displays the given file using its registered preview handler.</summary>
        public void PreviewFile(string filePath)
        {
            UnloadHandler();
            _message = null;

            string clsidStr = GetPreviewHandlerClsid(".pdf");
            if (clsidStr == null)
            {
                _message = "No PDF preview handler is registered on this machine.\n\n" +
                           "Please install Foxit PDF Reader or Adobe Acrobat Reader\n" +
                           "to enable PDF previews.";
                Invalidate();
                return;
            }

            var type = Type.GetTypeFromCLSID(new Guid(clsidStr), throwOnError: true);
            _handler = Activator.CreateInstance(type);

            if (_handler is IInitializeWithFile initFile)
                initFile.Initialize(filePath, 0); // STGM_READ
            else
            {
                _message = "The registered PDF preview handler does not support file initialization.";
                Marshal.ReleaseComObject(_handler);
                _handler = null;
                Invalidate();
                return;
            }

            var ph = (IPreviewHandler)_handler;
            var rect = ToRECT(ClientRectangle);
            ph.SetWindow(Handle, ref rect);
            ph.SetRect(ref rect);
            ph.DoPreview();
        }

        /// <summary>Stops the current preview and optionally shows a message in its place.</summary>
        public void ClearPreview(string message = null)
        {
            UnloadHandler();
            _message = message;
            Invalidate();
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_message != null && _handler == null)
            {
                e.Graphics.Clear(BackColor);
                using var font = new System.Drawing.Font("Segoe UI", 11f);
                var fmt = new System.Drawing.StringFormat
                {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center
                };
                e.Graphics.DrawString(_message, font, System.Drawing.Brushes.Gray, ClientRectangle, fmt);
            }
        }

        private void UnloadHandler()
        {
            if (_handler == null) return;

            if (_handler is IPreviewHandler ph)
            {
                try { ph.Unload(); } catch { }
            }

            Marshal.ReleaseComObject(_handler);
            _handler = null;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRect();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) ApplyRect();
        }

        private void ApplyRect()
        {
            if (_handler is IPreviewHandler ph)
            {
                var rect = ToRECT(ClientRectangle);
                try { ph.SetRect(ref rect); } catch { }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) UnloadHandler();
            base.Dispose(disposing);
        }

        private static string GetPreviewHandlerClsid(string extension)
        {
            // Primary location: HKCR\<ext>\ShellEx\{preview-handler-guid}
            using var key = Registry.ClassesRoot.OpenSubKey(
                $@"{extension}\ShellEx\{{{ShellExPreviewHandlerGuid}}}");
            return key?.GetValue(null) as string;
        }

        private static RECT ToRECT(System.Drawing.Rectangle r) =>
            new RECT { Left = r.Left, Top = r.Top, Right = r.Right, Bottom = r.Bottom };

        // ── COM interfaces ─────────────────────────────────────────────────────

        [ComImport]
        [Guid("8895b1c6-b41f-4c1c-a562-0d564250836f")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPreviewHandler
        {
            void SetWindow(IntPtr hwnd, ref RECT prc);
            void SetRect(ref RECT prc);
            void DoPreview();
            void Unload();
            void SetFocus();
            void QueryFocus(out IntPtr phwnd);
            [PreserveSig] int TranslateAccelerator(ref MSG pmsg);
        }

        [ComImport]
        [Guid("b7d14566-0509-4cce-a71f-0a554233bd9b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IInitializeWithFile
        {
            void Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszFilePath, uint grfMode);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd; public uint message;
            public IntPtr wParam; public IntPtr lParam;
            public uint time; public int ptX; public int ptY;
        }
    }
}
