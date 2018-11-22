using System;
using System.Collections.Generic;

using UnityEngine;

namespace VoxelSystem
{
    /// <summary>
    /// Texture Management Class
    /// </summary>
    public static class TextureManager
    {
        #region Fields

        /// <summary>
        /// The Width/Height of each Texture
        /// </summary>
        public const int TextureSize = 32;

        /// <summary>
        /// A List of each sub texture
        /// </summary>
        static List<Texture2D> SubTextures = new List<Texture2D>();

        /// <summary>
        /// A Texture Array that contains every defined Texture.
        /// </summary>
        public static Texture2DArray Texture = null;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a texture to the Array list
        /// </summary>
        /// <param name="texture">The texture to add</param>
        /// <returns>An ID associated with the newly added texture</returns>
        public static int AddTexture(Texture2D texture)
        {
            if (texture.width != texture.height && texture.width != TextureSize)
            {
                throw new Exception("Texture's width and height must both be " + TextureSize + " pixels!");
            }

            SubTextures.Add(texture);

            return SubTextures.Count - 1;
        }

        /// <summary>
        /// Gets a Texture ID by name
        /// </summary>
        /// <param name="name">The name of the Texture</param>
        /// <returns>The ID associated with the name or -1 if none was found</returns>
        public static int FindTextureByName(string name)
        {
            name = name.ToLower();

            int i = 0;

            foreach(Texture2D t in SubTextures)
            {
                if (t.name.ToLower() == name)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        /// <summary>
        /// Creates the 2D Texture Array
        /// </summary>
        public static void Build()
        {
            if (Texture != null)
            {
                GameObject.Destroy(Texture);
            }

            Texture = new Texture2DArray(TextureSize, TextureSize, SubTextures.Count, TextureFormat.RGBA32, false);

            int i = 0;

            foreach(Texture2D t in SubTextures)
            {
                Texture.SetPixels(t.GetPixels(), i, 0);

                i++;
            }

            Texture.filterMode = FilterMode.Point;

            Texture.Apply();
        }

        #endregion
    }
}
