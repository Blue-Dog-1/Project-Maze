using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze
{
    public class Pointer : MonoBehaviour
    {
        [SerializeField] Transform view;
        public Transform target { get; set; }
        void Start()
        {

        }

        public void Run(Transform target)
        {
            this.target = target;
            gameObject.SetActive(true);
            view.transform.parent.gameObject.SetActive(true);
        }

        void FixedUpdate()
        {
            if (target)
            {
                var vector = transform.position - target.position;
                var angle = Vector3.Angle(vector, Vector3.up);
                angle = Vector3.Cross(vector, Vector3.up).z < 0 ? angle : -angle;
                view.eulerAngles = new Vector3(0, 0, angle);
            }
        }
    }
}