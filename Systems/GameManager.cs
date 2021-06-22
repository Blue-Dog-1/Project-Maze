using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using ProjectMaze.Natwork;

namespace ProjectMaze
{
    public class GameManager : MonoBehaviour
    {
        public const string TEG_PLAYER = "Player";
        public const string TEG_ENEMY = "Ememy";
        public const string TEG_DOR = "Dor";

        static public GameManager main { get; private set; }
        //static public ContactFilter2D ContactFilterEnemy => main.contactFilterEnemy;

        [SerializeField] NetworkMamager networkMamager;
        [SerializeField] Joystick m_joystick;
        [SerializeField] CountdownTimer m_CountdownTimer;
        [SerializeField] Light2D m_GlobalLight2D;

        //[SerializeField] ContactFilter2D contactFilterEnemy;

        public Joystick joystick => m_joystick;

        void Start()
        {
            main = this;
            //m_joystick.controller = playerController;
        }
        
        void Update()
        {

        }
        static public void SwitchGlobalLight(bool turn)
        {
            main.m_GlobalLight2D.enabled = turn;
        }

        static public void OnBeginGame()
        {
            main.joystick.transform.parent.gameObject.SetActive(true);
            main.networkMamager.OnBeginGame();
            
        }
        static public void OnCountdownStart()
        {
            main.m_CountdownTimer.Begin();
            main.joystick.transform.parent.gameObject.SetActive(false);
        }
        private void OnDestroy()
        {
            main = null;
        }

    }
}