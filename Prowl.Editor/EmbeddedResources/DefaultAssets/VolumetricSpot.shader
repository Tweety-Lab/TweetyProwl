Shader "Default/VolumetricSpot"

Properties
{
    _MainColor("Main Color", Color) = (1, 1, 1, 1)
    _LightIntensity("Light Intensity", Float) = 1
    _Scattering("Scattering Coefficient", Float) = 1
    _StepCount("Step Count", Float) = 64
}

Pass "Volumetric"
{
    Tags { "RenderOrder" = "Transparent", "Queue" = "Transparent" }

    Blend
    {
        Src Color SourceAlpha
        Src Alpha One
        Dest Color InverseSourceAlpha
        Dest Alpha InverseSourceAlpha
        Mode Color Add
        Mode Alpha Add
    }

    Cull Back

    DepthStencil
    {
        DepthWrite Off
        DepthTest LessEqual
    }

    HLSLPROGRAM
        #pragma vertex Vertex
        #pragma fragment Fragment

        #include "Prowl.hlsl"

        struct Attributes
        {
            float3 position : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 position : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 worldPos : TEXCOORD1;
            float3 localPos : TEXCOORD2;
        };

        float4 _MainColor;
        float _LightIntensity;
        float _Scattering;
        float _StepCount;
        float4x4 _ModelMatrix;

        float4x4 _ModelMatrix;

        Varyings Vertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            float4 localPos = float4(input.position, 1.0);
                
            // Store local position for volume calculations
            output.localPos = input.position;
                
            // Use passed model matrix _ModelMatrix to calculate worldPos
            output.worldPos = mul(_ModelMatrix, localPos).xyz;
                
            // Use the global MVP matrix for final clip position
            output.position = mul(PROWL_MATRIX_MVP, localPos);
                
            return output;
        }

        float4 Fragment(Varyings input) : SV_TARGET
        {
            return _MainColor;
        }
    ENDHLSL
}
