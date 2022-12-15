using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AKG.Math;

namespace AKG
{
    public class Vertex
    {
        public Vertex(int position, int texture, int normal)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
        }

        /// <summary>
        /// Returns index of position
        /// </summary>
        public int Position { get; set; }

        public int Texture { get; set; }

        public int Normal { get; set; }
    }
}
