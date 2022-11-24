using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using AKG.Math;

namespace AKG
{
	public struct POINT
	{
		public int X;
		public int Y;

		public POINT(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		[System.Runtime.InteropServices.DllImport("User32.dll")]
		private static extern bool SetCursorPos(int X, int Y);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(out POINT pt);

		private DispatcherTimer inputTimer;
		private bool initial = true;
		private double[,] zBuffer = new double[1080, 1920];

		private List<Polygon> model;
		private List<Vec4> positions;
		private List<Vec4> multipliedPostions;
		private List<Vec3> normals;
		private Mat4 view;
		private Mat4 projection;
		private Mat4 viewPort;
		private Mat4 rotate;
		private Vec3 cameraPos = new Vec3(0, 0, -50);	
		private Vec3 target;
		private Vec3 right;
		private Vec3 up;
		private Vec3 light = new Vec3(20, 20, -20);

		private bool isChanged = false;
		private double horizontalAngle = 0;
		private double verticalAngle = 0;
		private double mouseSpeed = 0.0001;
		private double speed = 1;

		public MainWindow()
		{
			InitializeComponent();

			model = Parser.ParserObj("D:\\1.obj");
			positions = Parser.VertexPositions;
			normals = Parser.VertexNormals;
			for (int i = 0; i < model.Count; i++)
			{
				if (model[i].Vertices.Count != 3)
				{
                    int vC = 2;
                    while (vC < model[i].Vertices.Count - 1)
					{
						var vertecies = new List<Vertex>();
						vertecies.Add(model[i].Vertices[vC]);
                        vertecies.Add(model[i].Vertices[vC + 1]);
                        vertecies.Add(model[i].Vertices[(vC + 2) % model[i].Vertices.Count]);
						model.Add(new Polygon(vertecies));
						vC++;
                    }
				}
			}

			target = new Vec3(
				System.Math.Cos(verticalAngle) * System.Math.Sin(horizontalAngle),
				System.Math.Sin(verticalAngle),
				System.Math.Cos(verticalAngle) * System.Math.Cos(horizontalAngle));

			right = new Vec3(
				System.Math.Sin(horizontalAngle - System.Math.PI / 2),
				0,
				System.Math.Cos(horizontalAngle - System.Math.PI / 2));

			up = right * target;
			view = Mat4.CreateView(cameraPos, cameraPos + target, up);
			projection = Mat4.CreatePerspective(1920f / 1080, 1, 0.5, 200);
			viewPort = Mat4.CreateViewPort(1920, 1080, 0, 0);
			rotate = Mat4.CreateRotationY(0.01);
			MultiplyPositions();

			inputTimer = new DispatcherTimer();
			inputTimer.Interval = TimeSpan.FromMilliseconds(1000 / 60);
			inputTimer.Tick += (object? sender, EventArgs e) =>
			{
				var watch = new Stopwatch();
				watch.Start();
				ProcessMouseInput();
				watch.Stop();
				double a = watch.Elapsed.TotalMilliseconds;
				watch.Restart();
				ProcessKeyboardInput();
				watch.Stop();
				double b = watch.Elapsed.TotalMilliseconds;
				watch.Restart();
				if (isChanged)
				{
					view = Mat4.CreateView(cameraPos, cameraPos + target, up);
					MultiplyPositions();
					isChanged = false;
				}

				DrawModel();
				watch.Stop();
				double c = watch.Elapsed.TotalMilliseconds;
				Console.WriteLine($"{a} {b} {c}");
			};

			inputTimer.Start();
		}

		private List<(Vec4, Vec4)> GetLines(int index1, int index2, int index3)
		{
			var list = new List<(Vec4, Vec4)>();
			var point1 = multipliedPostions[index1];
			var point2 = multipliedPostions[index2];
			var point3 = multipliedPostions[index3];
			double deltaX1 = point3.X - point1.X;
			double deltaX2 = point3.X - point2.X;
			double deltaY1 = point3.Y - point1.Y;
			double deltaY2 = point3.Y - point2.Y;
			double deltaZ1 = point3.Z - point1.Z;
			double deltaZ2 = point3.Z - point2.Z;
			int l1 = System.Math.Abs(deltaX1) > System.Math.Abs(deltaY1) ? (int)System.Math.Abs(deltaX1) : (int)System.Math.Abs(deltaY1);
			int l2 = System.Math.Abs(deltaX2) > System.Math.Abs(deltaY2) ? (int)System.Math.Abs(deltaX2) : (int)System.Math.Abs(deltaY2);
			int l = System.Math.Max(l1, l2);

			double x1 = point1.X, x2 = point2.X,
				y1 = point1.Y, y2 = point2.Y,
				z1 = point1.Z, z2 = point2.Z;

			for (int i = 0; i < l; i++)
			{
				list.Add((new Vec4(x1, y1, z1, 1), new Vec4(x2, y2, z2, 1)));
				x1 += deltaX1 / l;
				x2 += deltaX2 / l;
				y1 += deltaY1 / l;
				y2 += deltaY2 / l;
				z1 += deltaZ1 / l;
				z2 += deltaZ2 / l;
			}

			return list;
		}

		unsafe private void DrawModel()
		{
			WriteableBitmap bmp = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
			for (int y = 0; y < 1080; y++)
			{
				for (int x = 0; x < 1920; x++)
				{
					zBuffer[y, x] = -1;
				}
			}

			bmp.Lock();
			var lines = new HashSet<(int first, int second)>(new LinesEqualityComparer());
			for (int i = 0; i < model.Count; i++)
			{
                Vec3 normal = (normals[model[i].Vertices[0].Normal] + normals[model[i].Vertices[1].Normal] +
                    normals[model[i].Vertices[2].Normal]) / 3;
				
				//if (Vec3.MultiplyScalar(normal, (Vec3)cameraPos + target) < 0)
				//{
				//	continue;
				//}

                Vec4 pos = (positions[model[i].Vertices[0].Position] + positions[model[i].Vertices[1].Position] +
                    positions[model[i].Vertices[2].Position]) / 3;
                Vec3 lightDir = Vec3.Normalize(light - (Vec3)pos);
                double a = Vec3.MultiplyScalar(normal, lightDir);
               

				a = System.Math.Max(a, 0.1);
				int color = (int)(255 * a);

				Vec4 first = multipliedPostions[model[i].Vertices[0].Position];
				for (int j = 1; j < model[i].Vertices.Count; j++)
				{
					Vec4 second = multipliedPostions[model[i].Vertices[j].Position];
					if (!lines.Contains((model[i].Vertices[j - 1].Position, model[i].Vertices[j].Position)))
					{
						DrawLine(first, second, bmp, color > 0 ? (byte)color : (byte)0);
						lines.Add((model[i].Vertices[j - 1].Position, model[i].Vertices[j].Position));
					}

					first = second;
				}

				foreach (var line in GetLines(model[i].Vertices[0].Position, model[i].Vertices[1].Position, model[i].Vertices[2].Position))
				{
					DrawLine(line.Item1, line.Item2, bmp, color > 0 ? (byte)color : (byte)0);
				}
			}

			bmp.AddDirtyRect(new Int32Rect(0, 0, 1920, 1080));
			bmp.Unlock();
			Image.Source = bmp;
		}

		unsafe private void DrawLine(Vec4 point1, Vec4 point2, WriteableBitmap bmp, byte color)
		{
			double deltaX = point2.X - point1.X;
			double deltaY = point2.Y - point1.Y;
			double deltaZ = point2.Z - point1.Z;
			int l = System.Math.Abs(deltaX) > System.Math.Abs(deltaY) ? (int)System.Math.Abs(deltaX) : (int)System.Math.Abs(deltaY);
			double x = point1.X;
			double y = point1.Y;
			double z = point1.Z;
			for (int i = 0; i < l; i++)
			{
				if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
				{
					if (zBuffer[(int)y, (int)x] == -1 || zBuffer[(int)y, (int)x] > z)
					{
						byte* p = (byte*)bmp.BackBuffer + ((int)y * bmp.BackBufferStride) + ((int)x * 3);
						p[2] = color;
						zBuffer[(int)y, (int)x] = z;
					}
				}

				x += deltaX / l;
				y += deltaY / l;
				z += deltaZ / l;
			}
		}

		private void MultiplyPositions()
		{
            multipliedPostions = positions
				.Select(p =>
				{
					p = p * view * projection;
					p.X /= p.W;
					p.Z /= p.W;
					p.Y /= p.W;
					p.W = 1;
					p = p * viewPort;
					return p;
				})
				.ToList();
		}

		private void ProcessMouseInput()
		{
			if (initial)
			{
				SetCursorPos((int)(this.Width / 2), (int)(this.Height / 2));
				initial = false;
			}

			POINT mousePos;
			GetCursorPos(out mousePos);
			double deltaX = this.Width / 2 - mousePos.X;
			double deltaY = this.Height / 2 - mousePos.Y;
			if (deltaX == 0 || deltaY == 0)
			{
				return;
			}

			horizontalAngle += mouseSpeed * (this.Width / 2 - mousePos.X);
			verticalAngle += mouseSpeed * (this.Height / 2 - mousePos.Y);

			target = new Vec3(
				System.Math.Cos(verticalAngle) * System.Math.Sin(horizontalAngle),
				System.Math.Sin(verticalAngle),
				System.Math.Cos(verticalAngle) * System.Math.Cos(horizontalAngle));

			right = new Vec3(
				System.Math.Sin(horizontalAngle - System.Math.PI / 2),
				0,
				System.Math.Cos(horizontalAngle - System.Math.PI / 2));

			up = right * target;
			isChanged = true;
			SetCursorPos((int)(this.Width / 2), (int)(this.Height / 2));
		}

		private void ProcessKeyboardInput()
		{
			Vec3 deltaPos = new Vec3();
			if (Keyboard.GetKeyStates(Key.A) == KeyStates.Down)
			{
				deltaPos -= right * speed;
			}
			if (Keyboard.GetKeyStates(Key.D) == KeyStates.Down)
			{
				deltaPos += right * speed;
			}
			if (Keyboard.GetKeyStates(Key.S) == KeyStates.Down)
			{
				deltaPos -= target * speed;
			}
			if (Keyboard.GetKeyStates(Key.W) == KeyStates.Down)
			{
				deltaPos += target * speed;
			}
			if (Keyboard.GetKeyStates(Key.Escape) == KeyStates.Down)
			{
				this.Close();
			}

			if (deltaPos.X != 0 || deltaPos.Y != 0 || deltaPos.Z != 0)
			{
				cameraPos += deltaPos;
				up = right * target;
				isChanged = true;
			}
		}

		class LinesEqualityComparer : IEqualityComparer<(int, int)>
		{
			public bool Equals((int, int) x, (int, int) y)
			{
				return (x.Item1 == y.Item1 && x.Item2 == y.Item2) ||
					(x.Item1 == y.Item2 && x.Item2 == y.Item1);
			}

			public int GetHashCode([DisallowNull] (int, int) obj)
			{
				return obj.Item1+ obj.Item2;
			}
		}

	}
}
