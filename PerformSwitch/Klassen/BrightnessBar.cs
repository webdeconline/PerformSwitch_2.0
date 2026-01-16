using System;
using System.Drawing;
using System.Windows.Forms;

namespace PerformSwitch
{
    public class BrightnessBar : Panel
    {
        public int Value { get; set; } = 100;
        public event EventHandler<int>? BrightnessChanged;

        private bool dragging;

        public BrightnessBar()
        {
            DoubleBuffered = true;

            MouseDown += (_, e) => { dragging = true; SetFromMouse(e.X); };
            MouseMove += (_, e) => { if (dragging) SetFromMouse(e.X); };
            MouseUp += (_, __) => dragging = false;
            MouseLeave += (_, __) => dragging = false;
        }

        private void SetFromMouse(int mouseX)
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
            using var fmt = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            g.DrawString($"{Value}%", f, br, new Rectangle(0, 22, Width, 18), fmt);
        }
    }
}
