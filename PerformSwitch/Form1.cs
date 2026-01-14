using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace PerformSwitch
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private bool realExit = false;

        // Power plans
        private const string BALANCED = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string HIGH = "419fb91b-9550-4aa8-8462-c04d74c03b2e";
        private const string ULTIMATE = "f4ac255a-0e98-40c5-bec0-d2b600140b2b";

        public Form1()
        {
            InitializeComponent();
            BuildUi();
            SetupTray();

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        private void SetupTray()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (_, __) => ExitApp());

            trayIcon = new NotifyIcon
            {
                Icon = new Icon("PFS.ico"),
                Text = "PerformSwitch",
                Visible = true,
                ContextMenuStrip = menu
            };

            trayIcon.MouseUp += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (Visible)
                    {
                        Hide();
                        ShowInTaskbar = false;
                    }
                    else
                    {
                        ShowInTaskbar = true;
                        WindowState = FormWindowState.Normal;
                        Show();
                        Activate();
                        BringToFront();
                    }
                }
            };
        }

        //druk op rechtermuisklik voor exit
        private void ExitApp()
        {
            realExit = true;

            trayIcon.Visible = false;
            trayIcon.Dispose();

            Application.Exit();
        }

        //----------------APP BUILDING----------------//
        private void BuildUi()
        {
            Text = "PerformSwitch";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(360, 600);

            try
            {
                BackgroundImage = Image.FromFile("background.png");
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { BackColor = Color.Black; }

            //de powerplan knoppen maken
            int x = 35, w = 290, h = 60;

            var c1 = new PerfCard { Title = "Balanced", SubTitle = "Quiet", Neon = Color.FromArgb(0, 255, 120), Location = new Point(x, 230), Size = new Size(w, h), Cursor = Cursors.Hand };
            c1.Clicked += (_, __) => SetPowerPlan(BALANCED);

            var c2 = new PerfCard { Title = "High Performance", SubTitle = "Fast", Neon = Color.FromArgb(80, 170, 255), Location = new Point(x, 305), Size = new Size(w, h), Cursor = Cursors.Hand };
            c2.Clicked += (_, __) => SetPowerPlan(HIGH);

            var c3 = new PerfCard { Title = "Ultimate Performance", SubTitle = "Gaming", Neon = Color.FromArgb(255, 120, 0), Location = new Point(x, 380), Size = new Size(w, h), Cursor = Cursors.Hand };
            c3.Clicked += (_, __) => SetPowerPlan(ULTIMATE);

            //knoppen toevoegen
            Controls.Add(c1);
            Controls.Add(c2);
            Controls.Add(c3);

            //Helderheid slider
            Controls.Add(new Label
            {
                Text = "Brightness",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(140, 455)
            });
            //slider zelf van de helderheid
            var bar = new BrightnessBar
            {
                Location = new Point(55, 480),
                Size = new Size(250, 40),
                BackColor = Color.Transparent
            };

            if (TryGetBrightness(out int b)) bar.Value = b;

            bar.BrightnessChanged += (_, val) =>
            {
                try { SetBrightness(val); } catch { }
            };

            Controls.Add(bar);

            //knop voor programma helemaal afsluiten
            var exit = new PerfCard
            {
                Title = "Exit",
                SubTitle = "",
                Neon = Color.FromArgb(255, 120, 0),
                Location = new Point(110, 525),
                Size = new Size(140, 42),
                Cursor = Cursors.Hand
            };
            exit.Clicked += (_, __) => ExitApp();
            Controls.Add(exit);

            //als je op het kruisje drukt dan  minimaliseer je naar tray
            FormClosing += (_, e) =>
            {
                if (!realExit)
                {
                    e.Cancel = true;
                    Hide();
                    ShowInTaskbar = false;
                }
            };
        }

        //de powerplan instellen
        private void SetPowerPlan(string guid)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = $"/setactive {guid}",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }



        //---------------------HELDERHEID---------------------//
        //de huidige helderheid opvragen
        private bool TryGetBrightness(out int brightness)
        {
            //standaardwaarde
            brightness = 60;
            try
            {
                //WMI query om de helderheid op te vragen
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorBrightness");
                //de resultaten doorlopen
                foreach (ManagementObject obj in searcher.Get())
                {
                    //de helderheid uitlezen
                    brightness = Convert.ToInt32(obj["CurrentBrightness"]);
                    //succesvol, true teruggeven
                    return true;
                }
            }
            catch { }
            //als er iets misgaat, false teruggeven
            return false;
        }
        //de helderheid instellen
        private void SetBrightness(int percent)
        {
            //waarde beperken tussen 0 en 100
            percent = Math.Clamp(percent, 0, 100);
            //wmi class gebruiken om de helderheid in te stellen
            using var mclass = new ManagementClass("WmiMonitorBrightnessMethods");
            //de scope instellen
            mclass.Scope = new ManagementScope(@"\\.\root\wmi");
            //de instanties ophalen
            using var instances = mclass.GetInstances();
            //voor elke instantie de helderheid instellen
            foreach (ManagementObject instance in instances)
                //de methode aanroepen om de helderheid in te stellen
                instance.InvokeMethod("WmiSetBrightness", new object[] { 1, percent });
        }
    }

    //----------------BUTTONS----------------//
    public class PerfCard : Panel
    {
        //eigenschappen voor de titel, subtitel en neon kleur van de knop
        public string Title { get; set; } = "";
        public string SubTitle { get; set; } = "";
        public Color Neon { get; set; } = Color.Lime;
        public event EventHandler? Clicked;

        //interne status voor hoveren en indrukken
        bool hovered, pressed;
        //constructor voor de knop
        public PerfCard()
        {
            //dubbel bufferen en transparante achtergrond
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            //muisevents afhandelen
            MouseEnter += (_, __) => { hovered = true; Invalidate(); };
            MouseLeave += (_, __) => { hovered = false; pressed = false; Invalidate(); };
            MouseDown += (_, __) => { pressed = true; Invalidate(); };
            MouseUp += (_, __) => { if (pressed) { pressed = false; Invalidate(); Clicked?.Invoke(this, EventArgs.Empty); } };
        }
        //override de paint methode om de knop te tekenen
        protected override void OnPaint(PaintEventArgs e)
        {
            //basis paint aanroepen
            base.OnPaint(e);
            //graphics object ophalen
            var g = e.Graphics;
            //antialiasing inschakelen
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //achtergrond tekenen met variabele alpha afhankelijk van hover/pressed status
            int a = pressed ? 130 : hovered ? 110 : 85;
            //teken de achtergrond en afgeronde rand
            using var bg = new SolidBrush(Color.FromArgb(a, 0, 0, 0));
            g.FillRounded(bg, new Rectangle(0, 0, Width - 1, Height - 1), 10);
            //teken de rand en de neon stip
            using var pen = new Pen(Color.FromArgb(200, Neon), hovered ? 2 : 1);
            g.DrawRounded(pen, new Rectangle(0, 0, Width - 1, Height - 1), 10);
            //neon stip en de kleur ervan
            using var dot = new SolidBrush(Color.FromArgb(230, Neon));
            g.FillEllipse(dot, 14, (Height / 2) - 7, 14, 14);
            //titels tekenen en overlay bij indrukken
            using var titleFont = new Font("Segoe UI", 12, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 9);
            //de kleur voor de subtitel met wat transparantie ervoor 
            using var subColor = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            //titel tekenen in wit 
            g.DrawString(Title, titleFont, Brushes.White, new Rectangle(0, 6, Width, 28), center);
            //subtitel tekenen in lichtere kleur als die bestaat 
            if (!string.IsNullOrWhiteSpace(SubTitle))
                g.DrawString(SubTitle, subFont, subColor, new Rectangle(0, 28, Width, 26), center);
            //overlay tekenen als de knop ingedrukt is 
            if (pressed)
            {
                using var overlay = new SolidBrush(Color.FromArgb(35, 255, 255, 255));
                g.FillRounded(overlay, new Rectangle(1, 1, Width - 3, Height - 3), 10);
            }
        }
    }

    //----------------HELDERHEIDBAR----------------//
    //custom slider voor helderheid
    public class BrightnessBar : Panel
    {
        //huidige waarde van de helderheid
        public int Value { get; set; } = 60;
        //event voor als de helderheid verandert
        public event EventHandler<int>? BrightnessChanged;
        //interne status voor slepen van de slider 
        bool dragging;
        //constructor voor de slider 
        public BrightnessBar()
        {
            //dubbel bufferen inschakelen en muisevents afhandelen
            DoubleBuffered = true;
            MouseDown += (_, e) => { dragging = true; SetFromMouse(e.X); };
            MouseMove += (_, e) => { if (dragging) SetFromMouse(e.X); };
            MouseUp += (_, __) => dragging = false;
            MouseLeave += (_, __) => dragging = false;
        }

        //waarde instellen op basis van muispositie en event triggeren
        void SetFromMouse(int mouseX)
        {
            //de waarde berekenen op basis van muispositie
            int pad = 10;
            //breedte zonder padding
            int w = Width - pad * 2;
            //x positie binnen de slider berekenen
            int x = Math.Clamp(mouseX - pad, 0, w);
            //waarde berekenen als percentage van de breedte
            Value = (int)Math.Round((x / (double)w) * 100);
            //hertekenen en event triggeren als de waarde verandert
            Invalidate();
            //event aanroepen als er een listener is
            BrightnessChanged?.Invoke(this, Value);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var track = new Rectangle(10, 10, Width - 20, 8);
            using var trackBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
            g.FillRounded(trackBrush, track, 6);

            int fillW = (int)(track.Width * (Value / 100.0));
            var fillRect = new Rectangle(track.X, track.Y, Math.Max(0, fillW), track.Height);
            using var fillBrush = new SolidBrush(Color.FromArgb(220, 255, 180, 0));
            g.FillRounded(fillBrush, fillRect, 6);

            int knobX = Math.Clamp(track.X + fillW, track.X, track.Right);
            using var knobBrush = new SolidBrush(Color.FromArgb(230, 255, 180, 0));
            g.FillEllipse(knobBrush, knobX - 7, track.Y - 6, 14, 14);

            using var f = new Font("Segoe UI", 9, FontStyle.Bold);
            using var br = new SolidBrush(Color.White);
            var s = $"{Value}%";
            var rect = new Rectangle(0, 22, Width, 18);
            using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(s, f, br, rect, fmt);
        }
    }

    //----------------BUTTON AFRONDING----------------//
    public static class GfxExt
    {
        public static void FillRounded(this Graphics g, Brush b, Rectangle r, int radius)
        {
            using var path = RoundedRect(r, radius);
            g.FillPath(b, path);
        }

        public static void DrawRounded(this Graphics g, Pen p, Rectangle r, int radius)
        {
            using var path = RoundedRect(r, radius);
            g.DrawPath(p, path);
        }

        static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
