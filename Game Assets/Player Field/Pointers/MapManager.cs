using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectMaze.Navigation;

namespace ProjectMaze
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] NavMesh _navMesh;
        [SerializeField] Point[] pointData;

        public Point startPoint { get; private set; }
        public Point endPoint { get; private set; }
        public Point[] guardianPoint { get; private set; }
        public PoinData poinData { get; private set; }

        bool isComplited;

        public NavMesh navMesh => _navMesh;

        void Start()
        {
        }

        /// <summary>
        /// call only master client
        /// </summary>
        public PoinData GeneratePoint(int countGuardian)
        {
            Point startPoint;
            int indexStartPoint;
            do
            {
                indexStartPoint = UnityEngine.Random.Range(0, pointData.Length - 1);
                startPoint = pointData[indexStartPoint];
            }
            while (!startPoint.isInputOutput);
            this.startPoint = startPoint;
            Debug.Log("Start Point ---- " + startPoint.gameObject.name);

            int indexEndPoint = UnityEngine.Random.Range(0, startPoint.opposite.Length - 1);
            Point endPoint = startPoint.opposite[indexEndPoint];
            this.endPoint = endPoint;
            Debug.Log("End Point ---- " + endPoint.gameObject.name);

            var guardianPoint = new Point[countGuardian];
            int[] indexGuardianPoint = new int[countGuardian];
            for (int i = 0; i < countGuardian; i++)
            {
                Point point;
                int index;
                do
                {
                    index = UnityEngine.Random.Range(0, pointData.Length - 1);
                    indexGuardianPoint[i] = index;
                    point = pointData[index];
                }
                while (point == startPoint || point == endPoint || ChackPointByDistense(guardianPoint, point));

                Debug.Log("guardian Point ---- " + point.gameObject.name);

                guardianPoint[i] = point;
            }

            this.guardianPoint = guardianPoint;

            poinData = new PoinData
            {
                indexStarPoint = indexStartPoint,
                indexEndPoint = indexEndPoint,
                indexGuardian = indexGuardianPoint
            };
            
            isComplited = true;
            return poinData;
        }

        bool ChackPointByDistense(Point[] point, Point newPoint)
        {
            for (int i = 0; i < point.Length; i++)
            {
                if (point[i] == null) continue;
                var distense = (point[i].transform.position - newPoint.transform.position).magnitude;
                if(distense < 10)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// coll only clien
        /// </summary>
        public void SetPoint(PoinData poinData)
        {
            startPoint = pointData[poinData.indexStarPoint];
            endPoint = startPoint.opposite[poinData.indexEndPoint];

            var guardianPoint = new Point[poinData.indexGuardian.Length];
            for (int i = 0; i < poinData.indexGuardian.Length; i++)
            {
                guardianPoint[i] = pointData[poinData.indexGuardian[i]];
            }

            this.guardianPoint = guardianPoint;
        }

        private void OnDrawGizmosSelected()
        {
            if(isComplited)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(startPoint.transform.position, .25f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(endPoint.transform.position, .35f);
                Gizmos.color = Color.cyan;

                for (int i = 0; i < guardianPoint.Length; i++)
                {
                    Gizmos.DrawSphere(guardianPoint[i].transform.position, 0.5f);
                }
            }

        }

    }

    public struct PoinData
    {
        public int indexStarPoint;
        public int indexEndPoint;
        public int[] indexGuardian;

        public static object Deserialize(byte[] data)
        {
            PoinData result = new PoinData();

            result.indexStarPoint = BitConverter.ToInt32(data, 0);
            result.indexEndPoint = BitConverter.ToInt32(data, 4);

            List<int> indexGuardian = new List<int>();

            for (int i = 8; i < data.Length; i += 4 )
            {
                indexGuardian.Add(BitConverter.ToInt32(data, i));
            }

            result.indexGuardian = indexGuardian.ToArray();
            return result;
        }
        public static byte[] Serialize(object obj)
        {
            PoinData data = (PoinData)obj;
            byte[] result = new byte[(data.indexGuardian.Length * 4) + 8];

            BitConverter.GetBytes(data.indexStarPoint).CopyTo(result, 0);
            BitConverter.GetBytes(data.indexEndPoint).CopyTo(result, 4);

            int stepFour = 8;
            for (int i = 0; i < data.indexGuardian.Length; i++)
            {
                BitConverter.GetBytes(data.indexGuardian[i]).CopyTo(result, stepFour);
                stepFour += 4;
            }

            return result;
        }
    }
}