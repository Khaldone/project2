Shader "Custom/Scroll Sprite Shader"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Visible ("Visible", Float) = 1
		_Offset("Offset", Float) = 1

		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			float4 _MainTex_ST;
		
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float _Visible;
			float _Offset;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				float2 newUV = TRANSFORM_TEX(uv, _MainTex);
				newUV.y += _Offset;
				fixed4 color = tex2D (_MainTex, newUV);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif 
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				if(_Visible > IN.texcoord.y)
					discard;
				
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}