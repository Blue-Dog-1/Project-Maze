using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectMaze
{
    public class ExitTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var player = collision.GetComponent<PlayerController>();
                player?.OnExit(transform);
        }
    }
}