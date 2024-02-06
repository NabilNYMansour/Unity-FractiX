using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class UICameraHandler : MonoBehaviour
{
    public Camera mainCam;
    private Camera uiCam;

    void Start()
    {
        uiCam = GetComponent<Camera>();
        uiCam.farClipPlane = mainCam.farClipPlane;
        uiCam.nearClipPlane = mainCam.nearClipPlane;
    }

    void Update()
    {
        uiCam.fieldOfView = mainCam.fieldOfView;
    }
}
