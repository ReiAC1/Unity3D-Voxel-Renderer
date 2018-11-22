//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2018 - Johannes Cronje, All rights reserved.
//////////////////////////////////////////////////////////////////////

using VoxelSystem.DataStorage;

namespace VoxelSystem.Generation
{
    /// <summary>
    /// An Interface to Generate Chunk data with
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Gets a Voxel at a specific world position
        /// </summary>
        /// <param name="x">The X position in the world</param>
        /// <param name="y">The Y position in the world</param>
        /// <param name="z">The Z position in the world</param>
        /// <returns>A Voxel at the given location</returns>
        Voxel GetValue(double x, double y, double z);
    }
}
