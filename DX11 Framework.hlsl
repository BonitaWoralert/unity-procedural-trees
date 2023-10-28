//--------------------------------------------------------------------------------------
// File: DX11 Framework.hlsl
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//Shader Variables
//--------------------------------------------------------------------------------------

Texture2D texDiffuse : register(t0);
SamplerState sampLinear : register(s0);


// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer ConstantBuffer : register( b0 )
{
	matrix World;
	matrix View;
	matrix Projection;
 
    float4 DiffuseLight;
    float4 DiffuseMaterial;
    float3 DirectionToLight;
    float pad;
    float4 AmbientLight;
    float4 AmbientMaterial;
    float4 SpecularMaterial;
    float4 SpecularLight;
    float SpecularPower;
    float3 EyeWorldPos;
}



//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float3 PosW : POSITION0;
    float3 NormalW : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
VS_OUTPUT VS( float3 Pos : POSITION, float3 Normal : NORMAL, float2 TexCoord : TEXCOORD )
{
    float4 pos4 = float4(Pos, 1.0f);
    float4 Normal4 = float4(Normal, 0.0f);
    
    VS_OUTPUT output = (VS_OUTPUT)0;
    output.Pos = mul( pos4, World); //model space -> world space
    output.Pos = mul( output.Pos, View ); //world space -> view space
    output.Pos = mul( output.Pos, Projection ); //view space -> projection space
    //final outputs
    output.PosW = output.Pos; //position
    output.NormalW = normalize(mul(Normal4, World)); //normals
    output.TexCoord = TexCoord; //texture coordinates
    return output;
}

float4 Hadamard(float4 a, float4 b)
{
    return (float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w));
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( VS_OUTPUT input ) : SV_Target
{
    //Diffuse Light calculations
    float DiffuseAmount = max(dot(normalize(DirectionToLight), input.NormalW), 0);
    float4 Diffuse = DiffuseAmount * (DiffuseMaterial * DiffuseLight);
    
    //Ambient Light calculations
    float4 Ambient = AmbientLight * AmbientMaterial;
    
    //Specular Light calculations
    float3 ReflectDir = normalize(reflect(-DirectionToLight, input.NormalW)); // finds the reflection from the light source hitting the normal
    float3 ViewerDir = normalize(EyeWorldPos - input.PosW.xyz); //direction towards camera/viewer
    float SpecIntensity = max(dot(ReflectDir, ViewerDir), 0); //dot product between reflect and viewer
    SpecIntensity = pow(SpecIntensity, SpecularPower); //specular intensity to the power of specular power to shrink the cone of reflectance
    float4 SpecPotential = Hadamard(SpecularLight, SpecularMaterial); 
    float4 Specular = Hadamard(SpecIntensity, SpecPotential); //final specular light
    
    //texture colour (must be multiplied by the rest of the lighting)
    float4 textureColour = texDiffuse.Sample(sampLinear, input.TexCoord);
    
    //final colour
    input.Color = textureColour * (Specular + Diffuse + Ambient);
    return input.Color;
}
