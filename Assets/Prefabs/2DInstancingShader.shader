Shader "Unlit/2DInstancingShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)  // Thêm màu với Alpha
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // Kích hoạt Alpha Blending
        // ZWrite Off // Tắt ZWrite để tránh che các object phía sau
        Cull Off  // Tắt culling để hiển thị cả hai mặt
        Pass
        {
            ZWrite On // Bật z-write
            ZTest LEqual // Kiểm tra độ sâu

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color; // Nhân texture với màu
                if (texColor.a < 0.1) discard; // Loại bỏ pixel quá trong suốt
                return texColor;
            }
            ENDCG
        }
    }
}
