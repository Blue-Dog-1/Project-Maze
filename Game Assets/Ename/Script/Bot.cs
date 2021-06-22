using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectMaze.Natwork;
using ProjectMaze.Navigation;

namespace ProjectMaze
{
    public class Bot : MonoBehaviour, IPlayer
    {
        [SerializeField] [Range(5, 50)] int _radiuse = 10;
        [SerializeField] LayerMask _layerMaskAI;
        [SerializeField] [Range(0, 10)] float velocity = 1;
        [SerializeField] LayerMask _IgnorLayerMask;
        public int ActorNumber { get; set; }

        Rigidbody2D rigidbody = null;
        public AI ai { get; set; }
        public bool isGuard { get; set; }

        public PhotonView photonView { get; set; }
        public bool isLost { get; set; }
        bool isActive = false;

        public void Initcialize(NavMesh navMesh)
        {
            ai = new AI(transform, _layerMaskAI, _radiuse, this, navMesh);
        }

        public void Start()
        {
            ActorNumber = NetworkMamager.main.GetPlayers().Length;
            NetworkMamager.main.AddPlayer(this);
            isGuard = true;
            ai.Start();
            rigidbody = GetComponent<Rigidbody2D>();
        }
        void Update()
        {
            if(isActive)
            ai.Update();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == GameManager.TEG_PLAYER) {
                ai.ToLook(collision.transform);
                return;
            }

        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == GameManager.TEG_PLAYER)
                ai.ToLook(null);
        }
        
        public void Move(Vector2 diraction)
        {
            var vector = (diraction * velocity) * Time.deltaTime;

            rigidbody.MovePosition(rigidbody.position + vector);

            Vector2 diff = vector.normalized;
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }

        public void Rotate(Vector2 vector)
        {
        }

        public void EndControl()
        {
        }
        public void DorTriget(Dor dor)
        {
            var vector = (dor.position_vertex - (Vector2)transform.position);
            
            Debug.DrawRay(transform.position, vector, Color.red);
            
            var hit = Physics2D.Raycast((Vector2)transform.position, vector, 1000, _IgnorLayerMask);
            if (hit.collider.gameObject == dor.gameObject)
            {
                var position = ((dor.position_PointRight - (Vector2)transform.position).magnitude <
                    (dor.position_PointLeft - (Vector2)transform.position).magnitude) ? dor.position_PointRight : dor.position_PointLeft;
                ai.Hide(position);
                dor.ChangeSpin += ai.DorListener;
            }
        }
        public void MakeGuardian() { }
        public void OnBeginMatch() 
        {
            Debug.LogFormat("bot '{0}' is active ", name);
            isActive = true;
        }
        public void OnCaptureEvent()
        {
            throw new System.NotImplementedException();
        }
#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            ai?.DrawGizmos();
        }
#endif
    }

}
