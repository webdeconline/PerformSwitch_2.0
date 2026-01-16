using System;
using System.Drawing;
using System.Windows.Forms;

namespace PerformSwitch
{
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

            // Klikken blijft werken zoals vroeger (Clicked event)
            Click += (_, __) => Clicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Achtergrond + border (simpel)
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using var bg = new SolidBrush(Color.FromArgb(90, 0, 0, 0));
            g.FillRounded(bg, rect, 10);

            using var pen = new Pen(Color.FromArgb(200, Neon), 2);
            g.DrawRounded(pen, rect, 10);

            // Tekst (Title/SubTitle)
            using var titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 9);
            using var subColor = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
            using var center = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

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
}
