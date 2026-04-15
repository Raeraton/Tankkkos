float4x4 World;
float4x4 ViewProj;
float texScale=3;

float3 SunPosition;
float SunShininess;

float3 CameraPosition;
float AmbientLight = 0.65;

float WaveHeight;
float WaveFreqs[32];
float WavePhas[32];




texture2D GrassTex;
sampler GrassTexSampler = sampler_state
{
    Texture = <GrassTex>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VSO
{
    float4 pos : POSITION;
    float4 normal : NORMAL;
    float4 screen : TEXCOORD;
    float4 wordPos : TEXCOORD2;
};

VSO VS(float4 inPos : POSITION) : POSITION
{
    VSO o;
    
    // world transform
    o.pos = mul(inPos, World);
    o.wordPos = o.pos;
    
    // waves
    o.pos.y = 0;
    o.normal = float4(0,1,0,0);
    for (uint i = 0; i < 32; i++)
    {
        float freq = WaveFreqs[i];
        float ampl = 1 / freq;
        float phase = WavePhas[i];
        
        o.pos.y += ampl * sin(o.pos.x * freq + phase) + ampl * sin(o.pos.z * freq + phase);
        o.normal.x += -ampl * freq * cos(o.pos.x * freq + phase);
        o.normal.z += -ampl * freq * cos(o.pos.z * freq + phase);
    }
    
    o.normal.x = o.normal.x / 2 * WaveHeight;
    o.normal.z = o.normal.z / 2 * WaveHeight;
    o.pos.y = o.pos.y / 2 * WaveHeight;
    
    // view transform
    o.pos = o.screen = mul(o.pos, ViewProj);
    
    return o;
}
float4 PS( VSO input ) : COLOR
{

    
    float3 color = tex2D(GrassTexSampler, input.wordPos.xz / texScale).xyz;
    float3 normal = normalize( input.normal.xyz );
    float3 sunDir = normalize((CameraPosition+SunPosition) - input.wordPos.xyz);
    
    float lightPow = saturate(dot(normal, sunDir));
    
    float3 viewDir = normalize(CameraPosition - input.wordPos.xyz);
    float3 halfWayVec = normalize(viewDir + sunDir);
    float shine = pow(saturate(dot(normal, halfWayVec)), 7);
    
    return float4(color * (lightPow * (1 - AmbientLight) + AmbientLight), 1) + float4(1, 1, 1, 1) * shine * SunShininess;
    
}

technique Water
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_3 VS();
        PixelShader = compile ps_4_0_level_9_3 PS();
    }
}
