using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class SknVertex
{
    public Vector3 Vertex;
    public byte[] BoneIndex { get; set; }
    public float[] BoneWeight { get; set; }
    public Vector3 Normal;
    public Vector2 UV;

    public SknVertex(BinaryReader reader)
    {
        this.Vertex = new Vector3();
        this.Vertex.x = reader.ReadSingle();
        this.Vertex.y = reader.ReadSingle();
        this.Vertex.z = reader.ReadSingle();

        this.BoneIndex = new byte[4];
        this.BoneWeight = new float[4];
        this.Normal = new Vector3();
        this.UV = new Vector2();

        for (int i = 0; i < 4; i++)
        {
            this.BoneIndex[i] = reader.ReadByte();
        }

        for (int i = 0; i < 4; i++)
        {
            this.BoneWeight[i] = reader.ReadSingle();
        }

        this.Normal.x = reader.ReadSingle();
        this.Normal.y = reader.ReadSingle();
        this.Normal.z = reader.ReadSingle();

        this.UV.x = reader.ReadSingle();
        this.UV.y = -reader.ReadSingle();
    }
}
public class SknDataMaterial
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int StartPosistion { get; set; }
    public int VertexCount { get; set; }
    public int StartVertextIndexPosition { get; set; }
    public int VertexIndexPosition { get; set; }
    public SknDataMaterial(BinaryReader reader)
    {
        this.Id = reader.ReadInt32();
        this.Name = new string(reader.ReadChars(64));
        this.StartPosistion = reader.ReadInt32();
        this.VertexCount = reader.ReadInt32();
        this.StartVertextIndexPosition = reader.ReadInt32();
        this.VertexIndexPosition = reader.ReadInt32();
    }
}
public class SknData
{
    public int Magic { get; set; }
    public short MaterialCount { get; set; }
    public short ObjectCount { get; set; }
    public List<SknDataMaterial> Materials { get; set; }
    public int IndexCount { get; set; }
    public int VertexCount { get; set; }
    public short[] Indexes { get; set; }
    public SknVertex[] Vertexes { get; set; }

    public SknData(BinaryReader reader)
    {
        this.Magic = reader.ReadInt32();
        this.ObjectCount = reader.ReadInt16();
        this.MaterialCount = reader.ReadInt16();
        this.Materials = new List<SknDataMaterial>();

        for (int i = 0; i < this.MaterialCount; i++)
        {
            this.Materials.Add(new SknDataMaterial(reader));
        }

        //this.IndexCount = reader.ReadInt32();
        this.IndexCount = reader.ReadInt32();
        this.VertexCount = reader.ReadInt32();

        this.Indexes = new short[this.IndexCount];

        for (int i = 0; i < this.IndexCount; i++)
        {
            this.Indexes[i] = reader.ReadInt16();
        }

        this.Vertexes = new SknVertex[this.VertexCount];

        for (int i = 0; i < this.VertexCount; i++)
        {
            this.Vertexes[i] = new SknVertex(reader);
        }
    }
}