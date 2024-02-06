using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class LightCameraHandler : MonoBehaviour

{
    public Shader shader;
    private Camera cam;
    private Material renderMat;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        GL.invertCulling = true;
        if (cam == null) cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.Depth;

        if (renderMat == null) renderMat = new Material(shader);
        else Graphics.Blit(source, destination);

        Graphics.Blit(source, destination, renderMat);
        GL.invertCulling = false;
    }
}
