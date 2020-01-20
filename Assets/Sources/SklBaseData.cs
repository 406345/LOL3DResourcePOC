using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Sources
{
    public class SklBoneExtra
    {
        public int BoneId { get; set; }
        public int BoneHash { get; set; }

        public SklBoneExtra(BinaryReader reader)
        {
            this.BoneId = reader.ReadInt32();
            this.BoneHash = reader.ReadInt32();
        }
    }

    public class SklBaseData
    {
        private SklHeader header;
        private BinaryReader reader;
        public SklBaseBoneData[] Bones;
        public List<string> BoneNames = new List<string>();
        public SklBoneExtra[] BoneExtra;

        public SklBaseData(SklHeader header, BinaryReader reader)
        {
            this.header = header;
            this.reader = reader;
        }
    }
}
