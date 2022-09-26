using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKG.Math;

namespace AKG
{
    public class VertexPosition
    {
        public VertexPosition(double x, double y, double z, double w = 1)
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

        public static explicit operator Vec4(VertexPosition vertex)
        {
            return new Vec4(vertex.X, vertex.Y, vertex.Z, vertex.W);
        }
    }
}
