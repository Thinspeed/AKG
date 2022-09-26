using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKG.Math;

namespace AKG
{
    public class VertexNormal
    {
        public VertexNormal(double i, double j, double k)
        {
            this.I = i;
            this.J = j;
            this.K = k;
        }

        public double I { get; set; }

        public double J { get; set; }

        public double K { get; set; }

        public static explicit operator Vec3(VertexNormal normal)
        {
            return new Vec3(normal.I, normal.J, normal.K);
        }
    }
}
