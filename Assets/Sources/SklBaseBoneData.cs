using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Sources
{
    public class SklBaseBoneData
    {
        public short Id { get; set; }
        public short Parent { get; set; }
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Quaternion;
        public int Hash { get; set; }

        public string Name;
    }
}
