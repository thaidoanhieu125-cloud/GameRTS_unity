Shader "GameRTS/Landscape Ground"
{
    Properties { _GrassTex("Grass",2D)="white"{} _WaterTex("Water",2D)="white"{} }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_GrassTex); SAMPLER(sampler_GrassTex);
            TEXTURE2D(_WaterTex); SAMPLER(sampler_WaterTex);
            float4 _Lakes[12]; int _LakeCount;
            struct Input { float4 positionOS:POSITION; };
            struct Output { float4 positionCS:SV_POSITION;float2 world:TEXCOORD0; };
            Output Vert(Input v) { Output o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.world=TransformObjectToWorld(v.positionOS.xyz).xy;return o; }
            half4 Frag(Output i):SV_Target
            {
                float2 p=i.world;
                half3 grass=SAMPLE_TEXTURE2D(_GrassTex,sampler_GrassTex,p/7.0).rgb;
                float variation=0.94+0.045*sin(p.x*.17+p.y*.08)+.035*cos(p.y*.21-p.x*.09);
                grass*=half3(.83,.98,.85)*variation;
                float distance=1000;
                [loop] for(int j=0;j<_LakeCount;j++)
                    distance=min(distance,(length((p-_Lakes[j].xy)/_Lakes[j].zw)-1)*min(_Lakes[j].z,_Lakes[j].w));
                // Inward-only variation gives organic shores inside the conservative CPU water mask.
                distance+=.55+.24*sin(p.x*.8+p.y*.22)+.18*sin(p.y*1.1)+.1*sin(p.x*1.9-p.y*.7);
                half3 sand=lerp(half3(.30,.28,.17),half3(.48,.43,.28),smoothstep(-.15,.5,distance));
                sand*=.88+.20*SAMPLE_TEXTURE2D(_GrassTex,sampler_GrassTex,p/2.8).r;
                half3 land=lerp(sand,grass,smoothstep(.12,.7,distance));
                half3 water=SAMPLE_TEXTURE2D(_WaterTex,sampler_WaterTex,p/9.0+float2(_Time.y*.002,0)).rgb;
                water=lerp(water*half3(.68,.84,.9),water*half3(.95,1.05,.92),smoothstep(-3,0,distance));
                float ripple=.018*sin(distance*15-_Time.y*1.7)*(1-smoothstep(0,.55,abs(distance+.22)));
                water+=ripple;
                return half4(lerp(water,land,smoothstep(-.04,.08,distance)),1);
            }
            ENDHLSL
        }
    }
}
