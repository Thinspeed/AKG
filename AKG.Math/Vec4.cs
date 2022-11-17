using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKG.Math
{
	public class Vec4 : ICloneable
	{
		public Vec4()
			: this(0, 0, 0, 0)
		{
		}

		public Vec4(double x, double y, double z, double w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}

		public double X { get; set; }

		public double Y { get; set; }

		public double Z { get; set; }

		public double W { get; set; }

        public static Vec4 operator +(Vec4 lvalue, Vec4 rvalue)
        {
            return new Vec4(lvalue.X + rvalue.X, lvalue.Y + rvalue.Y, lvalue.Z + rvalue.Z, lvalue.W + rvalue.W);
        }

        public static Vec4 operator +(Vec4 vec, double denominator)
        {
            return new Vec4(vec.X / denominator, vec.Y / denominator, vec.Z / denominator, vec.W / denominator);
        }

        public object Clone()
		{
			return new Vec4(X, Y, Z, W);
		}
	}
}
