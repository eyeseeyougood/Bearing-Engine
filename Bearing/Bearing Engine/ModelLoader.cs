using OpenTK.Mathematics;
using System.Linq;
using System.Collections.Generic;
using System;
using Assimp;
using Mesh = Bearing.Mesh;

namespace Bearing;

public static class ModelLoader
{
    /// <summary>
    /// This function only creates a mesh from the provided mesh id, this means that if your file has multiple objects, only one will be loaded.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>

    public static Mesh FileToMesh(string filepath, int meshID)
    {
        AssimpContext importer = new AssimpContext();
        Assimp.Scene model = importer.ImportFile(filepath, PostProcessSteps.Triangulate);

        Mesh result = Mesh.CreateEmpty();
        Assimp.Mesh impMesh = model.Meshes[meshID];

        List<Vector3> vertices = new List<Vector3>();
        impMesh.Vertices.ForEach((i) => { Vector3 pos = new Vector3(i.X, i.Y, i.Z) / 2.0f; vertices.Add(pos); });
        List<MeshVertex3D> verts = new List<MeshVertex3D>();

        int g = 0;
        foreach (Vector3D tex in impMesh.TextureCoordinateChannels[0])
        {
            MeshVertex3D newV = new MeshVertex3D();
            newV.position = vertices[g];
            newV.texCoord = new Vector2(tex.X, tex.Y);
            Vector3D v = impMesh.Normals[g];
            newV.normal = new Vector3(v.X, v.Y, v.Z);
            verts.Add(newV);
            g++;
        }

        int[] meshInd = impMesh.GetIndices();
        uint[] finalIndices = new uint[meshInd.Length];
        int k = 0;
        foreach (int i in meshInd)
        {
            finalIndices[k] = Convert.ToUInt32(i);
            k++;
        }

        result.vertices = verts.ToArray();
        result.indices = finalIndices;

        return result;
    }

    public static Mesh FileToMesh(string filepath)
    {
        AssimpContext importer = new AssimpContext();
        Assimp.Scene model = importer.ImportFile(filepath, PostProcessSteps.Triangulate);

        List<MeshVertex3D> finalVerts = new List<MeshVertex3D>();
        List<uint> finalIndices = new List<uint>();
        uint numInds = 0;
        for (int i = 0; i < model.MeshCount; i++)
        {
            Mesh tempMesh = FileToMesh(filepath, i);
            finalVerts.AddRange(tempMesh.vertices);
            foreach (uint index in tempMesh.indices)
            {
                finalIndices.Add(index+numInds);
            }
            numInds = (uint)finalIndices.Count;
        }

        Mesh result = Mesh.FromData(finalVerts.ToArray(), finalIndices.ToArray());

        return result;
    }
}