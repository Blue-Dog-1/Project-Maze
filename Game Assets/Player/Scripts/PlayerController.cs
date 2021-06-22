using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using ProjectMaze.Natwork;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMaze
{
    [AddComponentMenu("Project/PlayerController")]
    public class PlayerController : MonoBehaviour, IPlayer, IPunObservable
    {
        static public PlayerController Mine { get; set; }
        [SerializeField] TextMeshPro nickname;

        [SerializeField] SpriteRenderer skin;
        [SerializeField] Sprite skinGuard;
        [SerializeField] Sprite skinPlayer;

        [SerializeField] [Range(0, 1)] float velocity = .3f;
        [SerializeField] Rigidbody2D m_rigidbody;
        public PhotonView photonView { get; set; }
        public System.Action<CapturePlayerData> action;
        public Pointer pointer { get; set; }
        public bool isGuard { get; set; } = false;


        Vector2 vector;

        public bool isLost { get; set; }
        public int ActorNumber { get; set; }

        Camera camera;
        void Start()
        {
            photonView = GetComponent<PhotonView>();
            name = nickname.text = photonView.Owner.NickName;
            if (!photonView.IsMine)
            {
                nickname.color = Color.red;
                ActorNumber = photonView.Owner.ActorNumber;
            }
            else
            {
                nickname.color = Color.yellow;
                Mine = this;
            }

            NetworkMamager.main.AddPlayer(this);

            camera = Camera.main;
        }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isGuard);
            }
            else
            {
                isGuard = (bool)stream.ReceiveNext();
            }
        }

        void Update()
        {
            if (photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    isLost = true;
            }
        }

        public void Move(Vector2 diraction)
        {
            vector = (diraction * velocity) * Time.deltaTime;
            m_rigidbody.MovePosition(m_rigidbody.position + vector);
        }

        public void Rotate(Vector2 diraction)
        {
            var angle =  Vector3.Angle(diraction, Vector3.up);
            angle = Vector3.Cross(diraction, Vector3.up).z < 0? angle : -angle;
            skin.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        public void EndControl()
        {
            if (!photonView.IsMine) return;
            CameraTarget.main.transform.parent = null;
            cameraZoom();
            GameManager.main.joystick.controller = new PosController(CameraTarget.main.transform, velocity);
            GameManager.SwitchGlobalLight(true);
        }

        public void MakeGuardian()
        {
            isGuard = true;
            skin.sprite = skinGuard; 
        }
        public void MakePlayer()
        {
            isGuard = false;
            skin.sprite = skinPlayer;
        }
        public void OnBeginMatch()
        {
            nickname.gameObject.SetActive(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!NetworkMamager.matchStarted || !(photonView?.IsMine ?? false) ) return;

            var otherPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (otherPlayer == null) return;
            

            if (isGuard && !otherPlayer.isGuard)
            {
                action?.Invoke(new CapturePlayerData
                {
                    guardActorNumber = photonView.Owner.ActorNumber,
                    playerActorNumber = otherPlayer.photonView.Owner.ActorNumber
                });
            }

            if (otherPlayer.isGuard)
                OnCaught();
        }

        public void OnCaptureEvent()
        {
            OnCaught();
        }
        void OnCaught()
        {
            if(isGuard)
            {
                return;
            }

            isLost = true;
            Debug.Log(photonView.Owner.NickName + "is Lost ");

            skin.gameObject.SetActive(false);
            transform.gameObject.SetActive(false);

            if (photonView.IsMine)
            {
                Debug.Log("I Capture ");
                
                EndControl();
                var players = FindObjectsOfType<PlayerController>();
                foreach (var player in players)
                {
                    player.nickname.gameObject.SetActive(true);
                }

            }
        }
        
        async void cameraZoom()
        {
            while (Camera.main.orthographicSize < 10 - .5f)
            {
                camera.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 10, .03f);
                await Task.Run(() => Thread.Sleep(33));
            }
        }

        public void OnExit(Transform exitTriger)
        {
            if (!photonView.IsMine) return;

            var pos = exitTriger.position;
            pos.z = transform.position.z;
            transform.position = pos;
            EndControl();
            gameObject.SetActive(false);
            NetworkMamager.IWentOut(photonView.Owner.ActorNumber);
        }

        private void OnDestroy()
        {
            Mine = null;
        }
    }

    public interface IPlayer : IController
    {
        public int ActorNumber { get; set; }
        public Transform transform { get; }
        public PhotonView photonView { get; set; }
        public bool isGuard { get; set; }
        public bool isLost { get; set; }

        public void MakeGuardian();
        public void OnBeginMatch();
        public void OnCaptureEvent();
    }
    public interface IController
    {
        void Move(Vector2 localPos);
        void Rotate(Vector2 vector);
        void EndControl();
    }

    class GagController : IPlayer
    {
        public int ActorNumber { get; set; }
        public Transform transform { get; }
        public PhotonView photonView { get; set; }
        public bool isGuard { get; set; } = false;
        public bool isLost { get; set; }

        public void EndControl(){}
        public void Move(Vector2 localPos){}
        public void Rotate(Vector2 vector){}
        public void MakeGuardian() { }
        public void OnBeginMatch() { }
        public void OnCaptureEvent() { }

    }
    class PosController : IPlayer
    {
        public int ActorNumber { get; set; }
        float velocity;

        public PosController(Transform transform, float velocity)
        {
            this.transform = transform;
            this.velocity = velocity;
        }
        public Transform transform { get; }

        public PhotonView photonView { get; set; }
        public bool isGuard { get; set; } = false;
        public bool isLost { get; set; }

        public void EndControl() { }
        public void Move(Vector2 diraction) 
        {
            transform.Translate((diraction * velocity) * Time.deltaTime);
        }
        public void Rotate(Vector2 vector) { }
        public void MakeGuardian() { }
        public void OnBeginMatch() { }
        public void OnCaptureEvent() { }

    }


    public struct CapturePlayerData
    {
        public int guardActorNumber;
        public int playerActorNumber;

        public static object Deserialize(byte[] data) => new CapturePlayerData
        {
            guardActorNumber = data[0],
            playerActorNumber = data[4]
        };

        public static byte[] Serialize(object obj)
        {
            CapturePlayerData data = (CapturePlayerData)obj;
            byte[] result = new byte[8];
            BitConverter.GetBytes(data.guardActorNumber).CopyTo(result, 0);
            BitConverter.GetBytes(data.playerActorNumber).CopyTo(result, 4);
            return result;
        }

    }
}

