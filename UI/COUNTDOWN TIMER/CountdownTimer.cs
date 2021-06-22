using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectMaze
{
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] [Range(3, 10)] byte timer = 3;
        public void AnimationEvent()
        {
            timer -= 1;
            text.text = timer.ToString();

            if (timer > 0) return;

            transform.parent.gameObject.SetActive(false);
            GameManager.OnBeginGame();
        }

        public void Begin()
        {
            transform.parent.gameObject.SetActive(true);
        }
    }
}