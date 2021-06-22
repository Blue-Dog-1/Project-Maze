using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace ProjectMaze.Natwork
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameSettings m_GameSettings;
        [SerializeField] InputField inputName;
        [SerializeField] MatchCreation m_matchCreation;
        [SerializeField] RoomListingContent m_content;
        [SerializeField] [Range(1, 15)] byte m_requiredNumberOfPlayers = 2;
        public Text logText;
        public byte requiredNumberOfPlayers { get; private set; }

        public RoomInfo[] roomList { get; private set; }
        public static RoomInfo selectRoom { get; set; }
        void Start()
        {
            PhotonNetwork.NickName = "Player" + Random.Range(0, 9999);
            Log("Player's name is set to " + PhotonNetwork.NickName);
            PhotonNetwork.GameVersion = m_GameSettings.gameVersion.ToString();
            PhotonNetwork.ConnectUsingSettings();
            
            requiredNumberOfPlayers = m_requiredNumberOfPlayers;
        }
        public override void OnConnectedToMaster()
        {
            Log("Connected to Master");
            PhotonNetwork.JoinLobby();
        }
       
        public void PLAY()
        {
            if (roomList.Length == 0)
            {
                m_matchCreation.QueckCreateMatch(2);
                return;
            }
            JoinRoom();
        }
        /// <summary>
        /// will connect to the most crowded room
        /// </summary>
        public void JoinRoom()
        {
            int max = 0;
            RoomInfo roomInfo = roomList[0];
            foreach (var room in roomList)
            {
                if(room.PlayerCount > max)
                {
                    max = room.PlayerCount;
                    roomInfo = room;
                }
            }
            PhotonNetwork.JoinRoom(roomInfo.Name);
        
        }
        static public void JoinRoom(string nameRoom)
        {
            PhotonNetwork.JoinRoom(nameRoom);
        }
        public override void OnJoinedRoom()
        {
            Log("Joined the room");

            string nameScene = (string)PhotonNetwork.CurrentRoom.CustomProperties["name_scene"];

            PhotonNetwork.LoadLevel(nameScene);

        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            m_content.Clear();
            foreach (RoomInfo info in roomList)
            {
                if(info.PlayerCount > 0)
                m_content.SetRoomInfo(info);
            }

            m_matchCreation.OnRoomListUpdate(roomList);
            this.roomList = roomList.ToArray();
        }

        private void Log(string messenge)
        {
            Debug.Log(messenge);
            logText.text += "\n";
            logText.text += messenge;
        }


        public void InputField(string nickname)
        {
            nickname = inputName.text;
            if (nickname.Length < 5)
            {
                Log("not corect name");
                return;
            }
            Debug.Log(nickname);
            Debug.LogFormat("Change name {0} => {1}", PhotonNetwork.NickName, nickname);
            PhotonNetwork.NickName = nickname;

            Log(string.Format("Player's name is set to {0} " , nickname) );
        }

        public void QuitApplication()
        {
            Application.Quit();
        }

        public void OnApplicationQuit()
        {
            
        }
        private void Update()
        {
            if(Input.GetKey(KeyCode.Tilde))
            {
                OpenConsole();
            }

        }
        void OpenConsole()
        {
            if(Debug.isDebugBuild)
            {
                Debug.LogError("OPEN CONSOLE");
            }
        }
    }
}