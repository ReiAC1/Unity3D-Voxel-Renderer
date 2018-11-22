// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Diffuse"
{
	Properties
	{
		_TexArray("Array", 2DArray) = "" {}
	}

		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.5

		UNITY_DECLARE_TEX2DARRAY(_TexArray);

	struct VertexIn
	{
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
		float4 col : COLOR;
	};

	struct VertexOut
	{
		float4 pos : SV_POSITION;
		float3 uv : TEXCOORD0;
		float4 col : COLOR;
	};

	VertexOut vert(VertexIn v)
	{
		VertexOut o;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		o.col = v.col;

		return o;
	}

	float4 frag(VertexOut i) : COLOR
	{
		float3 light = i.col.rgb;

		half4 col = UNITY_SAMPLE_TEX2DARRAY(_TexArray, i.uv);
		return float4(col.xyz * light, 1.0);
	}

		ENDCG
	}
	}
}