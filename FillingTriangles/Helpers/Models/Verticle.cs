using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace FillingTriangles
{
    public class Vertex
    {
        public Vector3D Start { get; set; }
        public Vertex Next { get; set; }
        public Vertex Prev { get; set; }

        public Vertex(Vector3D start, Vertex prev, Vertex next) 
        {
            this.Start = start;
            this.Next = next;
            this.Prev = prev;
        }
    }
}
