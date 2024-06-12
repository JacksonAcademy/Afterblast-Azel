// Upgrade NOTE: upgraded instancing buffer 'BOXOPHOBICTheVegetationEngineElementsDefaultMotionWindPower' to new syntax.

// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BOXOPHOBIC/The Vegetation Engine/Elements/Default/Motion Wind Power"
{
	Properties
	{
		[StyledMessage(Info, Use the Wind Power elements to control the global wind power. Element Texture R and Particle Color R are used as value multipliers. Element Texture A and Particle Color A are used as element as alpha mask., 0,0)]_Message("Message", Float) = 0
		[StyledCategory(Render Settings)]_CategoryRender("[ Category Render ]", Float) = 0
		_ElementIntensity("Render Intensity", Range( 0 , 1)) = 1
		[HideInInspector]_ElementParams("Render Params", Vector) = (1,1,1,1)
		[StyledMessage(Warning, When using all layers the Global Volume will create one render texture for each layer to render the elements. Try using fewer layers when possible., _ElementLayerWarning, 1, 10, 10)]_ElementLayerWarning("Render Layer Warning", Float) = 0
		[StyledMessage(Info, When using a higher Layer number the Global Volume will create more render textures to render the elements. Try using fewer layers when possible., _ElementLayerMessage, 1, 10, 10)]_ElementLayerMessage("Render Layer Message", Float) = 0
		[StyledMask(TVELayers, Default 0 Layer_1 1 Layer_2 2 Layer_3 3 Layer_4 4 Layer_5 5 Layer_6 6 Layer_7 7 Layer_8 8, 0, 0)]_ElementLayerMask("Render Layer", Float) = 1
		[Enum(Constant,0,Seasons,1)]_ElementMode("Render Mode", Float) = 0
		[StyledCategory(Element Settings)]_CategoryElement("[ Category Element ]", Float) = 0
		[NoScaleOffset][StyledTextureSingleLine]_MainTex("Element Texture", 2D) = "white" {}
		[Space(10)][StyledVector(9)]_MainUVs("Element UVs", Vector) = (1,1,0,0)
		[StyledRemapSlider(_MainTexColorMinValue, _MainTexColorMaxValue, 0, 1)]_MainTexColorRemap("Element Color", Vector) = (0,0,0,0)
		[StyledRemapSlider(_MainTexAlphaMinValue, _MainTexAlphaMaxValue, 0, 1)]_MainTexAlphaRemap("Element Alpha", Vector) = (0,0,0,0)
		[HideInInspector]_MainTexColorMinValue("Element Color Min", Range( 0 , 1)) = 0
		[HideInInspector]_MainTexColorMaxValue("Element Color Max", Range( 0 , 1)) = 1
		[HideInInspector]_MainTexAlphaMinValue("Element Alpha Min", Range( 0 , 1)) = 0
		[HideInInspector]_MainTexAlphaMaxValue("Element Alpha Max", Range( 0 , 1)) = 1
		[Space(10)]_MainValue("Element Value", Range( 0 , 1)) = 1
		[Space(10)]_AdditionalValue1("Winter Value", Range( 0 , 1)) = 1
		_AdditionalValue2("Spring Value", Range( 0 , 1)) = 1
		_AdditionalValue3("Summer Value", Range( 0 , 1)) = 1
		_AdditionalValue4("Autumn Value", Range( 0 , 1)) = 1
		[StyledRemapSlider(_NoiseMinValue, _NoiseMaxValue, 0, 1)]_NoiseRemap("Noise Remap", Vector) = (0,0,0,0)
		[StyledMessage(Info, The Particle Velocity mode requires the particle to have custom vertex streams for Velocity and Speed set after the UV stream under the particle Renderer module. , _ElementDirectionMode, 40, 10, 0)]_ElementDirectionMessage("Element Direction Message", Float) = 0
		[Enum(Element Forward,10,Element Texture,20,Particle Translate,30,Particle Velocity,40)][Space(10)]_ElementDirectionMode("Direction Mode", Float) = 20
		[StyledCategory(Fading Settings)]_CategoryFade("[ Category Fade ]", Float) = 0
		[HDR][StyledToggle]_ElementRaycastMode("Enable Raycast Fading", Float) = 0
		[StyledToggle]_ElementVolumeFadeMode("Enable Volume Edge Fading", Float) = 0
		[Space(10)][StyledLayers()]_RaycastLayerMask("Raycast Layer", Float) = 1
		[ASEEnd]_RaycastDistanceEndValue("Raycast Distance", Float) = 2
		[HideInInspector]_ElementLayerValue("Legacy Layer Value", Float) = -1
		[HideInInspector]_InvertX("Legacy Invert Mode", Float) = 0
		[HideInInspector]_ElementFadeSupport("Legacy Edge Fading", Float) = 0
		[HideInInspector]_IsElementShader("_IsElementShader", Float) = 1
		[HideInInspector]_IsVersion("_IsVersion", Float) = 900
		[HideInInspector]_IsMotionElement("_IsMotionElement", Float) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" "DisableBatching"="True" }
	LOD 0

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Off
		ColorMask B
		ZWrite Off
		ZTest LEqual
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define TVE_IS_MOTION_ELEMENT


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform half _IsMotionElement;
			uniform half _Message;
			uniform half _ElementLayerMask;
			uniform half _ElementDirectionMessage;
			uniform half _ElementLayerMessage;
			uniform half _ElementLayerWarning;
			uniform float _ElementDirectionMode;
			uniform half _ElementRaycastMode;
			uniform half _RaycastLayerMask;
			uniform half _RaycastDistanceEndValue;
			uniform half4 _NoiseRemap;
			uniform half _CategoryFade;
			uniform float _ElementFadeSupport;
			uniform half _ElementLayerValue;
			uniform float _InvertX;
			uniform half _IsVersion;
			uniform half _IsElementShader;
			uniform half _CategoryRender;
			uniform half _CategoryElement;
			uniform half4 _MainTexAlphaRemap;
			uniform half4 _MainTexColorRemap;
			uniform half _MainValue;
			uniform half4 TVE_SeasonOptions;
			uniform half _AdditionalValue1;
			uniform half _AdditionalValue2;
			uniform half TVE_SeasonLerp;
			uniform half _AdditionalValue3;
			uniform half _AdditionalValue4;
			uniform half _ElementMode;
			uniform sampler2D _MainTex;
			uniform half4 _MainUVs;
			uniform half _MainTexColorMinValue;
			uniform half _MainTexColorMaxValue;
			uniform half _ElementIntensity;
			uniform half _MainTexAlphaMinValue;
			uniform half _MainTexAlphaMaxValue;
			uniform half4 TVE_ColorsCoords;
			uniform half4 TVE_ExtrasCoords;
			uniform half4 TVE_MotionCoords;
			uniform half4 TVE_VertexCoords;
			uniform half TVE_ElementsFadeValue;
			uniform half _ElementVolumeFadeMode;
			UNITY_INSTANCING_BUFFER_START(BOXOPHOBICTheVegetationEngineElementsDefaultMotionWindPower)
				UNITY_DEFINE_INSTANCED_PROP(half4, _ElementParams)
#define _ElementParams_arr BOXOPHOBICTheVegetationEngineElementsDefaultMotionWindPower
			UNITY_INSTANCING_BUFFER_END(BOXOPHOBICTheVegetationEngineElementsDefaultMotionWindPower)
			half4 IS_ELEMENT( half4 Colors, half4 Extras, half4 Motion, half4 Vertex )
			{
				#if defined (TVE_IS_COLORS_ELEMENT)
				return Colors;
				#elif defined (TVE_IS_EXTRAS_ELEMENT)
				return Extras;
				#elif defined (TVE_IS_MOTION_ELEMENT)
				return Motion;
				#elif defined (TVE_IS_VERTEX_ELEMENT)
				return Vertex;
				#else
				return Colors;
				#endif
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				half TVE_SeasonOptions_X50_g21473 = TVE_SeasonOptions.x;
				half Value_Winter158_g21473 = _AdditionalValue1;
				half Value_Spring159_g21473 = _AdditionalValue2;
				half TVE_SeasonLerp54_g21473 = TVE_SeasonLerp;
				half lerpResult168_g21473 = lerp( Value_Winter158_g21473 , Value_Spring159_g21473 , TVE_SeasonLerp54_g21473);
				half TVE_SeasonOptions_Y51_g21473 = TVE_SeasonOptions.y;
				half Value_Summer160_g21473 = _AdditionalValue3;
				half lerpResult167_g21473 = lerp( Value_Spring159_g21473 , Value_Summer160_g21473 , TVE_SeasonLerp54_g21473);
				half TVE_SeasonOptions_Z52_g21473 = TVE_SeasonOptions.z;
				half Value_Autumn161_g21473 = _AdditionalValue4;
				half lerpResult166_g21473 = lerp( Value_Summer160_g21473 , Value_Autumn161_g21473 , TVE_SeasonLerp54_g21473);
				half TVE_SeasonOptions_W53_g21473 = TVE_SeasonOptions.w;
				half lerpResult165_g21473 = lerp( Value_Autumn161_g21473 , Value_Winter158_g21473 , TVE_SeasonLerp54_g21473);
				half vertexToFrag11_g21476 = ( ( ( TVE_SeasonOptions_X50_g21473 * lerpResult168_g21473 ) + ( TVE_SeasonOptions_Y51_g21473 * lerpResult167_g21473 ) ) + ( ( TVE_SeasonOptions_Z52_g21473 * lerpResult166_g21473 ) + ( TVE_SeasonOptions_W53_g21473 * lerpResult165_g21473 ) ) );
				o.ase_texcoord1.x = vertexToFrag11_g21476;
				
				o.ase_texcoord1.yz = v.ase_texcoord.xy;
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				half Value_Main157_g21473 = _MainValue;
				half vertexToFrag11_g21476 = i.ase_texcoord1.x;
				half Value_Seasons1721_g21473 = vertexToFrag11_g21476;
				half Element_Mode55_g21473 = _ElementMode;
				half lerpResult181_g21473 = lerp( Value_Main157_g21473 , Value_Seasons1721_g21473 , Element_Mode55_g21473);
				half4 MainTex_RGBA587_g21473 = tex2D( _MainTex, ( ( ( 1.0 - i.ase_texcoord1.yz ) * (_MainUVs).xy ) + (_MainUVs).zw ) );
				half3 temp_cast_0 = (0.0001).xxx;
				half3 temp_cast_1 = (0.9999).xxx;
				half3 clampResult17_g21483 = clamp( (MainTex_RGBA587_g21473).rgb , temp_cast_0 , temp_cast_1 );
				half temp_output_7_0_g21487 = _MainTexColorMinValue;
				half3 temp_cast_2 = (temp_output_7_0_g21487).xxx;
				half temp_output_10_0_g21487 = ( _MainTexColorMaxValue - temp_output_7_0_g21487 );
				half3 temp_output_1765_0_g21473 = saturate( ( ( clampResult17_g21483 - temp_cast_2 ) / temp_output_10_0_g21487 ) );
				half Element_Remap_R73_g21473 = (temp_output_1765_0_g21473).x;
				half4 _ElementParams_Instance = UNITY_ACCESS_INSTANCED_PROP(_ElementParams_arr, _ElementParams);
				half Element_Params_R1738_g21473 = _ElementParams_Instance.x;
				half Element_Value1744_g21473 = ( Element_Remap_R73_g21473 * Element_Params_R1738_g21473 * i.ase_color.r );
				half Final_Extras_RGB213_g21473 = ( lerpResult181_g21473 * Element_Value1744_g21473 );
				half clampResult17_g21482 = clamp( (MainTex_RGBA587_g21473).a , 0.0001 , 0.9999 );
				half temp_output_7_0_g21484 = _MainTexAlphaMinValue;
				half temp_output_10_0_g21484 = ( _MainTexAlphaMaxValue - temp_output_7_0_g21484 );
				half Element_Remap_A74_g21473 = saturate( ( ( clampResult17_g21482 - temp_output_7_0_g21484 ) / ( temp_output_10_0_g21484 + 0.0001 ) ) );
				half Element_Params_A1737_g21473 = _ElementParams_Instance.w;
				half4 Colors37_g21490 = TVE_ColorsCoords;
				half4 Extras37_g21490 = TVE_ExtrasCoords;
				half4 Motion37_g21490 = TVE_MotionCoords;
				half4 Vertex37_g21490 = TVE_VertexCoords;
				half4 localIS_ELEMENT37_g21490 = IS_ELEMENT( Colors37_g21490 , Extras37_g21490 , Motion37_g21490 , Vertex37_g21490 );
				half4 temp_output_35_0_g21494 = localIS_ELEMENT37_g21490;
				half temp_output_7_0_g21495 = TVE_ElementsFadeValue;
				half2 temp_cast_3 = (temp_output_7_0_g21495).xx;
				half temp_output_10_0_g21495 = ( 1.0 - temp_output_7_0_g21495 );
				half2 temp_output_16_0_g21488 = saturate( ( ( abs( (( (temp_output_35_0_g21494).zw + ( (temp_output_35_0_g21494).xy * (WorldPosition).xz ) )*2.002 + -1.001) ) - temp_cast_3 ) / temp_output_10_0_g21495 ) );
				half2 temp_output_12_0_g21488 = ( temp_output_16_0_g21488 * temp_output_16_0_g21488 );
				half lerpResult842_g21473 = lerp( 1.0 , ( 1.0 - saturate( ( (temp_output_12_0_g21488).x + (temp_output_12_0_g21488).y ) ) ) , _ElementVolumeFadeMode);
				half Element_Alpha56_g21473 = ( _ElementIntensity * Element_Remap_A74_g21473 * Element_Params_A1737_g21473 * i.ase_color.a * lerpResult842_g21473 );
				half Final_Extras_A241_g21473 = Element_Alpha56_g21473;
				half4 appendResult1732_g21473 = (half4(0.0 , 0.0 , Final_Extras_RGB213_g21473 , Final_Extras_A241_g21473));
				
				
				finalColor = appendResult1732_g21473;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "TVEShaderElementGUI"
	
	Fallback Off
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.FunctionNode;131;-256,-1536;Inherit;False;Define Element Motion;67;;19304;6eebc31017d99e84e811285e6a5d199d;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;80,-1280;Half;False;True;-1;2;TVEShaderElementGUI;0;5;BOXOPHOBIC/The Vegetation Engine/Elements/Default/Motion Wind Power;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;;10;False;;0;5;False;;10;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;2;False;;True;True;False;False;True;False;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;2;False;;True;0;False;;True;False;0;False;;0;False;;True;4;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;PreviewType=Plane;DisableBatching=True=DisableBatching;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.RangedFloatNode;120;128,-1536;Half;False;Property;_Message;Message;1;0;Create;True;0;0;0;True;1;StyledMessage(Info, Use the Wind Power elements to control the global wind power. Element Texture R and Particle Color R are used as value multipliers. Element Texture A and Particle Color A are used as element as alpha mask., 0,0);False;0;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;0,-1536;Half;False;Property;_Banner;Banner;0;0;Create;True;0;0;0;False;1;StyledBanner(Motion Power Element);False;0;0;1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;189;384,-1536;Inherit;False;Compile All Elements;-1;;21472;5302407fc6d65554791e558e67d59358;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;190;-256,-1280;Inherit;False;Base Element;2;;21473;0e972c73cae2ee54ea51acc9738801d0;4,477,2,478,0,145,3,481,0;0;1;FLOAT4;0
WireConnection;0;0;190;0
ASEEND*/
//CHKSM=06EA4FB7255FF23C67493B78445E29C2C3792528