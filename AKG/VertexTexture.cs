using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKG.Math;

namespace AKG
{
    public class VertexTexture
    {
        public VertexTexture(double u, double v, double w)
        {
            this.U = u;
            this.V = v;
            this.W = w;
        }

        public double U { get; set; }

        public double V { get; set; }

        public double W { get; set; }

        public static explicit operator Vec3(VertexTexture vertex)
        {
            return new Vec3(vertex.U, vertex.W, vertex.W);
        }
    }
}
