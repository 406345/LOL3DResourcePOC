using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Sources
{
    public class AnmBone
    {
        public int BoneHash;
        public string AnimationName;
        public List<AnmDataFrame> Quaternion = new List<AnmDataFrame>();
        public List<AnmDataFrame> Scale = new List<AnmDataFrame>();
        public List<AnmDataFrame> Translate = new List<AnmDataFrame>();
    }
    public class AnmDataFrame
    {
        public string Name;
        /// <summary>
        /// 2:root 0: normal
        /// </summary>
        public int Type;
        public Quaternion Rotation;
        public Vector3 Position;
        public Vector3 Scale; 
        public float Time;

        public AnmDataFrame(BinaryReader reader)
        {
            this.Parse(reader);
        }

        public AnmDataFrame()
        {
            this.Rotation = Quaternion.identity;
            this.Position = Vector3.zero;
            this.Scale = Vector3.one;
        }

        protected virtual void Parse(BinaryReader reader)
        {
            this.Name = new string(reader.ReadChars(32));
            this.Type = reader.ReadInt32();
            this.Rotation = new Quaternion(
                     reader.ReadSingle(),
                     reader.ReadSingle(),
                     reader.ReadSingle(),
                     reader.ReadSingle()
                );
            this.Position = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
        }

    }

    public class AnmData
    {
        public int Size;
        public string Magic;
        public int Version;
        public int UN1;
        public int BoneCount;
        public int FrameCount;
        public float FPS;
        public AnmBone[] Frames;

        public AnmData(BinaryReader reader)
        {
            this.Parse(reader);
        }

        protected virtual float uncompressTime(int ct, float animationLength)
        {
            float ut;
            ut = ct / 65535.0f;
            ut = ut * animationLength;

            return ut;
        }
        protected virtual Vector3 uncompressVector(Vector3 min, Vector3 max, ushort sx, ushort sy, ushort sz)
        {
            Vector3 uv = new Vector3();

            uv = max - min;

            uv.x *= (sx / 65535.0f);
            uv.y *= (sy / 65535.0f);
            uv.z *= (sz / 65535.0f);

            uv = uv + min;

            return uv;
        }
        protected virtual Quaternion uncompressQuaternion(ushort flag, ushort sx, ushort sy, ushort sz)
        {
            float fx = (float)(1.414213562 * ((int)sx - 16384) / 32768.0);
            float fy = (float)(1.414213562 * ((int)sy - 16384) / 32768.0);
            float fz = (float)(1.414213562 * ((int)sz - 16384) / 32768.0);
            float fw = (float)(Math.Sqrt(1.0 - fx * fx - fy * fy - fz * fz));

            Quaternion uq = new Quaternion();

            switch (flag)
            {
                case 0:
                    uq.x = fw;
                    uq.y = fx;
                    uq.z = fy;
                    uq.w = fz;
                    break;

                case 1:
                    uq.x = fx;
                    uq.y = fw;
                    uq.z = fy;
                    uq.w = fz;
                    break;

                case 2:
                    uq.x = -fx;
                    uq.y = -fy;
                    uq.z = -fw;
                    uq.w = -fz;
                    break;

                case 3:
                    uq.x = fx;
                    uq.y = fy;
                    uq.z = fz;
                    uq.w = fw;
                    break;
            }

            return uq;
        }

        protected virtual void Parse(BinaryReader reader)
        {
            this.Size = reader.ReadInt32();
            this.Magic = new string(reader.ReadChars(4));
            this.Version = reader.ReadInt32();
            this.BoneCount = reader.ReadInt32();
            this.FrameCount = reader.ReadInt32();
            int un1 = reader.ReadInt32();

            var animationLength = reader.ReadSingle();
            this.FPS = reader.ReadSingle();

            un1 = reader.ReadInt32();
            un1 = reader.ReadInt32();
            un1 = reader.ReadInt32();
            un1 = reader.ReadInt32();
            un1 = reader.ReadInt32();
            un1 = reader.ReadInt32();

            var minTranslation = new Vector3(
                   reader.ReadSingle(),
                   reader.ReadSingle(),
                   reader.ReadSingle());

            var maxTranslation = new Vector3(
                   reader.ReadSingle(),
                   reader.ReadSingle(),
                   reader.ReadSingle());

            var minScale = new Vector3(
                               reader.ReadSingle(),
                               reader.ReadSingle(),
                               reader.ReadSingle());

            var maxScale = new Vector3(
                               reader.ReadSingle(),
                               reader.ReadSingle(),
                               reader.ReadSingle());

            var entriesOffset = reader.ReadInt32()+12;
            var indicesOffset = reader.ReadInt32()+12;
            var hashesOffset = reader.ReadInt32()+12;

            int[] hashEnties = new int[BoneCount];

            reader.BaseStream.Seek(hashesOffset, SeekOrigin.Begin);

            for (int i = 0; i < BoneCount; i++)
            {
                hashEnties[i] = reader.ReadInt32();
            }

            List<KeyValuePair<int, ulong>>[] compressedQuaternions = new List<KeyValuePair<int, ulong>>[BoneCount];
            List<KeyValuePair<int, ulong>>[] compressedTranslations = new List<KeyValuePair<int, ulong>>[BoneCount];
            List<KeyValuePair<int, ulong>>[] compressedScales = new List<KeyValuePair<int, ulong>>[BoneCount];

            for (int i = 0; i < BoneCount; i++)
            {
                compressedQuaternions[i] = new List<KeyValuePair<int, ulong>>();
                compressedTranslations[i] = new List<KeyValuePair<int, ulong>>();
                compressedScales[i] = new List<KeyValuePair<int, ulong>>();

            }

            reader.BaseStream.Seek(entriesOffset, SeekOrigin.Begin);

            for (int i = 0; i < FrameCount; i++)
            {
                var compressedTime = reader.ReadInt16();
                var hashId = reader.ReadByte();
                var dataType = reader.ReadByte();

                ulong data = 0;
                //data |= (uint)(reader.ReadByte() << 0);
                //data |= (uint)(reader.ReadByte() << 8);
                //data |= (uint)(reader.ReadByte() << 16);
                //data |= (uint)(reader.ReadByte() << 24);
                //data |= (uint)(reader.ReadByte() << 32);
                //data |= (uint)(reader.ReadByte() << 40);

                data |= (uint)(reader.ReadByte() << 40);
                data |= (uint)(reader.ReadByte() << 32);
                data |= (uint)(reader.ReadByte() << 24);
                data |= (uint)(reader.ReadByte() << 16);
                data |= (uint)(reader.ReadByte() << 8);
                data |= (uint)(reader.ReadByte() << 0);

                if (dataType == 0) // quaternionType
                {
                    compressedQuaternions[hashId].Add(new KeyValuePair<int, ulong> (compressedTime, data));
                }
                else if (dataType == 64) // translationType
                {
                    compressedTranslations[hashId].Add(new KeyValuePair<int, ulong>(compressedTime, data));
                }
                else if (dataType == 128) // scaleType
                {
                    compressedScales[hashId].Add(new KeyValuePair<int, ulong>(compressedTime, data));
                }
            }

            List<AnmBone> bones = new List<AnmBone>();

            for (int i = 0; i < BoneCount; i++)
            {
                AnmBone entity = new AnmBone();
                int boneHash = hashEnties[i];
                entity.BoneHash = boneHash;
                bool modified = false;
               
                foreach (var item in compressedTranslations[i])
                { 
                    var time = uncompressTime(item.Key, animationLength);
                    ushort mask = 0xFFFF;
                    ushort x = (ushort)(item.Value & mask);
                    ushort y = (ushort)(item.Value >> 16 & mask);
                    ushort z = (ushort)(item.Value >> 32 & mask);

                    var pos = uncompressVector(minTranslation, maxTranslation, x, y, z);

                    entity.Translate.Add(new AnmDataFrame()
                    {
                        Time = time,
                        Position = pos,
                    });

                    modified = true;
                }

                foreach (var item in compressedQuaternions[i])
                {
                    var time = uncompressTime(item.Key, animationLength);

                    ushort mask = 0x7FFF;
                    ushort flag = (ushort)(item.Value >> 45);
                    ushort x = (ushort)((item.Value >> 30) & mask);
                    ushort y = (ushort)((item.Value >> 15) & mask);
                    ushort z = (ushort)((item.Value) & mask);

                    var q = uncompressQuaternion(flag,x,y,z);

                    entity.Quaternion.Add(new AnmDataFrame()
                    {
                        Time = time,
                        Rotation = q,
                    });

                    modified = true;
                }

                foreach (var item in compressedScales[i])
                {
                    var time = uncompressTime(item.Key, animationLength);

                    ushort mask = 0xFFFF;
                    ushort x = (ushort)(item.Value & mask);
                    ushort y = (ushort)(item.Value >> 16 & mask);
                    ushort z = (ushort)(item.Value >> 32 & mask);

                    var scale = uncompressVector(minScale, maxScale, x, y, z);

                    entity.Scale.Add(new AnmDataFrame()
                    {
                        Time = time,
                        Scale = scale,
                    });

                    modified = true;
                }

                if(modified) bones.Add(entity);
            }

            this.Frames = bones.ToArray();
        }
    }
}
