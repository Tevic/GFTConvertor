using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System;

public partial class AeroForm
    {
        [DllImport("dwmapi.dll")]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int left, right, top, bottom;
        }

        public static  void AeroEffect(Form f1)
        {
            MARGINS m = new MARGINS()
            {
                left = -1
            };
            DwmExtendFrameIntoClientArea(f1.Handle, ref m);
            Color aeroColor = Color.FromArgb(164, 212, 211);
            f1.TransparencyKey = aeroColor;
            f1.BackColor = aeroColor;
        }
    }