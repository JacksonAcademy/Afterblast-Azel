﻿using UnityEngine;

namespace Kamgam.TriplanarShader
{
    public class DemoAPISimpleTMB : MonoBehaviour
    {
        public TriplanarMaterialSimpleTMB TriMat;

        public Texture[] Textures;

        public void ResetProperties()
        {
            TriMat.ResetProperties();
        }

        public void RandomColor()
        {
            TriMat.SetColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomTopColor()
        {
            TriMat.SetTopColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomMiddleColor()
        {
            TriMat.SetMainColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomBottomColor()
        {
            TriMat.SetBottomColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomEmissionColor()
        {
            TriMat.SetEmission(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void EmissionIntensity(float value)
        {
            TriMat.SetEmissionIntensity(value);
        }

        public void Blend(float value)
        {
            TriMat.SetBlend(value);
        }

        public void TopSkew(float value)
        {
            TriMat.SetTopSkew(value);
        }

        public void Metallic(float value)
        {
            TriMat.SetTopMetallic(value);
            TriMat.SetMainMetallic(value);
            TriMat.SetBottomMetallic(value);
        }

        public void Smoothness(float value)
        {
            TriMat.SetTopSmoothness(value);
            TriMat.SetMainSmoothness(value);
            TriMat.SetBottomSmoothness(value);
        }

        public void Offset(float value)
        {
            TriMat.SetTopOffset(new Vector2(value, value));
            TriMat.SetMainOffset(new Vector2(value, value));
            TriMat.SetBottomOffset(new Vector2(value, value));
        }

        public void Tiling(float value)
        {
            TriMat.SetTopTiling(new Vector2(value, value));
            TriMat.SetMainTiling(new Vector2(value, value));
            TriMat.SetBottomTiling(new Vector2(value, value));
        }

        int textureIndex = 0;

        public void RandomTexture()
        {
            textureIndex++;
            int index = textureIndex % Textures.Length;

            TriMat.SetMainTexture(Textures[index]);

            index = (textureIndex + 1) % Textures.Length;

            TriMat.SetTopTexture(Textures[index]);

            index = (textureIndex + 1) % Textures.Length;

            TriMat.SetBottomTexture(Textures[index]);
        }
    }
}