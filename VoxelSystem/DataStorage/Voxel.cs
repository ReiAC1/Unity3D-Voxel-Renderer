//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2018 - Johannes Cronje, All rights reserved.
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace VoxelSystem.DataStorage
{
    /// <summary>
    /// A Voxel will contain unique per-world block data.
    /// </summary>
    public struct Voxel
    {
        #region Fields

        /// <summary>
        /// The BlockID of the base block info
        /// </summary>
        public ushort BlockID;

        /// <summary>
        /// The higher the LightLevel, the brighter the block will be rendered as
        /// </summary>
        public byte LightLevel;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the Voxel with a BlockID and a LightLevel
        /// </summary>
        /// <param name="blockID">The ID of the Block</param>
        /// <param name="lightLevel">The amount of Light on the Block</param>
        public Voxel(ushort blockID, byte lightLevel)
        {
            BlockID = blockID;
            LightLevel = lightLevel;
        }

        #endregion
    }
}