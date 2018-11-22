//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2018 - Johannes Cronje, All rights reserved.
//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelSystem.Blocks
{
    class GrassBlock : Block
    {
        #region Fields

        /// <summary>
        /// The TextureID of the Dirt
        /// </summary>
        public int TextureID = 0;
        public int TopID = 0;

        #endregion

        #region Properties

        public override bool IsSolid
        {
            get
            {
                return true;
            }
        }

        public override bool IsTransparent
        {
            get
            {
                return false;
            }
        }

        public override string Name
        {
            get
            {
                return "Base.Grass";
            }
        }

        #endregion

        #region Methods

        public GrassBlock(int top, int id)
        {
            TopID = top;
            TextureID = id;
        }

        public override int GetTextureID(Face face)
        {
            if (face == Face.YPositive)
            {
                return TopID;
            }

            return TextureID;
        }

        #endregion
    }
}
