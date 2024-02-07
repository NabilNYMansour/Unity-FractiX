using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SkyboxFOVHandler : MonoBehaviour
{
    public Camera mainCam;
    private Camera skyboxCam;
    void Start()
    {
        skyboxCam = GetComponent<Camera>();
    }

    void Update()
    {
        skyboxCam.fieldOfView = mainCam.fieldOfView;
    }
}
