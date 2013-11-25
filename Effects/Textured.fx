#include <CommonParameters.fx>

float4x4 World;

texture Texture;
sampler2D TextureSampler = sampler_state
{
	texture = <Texture>;
	minfilter = LINEAR;
	magfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = MIRROR;
	AddressV = MIRROR;
};

struct TexturedVSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct TexturedVSOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

TexturedVSOutput TexturedVS(TexturedVSInput input)
{
    TexturedVSOutput output;
    
    float4x4 wvp = mul(World, mul(View, Projection));
    
    output.Position = mul(input.Position, wvp);

    output.TexCoord = input.TexCoord;

    return output;
}

float4 TexturedPS(TexturedVSOutput input) : COLOR0
{
    float4 colour = tex2D(TextureSampler, input.TexCoord);

    return colour;
}

technique Textured
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 TexturedVS();
        PixelShader = compile ps_2_0 TexturedPS();
    }
}
