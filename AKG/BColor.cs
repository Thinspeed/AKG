using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AKG
{
	public struct BColor
	{
		public BColor()
			: this(0, 0, 0) { }

		public BColor(int red, int green, int blue)
		{
			if (red < 0)
			{
				this.Red = 0;
			}
			else if (red > 255)
			{
				this.Red = 255;
			}
			else
			{
				this.Red = (byte)red;
			}

            if (green < 0)
            {
                this.Green = 0;
            }
            else if (green > 255)
            {
                this.Green = 255;
            }
            else
            {
                this.Green = (byte)green;
            }

            if (blue < 0)
            {
                this.Blue = 0;
            }
            else if (blue > 255)
            {
                this.Blue = 255;
            }
            else
            {
                this.Blue = (byte)blue;
            }
        }

		public BColor(byte red, byte green, byte blue)
		{
			this.Red = red;
			this.Green = green;
			this.Blue = blue;
		}

		public byte Red { get; set; }

		public byte Green { get; set; }

		public byte Blue { get; set; }

		public static BColor operator *(BColor color, double k)
		{
			return new BColor((byte)(color.Red * k), (byte)(color.Green * k), (byte)(color.Blue * k));
		}

		public static BColor operator +(BColor lvalue, BColor rvalue)
		{
			int r = lvalue.Red + rvalue.Red;
			int g = lvalue.Green + rvalue.Green;
			int b = lvalue.Blue + rvalue.Blue;

			return new BColor(r < 256 ? (byte)r : (byte)255, g < 256 ? (byte)g : (byte)255, b < 256 ? (byte)b : (byte)255);
		}
	}
}
