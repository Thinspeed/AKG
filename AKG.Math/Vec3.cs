namespace AKG.Math
{
	public class Vec3 : ICloneable
	{
		public Vec3()
			: this(0, 0, 0)
		{
		}

		public Vec3(double x, double y, double z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public double X { get; set; }
		
		public double Y { get; set; }
		
		public double Z { get; set; }

		public double Legth
		{
			get
			{
				return System.Math.Sqrt(X * X + Y * Y + Z * Z);
			}
		}

		public static double MultiplyScalar(Vec3 lvalue, Vec3 rvalue)
		{
			return lvalue.X * rvalue.X + lvalue.Y * rvalue.Y + lvalue.Z * rvalue.Z;
		}

		public static Vec3 Normalize(Vec3 vec)
		{
			return vec / vec.Legth;
		}

		public static Vec3 operator +(Vec3 lvalue, Vec3 rvalue)
		{
			return new Vec3(lvalue.X + rvalue.X, lvalue.Y + rvalue.Y, lvalue.Z + rvalue.Z);
		}

		public static Vec3 operator -(Vec3 lvalue, Vec3 rvalue)
		{
			return new Vec3(lvalue.X - rvalue.X, lvalue.Y - rvalue.Y, lvalue.Z - rvalue.Z);
		}

		public static Vec3 operator -(Vec3 vec)
		{
			return new Vec3(-vec.X, -vec.Y, -vec.Z);
		}

		public static Vec3 operator *(Vec3 lvalue, Vec3 rvalue)
		{
			return new Vec3(
				lvalue.Y * rvalue.Z - lvalue.Z * rvalue.Y,
                lvalue.Z * rvalue.X - lvalue.X * rvalue.Z,
                lvalue.X * rvalue.Y - lvalue.Y * rvalue.X);
		}

        public static Vec3 operator *(Vec3 vec, double value)
		{
			return new Vec3(vec.X * value, vec.Y * value, vec.Z * value);
		}

        public static Vec3 operator /(Vec3 vec, double devider)
		{
			return new Vec3(vec.X / devider, vec.Y / devider, vec.Z / devider);
		}

        public object Clone()
        {
            return new Vec3(X, Y, Z);
        }
    }
}