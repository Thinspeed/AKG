using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKG
{
    public class Model
    {
        public List<Polygon>? Polygons { get; set; }

        public Bitmap? Diffuse { get; set; }

        public BColor GetPixelColor(double u, double v)
        {
            if (Diffuse == null)
            {
                return new BColor();
            }

            int x = (int)(Diffuse.Width * u);
            int y = (int)(Diffuse.Height * (1 - v));
            Color temp = Diffuse.GetPixel(x, y);

            return new BColor(temp.R, temp.G, temp.B);
        }
    }
}
