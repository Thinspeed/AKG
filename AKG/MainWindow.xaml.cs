using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
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
using System.Windows.Media.TextFormatting;
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

		private Model model;
		private List<Vec4> positions;
		private List<Vec4> multipliedPostions;
		private List<Vec3> normals;
		private List<Vec3> multipliedNormals;
		private List<Vec3> textures;
		private Mat4 view;
		private Mat4 projection;
		private Mat4 viewPort;
		private Mat4 rotate;
		private Vec3 cameraPos = new Vec3(0, 50, 100);	
		private Vec3 target;
		private Vec3 right;
		private Vec3 up;
		private Vec3 light => new Vec3(0, 100, 100);

        private const double ambientLightK = 0.1;
        private BColor ambientLightColor = new BColor(255, 255, 255) * ambientLightK;
		private BColor specularColor = new BColor(255, 255, 255);

        private bool isChanged = false;
		private double horizontalAngle = System.Math.PI;
		private double verticalAngle = 0;
		private double mouseSpeed = 0.0001;
		private double speed = 1;

		public MainWindow()
		{
			InitializeComponent();

            //D:\\Models\\Intergalactic Spaceship\\
            model = new Model()
			{
				Polygons = Parser.ParserObj("D:\\Models\\Pink Soldier\\Model.obj"),
				Diffuse = new System.Drawing.Bitmap("D:\\Models\\Pink Soldier\\BaseColor Map.png"),
				NormalMap = new System.Drawing.Bitmap("D:\\Models\\Pink Soldier\\Normal Map.png"),
            };

			positions = Parser.VertexPositions;
			normals = Parser.VertexNormals;
			textures = Parser.VertexTexture;
			for (int i = 0; i < model.Polygons.Count; i++)
			{
				if (model.Polygons[i].Vertices.Count != 3)
				{
                    int vC = 2;
                    while (vC < model.Polygons[i].Vertices.Count - 1)
					{
						var vertecies = new List<Vertex>();
						vertecies.Add(model.Polygons[i].Vertices[vC]);
                        vertecies.Add(model.Polygons[i].Vertices[vC + 1]);
                        vertecies.Add(model.Polygons[i].Vertices[(vC + 2) % model.Polygons[i].Vertices.Count]);
						model.Polygons.Add(new Polygon(vertecies));
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
				ProcessMouseInput();
				ProcessKeyboardInput();
				if (isChanged)
				{
					view = Mat4.CreateView(cameraPos, cameraPos + target, up);
					MultiplyPositions();
					isChanged = false;
				}

				DrawModel();
			};

			inputTimer.Start();
		}

		private (List<(Vec4, Vec4)> pos, List<(Vec4, Vec4)> ver, List<(Vec3, Vec3)> norm, List<(Vec3, Vec3)> texture) GetLines(
			int index1, int index2, int index3,
			int indexN1, int indexN2, int indexN3,
			int indexT1, int indexT2, int indexT3)
		{
			var p = new List<(Vec4, Vec4)>();
			var v = new List<(Vec4, Vec4)>();
			var n = new List<(Vec3, Vec3)>();
			var t = new List<(Vec3, Vec3)>();

			var point1 = multipliedPostions[index1];
			var point2 = multipliedPostions[index2];
			var point3 = multipliedPostions[index3];

			var vertex1 = positions[index1];
			var vertex2 = positions[index2];
			var vertex3 = positions[index3];

			var normal1 = normals[indexN1];
			var normal2 = normals[indexN2];
			var normal3 = normals[indexN3];

			var texture1 = textures[indexT1];
            var texture2 = textures[indexT2];
            var texture3 = textures[indexT3];

            double deltaX1 = point3.X - point1.X;
			double deltaX2 = point3.X - point2.X;
			double deltaY1 = point3.Y - point1.Y;
			double deltaY2 = point3.Y - point2.Y;
			double deltaZ1 = point3.Z - point1.Z;
			double deltaZ2 = point3.Z - point2.Z;

            double deltaVX1 = vertex3.X - vertex1.X;
            double deltaVX2 = vertex3.X - vertex2.X;
            double deltaVY1 = vertex3.Y - vertex1.Y;
            double deltaVY2 = vertex3.Y - vertex2.Y;
            double deltaVZ1 = vertex3.Z - vertex1.Z;
            double deltaVZ2 = vertex3.Z - vertex2.Z;

            double deltaNX1 = normal3.X - normal1.X;
			double deltaNX2 = normal3.X - normal2.X;
			double deltaNY1 = normal3.Y - normal1.Y;
            double deltaNY2 = normal3.Y - normal2.Y;
            double deltaNZ1 = normal3.Z - normal1.Z;
            double deltaNZ2 = normal3.Z - normal2.Z;

            double deltaTX1 = texture3.X - texture1.X;
            double deltaTX2 = texture3.X - texture2.X;
            double deltaTY1 = texture3.Y - texture1.Y;
            double deltaTY2 = texture3.Y - texture2.Y;

            int l1 = System.Math.Abs(deltaX1) > System.Math.Abs(deltaY1) ? (int)System.Math.Abs(deltaX1) : (int)System.Math.Abs(deltaY1);
			int l2 = System.Math.Abs(deltaX2) > System.Math.Abs(deltaY2) ? (int)System.Math.Abs(deltaX2) : (int)System.Math.Abs(deltaY2);
			int l = System.Math.Max(l1, l2);

			double x1 = point1.X, x2 = point2.X,
				y1 = point1.Y, y2 = point2.Y,
				z1 = point1.Z, z2 = point2.Z,
				vx1 = vertex1.X, vx2 = vertex2.X,
				vy1 = vertex1.Y, vy2 = vertex2.Y,
				vz1 = vertex1.Z, vz2 = vertex2.Z,
				nx1 = normal1.X, nx2 = normal2.X,
				ny1 = normal1.Y, ny2 = normal2.Y,
				nz1 = normal1.Z, nz2 = normal2.Z,
				tx1 = texture1.X, tx2 = texture2.X,
				ty1 = texture1.Y, ty2 = texture2.Y;

            for (int i = 0; i < l; i++)
			{
				p.Add((new Vec4(x1, y1, z1, 1), new Vec4(x2, y2, z2, 1)));
				v.Add((new Vec4(vx1, vy1, vz1, 1), new Vec4(vx2, vy2, vz2, 1)));
				n.Add((new Vec3(nx1, ny1, nz1), new Vec3(nx2, ny2, nz2)));
				t.Add((new Vec3(tx1, ty1, 0), new Vec3(tx2, ty2, 0)));

				x1 += deltaX1 / l;
				x2 += deltaX2 / l;
				y1 += deltaY1 / l;
				y2 += deltaY2 / l;
				z1 += deltaZ1 / l;
				z2 += deltaZ2 / l;

                vx1 += deltaVX1 / l;
                vx2 += deltaVX2 / l;
                vy1 += deltaVY1 / l;
                vy2 += deltaVY2 / l;
                vz1 += deltaVZ1 / l;
                vz2 += deltaVZ2 / l;

                tx1 += deltaTX1 / l;
                tx2 += deltaTX2 / l;
                ty1 += deltaTY1 / l;
                ty2 += deltaTY2 / l;
            }

			return (p, v, n, t);
		}

		unsafe private void DrawModel()
		{
			if (model is null || model.Polygons is null)
			{
				return;
			}

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
			for (int i = 0; i < model.Polygons.Count; i++)
			{
                Vec3 normal = (normals[model.Polygons[i].Vertices[0].Normal] + normals[model.Polygons[i].Vertices[1].Normal] +
                    normals[model.Polygons[i].Vertices[2].Normal]) / 3;
                Vec4 pos = (positions[model.Polygons[i].Vertices[0].Position] + positions[model.Polygons[i].Vertices[1].Position] +
                    positions[model.Polygons[i].Vertices[2].Position]) / 3;

				if (Vec3.MultiplyScalar(Vec3.Normalize(normal), Vec3.Normalize(cameraPos - (Vec3)pos)) > 0)
				{
                    List<(Vec4, Vec4)> p;
                    List<(Vec4, Vec4)> v;
                    List<(Vec3, Vec3)> n;
					List<(Vec3, Vec3)> t;

                    (p, v, n, t) = GetLines(model.Polygons[i].Vertices[0].Position, model.Polygons[i].Vertices[1].Position, model.Polygons[i].Vertices[2].Position,
                        model.Polygons[i].Vertices[0].Normal, model.Polygons[i].Vertices[1].Normal, model.Polygons[i].Vertices[2].Normal,
                        model.Polygons[i].Vertices[0].Texture, model.Polygons[i].Vertices[1].Texture, model.Polygons[i].Vertices[2].Texture);

                    for (int j = 0; j < p.Count; j++)
                    {
                        DrawLine(bmp, p[j].Item1, p[j].Item2, v[j].Item1, v[j].Item2, n[j].Item1, n[j].Item2, t[j].Item1, t[j].Item2);
                    }
                }
			}

			bmp.AddDirtyRect(new Int32Rect(0, 0, 1920, 1080));
			bmp.Unlock();
			Image.Source = bmp;
		}

		unsafe private void DrawLine(WriteableBitmap bmp,
			Vec4 point1, Vec4 point2,
			Vec4 vertex1, Vec4 vertex2,
			Vec3 normal1, Vec3 normal2,
			Vec3 texture1, Vec3 texture2)
		{
			double deltaX = point2.X - point1.X;
			double deltaY = point2.Y - point1.Y;
			double deltaZ = point2.Z - point1.Z;

            double deltaVX = vertex2.X - vertex1.X;
            double deltaVY = vertex2.Y - vertex1.Y;
            double deltaVZ = vertex2.Z - vertex1.Z;

            double deltaNX = normal2.X - normal1.X;
            double deltaNY = normal2.Y - normal1.Y;
            double deltaNZ = normal2.Z - normal1.Z;

            double deltaTX = texture2.X - texture1.X;
            double deltaTY = texture2.Y - texture1.Y;

            int l = System.Math.Abs(deltaX) > System.Math.Abs(deltaY) ? (int)System.Math.Abs(deltaX) : (int)System.Math.Abs(deltaY);
			double x = point1.X, y = point1.Y, z = point1.Z;
			Vec4 v = vertex1;
			Vec3 n = normal1;
			Vec3 t = texture1;

            for (int i = 0; i < l; i++)
			{
				n = model.GetNormal(t.X, t.Y);
                Vec3 lightDir = light - (Vec3)v;
                double a = Vec3.MultiplyScalar(n, Vec3.Normalize(lightDir));
                a = System.Math.Max(a, 0);
                
				Vec3 r = 2 * Vec3.Normalize(n) * Vec3.MultiplyScalar(lightDir, Vec3.Normalize(n)) - lightDir;
				double b = Vec3.MultiplyScalar(Vec3.Normalize(r), Vec3.Normalize(cameraPos + target - (Vec3)v));
                BColor iS = specularColor * System.Math.Pow(System.Math.Max(b, 0), 2);
                BColor color = ambientLightColor + (model.GetPixelColor(t.X, t.Y) * a) + iS;

                if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
				{
					if (zBuffer[(int)y, (int)x] == -1 || zBuffer[(int)y, (int)x] > z)
					{
						byte* p = (byte*)bmp.BackBuffer + ((int)y * bmp.BackBufferStride) + ((int)x * 3);
						p[2] = color.Red; p[1] = color.Green; p[0] = color.Blue;
						zBuffer[(int)y, (int)x] = z;
					}
				}

				x += deltaX / l;
				y += deltaY / l;
				z += deltaZ / l;

                v.X += deltaVX / l;
                v.Y += deltaVY / l;
                v.Z += deltaVZ / l;

                n.X += deltaNX / l;
				n.Y += deltaNY / l;
				n.Z += deltaNZ / l;

				t.X += deltaTX / l;
				t.Y += deltaTY / l;
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
