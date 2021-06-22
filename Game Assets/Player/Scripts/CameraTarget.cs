using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    static CameraTarget m_main;
    public static CameraTarget main { get {
            if (m_main == null)
                m_main = GameObject.FindObjectOfType<CameraTarget>();
            return m_main;
        } }

    [SerializeField] [Range(1, 10)] int m_cameraVelocity = 6;

    Transform tranform_camera = null;
    private void Start()
    {
        tranform_camera = Camera.main.transform;
    }
    void Update()
    {
        var pos = Vector3.Lerp(tranform_camera.position, transform.position, m_cameraVelocity * Time.deltaTime);
        tranform_camera.position = pos;
    }
}
