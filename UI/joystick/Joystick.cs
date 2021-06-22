using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectMaze
{
    [AddComponentMenu("Project/UI/Joystick")]
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] [Range(30, 150)] int m_stickLimit;
        public IController controller { get; set; } = new GagController();

        RectTransform stickTransform;
        Vector2 diraction;
        void Start()
        {
            stickTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            controller.Move(stickTransform.localPosition - Vector3.zero * Time.deltaTime);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            stickTransform.localPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            stickTransform.position = eventData.position;
            stickTransform.localPosition = Vector3.ClampMagnitude(stickTransform.localPosition - Vector3.zero, m_stickLimit);

            diraction = (stickTransform.localPosition - Vector3.zero).normalized;
            controller.Rotate(diraction);
        }
    }
}