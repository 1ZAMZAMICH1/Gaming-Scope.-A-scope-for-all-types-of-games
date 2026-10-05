using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace CrosshairApp
{
    public class OverlayForm : Form
    {
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int GWL_EXSTYLE = -20;
        [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr h, int X, int Y, int cx, int cy, uint f);

        public Color cColor = Color.LimeGreen;
        public int cSize = 25;
        public int cThick = 2;
        public string cShape = "Dot";
        public bool isVisible = true;

        public OverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            ShowInTaskbar = false;
            DoubleBuffered = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_LAYERED | WS_EX_TRANSPARENT);
            SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0040);
            
            // Запускаем поток для чтения команд от Electron интерфейса
            Thread t = new Thread(new ThreadStart(ReadInput));
            t.IsBackground = true;
            t.Start();
        }

        void ReadInput()
        {
            try {
                while(true) {
                    string line = Console.ReadLine();
                    if(line == null) break; 
                    
                    // Формат: SHAPE|SIZE|THICK|#COLOR|VISIBLE
                    string[] parts = line.Split('|');
                    if(parts.Length == 5) {
                        cShape = parts[0];
                        cSize = int.Parse(parts[1]);
                        cThick = int.Parse(parts[2]);
                        cColor = ColorTranslator.FromHtml(parts[3]);
                        isVisible = parts[4] == "1";
                        this.Invoke(new MethodInvoker(delegate { this.Invalidate(); })); // Перерисовываем прицел
                    }
                }
            } catch {}
            Application.Exit();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!isVisible) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Pen pen = new Pen(cColor, cThick);
            Brush brush = new SolidBrush(cColor);
            int cx = Width / 2, cy = Height / 2, s = cSize, h = s/2;

            if (cShape == "Dot") g.FillEllipse(brush, cx - h, cy - h, s, s);
            else if (cShape == "Cross") { g.DrawLine(pen, cx - s, cy, cx + s, cy); g.DrawLine(pen, cx, cy - s, cx, cy + s); }
            else if (cShape == "CrossDot") { g.DrawLine(pen, cx - s, cy, cx + s, cy); g.DrawLine(pen, cx, cy - s, cx, cy + s); g.FillEllipse(new SolidBrush(Color.Red), cx - 3, cy - 3, 6, 6); }
            else if (cShape == "Circle") g.DrawEllipse(pen, cx - h, cy - h, s, s);
            else if (cShape == "Т-образный") { g.DrawLine(pen, cx - s, cy, cx + s, cy); g.DrawLine(pen, cx, cy, cx, cy + s); }
        }
    }

    static class Program
    {
        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
            Application.Run(new OverlayForm());
        }
    }
}
