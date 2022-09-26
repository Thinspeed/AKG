using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AKG.Math;

namespace AKG
{
    public class Polygon
    {
        public Polygon(List<Vertex> vertices)
        {
            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }

            this.Vertices = vertices;
        }

        public List<Vertex> Vertices { get; private set; }

        public int Count { get => Vertices.Count; }

        public Vertex this[int index]
        {
            set 
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                Vertices[index] = value;
            }

            get => Vertices[index];
        }

        //public void Transform(Mat4 mat)
        //{
        //    for (int i = 0; i < Vertices.Count; i++)
        //    {
        //        Vertices[i].Position = Vertices[i].Position * mat;
        //    }
        //}
    }
}
