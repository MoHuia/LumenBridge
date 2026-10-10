Shader "LumenBridge/Firefly Halo"
{
    Properties
    {
        // 光晕颜色由材质指定；alpha 控制整体亮度。
        _Color ("Halo Color", Color) = (1,0.65,0.16,0.35)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            // 相加混合让底色仍然可见，不写深度以免遮挡其他透明物体。
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // 将 UV 中心作为光心，向边缘平滑衰减；不需要外部图片。
                float radius = length(input.uv * 2.0 - 1.0);
                half fade = pow(saturate(1.0 - radius), 3.0);
                return half4(_Color.rgb, _Color.a * fade);
            }
            ENDHLSL
        }
    }
}
