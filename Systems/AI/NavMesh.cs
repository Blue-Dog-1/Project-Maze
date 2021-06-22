using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze.Navigation
{
    public class NavMesh : MonoBehaviour
    {
        [SerializeField] LayerMask m_layerMask;
        [SerializeField] float m_maxDistenceBetweenVertexs = 1;
        public List<Vertex> vertices { get; private set; } = new List<Vertex>();
        public List<Polygon> polygons { get; private set; } = new List<Polygon>();

        void Start()
        {
            GenerateNavMesh();
        }

        [ContextMenu("Generate NavMesh")]
        public void GenerateNavMesh()
        {
            vertices = new List<Vertex>();

            var dors = FindObjectsOfType<Dor>();

            // generate new Vertexs;
            for (int i = 0; i < dors.Length; i++)
            {
                if(dors[i].UnLock)
                vertices.Add( new Vertex(dors[i].position_vertex) );
            }
            // generate conection between vertexs
            for (int i = 0; i < vertices.Count; i++)
            {
                foreach (var oherVertex in vertices)
                {
                    if (oherVertex == vertices[i]) continue;
                    var diraction = oherVertex.position - vertices[i].position;
                    if(diraction.magnitude > m_maxDistenceBetweenVertexs) continue;
                    var hit = Physics2D.Raycast(vertices[i].position, diraction.normalized, m_maxDistenceBetweenVertexs, m_layerMask);
                    
                    if(hit.collider == null)
                    {
                        vertices[i].neighbors.Add(oherVertex);
                    }
                }

            }
        }

        public Vertex GetVertex(Vector2 pos)
        {
            Vertex result = vertices[0];
            float min = float.MaxValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                var distence = (vertices[i].position - pos).magnitude;
                if(min > distence)
                {
                    min = distence;
                    result = vertices[i];
                }
            }
            return result;
        }
#if UNITY_EDITOR 
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position,  transform.TransformPoint(Vector2.up * m_maxDistenceBetweenVertexs) );
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i].GizmosDraw();
            }
        }
#endif

    }
}
