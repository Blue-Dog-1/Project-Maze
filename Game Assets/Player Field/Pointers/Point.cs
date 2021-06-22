using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze
{
    public class Point : MonoBehaviour
    {
        [SerializeField] bool m_isInputOutput;
        [SerializeField] Point[] m_opposite = new Point[3];
        [SerializeField] Dor openDor;
        [SerializeField] SpriteRenderer exit;
        [SerializeField] ExitTrigger exitTrigger;

        public Point[] opposite => m_opposite;
        public ExitTrigger ExitTrigger => exitTrigger;

        public bool isInputOutput => m_isInputOutput;
        public bool isInput { get; private set; }
        public bool isOutput { get; private set; }


        void Start()
        {

        }

        public void Activate()
        {
            exit.gameObject.SetActive(true);
            openDor.Unlock();
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            if (exitTrigger)
                Gizmos.DrawLine(transform.position, exitTrigger.transform.position);
            if (openDor)
                Gizmos.DrawLine(transform.position, openDor.transform.position);
            
            Gizmos.color = Color.green;
            if (exit)
            Gizmos.DrawSphere(exit.transform.position, .2f);

        }

#endif
    }

}