Shader "Unlit/RelativisticGridTess"
{
    Properties
    {
        _GridScale ("Grid Scale (cells per unit)", Float) = 1.0
        _LineThickness ("Line Thickness", Float) = 0.02
        _Color ("Line Color", Color) = (0.8,0.8,0.8,1)
        _BGColor ("Background Color", Color) = (0.05,0.05,0.07,1)

        _CurvatureScale ("Curvature Scale", Float) = 1.0
        _Rs ("Schwarzschild Radius", Float) = 0.5
        _MaxDisp ("Max Displacement (abs)", Float) = 10.0

        _AnchorRadius ("Anchor Radius (flat beyond)", Float) = 20.0
        _FadeWidth ("Fade Width", Float) = 5.0

        _MassPosition ("Mass World Position", Vector) = (1000000,1000000,1000000,0)
        _TessellationUniform ("Base Tessellation", Range(1,64)) = 16
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex VS
            #pragma hull   HS
            #pragma domain DS
            #pragma fragment PS

            #include "UnityCG.cginc"

            float  _GridScale;
            float  _LineThickness;
            float4 _Color;
            float4 _BGColor;

            float  _CurvatureScale;
            float  _Rs;
            float  _MaxDisp;

            float  _AnchorRadius;  // r where we define z(r)=0
            float  _FadeWidth;     // smooth fade to 0 beyond anchor
            float4 _MassPosition;

            float  _TessellationUniform;

            struct appdata
            {
                float4 vertex : POSITION; // object space
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uvGrid : TEXCOORD0;
                float3 wpos   : TEXCOORD1;
            };

            // --- Utility: Flamm's paraboloid z(r) ---
            float FlammZ(float r, float rs)
            {
                // z(r) = 2*sqrt(rs*(r - rs)) for r >= rs
                if (r <= rs) return 0.0;
                return 2.0 * sqrt(rs * (r - rs));
            }

            // Vertex shader: pass-through (object space)
            appdata VS(appdata v) { return v; }

            // Tessellation constants
            struct HSConst
            {
                float Edge[3] : SV_TessFactor;
                float Inside  : SV_InsideTessFactor;
            };

            // Simple distance-adaptive tessellation:
            // more tessellation near the mass, less far away.
            HSConst PatchConstants(InputPatch<appdata,3> patch)
            {
                // Estimate a patch center in object space
                float3 p0 = patch[0].vertex.xyz;
                float3 p1 = patch[1].vertex.xyz;
                float3 p2 = patch[2].vertex.xyz;
                float3 pC = (p0 + p1 + p2) / 3.0;

                // To world
                float3 wC = mul(unity_ObjectToWorld, float4(pC,1)).xyz;

                // Distance in XZ from mass
                float r = length(wC.xz - _MassPosition.xz);
                float rs = max(_Rs, 1e-6);

                // Curvature proxy ~ 1 / (r - rs + eps)
                float eps = 0.1;
                float curvature = 1.0 / max(r - rs, eps);

                // Map curvature to tessellation factor
                float t = _TessellationUniform * saturate(curvature * rs); // scale by rs
                t = clamp(t, 2.0, 64.0);

                HSConst o;
                o.Edge[0] = t;
                o.Edge[1] = t;
                o.Edge[2] = t;
                o.Inside  = t;
                return o;
            }

            [domain("tri")]
            [partitioning("fractional_even")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("PatchConstants")]
            [outputcontrolpoints(3)]
            appdata HS(InputPatch<appdata,3> patch, uint i : SV_OutputControlPointID)
            {
                return patch[i];
            }

            [domain("tri")]
            v2f DS(HSConst hs,
                   const OutputPatch<appdata,3> patch,
                   float3 bary : SV_DomainLocation)
            {
                // Interpolate object-space vertex & uv
                appdata v;
                v.vertex = patch[0].vertex * bary.x +
                           patch[1].vertex * bary.y +
                           patch[2].vertex * bary.z;
                v.uv = patch[0].uv * bary.x +
                       patch[1].uv * bary.y +
                       patch[2].uv * bary.z;

                v2f o;

                // To world
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Radial distance in XZ from mass
                float rs = max(_Rs, 1e-6);
                float r  = length(wpos.xz - _MassPosition.xz);

                // Flamm z(r) and anchored version so z(anchor)=0
                float z    = FlammZ(r, rs);
                float zRef = FlammZ(max(r, _AnchorRadius), rs); // z at anchor (or beyond)
                float disp = -_CurvatureScale * (z - zRef);

                // Smooth fade to 0 beyond anchor
                // factor=1 inside, goes to 0 across [anchor, anchor+fadeWidth]
                float fade = 1.0 - smoothstep(_AnchorRadius, _AnchorRadius + _FadeWidth, r);
                disp *= fade;

                // Clamp visually
                disp = clamp(disp, -_MaxDisp, _MaxDisp);

                // Apply vertical displacement
                wpos.y += disp;

                o.wpos = wpos;
                o.uvGrid = wpos.xz * _GridScale;
                o.pos = mul(UNITY_MATRIX_VP, float4(wpos,1));
                return o;
            }

            // Grid function
            float gridLines(float2 uv, float thickness)
            {
                float2 g = abs(frac(uv) - 0.5);
                float2 fw = fwidth(uv);
                float a = smoothstep(thickness + fw.x, thickness, g.x)
                        + smoothstep(thickness + fw.y, thickness, g.y);
                return saturate(a);
            }

            float4 PS(v2f i) : SV_Target
            {
                float line = gridLines(i.uvGrid, _LineThickness);
                float3 col = lerp(_BGColor.rgb, _Color.rgb, line);
                return float4(col, 1);
            }

            ENDCG
        }
    }
}
