Shader "Hidden/DepthExtractor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthNormalsTexture);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 scrPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.scrPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float frag(v2f i) : SV_Target
            {
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv); // getting depth
                return (1. - depth);
            }
            ENDCG
        }
    }
}
