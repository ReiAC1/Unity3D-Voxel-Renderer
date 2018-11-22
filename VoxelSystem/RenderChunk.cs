//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2016 - A Dork of Pork, All rights reserved.
//////////////////////////////////////////////////////////////////////

using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using VoxelSystem.DataStorage;

namespace VoxelSystem
{
    /// <summary>
    /// A Component that handles a graphical slice of a Chunk
    /// </summary>
    public class RenderChunk : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// A field stating whether we must refresh the Mesh or not.
        /// </summary>
        bool Refresh = false;

        /// <summary>
        /// A field stating whether we are currently refreshing the Mesh or not.
        /// </summary>
        bool Refreshing = false;

        public int ChunkY = 0;
        public int ViewDistance = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Parent Chunk to get the model slice from
        /// </summary>
        public Chunk Parent { get; private set; }

        /// <summary>
        /// Gets the Y Based slice index of the Parent Chunk
        /// </summary>
        public int Slice { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Flags the Render Chunk to be refreshed
        /// </summary>
        public void RefreshMesh()
        {
            // TODO: Empty slices are still sometimes used to render blocks, so we cannot just ignore rendering them
            //if (Parent.EmptySlices[Slice])
            //{
            //    return;
            //}

            Refresh = true;
        }

        /// <summary>
        /// Initializes the Render Chunk with a Parent and Slice
        /// </summary>
        /// <param name="parent">The Parent Chunk</param>
        /// <param name="slice">The Y Based slice index</param>
        public void Setup(Chunk parent, int slice)
        {
            Parent = parent;
            Slice = slice;
        }

        /// <summary>
        /// Initializes the Game Object to be compatible with the Render Chunk
        /// </summary>
        void Start()
        {
            gameObject.AddComponent<MeshFilter>().mesh = new Mesh();
            gameObject.AddComponent<MeshRenderer>().material = Parent.Material;
        }

        /// <summary>
        /// Attemps to refresh the Mesh if needed as well as collects the finalized Mesh data when needed
        /// </summary>
        void Update()
        {
            if (Refresh && Mathf.Abs(ChunkY - Slice) <= ViewDistance)
            {
                if (!Parent.GeneratingMeshData && Parent.GeneratedVoxels && Parent.NeighborsGeneratedVoxels)
                {
                    Refreshing = true;
                    Refresh = false;
                    Parent.GenerateMesh(Slice);
                }
            }

            if (Refreshing && Parent.GeneratedMeshData && !Parent.GeneratingMeshData)
            {
                Parent.SetMeshData(GetComponent<MeshFilter>().mesh);
                Refreshing = false;
            }
        }

        #endregion
    }
}
