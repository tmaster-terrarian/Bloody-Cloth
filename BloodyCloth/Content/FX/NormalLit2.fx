float4x4 World;
float4x4 ViewProjection;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float4x4 WorldInverseTranspose;

float3 DiffuseLightDirection = float3(1, 0, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

float Shininess = 200;
float4 SpecularColor = float4(1, 1, 1, 1);	
float SpecularIntensity = 1;
float3 ViewVector = float3(1, 0, 0);

texture SpriteTexture;
sampler2D textureSampler = sampler_state {
    Texture = (SpriteTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float BumpConstant = 1;
texture NormalMap;
sampler2D bumpSampler = sampler_state {
	Texture = (NormalMap);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 Tangent : TEXCOORD2;
    float3 Binormal : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, ViewProjection);

    output.Normal = mul(input.Normal, WorldInverseTranspose);
    output.Tangent = mul(input.Tangent, WorldInverseTranspose);
    output.Binormal = mul(input.Binormal, WorldInverseTranspose);

    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Calculate the normal, including the information in the bump map
    float3 bump = BumpConstant * ((2 * tex2D(bumpSampler, input.TextureCoordinate)) - 1);
    float3 bumpNormal = (0.0, 0.0, 1.0) + (bump.x * input.Tangent + bump.y * input.Binormal);
    bumpNormal = normalize(bumpNormal);

    // Calculate the diffuse light component with the bump map normal
    float diffuseIntensity = dot(normalize(DiffuseLightDirection), bumpNormal);
    if(diffuseIntensity < 0)
        diffuseIntensity = 0;

    // Calculate the specular light component with the bump map normal
    float3 light = normalize(DiffuseLightDirection);
    float3 r = normalize(2 * dot(light, bumpNormal) * bumpNormal - light);
    float3 v = normalize(mul(normalize(ViewVector), World));
    float dotProduct = dot(r, v);

    float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * diffuseIntensity;

    // Calculate the texture color
    float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    textureColor.a = 1;

    // Combine all of these values into one (including the ambient light)
    return saturate(textureColor * (diffuseIntensity) + AmbientColor * AmbientIntensity + specular);
}

technique BumpMapped
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
