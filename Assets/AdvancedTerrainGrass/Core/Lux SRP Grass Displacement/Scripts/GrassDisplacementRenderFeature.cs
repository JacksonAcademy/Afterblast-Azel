﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR;

namespace Lux_SRP_GrassDisplacement
{
    public class GrassDisplacementRenderFeature : UnityEngine.Rendering.Universal.ScriptableRendererFeature
    {
        
        [System.Serializable]
        public enum RTDisplacementSize {
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024
        }

        [System.Serializable]
        public class GrassDisplacementSettings
        {
            public RTDisplacementSize Resolution = RTDisplacementSize._256;
            public float Size = 20.0f;
            public bool ShiftRenderTex = false;
        }

        public GrassDisplacementSettings settings = new GrassDisplacementSettings();
        GrassDisplacementPass m_GrassDisplacementPass;
        
        public override void Create()
        {
            m_GrassDisplacementPass = new GrassDisplacementPass();
            m_GrassDisplacementPass.renderPassEvent = RenderPassEvent.BeforeRendering; //BeforeRenderingShadows;

        //  Apply settings
            m_GrassDisplacementPass.m_Resolution = (int)settings.Resolution;
            m_GrassDisplacementPass.m_Size = settings.Size;
            m_GrassDisplacementPass.m_ShiftRenderTex = settings.ShiftRenderTex;
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_GrassDisplacementPass.Setup(renderingData);
            renderer.EnqueuePass(m_GrassDisplacementPass);
        }
    }




//  ---------------------------------------------------------
//  The Pass

    public class GrassDisplacementPass : ScriptableRenderPass
    {
        
        private const string ProfilerTag = "Render Lux Grass Displacement FX";
        private static ProfilingSampler m_ProfilingSampler = new(ProfilerTag);

        ShaderTagId m_GrassDisplacementFXShaderTag = new ShaderTagId("LuxGrassDisplacementFX");

        private SinglePassStereoMode m_StereoRenderingMode;

    //  There is no 0.5 in 8bit colors...
        Color m_ClearColor = new Color(127.0f/255.0f, 127.0f/255.0f,1,1);

        private RTHandle m_GrassDisplacementFX;

        private Matrix4x4 projectionMatrix;
        private Matrix4x4 worldToCameraMatrix;

        public float m_Size = 20.0f;
        public int m_Resolution = 256;
        public bool m_ShiftRenderTex = false;

        private float stepSize;
        private float oneOverStepSize;

        private Vector4 posSize = Vector4.zero;
        private static readonly int DisplacementTexPosSizePID = Shader.PropertyToID("_Lux_DisplacementPosition");
        private static readonly int _Lux_DisplacementRT = Shader.PropertyToID("_Lux_DisplacementRT");

        private FilteringSettings transparentFilterSettings { get; set; }

        private RenderTextureDescriptor descriptor;


#if !UNITY_2022_1_OR_NEWER
        internal static bool RTHandleNeedsReAlloc(
            RTHandle handle,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode,
            TextureWrapMode wrapMode,
            bool isShadowMap,
            int anisoLevel,
            float mipMapBias,
            string name,
            bool scaled)
        {
            if (handle == null || handle.rt == null)
                return true;
            if (handle.useScaling != scaled)
                return true;
            if (!scaled && (handle.rt.width != descriptor.width || handle.rt.height != descriptor.height))
                return true;
            return
                handle.rt.descriptor.depthBufferBits != descriptor.depthBufferBits ||
                (handle.rt.descriptor.depthBufferBits == (int)DepthBits.None && !isShadowMap && handle.rt.descriptor.graphicsFormat != descriptor.graphicsFormat) ||
                handle.rt.descriptor.dimension != descriptor.dimension ||
                handle.rt.descriptor.enableRandomWrite != descriptor.enableRandomWrite ||
                handle.rt.descriptor.useMipMap != descriptor.useMipMap ||
                handle.rt.descriptor.autoGenerateMips != descriptor.autoGenerateMips ||
                handle.rt.descriptor.msaaSamples != descriptor.msaaSamples ||
                handle.rt.descriptor.bindMS != descriptor.bindMS ||
                handle.rt.descriptor.useDynamicScale != descriptor.useDynamicScale ||
                handle.rt.descriptor.memoryless != descriptor.memoryless ||
                handle.rt.filterMode != filterMode ||
                handle.rt.wrapMode != wrapMode ||
                handle.rt.anisoLevel != anisoLevel ||
                handle.rt.mipMapBias != mipMapBias ||
                handle.name != name;
        }

        public static bool ReAllocateIfNeeded(
            ref RTHandle handle,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
        {
            if (RTHandleNeedsReAlloc(handle, descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name, false))
            {
                handle?.Release();
                //handle = RTHandles.Alloc(descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
                handle = RTHandles.Alloc(
                    descriptor.width,
                    descriptor.height,
                    1,
                    (DepthBits)descriptor.depthBufferBits,
                    GraphicsFormat.R8G8B8A8_SRGB, //descriptor.colorFormat,
                    filterMode,
                    wrapMode,
                    descriptor.dimension,
                    descriptor.enableRandomWrite,
                    descriptor.useMipMap,
                    descriptor.autoGenerateMips,
                    isShadowMap,
                    anisoLevel,
                    mipMapBias,
                    (MSAASamples)descriptor.msaaSamples,
                    false, //descriptor.bindTextureMS,
                    descriptor.useDynamicScale,
                    descriptor.memoryless,
                    name
                );
                return true;
            }
            return false;
        }
#endif


        public void Setup(in RenderingData renderingData)
        {
            descriptor = new RenderTextureDescriptor(m_Resolution, m_Resolution);
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.Default;
            descriptor.dimension = TextureDimension.Tex2D;

#if !UNITY_2022_1_OR_NEWER
            ReAllocateIfNeeded(ref m_GrassDisplacementFX, descriptor, name: "_Lux_DisplacementRT");
#else 
            RenderingUtils.ReAllocateIfNeeded(ref m_GrassDisplacementFX, descriptor, name: "_Lux_DisplacementRT");
#endif 

            Shader.SetGlobalTexture(_Lux_DisplacementRT, m_GrassDisplacementFX.rt);

        //  Set up all constants
            stepSize = m_Size / (float)m_Resolution;
            oneOverStepSize = 1.0f / stepSize;
            var halfSize = m_Size  * 0.5f;
            projectionMatrix = Matrix4x4.Ortho(-halfSize, halfSize, -halfSize, halfSize, 0.1f, 80.0f);
            projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
            worldToCameraMatrix.SetRow(0, new Vector4(1,0,0,0) ); //last is x pos
            worldToCameraMatrix.SetRow(1, new Vector4(0,0,1,0) ); //last is z pos
            worldToCameraMatrix.SetRow(2, new Vector4(0,1,0,0) ); //last is y pos
            worldToCameraMatrix.SetRow(3, new Vector4(0,0,0,1) );

            transparentFilterSettings = new FilteringSettings(RenderQueueRange.transparent);

        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_GrassDisplacementFX);
            ConfigureClear(ClearFlag.Color, m_ClearColor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.Clear();

                var drawSettings = CreateDrawingSettings(m_GrassDisplacementFXShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                var filteringSettings = transparentFilterSettings;

                var camera = renderingData.cameraData.camera;
                var cameraTransform = camera.transform;
                var cameraPos = cameraTransform.position;

#if ENABLE_VR && ENABLE_XR_MODULE
                var isStereoEnabled = renderingData.cameraData.xr.enabled; //isStereoEnabled; //
                if (isStereoEnabled) {
                    m_StereoRenderingMode = XRSettings.stereoRenderingMode;
                    cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                }
#endif

            //  Push cameraPos forward – if enabled    
                var camForward = cameraTransform.forward;
                // unstable
                // cameraPos.x += camForward.x * m_Size * 0.5f;
                // cameraPos.z += camForward.z * m_Size * 0.5f;
                if (m_ShiftRenderTex) {
                    var t_camForward = new Vector2(camForward.x, camForward.z);
                    t_camForward.Normalize();
                //  still rather unstable...
                    cameraPos.x += t_camForward.x * m_Size * 0.33f;
                    cameraPos.z += t_camForward.y * m_Size * 0.33f;
                }
            
            //  Store original Camera matrices
                var worldToCameraMatrixOrig = camera.worldToCameraMatrix;
                var projectionMatrixOrig = camera.projectionMatrix;

            //  Quantize movement to fit texel size of RT – this stabilzes the final visual result
                Vector2 positionRT = Vector2.zero; // bad
                positionRT.x = Mathf.Floor(cameraPos.x * oneOverStepSize) * stepSize;
                positionRT.y = Mathf.Floor(cameraPos.z * oneOverStepSize) * stepSize;

            //  Update the custom worldToCameraMatrix – we only have to update the translation/position
                worldToCameraMatrix.SetColumn(3, new Vector4(-positionRT.x, -positionRT.y, -cameraPos.y - 40.0f, 1) );
                cmd.SetViewProjectionMatrices(worldToCameraMatrix, projectionMatrix);
            
            //  ---------------------------------------------------------
            //  Calc and set grass shader params
                posSize.x = positionRT.x - m_Size * 0.5f;
                posSize.y = positionRT.y - m_Size * 0.5f;
                posSize.z = 1.0f / m_Size ;
                cmd.SetGlobalVector(DisplacementTexPosSizePID, posSize );

            //  ---------------------------------------------------------
            //  Call execute
                context.ExecuteCommandBuffer(cmd);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

            //  ---------------------------------------------------------
            //  Restore Camera matrices
                cmd.Clear();
                cmd.SetViewProjectionMatrices(worldToCameraMatrixOrig, projectionMatrixOrig);
#if ENABLE_VR && ENABLE_XR_MODULE
                if (isStereoEnabled) {
                    cmd.SetSinglePassStereo(m_StereoRenderingMode);
                }

            //  ---------------------------------------------------------
            //  Call execute a 2nd time
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
#endif
            }
            CommandBufferPool.Release(cmd);
        }
    }
}