//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2016 - A Dork of Pork, All rights reserved.
//////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace VoxelSystem.Blocks
{

    /// <summary>
    /// A base container for Block Types
    /// </summary>
    public abstract class Block
    {
        #region Types

        /// <summary>
        /// An enum containing all 6 possible Block faces
        /// </summary>
        public enum Face
        {
            XMinus,
            XPositive,
            YMinus,
            YPositive,
            ZMinus,
            ZPositive
        }

        #endregion

        #region Static

        #region Fields

        /// <summary>
        /// A static List containing every registered Block
        /// </summary>
        static List<Block> Blocks = new List<Block>();

        #endregion

        #region Methods

        /// <summary>
        /// Registers a Block to the VoxelSystem
        /// </summary>
        /// <param name="block">The Block to register</param>
        /// <returns>A unique integer ID to associate the Block with</returns>
        public static ushort RegisterBlock(Block block)
        {
            ushort id = (ushort)Blocks.Count;

            block.ID = id;

            Blocks.Add(block);

            return id;
        }

        /// <summary>
        /// Gets a registered Block in the Voxel System via ID
        /// </summary>
        /// <param name="id">The ID of the Block</param>
        /// <returns>The Block with the given ID</returns>
        public static Block GetBlock(ushort id)
        {
            return Blocks[id];
        }

        /// <summary>
        /// Gets a registered Block in the Voxel System via ID
        /// </summary>
        /// <param name="name">The Name of the Block</param>
        /// <returns>The Block with the given Name or null if no Block was found</returns>
        public static Block GetBlock(string name)
        {
            name = name.ToLower();

            foreach(Block b in Blocks)
            {
                if (b.Name.ToLower() == name)
                {
                    return b;
                }
            }

            return null;
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the Block
        /// </summary>
        public ushort ID { get; private set; }

        /// <summary>
        /// Gets whether the Block is transparent or not
        /// </summary>
        public abstract bool IsTransparent { get; }

        /// <summary>
        /// Gets whether the Block is solid or not
        /// </summary>
        public abstract bool IsSolid { get; }

        /// <summary>
        /// Gets the name of the Block
        /// </summary>
        public abstract string Name { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the TextureID dependant on the Block's face
        /// </summary>
        /// <param name="face">The face to retrieve the ID from</param>
        /// <returns>An ID associated with the texture</returns>
        public abstract int GetTextureID(Face face);

        #endregion
    }
}
