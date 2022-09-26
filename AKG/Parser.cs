using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using AKG.Math;

namespace AKG
{
    public static class Parser
    {
        public static List<Vec4> VertexPositions = new List<Vec4>();
        public static List<Vec3> VertexTexture = new List<Vec3>();
        public static List<Vec3> VertexNormals = new List<Vec3>();

        public static List<Polygon> ParserObj(string path)
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException($"File {path} does not exist", nameof(path));
            }

            var polygons = new List<Polygon>();
            using FileStream stream = fileInfo.OpenRead();
            using StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts[0] == "v")
                {
                    VertexPositions.Add(ParsePosition(parts));
                }
                else if (parts[0] == "vt")
                {
                    VertexTexture.Add(ParseTexture(parts));
                }
                else if (parts[0] == "vn")
                {
                    VertexNormals.Add(ParseNormal(parts));
                }
                else if (parts[0] == "f")
                {
                    var vertecies = new List<Vertex>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        (int pos, int tex, int norm) indecies = ParseVertex(parts[i]);
                        vertecies.Add(new Vertex(
                            indecies.pos - 1,
                            indecies.tex == 0 ? new Vec3() : VertexTexture[indecies.tex - 1],
                            indecies.norm == 0 ? new Vec3() : VertexNormals[indecies.norm - 1]));
                    }

                    polygons.Add(new Polygon(vertecies));
                }
            }

            return polygons;
        }

        private static Vec4 ParsePosition(string[] data)
        {
            if (data.Length == 4)
            {
                return new Vec4(
                    double.Parse(data[1], CultureInfo.InvariantCulture),
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture),
                    1);
            }
            else if (data.Length == 5)
            {
                return new Vec4(
                    double.Parse(data[1], CultureInfo.InvariantCulture),
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture),
                    double.Parse(data[4], CultureInfo.InvariantCulture));
            }
            else
            {
                throw new Exception("Invalid data");
            }
        }

        private static Vec3 ParseTexture(string[] data)
        {
            if (data.Length == 2)
            {
                return new Vec3(double.Parse(data[1], CultureInfo.InvariantCulture), 0, 0);
            }
            else if (data.Length == 3)
            {
                return new Vec3(double.Parse(data[1], CultureInfo.InvariantCulture), double.Parse(data[2], CultureInfo.InvariantCulture), 0);
            }
            else if (data.Length == 4)
            {
                return new Vec3(
                    double.Parse(data[1], CultureInfo.InvariantCulture),
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture));
            }
            else
            {
                throw new Exception("Invalid data");
            }
        }

        private static Vec3 ParseNormal(string[] data)
        {
            if (data.Length != 4)
            {
                throw new Exception("Invalid data");
            }

            return new Vec3(
                    double.Parse(data[1], CultureInfo.InvariantCulture),
                    double.Parse(data[2], CultureInfo.InvariantCulture),
                    double.Parse(data[3], CultureInfo.InvariantCulture));
        }

        private static (int pos, int tex, int norm) ParseVertex(string data)
        {
            string[] parts = data.Split('/');
            if (parts.Length == 1)
            {
                return (int.Parse(parts[0]), 0, 0);
            }
            else if (parts.Length == 2)
            {
                return (int.Parse(parts[0]), int.Parse(parts[1]), 0);
            }
            else if (parts.Length == 3)
            {
                return (
                    int.Parse(parts[0]),
                    parts[1] == string.Empty ? 0 : int.Parse(parts[1]),
                    int.Parse(parts[2]));
            }
            else
            {
                throw new Exception("Invalid data");
            }
        }
    }
}
