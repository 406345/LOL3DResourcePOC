using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Sources.Editor
{
    class LOLModelImporter
    {
        string SKNFile;
        string SKLFile;
        string[] ANMFiles;
        SklBaseData sklData = null;
        SknData sknData;

        public LOLModelImporter(string dir)
        {
            SKNFile = System.IO.Directory.GetFiles(dir, "*.skn").FirstOrDefault();
            SKLFile = System.IO.Directory.GetFiles(dir, "*.skl").FirstOrDefault();
            ANMFiles = System.IO.Directory.GetFiles(dir + "/Animations", "*.anm");
        }

        private GameObject generateMesh()
        {
            var SKN = System.IO.File.ReadAllBytes(this.SKNFile);
            BinaryReader reader = new BinaryReader(new MemoryStream(SKN));
            sknData = new SknData(reader);
            var data = sknData;

            GameObject obj = new GameObject();
            var meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            //var meshrender = obj.AddComponent<MeshRenderer>();

            var skinmesh = obj.AddComponent<SkinnedMeshRenderer>();
            skinmesh.sharedMesh = meshFilter.sharedMesh;

            var m = meshFilter.sharedMesh;
            m.name = data.Materials[0].Name;
            m.SetVertices(data.Vertexes.Select(x => x.Vertex).ToList());
            m.SetIndices(data.Indexes.Select(x => (int)x).ToArray(), MeshTopology.Triangles, 0);
            m.uv = (data.Vertexes.Select(x => x.UV).ToArray());
            m.normals = (data.Vertexes.Select(x => x.Normal).ToArray());
            m.boneWeights = data.Vertexes.Select(x => new BoneWeight()
            {
                weight0 = x.BoneWeight[0],
                weight1 = x.BoneWeight[1],
                weight2 = x.BoneWeight[2],
                weight3 = x.BoneWeight[3],

                boneIndex0 = x.BoneIndex[0],
                boneIndex1 = x.BoneIndex[1],
                boneIndex2 = x.BoneIndex[2],
                boneIndex3 = x.BoneIndex[3],

            }).ToArray();

            return obj;
        }
        private Matrix4x4 bindBone(GameObject parent, GameObject ret, SklBaseBoneData bone)
        {
            ret.transform.parent = parent.transform;
            ret.transform.localPosition = bone.Position;
            ret.transform.localRotation = bone.Quaternion;
            ret.transform.localScale = bone.Scale;
            var mat = ret.transform.worldToLocalMatrix;// * parent.transform.localToWorldMatrix;
            return mat;
        }
        private GameObject bindBones(GameObject obj)
        {
            var SKN = System.IO.File.ReadAllBytes(this.SKLFile);
            BinaryReader reader = new BinaryReader(new MemoryStream(SKN));
            var header = new SklHeader(reader);
            sklData = null;

            if (header.Version == 0)
            {
                sklData = new SklData0(header, reader);
            }
            else if (header.Version == 1 || header.Version == 2)
            {
                sklData = new SklData1(header, reader);
            }

            var render = obj.GetComponent<SkinnedMeshRenderer>();

            var b = sklData.Bones.Where(x => x.Parent == -1).FirstOrDefault();
            GameObject[] bonesObject = new GameObject[sklData.Bones.Length];
            Matrix4x4[] bindPoses = new Matrix4x4[sklData.Bones.Length];
            bonesObject[0] = new GameObject(b.Name);
            bindPoses[0] = bindBone(obj, bonesObject[0], b);
            render.rootBone = bonesObject[0].transform;

            for (int i = 1; i < sklData.Bones.Length; i++)
            {
                var bone = sklData.Bones[i];
                bonesObject[i] = new GameObject(bone.Name);
                //bonesObject[i].transform.parent = obj.transform;

                if (bone.Parent == -1)
                {
                    bindPoses[i] = bindBone(
                      obj,
                      bonesObject[bone.Id],
                      bone
                      );
                }
                else
                {
                    bindPoses[i] = bindBone(
                      bonesObject[bone.Parent],
                      bonesObject[bone.Id],
                      bone
                      );
                }

            }

            render.bones = bonesObject.Select(x => x != null ? x.transform : null).ToArray();
            render.sharedMesh.bindposes = bindPoses;

            return obj;

        }

        private string GetRelatedPath(GameObject node, string name)
        {
            if (node.name == name)
            {
                return node.name;
            }

            if (node.transform.childCount == 0)
            {
                return null;
            }

            for (int i = 0; i < node.transform.childCount; i++)
            {
                var n = GetRelatedPath(node.transform.GetChild(i).gameObject, name);

                if (n != null)
                {
                    return node.name + "/" + n;
                }
            }

            return null;
        }
        private GameObject generateAnimation(GameObject obj)
        {
            var animation = obj.AddComponent<Animation>();

            var SKN = System.IO.File.ReadAllBytes(this.ANMFiles[12]);
            BinaryReader reader = new BinaryReader(new MemoryStream(SKN));
            AnmHeader header = new AnmHeader(reader);
            AnmData ad = new AnmData(reader);

            AnimationClip clip = new AnimationClip();
            clip.name = "clip1";
            clip.wrapMode = WrapMode.Once;
            clip.legacy = true;
            clip.frameRate = ad.FPS;

            foreach (var item in ad.Frames)
            {
                var curvex = new AnimationCurve();
                var curvey = new AnimationCurve();
                var curvez = new AnimationCurve();
                var curvew = new AnimationCurve();

                var boneExtra = sklData.Bones.Where(x => x.Hash == item.BoneHash).FirstOrDefault();
                string boneName = "u" + item.BoneHash;

                if (boneExtra != null)
                    boneName = boneExtra.Name;

                foreach (var k in item.Quaternion)
                {
                    Vector3 v = k.Rotation.eulerAngles;
                    curvex.AddKey(k.Time, v.x);
                    curvey.AddKey(k.Time, v.y);
                    curvez.AddKey(k.Time, v.z);
                    //curvez.AddKey(k.Time, v.w);
                }

                var t = GetRelatedPath(obj, boneName);

                t = t.Replace(obj.name + "/", "");
                //localRotation
                //rotation
                clip.SetCurve(t, typeof(Transform), "localEulerAnglesRaw.x", curvex);
                clip.SetCurve(t, typeof(Transform), "localEulerAnglesRaw.y", curvey);
                clip.SetCurve(t, typeof(Transform), "localEulerAnglesRaw.z", curvez);
                //clip.SetCurve(t, typeof(Transform), "m_LocalEulerAngles.w", curvew);
            }

            animation.AddClip(clip, clip.name);
            animation.Play(clip.name);

            return obj;
        }
        public GameObject GetObject()
        {
            var obj = generateMesh();
            obj = bindBones(obj);
            obj = generateAnimation(obj);
            return obj;
        }
    }
}
