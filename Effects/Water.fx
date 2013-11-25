#include <CommonParameters.fx>

float4x4 World;

float4x4 MirrorView;

DEFINE_TEX_SAMPLER(Reflection, LINEAR, LINEAR, LINEAR, MIRROR, MIRROR);
DEFINE_TEX_SAMPLER(Refraction, LINEAR, LINEAR, LINEAR, MIRROR, MIRROR);

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);

	float4x4 m = mul(World, mul(MirrorView, Projection));    
    output.TexCoord = mul(input.Position, m);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = input.TexCoord.x / input.TexCoord.w / 2.0f + 0.5f;
    ProjectedTexCoords.y = -input.TexCoord.y / input.TexCoord.w / 2.0f + 0.5f;    
    
    return tex2D(TEX_SAMPLER(Reflection), ProjectedTexCoords);   
}

technique Water
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
