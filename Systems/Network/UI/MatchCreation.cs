using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

namespace ProjectMaze
{
    public class MatchCreation : MonoBehaviour
    {
        const byte MAXPLAYERS = 10;
        const byte MIMPLAYERS = 2;

        public readonly Dictionary<byte, string> Scene = new Dictionary<byte, string> { 
            [2] = "SCENE_3x3", [3] = "SCENE_3x3",
            [4] = "SCENE_4x4",
            [5] = "SCENE_5x5",
            [6] = "SCENE_5x6",
            [7] = "SCENE_6x6",
            [8] = "SCENE_6x7",
            [9] = "SCENE_7x7", [10] = "SCENE_7x7",
        };
        public static byte[] b { get; private set; }

        [SerializeField] GameSettings m_gameSetting;
        [SerializeField] TMP_InputField m_nameField;

        [SerializeField] Slider m_CountPlayers;
        [SerializeField] Slider m_CountRatio;
        [SerializeField] TextMeshProUGUI m_textCountAllPlayers;
        [SerializeField] TextMeshProUGUI m_textCountPlayers;
        [SerializeField] TextMeshProUGUI m_textCountGuardians;


        [SerializeField] Toggle m_isVisible;


        string nameScene;

        string nameMatch;
        public byte requiredNumberOfPlayers => (byte)m_CountRatio.value;
        public byte requiredNumberOfGuardians => (byte)(m_CountRatio.maxValue - m_CountRatio.value);
        public byte requiredNumberOfAllPlayers => (byte)(m_CountPlayers.value);

        bool nameIsTaken;
        List<RoomInfo> roomList;

        private void Start()
        {
            m_CountPlayers.maxValue = MAXPLAYERS;
            m_CountPlayers.minValue = MIMPLAYERS;
        }
        public void OnEndEditNameScene()
        {
            nameMatch = m_nameField.text;

            if (nameMatch.Length < 5)
            {
                Debug.LogFormat("Error: '{0}' too short name", nameMatch);
                return;
            }
            OnRoomListUpdate(roomList);
        }

        public void Open()
        {
            gameObject.SetActive(true);

            m_textCountPlayers.text = requiredNumberOfPlayers.ToString();
            m_textCountGuardians.text = requiredNumberOfGuardians.ToString();
        }

        public void CreateMatch()
        {
            nameScene = Scene[requiredNumberOfAllPlayers];

            OnRoomListUpdate(roomList);

            b = new byte[requiredNumberOfAllPlayers];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (i < requiredNumberOfPlayers) ? b[i] = 0 : b[i] = 1;
            }

            ExitGames.Client.Photon.Hashtable setValue = new ExitGames.Client.Photon.Hashtable();
            setValue.Add("name_scene", nameScene);

            PhotonNetwork.CreateRoom(nameMatch, new RoomOptions { MaxPlayers = requiredNumberOfAllPlayers, CustomRoomProperties = setValue, IsVisible = m_isVisible.isOn });
            gameObject.SetActive(false);
        }
        public void QueckCreateMatch(int countPlayer)
        {
            OnRoomListUpdate(roomList);
            b = new byte[countPlayer];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = (i < 1) ? b[i] = 0 : b[i] = 1;
            }
            
            ExitGames.Client.Photon.Hashtable setValue = new ExitGames.Client.Photon.Hashtable();
            setValue.Add("name_scene", Scene[2]);
            PhotonNetwork.CreateRoom(nameMatch, new RoomOptions { MaxPlayers = (byte)countPlayer, CustomRoomProperties = setValue, IsVisible = m_isVisible.isOn });
            gameObject.SetActive(false);
        }
        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var room in roomList)
            {
                if(room.Name == nameMatch)
                {
                    nameIsTaken = false;
                    Debug.LogError(string.Format("Name match '{0}' is TAKEN", nameMatch) );
                    break;
                }
            }
            this.roomList = roomList;
        }

        public void OnChangeSilderPlayers()
        {
            m_CountRatio.maxValue = Mathf.Clamp(m_CountPlayers.value, 1, MAXPLAYERS - 1);

            m_textCountAllPlayers.text = m_CountPlayers.value.ToString();
            OnChangeSilderRatio();
        }

        public void OnChangeSilderRatio()
        {
            m_CountRatio.value = Mathf.Clamp(m_CountRatio.value, 1, m_CountRatio.maxValue - 1);

            m_textCountPlayers.text = m_CountRatio.value.ToString();
            m_textCountGuardians.text = (m_CountRatio.maxValue - m_CountRatio.value).ToString();
        }



    }
}