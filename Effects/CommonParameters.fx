#ifndef COMMON_PARAMETERS_FX
#define COMMON_PARAMETERS_FX

#define DEFINE_TEX_SAMPLER(tex, min, mag, mip, u, v)	\
	texture tex##Texture;					\
	sampler2D tex##Sampler = sampler_state	\
	{										\
		texture = <tex##Texture>;			\
		minfilter = min;					\
		magfilter = mag;					\
		mipfilter = mip;					\
		AddressU = u;						\
		AddressV = v;						\
	}

#define TEX_SAMPLER(tex)	tex##Sampler


shared float4x4 View;
shared float4x4 Projection;


float4 VertexShaderFunction(float4 Position : POSITION0) : POSITION0
{
    return float4(0, 0, 0, 1);
}

float4 PixelShaderFunction(float4 input : POSITION0) : COLOR0
{
    return float4(1, 0, 0, 1);
}

technique CommonParametersTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
#endif // COMMON_PARAMETERS_FX
