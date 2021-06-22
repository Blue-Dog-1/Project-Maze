using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProjectMaze
{
    public class Dor : MonoBehaviour
    {
        [SerializeField] Vector2 m_position_vertex;
        [SerializeField] Vector2 m_position_PointLeft;
        [SerializeField] Vector2 m_position_PointRight;

        [SerializeField] bool m_Unlock = true;

        public Vector2 position_vertex => transform.TransformPoint(m_position_vertex);
        public Vector2 position_PointLeft => m_position_PointLeft;
        public Vector2 position_PointRight => m_position_PointRight;
        

        public bool UnLock => m_Unlock;
        public Action<Dor> CollisonDor;

        Rigidbody2D rigidbody;
        float angle;
        int m_spin;
        public int spin { get => (angle - transform.localEulerAngles.z > 0)? 1 : 0; }

        public event Action<Dor> ChangeSpin;
        private void Start()
        {
            var enemys = FindObjectsOfType<Bot>();
            for (int i = 0; i < enemys.Length; i++)
                CollisonDor += enemys[i].DorTriget;

            m_position_PointLeft = transform.TransformPoint(m_position_PointLeft);
            m_position_PointRight = transform.TransformPoint(m_position_PointRight);

            rigidbody = GetComponent<Rigidbody2D>();
            angle = transform.localEulerAngles.z;

            if(!m_Unlock)
            {
                rigidbody.bodyType = RigidbodyType2D.Static;
            }

#if UNITY_EDITOR
            gameStart = true;
#endif
        }
        private void FixedUpdate()
        {
            var _angle = angle - transform.localEulerAngles.z;
            int _spin = (_angle > 0) ? 1 : 0;
            if (_spin != m_spin) ChangeSpin?.Invoke(this);
            m_spin = _spin;
            
            rigidbody.AddTorque(_angle * Time.fixedDeltaTime * 10);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if(collision.gameObject.tag == GameManager.TEG_PLAYER)
                CollisonDor?.Invoke(this);
        }

        public void Unlock()
        {
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }

#if UNITY_EDITOR
        [SerializeField] [Range(0, .5f)] float radiuseGizmosSphere = .5f;
        bool gameStart;
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(position_vertex, radiuseGizmosSphere);
            if (!gameStart)
            {
                Gizmos.DrawSphere(transform.TransformPoint(m_position_PointLeft), radiuseGizmosSphere);
                Gizmos.DrawSphere(transform.TransformPoint(m_position_PointRight), radiuseGizmosSphere);
            }
            else
            {
                Gizmos.DrawSphere(m_position_PointLeft, radiuseGizmosSphere);
                Gizmos.DrawSphere(m_position_PointRight, radiuseGizmosSphere);
            }
        }
#endif

    }
}
