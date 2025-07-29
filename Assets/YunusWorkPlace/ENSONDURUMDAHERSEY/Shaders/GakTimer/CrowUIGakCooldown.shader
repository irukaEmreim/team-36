Shader "UI/CrowUIGakCooldown"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CooldownProgress ("Cooldown Progress", Range(0,1)) = 0
        _CooldownColor ("Cooldown Color", Color) = (1, 0, 0, 1)
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
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
            float _CooldownProgress;
            float4 _CooldownColor;
            float _PulseAmount;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            float2 ApplyPulse(float2 uv, float amount)
            {
                float2 center = float2(0.5, 0.5);
                return lerp(center, uv, 1.0 - amount);
            }


            fixed4 frag(v2f i) : SV_Target
{
    float2 pulsedUV = ApplyPulse(i.uv, _PulseAmount);
    fixed4 texColor = tex2D(_MainTex, pulsedUV);

    if (texColor.a < 0.01)
        discard;

    float mask = step(i.uv.y, _CooldownProgress); 
    float blendAmount = 0.8;

    fixed4 result = texColor;
    if (mask > 0.0)
    {
        result.rgb = lerp(texColor.rgb, _CooldownColor.rgb, blendAmount);
    }

    result.a = texColor.a;
    return result;
}

            ENDCG
        }
    }
}
