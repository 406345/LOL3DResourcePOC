using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Sources
{
    
    public class SklBoneData0 : SklBaseBoneData
    {
        public short Zero { get; set; }

        public short UnField { get; set; }
        public float TwoPointOne { get; set; }
        public Vector3 ct;
        public int[] Extra;

        public SklBoneData0(BinaryReader reader)
        {
            this.Zero = reader.ReadInt16();
            this.Id = reader.ReadInt16();
            this.Parent = reader.ReadInt16();
            this.UnField = reader.ReadInt16();
            this.Hash = reader.ReadInt32();
            this.TwoPointOne = reader.ReadSingle();

            this.Position = new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                -reader.ReadSingle());

            this.Scale = new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());

            this.Quaternion = new Quaternion(
                reader.ReadSingle(),
                reader.ReadSingle(),
                -reader.ReadSingle(),
                -reader.ReadSingle());

            ct = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            this.Extra = new int[8];
            for (int i = 0; i < 8; i++)
            {
                this.Extra[i] = reader.ReadInt32();
            }
        }

    }

    public class SklData0 : SklBaseData
    {
        public int bonesStart;
        public int animationStart;
        public int boneIndicesStart;
        public int boneIndicesEnd;
        public int halfayBetweenBoneindicesAndStrings;
        public int boneNamesStart;


        public SklData0(SklHeader header, BinaryReader reader)
            : base(header, reader)
        {
            int un = reader.ReadInt16();
            int boneCount = reader.ReadInt16();
            int boneIndexCount = reader.ReadInt32();
            this.Bones = new SklBoneData0[boneCount];

            this.bonesStart = reader.ReadInt32();
            this.animationStart = reader.ReadInt32();
            this.boneIndicesStart = reader.ReadInt32();
            this.boneIndicesEnd = reader.ReadInt32();
            this.halfayBetweenBoneindicesAndStrings = reader.ReadInt32();
            this.boneNamesStart = reader.ReadInt32();
            var gap = reader.ReadBytes(20);

            for (int i = 0; i < boneCount; i++)
            {
                Bones[i] = new SklBoneData0(reader);
            }

            this.BoneExtra = new SklBoneExtra[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                this.BoneExtra[i] = new SklBoneExtra(reader);
            }

            reader.BaseStream.Seek(boneNamesStart, SeekOrigin.Begin);

            for (int i = 0; i < boneCount; i++)
            {
                string name = "";
                string c = "";
                do
                {
                    c = new string(reader.ReadChars(4));
                    name += c;
                }
                while (c.IndexOf('\0') == -1);

                BoneNames.Add(name.Substring(0, name.IndexOf('\0')));
            }

            foreach (var item in this.Bones)
            {
                item.Name = BoneNames[item.Id];
            }
        }
    }
}
