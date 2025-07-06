Shader "UI/CrowUIHealth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Health ("Health Level", Range(0,1)) = 1
        _FillColor ("Fill Color", Color) = (1, 0, 0, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Lighting Off ZWrite Off Cull Off Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Health;
            float4 _FillColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
{
    fixed4 texColor = tex2D(_MainTex, i.uv);

    if (texColor.a < 0.01)
        discard;

    float fillMask = step(i.uv.y, 1.0 - _Health);

    // Fill mask doluysa kırmızı overlay uygula ama orijinal doku görünsün
    float blendStrength = 0.75; // 0.0 = hiç karışmaz, 1.0 = tam kırmızı

    fixed4 blended = texColor;
    if (fillMask > 0.0)
    {
        blended.rgb = lerp(texColor.rgb, _FillColor.rgb, blendStrength);
    }

    blended.a = texColor.a;
    return blended;
}

            ENDCG
        }
    }
}
