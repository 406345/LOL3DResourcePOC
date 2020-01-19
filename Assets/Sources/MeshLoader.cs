using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class MeshLoader : MonoBehaviour {

    public TextAsset SKN;
    public TextAsset SKL;
    SknData data;

    // Use this for initialization
    void Start () {
        BinaryReader reader = new BinaryReader(new MemoryStream(SKN.bytes));
        data = new SknData(reader);

        var m = new Mesh();
        m.SetVertices(data.Vertexes.Select(x => x.Vertex).ToList());
        m.SetIndices(data.Indexes.Select(x => (int)x).ToArray(), MeshTopology.Triangles, 1);
        m.SetUVs(1, data.Vertexes.Select(x => x.UV).ToList());

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
