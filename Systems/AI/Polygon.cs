using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze.Navigation
{
    public class Polygon
    {
        public Vector2 position;
        public List<Vertex> vertices { get; protected set; } = new List<Vertex>();
    }
}
