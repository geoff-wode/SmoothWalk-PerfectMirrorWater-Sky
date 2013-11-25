#include <CommonParameters.fx>

float4x4 World;

float3 LightDirection;
float AmbientLight = 0.3;

DEFINE_TEX_SAMPLER(Sand, LINEAR, LINEAR, LINEAR, WRAP, WRAP);
DEFINE_TEX_SAMPLER(Grass, LINEAR, LINEAR, LINEAR, WRAP, WRAP);
DEFINE_TEX_SAMPLER(Rock, LINEAR, LINEAR, LINEAR, MIRROR, MIRROR);
DEFINE_TEX_SAMPLER(Snow, LINEAR, LINEAR, LINEAR, MIRROR, MIRROR);

struct MultiTexturedVSInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float4 TexWeights : TEXCOORD1;
};

struct MultiTexturedVSOutput
{
    float4 Position : POSITION0;
    float3 Normal : TEXCOORD0;
    float3 LightDir : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float4 TexWeights : TEXCOORD3;
    float Depth : TEXCOORD4;
};

MultiTexturedVSOutput MultiTexturedVS(MultiTexturedVSInput input)
{
    MultiTexturedVSOutput output;

	float4x4 wvp = mul(World, mul(View, Projection));

    output.Position = mul(input.Position, wvp);

    output.Normal = mul(normalize(input.Normal), World);
    
    output.TexCoord = input.TexCoord;
    output.TexWeights = input.TexWeights;
    
    output.LightDir = -LightDirection;
    
    output.Depth = output.Position.z / output.Position.w;

    return output;
}

float4 MultiTexturedPS(MultiTexturedVSOutput input) : COLOR0
{
    float NdotL = saturate(dot(input.Normal, input.LightDir));
    
    float lightFactor = saturate(NdotL + AmbientLight);
    
    float4 farColour;
   	farColour  = tex2D(TEX_SAMPLER(Sand), input.TexCoord) * input.TexWeights.x;
	farColour += tex2D(TEX_SAMPLER(Grass), input.TexCoord) * input.TexWeights.y;
	farColour += tex2D(TEX_SAMPLER(Rock), input.TexCoord) * input.TexWeights.z;
	farColour += tex2D(TEX_SAMPLER(Snow), input.TexCoord) * input.TexWeights.w;
	
	float4 nearColour;
	nearColour  = tex2D(TEX_SAMPLER(Sand), input.TexCoord * 3) * input.TexWeights.x;
	nearColour += tex2D(TEX_SAMPLER(Grass), input.TexCoord * 3) * input.TexWeights.y;
	nearColour += tex2D(TEX_SAMPLER(Rock), input.TexCoord * 3) * input.TexWeights.z;
	nearColour += tex2D(TEX_SAMPLER(Snow), input.TexCoord * 3) * input.TexWeights.w;
	
	// The distance from the camera at which extra detail will blend in.
	// NB. Distance == 0 --> near clipping plane. Distance == 1 --> far clipping plane.
	const float blendDistance = 0.99;

	// The size of the band of blending for extra detail - bigger gives smoother transitions
	const float blendWidth = 0.01;

	float blendFactor = saturate(input.Depth - blendDistance) / blendWidth;
	
	float4 blendedColour = lerp(nearColour, farColour, blendFactor);

	float4 finalColour = blendedColour * lightFactor;	
    return finalColour;
}

technique MultiTextured
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 MultiTexturedVS();
        PixelShader = compile ps_2_0 MultiTexturedPS();
    }
}
