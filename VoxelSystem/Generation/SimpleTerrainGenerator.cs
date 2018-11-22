//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2018 - Johannes Cronje, All rights reserved.
//////////////////////////////////////////////////////////////////////

using UnityEngine;

using LibNoise;
using LibNoise.Modifiers;

using VoxelSystem.DataStorage;

namespace VoxelSystem.Generation
{
    /// <summary>
    /// A Class that handles generating simple terrain. (Will be removed in final product)
    /// </summary>
    public class SimpleTerrainGenerator : IGenerator
    {
        Perlin Perlin;
        RidgedMultifractal MF;
        Blend Blend;

        public SimpleTerrainGenerator()
        {
            Perlin = new Perlin();
            MF = new RidgedMultifractal();

            Blend = new Blend(Perlin, MF, new LibNoise.Modifiers.Constant(.5));
        }

        public Voxel GetValue(double x, double y, double z)
        {
            Voxel a = GetVoxel(x, y, z);

            if (a.BlockID == 1)
            {
                if (y < Chunk.Height - 1)
                {
                    Voxel b = GetVoxel(x, y + 1, z);

                    if (b.BlockID == 0)
                    {
                        a.BlockID = 2;
                    }
                }
                else
                {
                    a.BlockID = 2;
                }
            }

            return a;
        }

        Voxel GetVoxel(double x, double y, double z)
        {
            double density = ((128 - y) / 32) + Blend.GetValue(x * 0.01, y * 0.01, z * 0.01);

            double ridged = MF.GetValue(x * 0.01, y * 0.01, z * 0.01);

            if (ridged > 1.1)
            {
                density = -1;
            }

            if (density > 0)
            {
                return new Voxel(1, 15);
            }

            return new Voxel(0, 15);
        }
    }
}
