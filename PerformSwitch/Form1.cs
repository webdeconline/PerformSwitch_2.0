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
        private bool realExit;

        //De GUID van de power plans
        private const string BALANCED = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string HIGH = "419fb91b-9550-4aa8-8462-c04d74c03b2e";
        private const string ULTIMATE = "f4ac255a-0e98-40c5-bec0-d2b600140b2b";

        //DE STANDAARD LOGO INSTELLINGEN//
        private const int LogoTop = 55;
        private const int LogoWidth = 420;
        private const int LogoHeight = 300;
        private const int GapAfterLogo = 1;

        //breedte blijft vast , maar de hoogte kan aangepast worden aan de hand van de content
        private const int AppWidth = 360;
        private const int AppMinHeight = 600;



        //------------HIER START DE CODE VAN DE APPLICATIE------------//

        //--------De constructor--------//
        public Form1()
        {
            InitializeComponent();
            BuildUi();
            SetupTray();

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();
        }

        //------De tray setup------//
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
                if (e.Button != MouseButtons.Left) return;

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
                }
            };
        }

        //------Exit applicatie------//
        private void ExitApp()
        {
            realExit = true;
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        //------De applicatie beginnen bouwen en hoe het eruit zal zien------//
        private void BuildUi()
        {
            Text = "PerformSwitch";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // We starten met minimum size; later verhogen we hoogte indien nodig
            ClientSize = new Size(AppWidth, AppMinHeight);

            try
            {
                BackgroundImage = Image.FromFile("background.png");
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch
            {
                BackColor = Color.Black;
            }

            int y = 0;

            //---------- LOGO ----------//
            y = AddLogoAndGetBottomY();

            //Extra ruimte onder logo//
            y += GapAfterLogo;

            //----------QUICK LAUNCH----------//
            int miniW = 90, miniH = 40, gap = 12;
            int miniX = (ClientSize.Width - (miniW * 3 + gap * 2)) / 2;

            Controls.Add(MakeMiniCard("Steam", new Point(miniX, y), miniW, miniH,
                () => OpenApp("steam://open/main", @"C:\Program Files (x86)\Steam\Steam.exe")));

            Controls.Add(MakeMiniCard("Discord", new Point(miniX + miniW + gap, y), miniW, miniH,
                () => OpenApp(null, @"%LOCALAPPDATA%\Discord\Update.exe")));

            Controls.Add(MakeMiniCard("Epic", new Point(miniX + (miniW + gap) * 2, y), miniW, miniH,
                () => OpenApp(null, @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe")));

            y += miniH + 25;

            //----------PERFORMANCE CARDS----------//
            int cardW = 290, cardH = 60, cardGap = 15;
            int cardX = (ClientSize.Width - cardW) / 2;


            //De Balanced kaart//
            var c1 = new PerfCard
            {
                Title = "Balanced",
                SubTitle = "Quiet",
                Neon = Color.FromArgb(0, 255, 120),
                Location = new Point(cardX, y),
                Size = new Size(cardW, cardH),
                Cursor = Cursors.Hand
            };
            c1.Clicked += (_, __) => SetPowerPlan(BALANCED);

            //De High Performance kaart//
            var c2 = new PerfCard
            {
                Title = "High Performance",
                SubTitle = "Fast",
                Neon = Color.FromArgb(80, 170, 255),
                Location = new Point(cardX, y + cardH + cardGap),
                Size = new Size(cardW, cardH),
                Cursor = Cursors.Hand
            };
            c2.Clicked += (_, __) => SetPowerPlan(HIGH);

            //De Ultimate Performance kaart//
            var c3 = new PerfCard
            {
                Title = "Ultimate Performance",
                SubTitle = "Gaming",
                Neon = Color.FromArgb(255, 120, 0),
                Location = new Point(cardX, y + (cardH + cardGap) * 2),
                Size = new Size(cardW, cardH),
                Cursor = Cursors.Hand
            };
            c3.Clicked += (_, __) => SetPowerPlan(ULTIMATE);

            //Kaarten worden toegevoegd aan de form//
            Controls.Add(c1);
            Controls.Add(c2);
            Controls.Add(c3);
            //Y positie wordt aangepast//
            y = c3.Bottom + 18;

            //----------BRIGHTNESS----------//
            //Brightness label//
            var lbl = new Label
            {
                Text = "Brightness",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Left = (ClientSize.Width - 80) / 2,
                Top = y
            };
            Controls.Add(lbl);

            //Brightness bar//
            var bar = new BrightnessBar
            {
                Size = new Size(250, 40),
                BackColor = Color.Transparent,
                Left = (ClientSize.Width - 250) / 2,
                Top = lbl.Bottom + 8
            };
            //Verander de brightness naar de gekozen waarde//
            if (TryGetBrightness(out int b)) bar.Value = b;
            bar.BrightnessChanged += (_, val) => { try { SetBrightness(val); } catch { } };
            Controls.Add(bar);

            //----------EXIT----------//
            var exit = new PerfCard
            {
                Title = "Exit",
                Neon = Color.FromArgb(255, 120, 0),
                Size = new Size(140, 42),
                Location = new Point((ClientSize.Width - 140) / 2, bar.Bottom + 8),
                Cursor = Cursors.Hand
            };
            exit.Clicked += (_, __) => ExitApp();
            Controls.Add(exit);

            // ? Zorg dat ALLES altijd in beeld past (geen overlap + geen afknippen)
            EnsureFits(exit.Bottom + 12);

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

        // Voeg logo toe en geef de Y terug waar de content mag verdergaan.
        // Als logo te breed is, schalen we de breedte naar het scherm zodat het nooit buiten de app valt.
        private int AddLogoAndGetBottomY()
        {
            // default als logo ontbreekt
            int fallbackBottom = LogoTop;

            try
            {
                Image img = Image.FromFile("logo.png");

                // ? Nooit buiten de app: max breedte = app breedte - marge
                int maxW = ClientSize.Width - 40;
                int w = Math.Min(LogoWidth, maxW);

                // Hoogte gebruiken zoals jij instelt (mag groot zijn)
                int h = LogoHeight;

                var logo = new PictureBox
                {
                    Image = img,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(w, h),
                    BackColor = Color.Transparent,
                    Left = (ClientSize.Width - w) / 2,
                    Top = LogoTop
                };

                Controls.Add(logo);
                return logo.Bottom;
            }
            catch
            {
                return fallbackBottom;
            }
        }

        // Als content onderaan buiten de form valt, maken we de form hoger.
        private void EnsureFits(int requiredBottom)
        {
            int needed = Math.Max(AppMinHeight, requiredBottom);
            if (ClientSize.Height < needed)
                ClientSize = new Size(AppWidth, needed);
        }

        private PerfCard MakeMiniCard(string title, Point location, int w, int h, Action onClick)
        {
            var c = new PerfCard
            {
                Title = title,
                Neon = Color.FromArgb(255, 180, 0),
                Location = location,
                Size = new Size(w, h),
                Cursor = Cursors.Hand
            };
            c.Clicked += (_, __) => onClick();
            return c;
        }

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

        private void OpenApp(string? uri, string? exePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
                    return;
                }

                if (!string.IsNullOrWhiteSpace(exePath))
                {
                    exePath = Environment.ExpandEnvironmentVariables(exePath);
                    Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                }
            }
            catch
            {
                MessageBox.Show("Kon de app niet openen. Pad/URI klopt misschien niet.", "PerformSwitch");
            }
        }

        // -------- BRIGHTNESS (WMI) --------
        private bool TryGetBrightness(out int brightness)
        {
            brightness = 60;
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM WmiMonitorBrightness");
                foreach (ManagementObject obj in searcher.Get())
                {
                    brightness = Convert.ToInt32(obj["CurrentBrightness"]);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void SetBrightness(int percent)
        {
            percent = Math.Clamp(percent, 0, 100);
            using var mclass = new ManagementClass("WmiMonitorBrightnessMethods");
            mclass.Scope = new ManagementScope(@"\\.\root\wmi");
            using var instances = mclass.GetInstances();
            foreach (ManagementObject instance in instances)
                instance.InvokeMethod("WmiSetBrightness", new object[] { 1, percent });
        }
    }

    // -------- CARD --------
    public class PerfCard : Panel
    {
        public string Title { get; set; } = "";
        public string SubTitle { get; set; } = "";
        public Color Neon { get; set; } = Color.Lime;

        public event EventHandler? Clicked;

        public PerfCard()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;

            //Klikken blijft werken zoals vroeger (Clicked event)
            Click += (_, __) => Clicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //Achtergrond + border (simpel)
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using var bg = new SolidBrush(Color.FromArgb(90, 0, 0, 0));
            g.FillRounded(bg, rect, 10);

            using var pen = new Pen(Color.FromArgb(200, Neon), 2);
            g.DrawRounded(pen, rect, 10);

            //Tekst (Title/SubTitle)
            using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 9);
            using var subColor = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            if (string.IsNullOrWhiteSpace(SubTitle))
            {
                g.DrawString(Title, titleFont, Brushes.White, rect, center);
            }
            else
            {
                g.DrawString(Title, titleFont, Brushes.White, new Rectangle(0, 6, Width, 26), center);
                g.DrawString(SubTitle, subFont, subColor, new Rectangle(0, 30, Width, 18), center);
            }
        }
    }

    // -------- BRIGHTNESS BAR --------
    public class BrightnessBar : Panel
    {
        public int Value { get; set; } = 60;
        public event EventHandler<int>? BrightnessChanged;

        bool dragging;

        public BrightnessBar()
        {
            DoubleBuffered = true;
            MouseDown += (_, e) => { dragging = true; SetFromMouse(e.X); };
            MouseMove += (_, e) => { if (dragging) SetFromMouse(e.X); };
            MouseUp += (_, __) => dragging = false;
            MouseLeave += (_, __) => dragging = false;
        }

        void SetFromMouse(int mouseX)
        {
            int pad = 10;
            int w = Width - pad * 2;
            int x = Math.Clamp(mouseX - pad, 0, w);
            Value = (int)Math.Round((x / (double)w) * 100);
            Invalidate();
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
            using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString($"{Value}%", f, br, new Rectangle(0, 22, Width, 18), fmt);
        }
    }

    // -------- Rounded helpers --------
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
