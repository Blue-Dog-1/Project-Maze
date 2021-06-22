using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using ProjectMaze.Navigation;
using System;

namespace ProjectMaze
{
    [System.Serializable]
    public class AI
    {
        public int _radius = 10;
        public Transform _transform;
        public LayerMask _layerMask;
        public State state;

        Bot _bot { get; set; }
        Vector2 position2 => _transform.position;

        Vertex navVertex;
        Vector2 targetPos = Vector2.zero;
        Transform pleyar;
        public Target positionTraget { get; set; }

        bool isSee;
        public NavMesh navMesh { get; set; }
        public AI(Transform transform, LayerMask layerMask, int radius, Bot bot, NavMesh navMesh)
        {
            _transform = transform;
            _layerMask = layerMask;
            _radius = radius;
            _bot = bot;
            this.navMesh = navMesh;
        }
        public void Start()
        {
            navVertex = navMesh.GetVertex(_transform.position);
            targetPos = navVertex.position;
            
            state = new Idel(navVertex, targetPos, _transform);
        }
        public void Update()
        {
            var pos = (Vector2)_transform.position;
            if (true)
            {
                float minDistence = (pos - navVertex.position).magnitude;
                Vertex bufer = navVertex;
                for (int i = 0; i < navVertex.neighbors.Count; i++)
                {
                    var distence = (pos - navVertex.neighbors[i].position).magnitude;
                    if (distence < minDistence)
                    {
                        bufer = navVertex.neighbors[i];
                        minDistence = distence;
                    }
                }
                navVertex = bufer;
            }

            if (pleyar != null)
                See();
            _bot.Move(state.Move(pleyar.position));

        }
        public void ToLook(Transform transform)
        {
            pleyar = transform;
        }
       
        void See()
        {
            var result = Physics2D.Raycast(_transform.position, (pleyar.position - _transform.position).normalized, 100, _layerMask);

            if (result.collider.gameObject.tag == GameManager.TEG_PLAYER)
            {
                ImSeePlayer();
                return;
            }
            else if (isSee)
            {
                isSee = false;
                targetPos = navVertex.position;
            }
        }

        void ImSeePlayer()
        {
            Debug.Log("See Plaer!!");
            targetPos = pleyar.position;
            isSee = true;
            Go(pleyar.position);
        }

        public void Go(Vector2 target)
        {
            var pos = position2;
            float min = (target - pos).magnitude;
            targetPos = target;
            for (int i = 0; i < navVertex.neighbors.Count; i++)
            {
                var distence = (navVertex.neighbors[i].position - target).magnitude;
                if (distence < min)
                {
                    min = distence;
                    targetPos = navVertex.neighbors[i].position;
                }
            }
            state.targetPos = targetPos;
            state = new Follow(state);
        }
        
        public Vector2 Move(Vector2 target)
        {
            #region legasy code
            //var pos = position2;
            //float distenceToTarget = (target - pos).magnitude;
            //if (distenceToTarget < .1f)
            //{
            //    go = false;
            //    return Vector2.zero;
            //}
            //if ( (targetPos - pos).magnitude < .1f)
            //{
            //    float min = distenceToTarget;
            //    targetPos = target;
            //    for (int i = 0; i < navVertex.neighbors.Count; i++)
            //    {
            //        var distence = (navVertex.neighbors[i].position - target).magnitude;
            //        if(distence < min )
            //        {
            //            min = distence;
            //            targetPos = navVertex.neighbors[i].position;
            //        }
            //    }
            //}

            //Vector2 vector = targetPos - pos; 
            //return vector;
            #endregion

            return state.Move(target);
        }

        public void DorTriger(Dor dor)
        {
            Debug.Log("triger " + dor.gameObject.name);
        }
        public void Hide(Vector2 point)
        {
            positionTraget = new Target(point);

            state.targetPos = targetPos = point;
            
            state = new Hide(state);
        }
       
        public void DorListener(Dor dor) 
        {
            dor.ChangeSpin -= DorListener;
            state = new Search(state);
        }

       


#if UNITY_EDITOR
        public void DrawGizmos()
        {
            Gizmos.color = Color.red;
            if (navVertex != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(navVertex.position, .4f);
                
                foreach (var item in navVertex.neighbors)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(item.position, .3f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(navVertex.position, item.position);
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(targetPos, .2f);
        }
#endif

    }

    public abstract class State
    {
        public State nextState;
        public Vector2 targetPos;
        public Vertex navVertex;
        public Transform transform;
        protected Vector2 position2 => transform.position;
        public abstract Vector2 Move(Vector2 vector);

        public State(Vertex navVertex, Vector2 targetPos, Transform transform)
        {
            this.navVertex = navVertex;
            this.targetPos = targetPos;
            this.transform = transform;
        }

        public State(State _state)
        {
            targetPos = _state.targetPos;
            navVertex = _state.navVertex;
            transform = _state.transform;
        }
        
    }

    public class Idel : State
    {
        public Idel(Vertex navVertex, Vector2 targetPos, Transform transform) : base(navVertex, targetPos, transform) { }
        public Idel(State state) : base(state) { }
        public override Vector2 Move(Vector2 vector)
        {
            Debug.Log("State — Idel");
            return Vector2.zero;
        }
       
    }

    public class Follow : State
    {
        public Follow(Vertex navVertex, Vector2 targetPos, Transform transform) : base(navVertex, targetPos, transform) { }
        public Follow(State state) : base(state) { }

        public override Vector2 Move(Vector2 target)
        {
            Debug.Log("State — Follow");

            var pos = position2;
            float distenceToTarget = (target - pos).magnitude;
            if (distenceToTarget < .1f)
            {
                return Vector2.zero;
            }
            if ((targetPos - pos).magnitude < .1f)
            {
                float min = distenceToTarget;
                targetPos = target;
                for (int i = 0; i < navVertex.neighbors.Count; i++)
                {
                    var distence = (navVertex.neighbors[i].position - target).magnitude;
                    if (distence < min)
                    {
                        min = distence;
                        targetPos = navVertex.neighbors[i].position;
                    }
                }
            }

            Vector2 vector = targetPos - pos;
            return vector;
        }

    }

    public class Hide : State
    {
        
        public Hide(Vertex navVertex, Vector2 targetPos, Transform transform) : base(navVertex, targetPos, transform)
        {}
        public Hide(State state) : base(state) 
        {
            nextState = state;
        }
        public override Vector2 Move(Vector2 target)
        {
            Debug.Log("State — Hiden");

            var pos = position2;
            float distenceToTarget = (targetPos - pos).magnitude;
            if (distenceToTarget < .1f)
            {
                nextState = new Idel(nextState);
                return Vector2.zero;
            }

            Vector2 vector = targetPos - pos;
            return vector;
        }
       
    }

    public class Search : State
    {
        public Search(Vertex navVertex, Vector2 targetPos, Transform transform) : base(navVertex, targetPos, transform) { }
        public Search(State state) : base(state) { }

        public override Vector2 Move(Vector2 vector)
        {
            Debug.Log("State — Search");

            return Vector2.zero;
        }
      
    }


    public class Target
    {
        public virtual Vector2 position { get; protected set; }
        public Target(Vector2 pos)
        {
            position = pos;
        }
    }

    public class TargetTranform : Target
    {
        Transform transform;
        public override Vector2 position => transform.position;
        public TargetTranform(Transform transform) : base(transform.position)
        {
            this.transform = transform;
        }

    }

}
