using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Project/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] byte m_GameVersion = 1;

        public byte gameVersion => m_GameVersion;
    }
}
