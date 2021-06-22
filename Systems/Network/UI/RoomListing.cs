using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.EventSystems;

namespace ProjectMaze.Natwork
{
    public class RoomListing : MonoBehaviour, IPointerClickHandler
    {
        static List<RoomListing> RoomListingList;
        [SerializeField] Text m_text;
        [SerializeField] SpriteRenderer sprite;
        public RoomInfo roomInfo { get; private set; }

        public void SetRoomInfo(RoomInfo roomInfo)
        {
            m_text.text = roomInfo.PlayerCount + ", " + roomInfo.Name;
            this.roomInfo = roomInfo;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            LobbyManager.selectRoom = roomInfo;
            sprite.color = Color.red;
            for (int i = 0; i < RoomListingList.Count; i++)
            {
                RoomListingList[i].sprite.color = Color.red;
            }
            
        }
        private void OnDestroy()
        {
            RoomListingList.Remove(this);
        }

    }
}