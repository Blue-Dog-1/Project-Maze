using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ProjectMaze.Natwork
{
    public class NetworkMamager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static NetworkMamager main { get {
                if (_main == null)
                    _main = GameObject.FindObjectOfType<NetworkMamager>();
                return _main;
            } }
        static NetworkMamager _main;

        public static bool matchStarted { get; private set; } = false;
        [SerializeField] bool isOffline;
        [SerializeField] MapManager mapManager;
        [SerializeField] PlayerController playerPrefab;
        [SerializeField] Bot BotPrefab;
        [SerializeField] Transform Lobby;
        [SerializeField] GameObject m_light;
        [SerializeField] Pointer pointer;
        [SerializeField] bool m_randomise;
        [Header("players timeout in seconds")]
        [SerializeField] [Range(2, 200)] int _playersWaitingTimes = 5;
        PhotonView m_photonView = null;
        IClient client;

        List<IPlayer> players = new List<IPlayer>();
        public bool randomise => m_randomise;
        public Pointer Pointer { get; private set; }
        public int playerCount => players.Count;
        public void Start()
        {
            matchStarted = false;
            if (isOffline) return;

            PhotonPeer.RegisterType(typeof(PoinData), 22, PoinData.Serialize, PoinData.Deserialize);
            PhotonPeer.RegisterType(typeof(CapturePlayerData), 33, CapturePlayerData.Serialize, CapturePlayerData.Deserialize);
            
            var pos = Lobby.position;
            pos.x += Random.Range(-5f, 5f);
            pos.y += Random.Range(-5f, 5f);

            var player = PhotonNetwork.Instantiate(playerPrefab.gameObject.name, pos, Quaternion.identity);

            m_photonView = player.GetComponent<PhotonView>();

            client = (PhotonNetwork.IsMasterClient)? 
                client = new MasterClient(mapManager) : 
                client = new Client();

            if (m_photonView.IsMine)
            {
                GameManager.main.joystick.controller = PlayerController.Mine = player.GetComponent<PlayerController>();

                Instantiate(m_light, player.transform.position, Quaternion.identity, player.transform);

                PlayerController.Mine.pointer = Pointer = pointer;
                var cameratarget = CameraTarget.main.transform;
                cameratarget.parent = player.transform;
                var cameraPos = Vector3.zero;
                cameraPos.z = cameratarget.localPosition.z;
                cameratarget.localPosition = cameraPos;
                
                PlayerController.Mine.action = CatchThePlayer;
            }

            Wait(_playersWaitingTimes);
        }
        public void InstantiateBot()
        {
            var pos = Lobby.position;
            pos.x += Random.Range(-5f, 5f);
            pos.y += Random.Range(-5f, 5f);

            var botPrefab = PhotonNetwork.Instantiate(BotPrefab.gameObject.name, pos, Quaternion.identity);
            var bot = botPrefab.GetComponent<Bot>();
            bot.Initcialize(mapManager.navMesh);
        }
        public void OnDestroy()
        {
            matchStarted = false;
        }
        public void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("Player {0} entered room", newPlayer.NickName);
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("Player {0} left room", otherPlayer.NickName);
        }

        void Update()
        {
            client?.Update();
        }

        async void Wait(int second)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            await Task.Run(() => Thread.Sleep(second * 1000));
            var room = PhotonNetwork.CurrentRoom;
            int players = room.MaxPlayers - room.PlayerCount; 
            if(players > 0)
                for (int i = 0; i < players; i++)
                {
                    InstantiateBot();
                }
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case (byte)EventCode.Players:
                    OnAllPlayersConnected();
                    OnPreBeginMatch(photonEvent.CustomData);
                    break;
                case (byte)EventCode.OnStationing:
                    OnStationing(photonEvent.CustomData);
                    break;
                case (byte)EventCode.PlayerExited:
                    OnPlayerExited(photonEvent.CustomData);
                    break;
                case (byte)EventCode.PlayerCaught:
                    OnPlayerCaught(photonEvent.CustomData);
                    break;
                default:
                    break;
            }
        }
        public void AddPlayer(IPlayer player)
        {
            players.Add(player);
        }
        public IPlayer[] GetPlayers() => players.ToArray();
        void OnAllPlayersConnected()
        {
            if(!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("--------  On All Players Connected ");
            }
        }

        void OnPreBeginMatch(object obj)
        {
            byte[] valuePairs = (byte[])obj;
            Debug.Log("OnBegingGame");
            var player = players.OrderBy(p => p.ActorNumber).ToArray();

            for (int i = 0; i < player.Length; i++)
            {
                Debug.Log(valuePairs[i]);
                if (valuePairs[i] == 1)
                    player[i].MakeGuardian();
            }
          
        }

        void OnStationing(object obj)
        {
            PoinData poinData = (PoinData)obj;
            GameManager.OnCountdownStart();

            var allPlayers = this.players.OrderBy(p => p.ActorNumber).ToList();
            
            mapManager.SetPoint(poinData);
            if (!PlayerController.Mine.isGuard)
                mapManager.endPoint.Activate();

            var players = allPlayers.Where(p => !p.isGuard).ToArray();
            for (int i = 0; i < players.Length; i++)
                players[i].transform.position = mapManager.startPoint.transform.position;

            var guardians = allPlayers.Where(p => p.isGuard).ToArray();
            for (int i = 0; i < guardians.Length; i++)
                guardians[i].transform.position = mapManager.guardianPoint[i].transform.position;

            matchStarted = true;

            for (int i = 0; i < allPlayers.Count; i++)
            {
                allPlayers[i].OnBeginMatch();
            }
        }
        public void OnBeginGame()
        {
            if (!PlayerController.Mine.isGuard)
            {
                pointer.Run(mapManager.endPoint.ExitTrigger.transform);
            }
        }

        void OnPlayerExited(object obj)
        {
            int actorNumber = (int)obj;
            var player = this.players.Where(p => p.ActorNumber == actorNumber).ToList()[0];

            if(player != PlayerController.Mine)
                player.transform.gameObject.SetActive(false);

            var players = this.players.Where(p => !p.isGuard).ToArray();

            bool isAllLost = true;
            foreach (var item in players)
            {
                if (!item.isLost) isAllLost = false;
            }

            if(isAllLost)
            {
                OnMatchOver();
            }
        }
        void OnMatchOver()
        {
        }

        static public void IWentOut(int actorNumber)
        {
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(((byte)EventCode.PlayerExited), actorNumber, options, sendOptions);
        }

        void CatchThePlayer(CapturePlayerData data)
        {
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(((byte)EventCode.PlayerCaught), data, options, sendOptions);
        }

        void OnPlayerCaught(object obj)
        {
            CapturePlayerData data = (CapturePlayerData)obj;
            var guard = players.Where(p => p.ActorNumber == data.guardActorNumber).ToArray()[0];
            var player = players.Where(p => p.ActorNumber == data.playerActorNumber).ToArray()[0];
            
            Debug.LogFormat("Guard '{0}' captur — player '{1}'", guard.transform.name, player.transform.name);
            Debug.Log("Rise Event 'Cupture' ");

            guard.OnCaptureEvent();
            player.OnCaptureEvent();
        }
    }

    enum EventCode : byte
    {
        Players = 1,
        typePlayer =2,
        OnStationing = 3,
        PlayerExited = 4,
        PlayerCaught = 5,
        other = 10,
    }

    interface IClient
    {
        public void Update();
    }

    class MasterClient : IClient
    {
        bool matchHasStarted = false;
        MapManager mapManager;
        NetworkMamager networkMamager;
        public MasterClient(MapManager mapManager)
        {
            this.mapManager = mapManager;
            networkMamager = NetworkMamager.main;
        }

        public void OnAllPlayersConnected()
        {
        }
        public void Update()
        {
            if (networkMamager.playerCount == PhotonNetwork.CurrentRoom.MaxPlayers && !matchHasStarted )
                RaiseEventStart();
        }
        public async void RaiseEventStart()
        {
            matchHasStarted = true;

            await Task.Run(() => Thread.Sleep(3000) );

            var players = NetworkMamager.main.GetPlayers();
            players = players.OrderBy(p => p.ActorNumber).ToArray();
            var playersType = new byte[players.Length];

            byte[] b = MatchCreation.b;
            if (NetworkMamager.main.randomise)
                b.Shuffle();

            for (int i = 0; i < players.Length; i++)
            {
                playersType[i] = b[i];
            }

            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(((byte)EventCode.Players), playersType, options, sendOptions);

            await Task.Run(() => Thread.Sleep(1000) );


            var guard = players.Where(p => p.isGuard).ToArray();
            PoinData poinData = mapManager.GeneratePoint(guard.Length);

            PhotonNetwork.RaiseEvent(((byte)EventCode.OnStationing), poinData, options, sendOptions);
        }

    }
    class Client : IClient
    {
        public void Update()
        {

        }
    }
    public static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            System.Random random = new System.Random();

            for (int i = list.Count - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                var temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }
    }
}

