//////////////////////////////////////////////////////////////////////
/// Copyright (c) 2018 - Johannes Cronje, All rights reserved.
//////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using UnityEngine;

using VoxelSystem.Generation;
using VoxelSystem.Blocks;

namespace VoxelSystem.DataStorage
{
    /// <summary>
    /// A storage container that contains Voxels
    /// </summary>
    public class Chunk
    {
        #region Fields

        /// <summary>
        /// The size of the Chunk, in terms of width and depth
        /// </summary>
        public const int Size = 16;

        /// <summary>
        /// The size of the Chunk, in terms of height
        /// </summary>
        public const int Height = 256;

        /// <summary>
        /// An Array of all the Chunk Renders
        /// </summary>
        public RenderChunk[] Renders = new RenderChunk[Height / Size];

        /// <summary>
        /// The stored Voxel data
        /// </summary>
        Voxel[] Data = new Voxel[Size * Size * Height];

        /// <summary>
        /// A List containing all neighboring Chunks
        /// </summary>
        List<Chunk> Neighbors = new List<Chunk>();

        /// <summary>
        /// Vertex information of the Chunk Mesh
        /// </summary>
        List<Vector3> Vertices = new List<Vector3>();

        /// <summary>
        /// Color information of the Chunk Mesh
        /// </summary>
        List<Color> Colors = new List<Color>();

        /// <summary>
        /// Index information of the Chunk Mesh
        /// </summary>
        List<int> Indices = new List<int>();

        /// <summary>
        /// UV information of the Chunk Mesh
        /// </summary>
        List<Vector3> UVs = new List<Vector3>();

        /// <summary>
        /// A private bool checking to see if lighting was generated
        /// </summary>
        bool LightingGenerated = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets a voxel with the given local coords
        /// </summary>
        /// <param name="x">The X Position of the Voxel</param>
        /// <param name="y">The Y Position of the Voxel</param>
        /// <param name="z">The Z Position of the Voxel</param>
        /// <returns>The Voxel at the given Position</returns>
        public Voxel this[int x, int y, int z]
        {
            get { return Data[x + (y * Size) + (z * Height * Size)]; }

            set { Data[x + (y * Size) + (z * Height * Size)] = value; }
        }

        /// <summary>
        /// The Position of the Chunk (in Chunk units)
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Gets if the Chunk model needs to be re-created
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets if the Chunk has generated Voxels
        /// </summary>
        public bool GeneratedVoxels { get; private set; }

        /// <summary>
        /// Returns whether all neighbors has finished generating voxels or not
        /// </summary>
        public bool NeighborsGeneratedVoxels
        {
            get
            {
                for (int i = 0; i < Neighbors.Count; i++)
                {
                    Chunk c = Neighbors[i];
                    if (!c.GeneratedVoxels) { return false; }
                }

                return true;
            }
        }

        /// <summary>
        /// Gets if the Chunk has generated Mesh data
        /// </summary>
        public bool GeneratedMeshData { get; private set; }

        /// <summary>
        /// Gets whether or not the Chunk is generating it's Mesh
        /// </summary>
        public bool GeneratingMeshData { get; private set; }

        /// <summary>
        /// Gets the Material to render the chunk with
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// Gets an array of bools determining which slices are empty and which contains valid data
        /// </summary>
        public bool[] EmptySlices { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Marks the Chunk as Dirty
        /// </summary>
        /// <param name="includeNeighbors">Whether or not to mark surrounding chunks as dirty, too</param>
        public void MakeDirty(bool includeNeighbors)
        {
            IsDirty = true;

            if (includeNeighbors)
            {
                foreach (Chunk n in Neighbors)
                {
                    n.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Links two Chunks together as neighbors
        /// </summary>
        /// <param name="neighbor">The other Chunk to link as a neighbor</param>
        public void AddNeighbor(Chunk neighbor)
        {
            if (!Neighbors.Contains(neighbor))
            {
                neighbor.Neighbors.Add(this);
            }

            Neighbors.Add(neighbor);
        }

        /// <summary>
        /// Safely disposes of the Chunk and all it's components
        /// </summary>
        public void Dispose()
        {
            GameObject.Destroy(Renders[0].transform.parent.gameObject);
            foreach(RenderChunk c in Renders)
            {
                GameObject.Destroy(c.gameObject);
            }

            for (int i = 0; i < Neighbors.Count; i++)
            {
                Neighbors[i].Neighbors.Remove(this);
            }

            Neighbors.Clear();
        }

        /// <summary>
        /// Updates the Chunk
        /// </summary>
        /// <param name="yChunk">The Chunk in terms of the camera's Y position</param>
        /// <param name="viewDistance">The Chunk Render Distance to draw at</param>
        public void Update(int yChunk, int viewDistance)
        {
            if (viewDistance > 3)
            {
                viewDistance = 3;
            }

            for (int i = 0; i < Renders.Length; i++)
            {
                Renders[i].ChunkY = yChunk;
                Renders[i].ViewDistance = viewDistance;
            }

            if (IsDirty)
            {
                LightingGenerated = false;
                IsDirty = false;

                for (int i = 0; i < Renders.Length; i++)
                {
                    Renders[i].RefreshMesh();
                }
            }
        }

        /// <summary>
        /// Creates a Thread that generates the Voxels inside of the Chunk
        /// </summary>
        /// <param name="generator">The Generator to populate the Chunk with</param>
        public void GenerateChunk(IGenerator generator)
        {
            ThreadPool.QueueUserWorkItem(GenerateChunkThread, generator);
        }

        /// <summary>
        /// Generates a Mesh with the given
        /// </summary>
        /// <param name="slice">Which Y-Axis slice to generate</param>
        public void GenerateMesh(int slice)
        {
            GeneratedMeshData = false;
            GeneratingMeshData = true;

            ThreadPool.QueueUserWorkItem(GenerateMeshThread, slice);
        }

        /// <summary>
        /// Sets the Mesh data to a given Mesh
        /// </summary>
        /// <param name="mesh">The Mesh to give the data to</param>
        public void SetMeshData(Mesh mesh)
        {
            mesh.Clear();

            mesh.vertices = Vertices.ToArray();
            mesh.colors = Colors.ToArray();
            mesh.SetUVs(0, UVs);

            mesh.SetIndices(Indices.ToArray(), MeshTopology.Triangles, 0, true);

            mesh.UploadMeshData(false);

            GeneratedMeshData = false;
        }

        /// <summary>
        /// Gets the light color in accordance to the light level and face given
        /// </summary>
        /// <param name="level">The Light Level</param>
        /// <param name="face">The Block's face</param>
        /// <returns>A Color representing how bright the block's face should be</returns>
        Color GetLightColor(byte level, Block.Face face)
        {
            if (face == Block.Face.YPositive)
            {
                level += 1;
            }
            else if (face == Block.Face.YMinus && level > 1)
            {
                level -= 2;
            }
            else if  (level > 0)
            {
                level -= 1;
            }

            if (level >= 15)
            {
                return Color.white;
            }

            float Light0 = 0.05f;
            float Light14 = 0.85f;

            float light = Mathf.Pow((Light14 - Light0) / 15.0f * level, 2.0f) * 1.25f + 0.07f;

            return new Color(light, light, light);
        }

        /// <summary>
        /// Generates the Voxels inside of the Chunk (Thread Task)
        /// </summary>
        /// <param name="generator">The Generator to populate the Chunk with</param>
        void GenerateChunkThread(object state)
        {
            // convert our state object into the generator object we sent in the GenerateChunk method
            IGenerator generator = (IGenerator)state;

            // create a temp array of voxels to store our data in while we generate
            Voxel[] temp = new Voxel[Data.Length];

            bool[] tempEmptyChunk = new bool[Height / Size];

            for (int  i = 0; i < Height / Size; i++)
            {
                tempEmptyChunk[i] = true;
            }

            ushort id = 1;

            // loop through each axis for the full length of the chunk (looping z/y/x instead of x/y/z is slightly faster, or so I've been told)
            for (int z = 0; z < Size; z++)
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        // Set a Voxel depending on the x/y/z position and the global chunk position. The actual Voxel is generated in the Generator
                        Voxel v = generator.GetValue((Position.x * Size) + x, y, (Position.y * Size) + z);

                        temp[x + (y * Size) + (z * Height * Size)] = v;

                        if ((x == 0 && y == 0 && z == 0) || Mathf.Floor(y / Size) != Mathf.Floor((y - 1) / Size))
                        {
                            id = v.BlockID;
                        }

                        if (v.BlockID != id)
                        {
                            tempEmptyChunk[Mathf.FloorToInt(y / Size)] = false;
                        }
                    }
                }
            }

            for (int y = Height - 2; y >= 0; y--)
            {
                for (int x = 0; x < Size; x++)
                {
                    for (int z = 0; z < Size; z++)
                    {
                        // Get Light (simple for now)
                        byte light = 15;

                        Voxel a = temp[x + ((y + 1) * Size) + (z * Height * Size)];

                        if (Block.GetBlock(a.BlockID).IsTransparent)
                        {
                            light = a.LightLevel;
                        }
                        else
                        {
                            int t = a.LightLevel - 1;

                            if (t < 0) { t = 0; }

                            light = (byte)t;
                        }

                        if (light > 32)
                        {
                            light = 0;
                        }

                        Voxel v = temp[x + (y * Size) + (z * Height * Size)];

                        v.LightLevel = light;

                        temp[x + (y * Size) + (z * Height * Size)] = v;
                    }
                }
            }

            // Only set data AFTER we're fully done with the thread
            Data = temp;

            // Now that we did generate the voxels, set this flag to true, which will then allow us to create mesh data
            GeneratedVoxels = true;

            // Set the Empty Slices
            EmptySlices = tempEmptyChunk;

            for (int i = 0; i < Renders.Length; i++)
            {
                Renders[i].RefreshMesh();
            }
        }

        /// <summary>
        /// Generates a Mesh with the given (Thread Task)
        /// </summary>
        /// <param name="slice">Which Y-Axis slice to generate</param>
        void GenerateMeshThread(object state)
        {
            if (!LightingGenerated)
            {
                try
                {
                    GenerateLighting();
                }
                catch(System.Exception e)
                {
                    Debug.Log(e);
                }
            }

            // Convert state into slice
            int slice = (int)state;

            List<Vector3> tV = new List<Vector3>();
            List<Vector3> tUV = new List<Vector3>();
            List<Color> tC = new List<Color>();
            List<int> tI = new List<int>();

            Voxel current, side;
            Block currentB, sideB;

            int index;

            // loop through each axis for the full length and depth of the chunk, but only do a slice of the Y axis
            for (int z = 0; z < Size; z++)
            {
                for (int y = slice * Size; y < (slice + 1) * Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        current = this[x, y, z];
                        currentB = Block.GetBlock(current.BlockID);

                        #region Check XMinus Face

                        if (x > 0)
                        {
                            side = this[x - 1, y, z];
                        }
                        else
                        {
                            side = new Voxel(1, 15);

                            Chunk c = GetNeighbor(Block.Face.XMinus);

                            if (c != null)
                            {
                                side = c[Size - 1, y, z];
                            }
                        }

                        sideB = Block.GetBlock(side.BlockID);

                        // Create face if needed
                        if (sideB.IsTransparent && currentB.GetTextureID(Block.Face.XMinus) > -1)
                        {
                            index = tV.Count;

                            tV.Add(new Vector3(x, y, z));
                            tV.Add(new Vector3(x, y + 1, z));
                            tV.Add(new Vector3(x, y + 1, z + 1));
                            tV.Add(new Vector3(x, y, z + 1));

                            Color c = GetLightColor(current.LightLevel, Block.Face.XMinus);

                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);

                            tUV.Add(new Vector3(0, 0, currentB.GetTextureID(Block.Face.XMinus)));
                            tUV.Add(new Vector3(0, 1, currentB.GetTextureID(Block.Face.XMinus)));
                            tUV.Add(new Vector3(1, 1, currentB.GetTextureID(Block.Face.XMinus)));
                            tUV.Add(new Vector3(1, 0, currentB.GetTextureID(Block.Face.XMinus)));

                            tI.Add(index); tI.Add(index + 2); tI.Add(index + 1);
                            tI.Add(index); tI.Add(index + 3); tI.Add(index + 2);
                        }

                        #endregion

                        #region Check XPositive Face

                        if (x < Size - 1)
                        {
                            side = this[x + 1, y, z];
                        }
                        else
                        {
                            side = new Voxel(1, 15);

                            Chunk c = GetNeighbor(Block.Face.XPositive);

                            if (c != null)
                            {
                                side = c[0, y, z];
                            }
                        }

                        sideB = Block.GetBlock(side.BlockID);

                        // Create face if needed
                        if (sideB.IsTransparent && currentB.GetTextureID(Block.Face.XPositive) > -1)
                        {

                            index = tV.Count;

                            tV.Add(new Vector3(x + 1, y, z));
                            tV.Add(new Vector3(x + 1, y + 1, z));
                            tV.Add(new Vector3(x + 1, y + 1, z + 1));
                            tV.Add(new Vector3(x + 1, y, z + 1));

                            Color c = GetLightColor(current.LightLevel, Block.Face.XPositive);

                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);

                            tUV.Add(new Vector3(0, 0, currentB.GetTextureID(Block.Face.XPositive)));
                            tUV.Add(new Vector3(0, 1, currentB.GetTextureID(Block.Face.XPositive)));
                            tUV.Add(new Vector3(1, 1, currentB.GetTextureID(Block.Face.XPositive)));
                            tUV.Add(new Vector3(1, 0, currentB.GetTextureID(Block.Face.XPositive)));

                            tI.Add(index); tI.Add(index + 1); tI.Add(index + 2);
                            tI.Add(index); tI.Add(index + 2); tI.Add(index + 3);
                        }

                        #endregion

                        #region Check YMinus Face

                        if (y > 0)
                        {
                            side = this[x, y - 1, z];
                        }
                        else
                        {
                            side = new Voxel(0, 15);
                        }

                        sideB = Block.GetBlock(side.BlockID);

                        // Create face if needed
                        if (sideB.IsTransparent && currentB.GetTextureID(Block.Face.YMinus) > -1)
                        {
                            index = tV.Count;

                            tV.Add(new Vector3(x, y, z));
                            tV.Add(new Vector3(x + 1, y, z));
                            tV.Add(new Vector3(x + 1, y, z + 1));
                            tV.Add(new Vector3(x, y, z + 1));

                            Color c = GetLightColor(current.LightLevel, Block.Face.YMinus);

                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);

                            tUV.Add(new Vector3(0, 0, currentB.GetTextureID(Block.Face.YMinus)));
                            tUV.Add(new Vector3(1, 0, currentB.GetTextureID(Block.Face.YMinus)));
                            tUV.Add(new Vector3(1, 1, currentB.GetTextureID(Block.Face.YMinus)));
                            tUV.Add(new Vector3(0, 1, currentB.GetTextureID(Block.Face.YMinus)));

                            tI.Add(index); tI.Add(index + 1); tI.Add(index + 2);
                            tI.Add(index); tI.Add(index + 2); tI.Add(index + 3);
                        }


                        #endregion

                        #region Check YPositive Face

                        if (y < Height - 1)
                        {
                            side = this[x, y + 1, z];
                        }
                        else
                        {
                            side = new Voxel(0, 15);
                        }

                        sideB = Block.GetBlock(side.BlockID);

                        // Create face if needed
                        if (sideB.IsTransparent && currentB.GetTextureID(Block.Face.YPositive) > -1)
                        {
                            index = tV.Count;

                            tV.Add(new Vector3(x, y + 1, z));
                            tV.Add(new Vector3(x + 1, y + 1, z));
                            tV.Add(new Vector3(x + 1, y + 1, z + 1));
                            tV.Add(new Vector3(x, y + 1, z + 1));

                            Color c = GetLightColor(current.LightLevel, Block.Face.YPositive);

                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);

                            tUV.Add(new Vector3(0, 0, currentB.GetTextureID(Block.Face.YPositive)));
                            tUV.Add(new Vector3(1, 0, currentB.GetTextureID(Block.Face.YPositive)));
                            tUV.Add(new Vector3(1, 1, currentB.GetTextureID(Block.Face.YPositive)));
                            tUV.Add(new Vector3(0, 1, currentB.GetTextureID(Block.Face.YPositive)));

                            tI.Add(index); tI.Add(index + 2); tI.Add(index + 1);
                            tI.Add(index); tI.Add(index + 3); tI.Add(index + 2);
                        }

                        #endregion

                        #region Check ZMinus Face

                        if (z > 0)
                        {
                            side = this[x, y, z - 1];
                        }
                        else
                        {
                            side = new Voxel(1, 15);

                            Chunk c = GetNeighbor(Block.Face.ZMinus);

                            if (c != null)
                            {
                                side = c[x, y, Size - 1];
                            }
                        }

                        sideB = Block.GetBlock(side.BlockID);

                        // Create face if needed
                        if (sideB.IsTransparent && currentB.GetTextureID(Block.Face.ZMinus) > -1)
                        {
                            index = tV.Count;

                            tV.Add(new Vector3(x, y, z));
                            tV.Add(new Vector3(x, y + 1, z));
                            tV.Add(new Vector3(x + 1, y + 1, z));
                            tV.Add(new Vector3(x + 1, y, z));

                            Color c = GetLightColor(current.LightLevel, Block.Face.ZMinus);

                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);

                            tUV.Add(new Vector3(0, 0, currentB.GetTextureID(Block.Face.ZMinus)));
                            tUV.Add(new Vector3(0, 1, currentB.GetTextureID(Block.Face.ZMinus)));
                            tUV.Add(new Vector3(1, 1, currentB.GetTextureID(Block.Face.ZMinus)));
                            tUV.Add(new Vector3(1, 0, currentB.GetTextureID(Block.Face.ZMinus)));

                            tI.Add(index); tI.Add(index + 1); tI.Add(index + 2);
                            tI.Add(index); tI.Add(index + 2); tI.Add(index + 3);
                        }

                        #endregion

                        #region Check ZPositive Face

                        if (z < Size - 1)
                        {
                            side = this[x, y, z + 1];
                        }
                        else
                        {
                            side = new Voxel(1, 15);

                            Chunk c = GetNeighbor(Block.Face.ZPositive);

                            if (c != null)
                            {
                                side = c[x, y, 0];
                            }
                        }

                        sideB = Block.GetBlock(side.BlockID);

                        // Create face if needed
                        if (sideB.IsTransparent && currentB.GetTextureID(Block.Face.ZPositive) > -1)
                        {
                            index = tV.Count;

                            tV.Add(new Vector3(x, y, z + 1));
                            tV.Add(new Vector3(x, y + 1, z + 1));
                            tV.Add(new Vector3(x + 1, y + 1, z + 1));
                            tV.Add(new Vector3(x + 1, y, z + 1));

                            Color c = GetLightColor(current.LightLevel, Block.Face.ZPositive);

                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);
                            tC.Add(c);


                            tUV.Add(new Vector3(0, 0, currentB.GetTextureID(Block.Face.ZPositive)));
                            tUV.Add(new Vector3(0, 1, currentB.GetTextureID(Block.Face.ZPositive)));
                            tUV.Add(new Vector3(1, 1, currentB.GetTextureID(Block.Face.ZPositive)));
                            tUV.Add(new Vector3(1, 0, currentB.GetTextureID(Block.Face.ZPositive)));

                            tI.Add(index); tI.Add(index + 2); tI.Add(index + 1);
                            tI.Add(index); tI.Add(index + 3); tI.Add(index + 2);
                        }

                        #endregion

                    }
                }
            }

            Vertices = tV;
            Colors = tC;
            Indices = tI;
            UVs = tUV;

            GeneratedMeshData = true;
            GeneratingMeshData = false;
        }

        /// <summary>
        /// Generates Lighting (TODO: Make this run faster, somehow...)
        /// </summary>
        void GenerateLighting()
        {
            LightingGenerated = true;

            for (int y = Height - 2; y >= 0; y--)
            {
                #region X Sweep

                for (int z = 0; z < Size; z++)
                {
                    #region X- Sweep

                    for (int x = 0; x < Size; x++)
                    {
                        Voxel current = this[x, y, z];
                        Voxel other;

                        if (x > 0)
                        {
                            other = this[x - 1, y, z];
                        }
                        else
                        {
                            Chunk c = GetNeighbor(Block.Face.XMinus);

                            if (c != null)
                            {
                                other = c[Size - 1, y, z];
                            }
                            else
                            {
                                other = current;
                            }
                        }

                        if (other.LightLevel - 1 > current.LightLevel)
                        {
                            int l = other.LightLevel - 1;

                            current.LightLevel = (byte)l;
                        }

                        this[x, y, z] = current;
                    }

                    #endregion

                    #region X+ Sweep

                    for (int x = Size - 1; x >= 0; x--)
                    {
                        Voxel current = this[x, y, z];
                        Voxel other;

                        if (x < Size - 1)
                        {
                            other = this[x + 1, y, z];
                        }
                        else
                        {
                            Chunk c = GetNeighbor(Block.Face.XPositive);

                            if (c != null)
                            {
                                other = c[0, y, z];
                            }
                            else
                            {
                                other = current;
                            }
                        }

                        if (other.LightLevel - 1 > current.LightLevel)
                        {
                            int l = other.LightLevel - 1;

                            current.LightLevel = (byte)l;
                        }

                        this[x, y, z] = current;
                    }

                    #endregion
                }
                #endregion

                #region Z Sweep

                for (int x = 0; x < Size; x++)
                {
                    #region Z- Sweep

                    for (int z = 0; z < Size; z++)
                    {
                        Voxel current = this[x, y, z];
                        Voxel other;

                        if (z > 0)
                        {
                            other = this[x, y, z - 1];
                        }
                        else
                        {
                            Chunk c = GetNeighbor(Block.Face.ZMinus);

                            if (c != null)
                            {
                                other = c[x, y, Size - 1];
                            }
                            else
                            {
                                other = current;
                            }
                        }

                        if (other.LightLevel - 1 > current.LightLevel)
                        {
                            int l = other.LightLevel - 1;

                            current.LightLevel = (byte)l;
                        }

                        this[x, y, z] = current;
                    }

                    #endregion

                    #region Z+ Sweep

                    for (int z = Size - 1; z >= 0; z--)
                    {
                        Voxel current = this[x, y, z];
                        Voxel other;

                        if (z < Size - 1)
                        {
                            other = this[x, y, z + 1];
                        }
                        else
                        {
                            Chunk c = GetNeighbor(Block.Face.ZPositive);

                            if (c != null)
                            {
                                other = c[x, y, 0];
                            }
                            else
                            {
                                other = current;
                            }
                        }

                        if (other.LightLevel - 1 > current.LightLevel)
                        {
                            int l = other.LightLevel - 1;

                            current.LightLevel = (byte)l;
                        }

                        this[x, y, z] = current;
                    }

                    #endregion
                }

                #endregion
            }
        }

        /// <summary>
        /// Gets a neighboring Chunk
        /// </summary>
        /// <param name="face">The face to check against</param>
        /// <returns>The neighbor or null if no neighbor was found</returns>
        Chunk GetNeighbor(Block.Face face)
        {
            if (face == Block.Face.XMinus)
            {
                for (int i = 0; i < Neighbors.Count; i++)
                {
                    Chunk c = Neighbors[i];
                    if (c.Position.x == Position.x - 1 && c.Position.y == Position.y)
                    {
                        return c;
                    }
                }
            }
            else if (face == Block.Face.XPositive)
            {
                for (int i = 0; i < Neighbors.Count; i++)
                {
                    Chunk c = Neighbors[i];
                    if (c.Position.x == Position.x + 1 && c.Position.y == Position.y)
                    {
                        return c;
                    }
                }
            }
            if (face == Block.Face.ZMinus)
            {
                for (int i = 0; i < Neighbors.Count; i++)
                {
                    Chunk c = Neighbors[i];
                    if (c.Position.x == Position.x && c.Position.y == Position.y - 1)
                    {
                        return c;
                    }
                }
            }
            else if (face == Block.Face.ZPositive)
            {
                for (int i = 0; i < Neighbors.Count; i++)
                {
                    Chunk c = Neighbors[i];
                    if (c.Position.x == Position.x && c.Position.y == Position.y + 1)
                    {
                        return c;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the Chunk with a Position
        /// </summary>
        /// <param name="position">The Position of the Chunk (in chunk units)</param>
        /// /// <param name="centerPos">The Height (in Chunk Slices) on which the camera is located at</param>
        /// <param name="generator">The Generation module for the terrain</param>
        /// <param name="material">The Rendering Material for the Chunk</param>
        public Chunk(Vector2 position, int centerPos, IGenerator generator, Material material)
        {
            //TODO: Sort RenderChunks from closest to nearest.

            GeneratedVoxels = false;
            GeneratedMeshData = false;

            Position = position;
            Material = material;

            GenerateChunk(generator);

            GameObject parentChunk = new GameObject(string.Format("Chunk {0},{1}", Position.x, Position.y));
            parentChunk.transform.position = new Vector3(Position.x * Size, 0, Position.y * Size);

            for (int i = 0; i < Height / Size; i++)
            {
                GameObject gameObject = new GameObject(string.Format("Slice {0}", i));
                gameObject.transform.parent = parentChunk.transform;
                gameObject.transform.localPosition = new Vector3(0, 0, 0);

                Renders[i] = gameObject.AddComponent<RenderChunk>();
                Renders[i].Setup(this, i);
            }
        }

        #endregion
    }
}
