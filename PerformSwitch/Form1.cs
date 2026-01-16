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

        // Power plans GUIDs
        private const string BALANCED = "381b4222-f694-41f0-9685-ff5bb260df2e";
        private const string HIGH = "419fb91b-9550-4aa8-8462-c04d74c03b2e";
        private const string ULTIMATE = "f4ac255a-0e98-40c5-bec0-d2b600140b2b";

        // Start size (kleiner dan vroeger)
        private static readonly Size StartClient = new Size(360, 780);

        // Minimum size: voorkomt scrollen (je kan niet kleiner dan dit)
        private static readonly Size MinClient = new Size(300, 760);

        // UI refs
        private PictureBox? logo;
        private PerfCard? miniSteam, miniDiscord, miniEpic;
        private PerfCard? c1, c2, c3;
        private Label? lblBrightness;
        private BrightnessBar? bar;
        private PerfCard? exit;

        public Form1()
        {
            BuildUi();
            SetupTray();
            ShowInTaskbar = true;
        }

        private void BuildUi()
        {
            Text = "PerformSwitch";
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;

            ClientSize = StartClient;
            MinimumSize = new Size(MinClient.Width + (Width - ClientSize.Width), MinClient.Height + (Height - ClientSize.Height)); // incl. borders

            try
            {
                BackgroundImage = Image.FromFile("background2.jpg");
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch
            {
                BackColor = Color.Black;
            }

            // ------- Logo -------
            logo = TryCreateLogo();
            if (logo != null) Controls.Add(logo);

            // ------- Quick launch -------
            miniSteam = MakeMiniCard("Steam", () => OpenApp("steam://open/main", @"C:\Program Files (x86)\Steam\Steam.exe"));
            miniDiscord = MakeMiniCard("Discord", () => OpenApp(null, @"%LOCALAPPDATA%\Discord\Update.exe"));
            miniEpic = MakeMiniCard("Epic", () => OpenApp(null, @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe"));

            Controls.Add(miniSteam);
            Controls.Add(miniDiscord);
            Controls.Add(miniEpic);

            // ------- Performance cards -------
            c1 = MakeCard("Balanced", "Quiet", Color.FromArgb(0, 255, 120), () => SetPowerPlan(BALANCED));
            c2 = MakeCard("High Performance", "Fast", Color.FromArgb(80, 170, 255), () => SetPowerPlan(HIGH));
            c3 = MakeCard("Ultimate Performance", "Gaming", Color.FromArgb(255, 120, 0), () => SetPowerPlan(ULTIMATE));

            Controls.Add(c1);
            Controls.Add(c2);
            Controls.Add(c3);

            // ------- Brightness -------
            lblBrightness = new Label
            {
                Text = "Brightness",
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            Controls.Add(lblBrightness);

            bar = new BrightnessBar { BackColor = Color.Transparent };
            if (TryGetBrightness(out int b)) bar.Value = b;
            bar.BrightnessChanged += (_, val) => { try { SetBrightness(val); } catch { } };
            Controls.Add(bar);

            // ------- Exit -------
            exit = new PerfCard
            {
                Title = "Exit",
                Neon = Color.FromArgb(255, 120, 0),
                Cursor = Cursors.Hand
            };
            exit.Clicked += (_, __) => ExitApp();
            Controls.Add(exit);

            // Layout: 1 functie die alles herpositioneert + resizet
            Shown += (_, __) => LayoutNow();
            Resize += (_, __) => LayoutNow();

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

        // ===================== RESPONSIVE LAYOUT (NO SCROLL) =====================
        private void LayoutNow()
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            // margins/gaps schalen een beetje mee
            int side = Clamp(w / 12, 16, 34);
            int top = Clamp(h / 18, 16, 34);
            int gap = Clamp(h / 30, 10, 18);

            // content width (cards/balk)
            int contentW = Clamp(w - side * 2, 260, 520);
            int centerX = (w - contentW) / 2;

            int y = top;

            // ---- Logo ----
            if (logo != null)
            {
                int logoH = Clamp((int)(h * 0.32), 130, 260); // kleiner bij klein venster, groter bij groot venster
                logo.SetBounds(centerX, y, contentW, logoH);
                y += logoH + gap;
            }

            // ---- Quick launch (3 mini cards) ----
            int miniGap = Clamp(w / 40, 8, 14);
            int miniH = Clamp((int)(h * 0.075), 32, 44);

            int miniW = (contentW - miniGap * 2) / 3;
            miniW = Clamp(miniW, 70, 140);

            int quickRowW = miniW * 3 + miniGap * 2;
            int quickX = (w - quickRowW) / 2;

            miniSteam?.SetBounds(quickX, y, miniW, miniH);
            miniDiscord?.SetBounds(quickX + miniW + miniGap, y, miniW, miniH);
            miniEpic?.SetBounds(quickX + (miniW + miniGap) * 2, y, miniW, miniH);

            y += miniH + gap + 4;

            // ---- Performance cards ----
            int cardH = Clamp((int)(h * 0.11), 52, 72);
            int cardGap = Clamp(h / 28, 10, 16);

            int cardW = Clamp(contentW, 260, 520);
            int cardX = (w - cardW) / 2;

            c1?.SetBounds(cardX, y, cardW, cardH);
            y += cardH + cardGap;

            c2?.SetBounds(cardX, y, cardW, cardH);
            y += cardH + cardGap;

            c3?.SetBounds(cardX, y, cardW, cardH);
            y += cardH + gap;

            // ---- Brightness ----
            if (lblBrightness != null)
            {
                lblBrightness.Top = y;
                lblBrightness.Left = (w - lblBrightness.Width) / 2;
                y = lblBrightness.Bottom + 6;
            }

            int barH = Clamp((int)(h * 0.08), 34, 44);
            int barW = Clamp(contentW - 40, 220, 520);
            int barX = (w - barW) / 2;

            bar?.SetBounds(barX, y, barW, barH);
            y += barH + gap;

            // ---- Exit ----
            int exitH = Clamp((int)(h * 0.08), 36, 46);
            int exitW = Clamp((int)(contentW * 0.55), 140, 260);
            int exitX = (w - exitW) / 2;

            exit?.SetBounds(exitX, y, exitW, exitH);
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

        // ===================== UI FACTORIES =====================
        private PictureBox? TryCreateLogo()
        {
            try
            {
                var img = Image.FromFile("logo.png");
                return new PictureBox
                {
                    Image = img,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };
            }
            catch
            {
                return null;
            }
        }

        private PerfCard MakeMiniCard(string title, Action onClick)
        {
            var c = new PerfCard
            {
                Title = title,
                Neon = Color.FromArgb(255, 180, 0),
                Cursor = Cursors.Hand
            };
            c.Clicked += (_, __) => onClick();
            return c;
        }

        private PerfCard MakeCard(string title, string sub, Color neon, Action onClick)
        {
            var c = new PerfCard
            {
                Title = title,
                SubTitle = sub,
                Neon = neon,
                Cursor = Cursors.Hand
            };
            c.Clicked += (_, __) => onClick();
            return c;
        }

        // ===================== TRAY =====================
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

        private void ExitApp()
        {
            realExit = true;
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        // ===================== LOGIC =====================
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
}
