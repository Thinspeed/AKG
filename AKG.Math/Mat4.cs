using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKG.Math
{
	public class Mat4
	{
		private Vec4[] vectors;

		public Mat4(Vec4[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}
			if (data.Length != 4)
			{
				throw new ArgumentException($"Length of {nameof(data)} must be 4");
			}

			this.vectors = data;
		}

		public Vec4 this[int index]
		{
			private set { vectors[index] = value; }

			get => vectors[index];
		}

		public static Mat4 CreateTranslation(Vec3 vec)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(1, 0, 0, vec.X),
				new Vec4(0, 1, 0, vec.Y),
				new Vec4(0, 0, 1, vec.Z),
				new Vec4(0, 0, 0, 1)
			});
		}

		public static Mat4 CreateScale(Vec3 vec)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(vec.X, 0, 0, 0),
				new Vec4(0, vec.Y, 0, 0),
				new Vec4(0, 0, vec.Z, 0),
				new Vec4(0, 0, 0, 1)
			});
		}

		public static Mat4 CreateRotationX(double angle)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(1, 0, 0, 0),
				new Vec4(0, System.Math.Cos(angle), -System.Math.Sin(angle), 0),
				new Vec4(0, System.Math.Sin(angle), System.Math.Cos(angle), 0),
				new Vec4(0, 0, 0, 1),
			});
		}

		public static Mat4 CreateRotationY(double angle)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(System.Math.Cos(angle), 0, System.Math.Sin(angle), 0),
				new Vec4(0, 1, 0, 0),
				new Vec4(-System.Math.Sin(angle), 0, System.Math.Cos(angle), 0),
				new Vec4(0, 0, 0, 1),
			});
		}

		public static Mat4 CreateRotationZ(double angle)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(System.Math.Cos(angle), -System.Math.Sin(angle), 0, 0),
				new Vec4(System.Math.Sin(angle), System.Math.Cos(angle), 0, 0),
				new Vec4(0, 0, 1, 0),
				new Vec4(0, 0, 0, 1),
			});
		}

		public static Mat4 CreateView(Vec3 eye, Vec3 target, Vec3 up)
		{
			Vec3 zAxis = Vec3.Normalize(eye - target);
			Vec3 xAxis = Vec3.Normalize(up * zAxis);
			Vec3 yAxis = up;//zAxis * xAxis;

			return new Mat4(new Vec4[]
			{
				new Vec4(xAxis.X, xAxis.Y, xAxis.Z, -Vec3.MultiplyScalar(xAxis, eye)),
				new Vec4(yAxis.X, yAxis.Y, yAxis.Z, -Vec3.MultiplyScalar(yAxis, eye)),
				new Vec4(zAxis.X, zAxis.Y, zAxis.Z, -Vec3.MultiplyScalar(zAxis, eye)),
				new Vec4(0, 0, 0, 1)
			});

			//return new Mat4(new Vec4[]
			//{
			//	new Vec4(xAxis.X, yAxis.X, zAxis.X, 0),
			//	new Vec4(xAxis.Y, yAxis.Y, zAxis.Y, 0),
			//	new Vec4(xAxis.Z, yAxis.Z, zAxis.Z, 0),
			//	new Vec4(-Vec3.MultiplyScalar(xAxis, eye), -Vec3.MultiplyScalar(yAxis, eye), -Vec3.MultiplyScalar(zAxis, eye), 1)
			//});
		}

		public static Mat4 CreateOrtho(double width, double height, double zNear, double zFar)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(2.0 / width, 0, 0, 0),
				new Vec4(0, 2.0 / height, 0, 0),
				new Vec4(0, 0, 1.0 / (zNear - zFar), zNear / (zNear - zFar)),
				new Vec4(0, 0, 0, 1)
			});
		}

		public static Mat4 CreatePerspective(double width, double height, double zNear, double zFar)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4((2.0 * zNear) / width, 0, 0, 0),
				new Vec4(0, (2.0 * zNear) / height, 0, 0),
				new Vec4(0, 0, zFar / (zNear - zFar), -1),
				new Vec4(0, 0, (zNear * zFar) / (zNear - zFar), 0)
			});
		}

		public static Mat4 CreateViewPort(double width, double height, double xMin, double yMin)
		{
			return new Mat4(new Vec4[]
			{
				new Vec4(width / 2, 0, 0, xMin + width / 2),
				new Vec4(0, -height / 2, 0, yMin + height / 2),
				new Vec4(0, 0, 1, 0),
				new Vec4(0, 0, 0, 1)
			});
		}

		public static Vec4 operator *(Vec4 vec, Mat4 mat)
		{
			Vec4 res = new Vec4(
				vec.X * mat[0].X + vec.Y * mat[0].Y + vec.Z * mat[0].Z + vec.W * mat[0].W,
				vec.X * mat[1].X + vec.Y * mat[1].Y + vec.Z * mat[1].Z + vec.W * mat[1].W,
				vec.X * mat[2].X + vec.Y * mat[2].Y + vec.Z * mat[2].Z + vec.W * mat[2].W,
				vec.X * mat[3].X + vec.Y * mat[3].Y + vec.Z * mat[3].Z + vec.W * mat[3].W);

			return res;
		}

		public static Vec4 operator *(Mat4 mat, Vec4 vec)
		{
			return new Vec4(
				vec.X * mat[0].X + vec.Y * mat[0].Y + vec.Z * mat[0].Z + vec.W * mat[0].W,
				vec.X * mat[1].X + vec.Y * mat[1].Y + vec.Z * mat[1].Z + vec.W * mat[1].W,
				vec.X * mat[2].X + vec.Y * mat[2].Y + vec.Z * mat[2].Z + vec.W * mat[2].W,
				vec.X * mat[3].X + vec.Y * mat[3].Y + vec.Z * mat[3].Z + vec.W * mat[3].W);
		}
	}
}
