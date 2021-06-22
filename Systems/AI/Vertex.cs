using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze.Navigation
{
    public class Vertex
    {
        public Vector2 position;
        public List<Vertex> neighbors { get; protected set; } = new List<Vertex>();

        public Vertex(Vector2 pos)
        {
            position = pos;
        }

#if UNITY_EDITOR
        public void GizmosDraw()
        {
            for (int i = 0; i < neighbors.Count; i++)
            {
                Gizmos.DrawLine(position, neighbors[i].position);
            }
        }
#endif
    }
}