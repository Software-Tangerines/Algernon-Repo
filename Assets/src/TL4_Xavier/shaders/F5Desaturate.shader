// F5 full-screen desaturation (built-in render pipeline). ViewDirector.OnRenderImage blits through it.
// _Saturation is a global set by ViewDirector every frame: 1 = full colour, 0 = greyscale.
Shader "Hidden/F5/Desaturate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Saturation;

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float grey = dot(c.rgb, float3(0.299, 0.587, 0.114));   // perceived brightness
                c.rgb = lerp(grey.xxx, c.rgb, _Saturation);
                return c;
            }
            ENDCG
        }
    }
}
