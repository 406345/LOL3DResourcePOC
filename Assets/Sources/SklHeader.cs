using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Sources
{
    public class SklHeader
    {
        public byte[] Magic { get; set; }
        public int Version { get; set; }
        //public int Hash { get; set; }
        //public int BoneCount { get; set; }
        public SklHeader(BinaryReader reader)
        {
            this.Magic = reader.ReadBytes(8);
            this.Version = reader.ReadInt32();
            //this.Hash = reader.ReadInt32();
            //this.BoneCount = reader.ReadInt32();
        }
    }
}
