using System.Linq;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class CameraCompShaderSetter : MonoBehaviour
{
    //======|Public params|======//
    [Header("Compute Shaders")]
    public ComputeShader rayMarchShader; // see "Shaders/RayCompShader.compute"
    public ComputeShader coneMarchShader; // see "Shaders/ConeCompShader.compute"
    public ComputeShader fxaaShader; // see "Shaders/FXAACompShader.compute"
    public ComputeShader depthBlurShader; // see "Shaders/DepthBlurCompShader.compute"
    public ComputeShader vfxShader; // see "Shaders/VFXCompShader.compute"

    [Header("Cameras")]
    public Light dirLight;
    public Camera lightCam;
    public Camera uiCam;
    public Camera skyboxCam;

    [Header("Raymarch Sky Colors")]
    public Color fogColorGround;
    public Color fogColorSky;
    public Color sunColor;

    [Header("Raymarch Params")]
    public float ShadowDetail = 15f;
    public float MaxShadowSteps = 64;
    public float RayMarchDetail = 1f;

    [Header("Cone Params")]
    public float[] subdivisions = { 4, 8, 16, 32, 64 };

    [Header("Raymarch and Conemarch Params")]
    public float MaxSteps = 512;
    public float MaxDis = 2000;
    public float HitEps = 0.001f;
    public float SlopeEps = 0.01f;

    [Header("Visual Settings")]
    public bool shadowsOn = true;
    public bool ambientOccOn = true;
    public bool fxaaOn = true;
    public bool depthBlurOn = true;
    [Range(1f, 10f)]
    public int depthBlurRadius = 3;

    [Header("Scene number")]
    public int scene = 0; // for changes in the scene

    //======|RenderTextures|======//
    private RenderTexture target;
    private RenderTexture DepthBuffer;
    private RenderTexture skyboxRender;
    private RenderTexture uiRender;
    private RenderTexture[] coneMarchDataInArr;
    private RenderTexture[] coneMarchDataOutArr;

    //======|This object's camera component|======//
    private Camera cam;

    //======|Static cone marching passes pixel divisions|======//
    private static float[] conePassPixelDiv = { 32, 16, 8, 4, 2 }; // NOTE: must have the same number of divisions as subdivisions.

    //======|Comp shaders work group numbers|======//
    private int shaderThreadGroupsX;
    private int shaderThreadGroupsY;


    /// <summary>
    /// Initializes a 2D render texture given width, height and format of the desired texture.
    /// </summary>
    RenderTexture initBuffer(float width, float height, RenderTextureFormat format) // float input to handle cone marching divisions
    {
        RenderTexture data = new RenderTexture((int)width, (int)height, 0, format, RenderTextureReadWrite.Linear);
        data.enableRandomWrite = true;
        data.Create();
        return data;
    }

    /// <summary>
    /// Cone marching compute shader dispatcher.
    /// </summary>
    void ConeMarch()
    {
        //======|Shared declarations|======//
        float subdiv, pixelDiv, prevPassRatio, width, height;
        int threadGroupsX, threadGroupsY;

        //======|Camera uniform setters|======//
        coneMarchShader.SetFloat("_camTanFov", Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
        coneMarchShader.SetFloat("_camAspect", cam.aspect);
        coneMarchShader.SetVector("_camForward", cam.transform.forward);
        coneMarchShader.SetVector("_camPos", cam.transform.position);
        coneMarchShader.SetMatrix("_camProjMat", cam.projectionMatrix);
        coneMarchShader.SetMatrix("_camInvProjMat", cam.projectionMatrix.inverse);
        coneMarchShader.SetMatrix("_camToWorldMat", cam.cameraToWorldMatrix);
        coneMarchShader.SetMatrix("_camWorldToCamMat", cam.worldToCameraMatrix);

        //======|Marching uniform setters|======//
        coneMarchShader.SetFloat("MAX_STEPS", MaxSteps);
        coneMarchShader.SetFloat("MAX_DIS", MaxDis);
        coneMarchShader.SetFloat("HIT_EPS", HitEps);
        coneMarchShader.SetFloat("SLOPE_EPS", SlopeEps);

        //======|Scene uniform setter|======//
        coneMarchShader.SetInt("_scene", scene);

        //======|Passes|======//
        for (int i = 0; i < subdivisions.Length; i++) // For each pass
        {
            subdiv = subdivisions[i]; // get the current subdivision of the screeen.
            pixelDiv = conePassPixelDiv[i]; // get the current pixel division of the screen.
            prevPassRatio = i == 0 ? 1f : conePassPixelDiv[i - 1] / conePassPixelDiv[i]; // calculate the previous pass' ratio
            width = cam.pixelWidth / pixelDiv; // get the current pass' texture screen width
            height = cam.pixelHeight / pixelDiv; // get the current pass' texture screen height

            // update the textures
            coneMarchShader.SetTexture(0, "ConeMarchDataIn", i == 0 ? coneMarchDataInArr[i] : coneMarchDataInArr[i - 1]);
            coneMarchShader.SetTexture(0, "ConeMarchDataOut", coneMarchDataOutArr[i]);

            // update the uniforms to the current pass params
            coneMarchShader.SetFloat("subdiv", subdiv);
            coneMarchShader.SetFloat("pixelDiv", pixelDiv);
            coneMarchShader.SetFloat("prevPassRatio", prevPassRatio);
            coneMarchShader.SetBool("isFirst", i == 0);

            // update the work group numbers.
            threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            threadGroupsY = Mathf.CeilToInt(height / 8.0f);

            // dispatch comp shader
            coneMarchShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            // copy the current output as the next pass' new input
            Graphics.CopyTexture(coneMarchDataOutArr[i], coneMarchDataInArr[i]);
        }
    }

    /// <summary>
    /// Ray marching compute shader dispatcher. Will also apply the final color on the target renderTexture.
    /// </summary>
    void RunMarcher()
    {
        //======|RenderTexture uniform setters|======//
        rayMarchShader.SetTexture(0, "Result", target);
        rayMarchShader.SetTexture(0, "UnityRendered", cam.activeTexture);
        rayMarchShader.SetTexture(0, "LightCameraDepthTexture", lightCam.activeTexture);
        rayMarchShader.SetTexture(0, "SkyboxRendered", skyboxRender); // note that it wont work in edit mode
        rayMarchShader.SetTexture(0, "UIRendered", uiRender); // note that it wont work in edit mode
        rayMarchShader.SetTexture(0, "DepthBuffer", DepthBuffer);
        rayMarchShader.SetTextureFromGlobal(0, "_CameraDepthTexture", "_CameraDepthTexture");

        //======|Cone marching uniform setters|======//
        rayMarchShader.SetTexture(0, "ConeMarchData", coneMarchDataOutArr.Last());
        rayMarchShader.SetFloat("finalPixelDiv", conePassPixelDiv.Last());

        //======|Camera uniform setters|======//
        rayMarchShader.SetFloat("_camTanFov", Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad));
        rayMarchShader.SetFloat("_camAspect", cam.aspect);
        rayMarchShader.SetVector("_camForward", cam.transform.forward);
        rayMarchShader.SetVector("_camPos", cam.transform.position);
        rayMarchShader.SetMatrix("_camProjMat", cam.projectionMatrix);
        rayMarchShader.SetMatrix("_camInvProjMat", cam.projectionMatrix.inverse);
        rayMarchShader.SetMatrix("_camToWorldMat", cam.cameraToWorldMatrix);
        rayMarchShader.SetMatrix("_camWorldToCamMat", cam.worldToCameraMatrix);

        //======|Lighting info uniform setters|======//
        rayMarchShader.SetFloat("_LightCamSize", lightCam.orthographicSize);
        rayMarchShader.SetMatrix("_LightWorldToCamMat", lightCam.worldToCameraMatrix);
        rayMarchShader.SetFloat("_LightNear", lightCam.nearClipPlane);
        rayMarchShader.SetFloat("_LightFar", lightCam.farClipPlane);
        rayMarchShader.SetVector("_LightDir", -dirLight.transform.forward.normalized);

        //======|Marching uniform setters|======//
        rayMarchShader.SetFloat("MAX_STEPS", MaxSteps);
        rayMarchShader.SetFloat("MAX_DIS", MaxDis);
        rayMarchShader.SetFloat("HIT_EPS", HitEps);
        rayMarchShader.SetFloat("SLOPE_EPS", SlopeEps);

        //======|Shadow uniform setters|======//
        rayMarchShader.SetFloat("SHADOW_DETAIL", ShadowDetail);
        rayMarchShader.SetFloat("MAX_SHADOW_STEPS", MaxShadowSteps);
        rayMarchShader.SetFloat("RAYMARCH_DETAIL", RayMarchDetail);

        //======|Settings uniform setters|======//
        rayMarchShader.SetBool("SHADOWS_ON", shadowsOn);
        rayMarchShader.SetBool("AO_ON", ambientOccOn);

        //======|Skybox coloring uniform setters|======//
        rayMarchShader.SetVector("_FOG_COLOR_GROUND", fogColorGround);
        rayMarchShader.SetVector("_FOG_COLOR_SKY", fogColorSky);
        rayMarchShader.SetVector("_SUN_COLOR", sunColor);

        //======|Scene uniform setter|======//
        rayMarchShader.SetInt("_scene", scene);

        //======|Dispach|======//
        rayMarchShader.Dispatch(0, shaderThreadGroupsX, shaderThreadGroupsY, 1);
    }

    /// <summary>
    /// FXAA comp shader dispatcher.
    /// </summary>
    void FXAAVFX()
    {
        fxaaShader.SetTexture(0, "Result", target);
        fxaaShader.Dispatch(0, shaderThreadGroupsX, shaderThreadGroupsY, 1);
    }

    /// <summary>
    /// Depth blur VFX comp shader dispatcher.
    /// </summary>
    void DepthBlurVFX()
    {
        depthBlurShader.SetTexture(0, "Result", target);
        depthBlurShader.SetTexture(0, "DepthBuffer", DepthBuffer);
        depthBlurShader.SetInt("RADIUS", depthBlurRadius);

        depthBlurShader.Dispatch(0, shaderThreadGroupsX, shaderThreadGroupsY, 1);
    }

    /// <summary>
    /// Other VFX comp shader dispatcher.
    /// </summary>
    void VFX()
    {
        vfxShader.SetTexture(0, "Result", target);
        vfxShader.Dispatch(0, shaderThreadGroupsX, shaderThreadGroupsY, 1);
    }

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.Depth;

        //======|Init textures|======//
        target = initBuffer(cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.ARGBFloat);
        uiRender = initBuffer(cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.ARGBFloat);
        skyboxRender = initBuffer(cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.RFloat);
        DepthBuffer = initBuffer(cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.RFloat);

        coneMarchDataInArr = new RenderTexture[conePassPixelDiv.Length];
        coneMarchDataOutArr = new RenderTexture[conePassPixelDiv.Length];

        float pixelDiv, width, height;
        for (int i = 0; i < conePassPixelDiv.Length; i++)
        {
            pixelDiv = conePassPixelDiv[i];
            width = cam.pixelWidth / pixelDiv;
            height = cam.pixelHeight / pixelDiv;
            coneMarchDataInArr[i] = initBuffer(width, height, RenderTextureFormat.RFloat);
            coneMarchDataOutArr[i] = initBuffer(width, height, RenderTextureFormat.RFloat);
        }
    }

    private void Start()
    {
        lightCam.depthTextureMode |= DepthTextureMode.Depth;
        skyboxCam.targetTexture = skyboxRender;
        uiCam.targetTexture = uiRender;

        // Match polygonal rendering distance to raymarched rendering max distance
        cam.farClipPlane = MaxDis + 1;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (cam != null && lightCam != null && lightCam.activeTexture != null)
        {
            //======|Get threads size|======//
            shaderThreadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
            shaderThreadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);

            //======|Marching|======//
            ConeMarch();
            RunMarcher();

            //======|VFX|======//
            if (fxaaOn) FXAAVFX();
            if (depthBlurOn) DepthBlurVFX();
            VFX();

            //======|Blit to frame buffer|======//
            Graphics.Blit(target, destination);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void OnDisable()
    {
        //======|Clean up|======//
        DepthBuffer.Release();
        target.Release();
        skyboxRender.Release();
        uiRender.Release();

        for (int i = 0; i < conePassPixelDiv.Length; i++)
        {
            coneMarchDataInArr[i].Release();
            coneMarchDataOutArr[i].Release();
        }
    }
}
