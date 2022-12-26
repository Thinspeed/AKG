using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKG.Math;

namespace AKG
{
	public class Model
	{
		public List<Polygon>? Polygons { get; set; }

		public Bitmap? Diffuse { get; set; }

		public Bitmap? NormalMap { get; set; }

        public Bitmap? MRAOMap { get; set; }

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

		public Vec3 GetNormal(double u, double v)
		{
			if (NormalMap == null)
			{
				return new Vec3();
			}

			int x = (int)(NormalMap.Width * u);
			int y = (int)(NormalMap.Height * (1 - v));
			Color temp = NormalMap.GetPixel(x, y);

			return new Vec3((double)temp.R * 2 / 255 - 1, (double)temp.G * 2 / 255 - 1, (double)temp.B * 2 / 255 - 1);
		}

		public double GetSpecularK(double u, double v)
		{
			if (MRAOMap == null)
			{
				return 0;
			}

            int x = (int)(MRAOMap.Width * u);
            int y = (int)(MRAOMap.Height * (1 - v));
            Color temp = MRAOMap.GetPixel(x, y);

			return (1 - (double)temp.G / 255);
        }
	}
}
