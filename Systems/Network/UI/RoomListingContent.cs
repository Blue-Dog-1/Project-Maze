using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
namespace ProjectMaze.Natwork
{
    public class RoomListingContent : MonoBehaviour
    {
        [SerializeField] RoomListing m_roomListing;


        public void SetRoomInfo(RoomInfo roomInfo)
        {
            RoomListing listing = Instantiate(m_roomListing, transform);
            if (listing != null)
            {
                listing.SetRoomInfo(roomInfo);
            }
        }
        public void Clear()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

    }

}
