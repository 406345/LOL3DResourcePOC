using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Sources
{
    public class SklBoneData
    {
        public string Name { get; set; }
        public int ParentIndex { get; set; }
        public float Scale { get; set; }
        public Matrix4x4 Matrix;

        public SklBoneData(BinaryReader reader)
        {
            this.Name = new string(reader.ReadChars(32));
            this.ParentIndex = reader.ReadInt32();
            this.Scale = reader.ReadSingle();
            this.Matrix = new Matrix4x4(
                    new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    new Vector4(0, 0, 0, 1f)
                    );
        }

    }
    public class SklData
    {
        public byte[] Magic { get; set; }
        public int ObjCount { get; set; }
        public int Hash { get; set; }
        public int SklCount { get; set; }
        public SklBoneData[] Bones;


        public SklData(BinaryReader reader)
        {
            this.Magic = reader.ReadBytes(8);
            this.ObjCount = reader.ReadInt32();
            this.Hash = reader.ReadInt32();
            this.SklCount = reader.ReadInt32();
            this.Bones = new SklBoneData[this.SklCount];
            for (int i = 0; i < this.SklCount; i++)
            {
                this.Bones[i] = new SklBoneData(reader);
            }
        }
    }
}
