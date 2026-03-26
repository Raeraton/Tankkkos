float4x4 WorldViewProj;



float4 VS(float4 inPos : POSITION) : POSITION
{
    return mul(inPos, WorldViewProj);
}
float4 PS() : COLOR
{
    return float4(0.2, 0.5, 1, 1);
}
technique Water
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_3 VS();
        PixelShader = compile ps_4_0_level_9_3 PS();
    }
}
