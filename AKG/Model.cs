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

        public System.Windows.Media.Color GetPixelColor(double u, double v)
        {
            if (Diffuse == null)
            {
                return new System.Windows.Media.Color();
            }

            int x = (int)(Diffuse.Width * u);
            int y = (int)(Diffuse.Height * (1 - v));
            Color temp = Diffuse.GetPixel(x, y);

            return new System.Windows.Media.Color()
            {
                R = temp.R,
                G = temp.G,
                B = temp.B
            };
        }
    }
}
