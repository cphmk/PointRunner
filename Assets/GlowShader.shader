Shader "Custom/GlowShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" { }  // Base texture (e.g., the coin texture)
        _GlowColor ("Glow Color", Color) = (1,1,1,1) // Color of the glow effect
        _Emission ("Emission", Color) = (1,1,1,1)  // The glow emission color
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass {
            // Vertex and fragment shader
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Struct to hold vertex data
            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            // Struct to pass data from vertex to fragment
            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            // Emission color (the color of the glow)
            float4 _Emission;
            float4 _GlowColor; // Glow color
            sampler2D _MainTex;  // Base texture

            // Vertex shader
            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);  // Transform the vertex position
                o.color = _GlowColor;  // Set the glow color
                return o;
            }

            // Fragment shader
            half4 frag(v2f i) : COLOR
            {
                // Emit the glow color as the fragment color
                return i.color * _Emission;  // Multiply the glow color by emission intensity
            }
            ENDCG
        }
    }
}
