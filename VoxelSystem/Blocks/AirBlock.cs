//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2016 - A Dork of Pork, All rights reserved.
//////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelSystem.Blocks
{
    class AirBlock : Block
    {
        #region Properties

        public override bool IsSolid
        {
            get
            {
                return false;
            }
        }

        public override bool IsTransparent
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "Base.Air";
            }
        }

        #endregion

        #region Methods

        public override int GetTextureID(Face face)
        {
            return -1;
        }

        #endregion
    }
}
