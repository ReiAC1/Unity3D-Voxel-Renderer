//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2018 - Johannes Cronje, All rights reserved.
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

using VoxelSystem.Blocks;
using VoxelSystem.DataStorage;
using VoxelSystem.Generation;

namespace VoxelSystem
{
    /// <summary>
    /// A Unity Component that handles Voxel Terrain
    /// </summary>
    public class VoxelTerrainHandler : MonoBehaviour
    {
        #region WYSIWYG

        /// <summary>
        /// The radius of Chunks to draw
        /// </summary>
        public int ChunkViewRadius = 5;

        /// <summary>
        /// The Main Transform to base Chunk creation around
        /// </summary>
        public Transform MainTransform;


        /// <summary>
        /// The Chunk Material to render with
        /// </summary>
        public Material ChunkMaterial;

        /// <summary>
        /// A List of all available textures
        /// </summary>
        public Texture2D[] TextureList;

        /// <summary>
        /// The Voxel Terrain Generator
        /// </summary>
        public IGenerator Generator;

        #endregion

        #region Fields

        List<Chunk> Chunks = new List<Chunk>();

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the VoxelTerrainHandler
        /// </summary>
        void Start()
        {
            int a, b;
            System.Threading.ThreadPool.GetMinThreads(out a, out b);
            System.Threading.ThreadPool.SetMaxThreads(a, b);

            foreach(Texture2D t in TextureList)
            {
                TextureManager.AddTexture(t);
            }

            TextureManager.Build();

            Block.RegisterBlock(new AirBlock());
            Block.RegisterBlock(new DirtBlock(TextureManager.FindTextureByName("dirt")));
            Block.RegisterBlock(new GrassBlock(TextureManager.FindTextureByName("grass"), TextureManager.FindTextureByName("grass side")));

            ChunkMaterial.SetTexture("_TexArray", TextureManager.Texture);

            Generator = new SimpleTerrainGenerator();
        }

        /// <summary>
        /// This update handles creation/deletion and updates of chunks
        /// </summary>
        void Update()
        {
            Vector2 centerChunk = new Vector2(Mathf.Floor(MainTransform.position.x / Chunk.Size), Mathf.Floor(MainTransform.position.z / Chunk.Size));
            int centerY = Mathf.FloorToInt(MainTransform.position.y) / Chunk.Size;
            int creationAmount = 0;
            int maxCreations = 3;

            for (int i = 0; i < Chunks.Count; i++)
            {
                if (Vector2.Distance(Chunks[i].Position, centerChunk) > ChunkViewRadius)
                {
                    Chunks[i].Dispose();
                    Chunks.RemoveAt(i);
                    i--;
                }
                else
                {
                    Chunks[i].Update(centerY, ChunkViewRadius);
                }
            }

            for (int x = -ChunkViewRadius; x <= ChunkViewRadius; x++)
            {
                if (creationAmount > maxCreations)
                {
                    break;
                }

                for (int z = -ChunkViewRadius; z <= ChunkViewRadius; z++)
                {
                    if (creationAmount > maxCreations)
                    {
                        break;
                    }

                    if (Vector2.Distance(new Vector2(x, z) + centerChunk, centerChunk) > ChunkViewRadius)
                    {
                        continue;
                    }

                    bool found = false;

                    for (int i = 0; i < Chunks.Count; i++)
                    {
                        Chunk c = Chunks[i];

                        if (c.Position.x == x + centerChunk.x && c.Position.y == z + centerChunk.y)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        creationAmount++;

                        Vector2 pos = new Vector2(x, z) + centerChunk;

                        Chunk c = new Chunk(pos, centerY, Generator, ChunkMaterial);

                        foreach (Chunk c2 in Chunks)
                        {
                            if (Mathf.Abs(c.Position.x - c2.Position.x) <= 1 && Mathf.Abs(c.Position.y - c2.Position.y) <= 1)
                            {
                                c2.AddNeighbor(c);
                                c2.MakeDirty(false);
                            }
                        }

                        Chunks.Add(c);
                    }
                }
            }

        }

        #endregion
    }
}
