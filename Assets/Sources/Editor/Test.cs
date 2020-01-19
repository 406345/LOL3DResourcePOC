using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using Assets.Sources;

public class Test : MonoBehaviour
{

    [MenuItem("Test/LoadSknData", false, 1)]
    static void LoadSknData()
    {
        loadSKL("Ahri");
    }
    static Matrix4x4 bindBone(GameObject parent, GameObject ret , SklBoneData0 bone)
    { 
        ret.transform.parent = parent.transform; 
        ret.transform.localPosition = bone.Position;
        ret.transform.localRotation = bone.Quaternion;
        ret.transform.localScale = bone.Scale;
        var mat = ret.transform.worldToLocalMatrix;// * parent.transform.localToWorldMatrix;
        return mat;
    }
    static GameObject loadSKL(string chrName)
    {
        GameObject obj = loadSkn(chrName + ".skn");
        var SKN = Resources.Load<TextAsset>(chrName + ".skl");
        BinaryReader reader = new BinaryReader(new MemoryStream(SKN.bytes));
        var header = new SklHeader(reader);
        var body = new SklData0(header, reader);

        var render = obj.GetComponent<SkinnedMeshRenderer>();
        
        Queue<SklBoneData0> bones = new Queue<SklBoneData0>();
        Queue<GameObject> objs = new Queue<GameObject>();
        var b = body.Bones.Where(x => x.Parent == -1).FirstOrDefault();
        GameObject[] bonesObject = new GameObject[body.Bones.Length];
        Matrix4x4[] bindPoses = new Matrix4x4[body.Bones.Length];
        bonesObject[0] = new GameObject(b.Name);
        bindPoses[0] = bindBone(obj, bonesObject[0], b);

        for (int i = 1; i < body.Bones.Length; i++)
        {
            var bone = body.Bones[i];
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

        render.bones = bonesObject.Select(x => x!=null?x.transform:null).ToArray();
        render.sharedMesh.bindposes = bindPoses;

        return obj;

    }

    static GameObject loadSkn(string file)
    {
        var SKN = Resources.Load<TextAsset>(file);
        BinaryReader reader = new BinaryReader(new MemoryStream(SKN.bytes));
        var data = new SknData(reader);

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
}
