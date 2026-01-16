using System.Drawing;
using System.Drawing.Drawing2D;

namespace PerformSwitch
{
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
        
        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
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
