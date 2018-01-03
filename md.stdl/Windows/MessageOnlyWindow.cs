using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using md.stdl.Interaction;
using SharpDX.RawInput;
using VVVV.Utils.Win32;

namespace md.stdl.Windows
{
    public class MessageOnlyWindow : NativeWindow, IDisposable
    {
        public MessageOnlyWindow()
        {
            // Create a message-only window
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632599%28v=vs.85%29.aspx#message_only
            var cp = new CreateParams
            {
                ClassName = "Message",
                Parent = Const.HWND_MESSAGE
            };
            this.CreateHandle(cp);
        }

        ~MessageOnlyWindow()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (this.Handle != IntPtr.Zero)
                this.DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            OnWndProc?.Invoke(this, m);
            base.WndProc(ref m);
        }

        public event EventHandler<Message> OnWndProc;
    }
}
