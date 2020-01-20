using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.Sources
{
    public class AnmHeader
    {
        public string Magic;
        public int Version;

        public AnmHeader(BinaryReader reader)
        {
            this.Magic = new string(reader.ReadChars(8));
            this.Version = reader.ReadInt32();
        }
    }
}
