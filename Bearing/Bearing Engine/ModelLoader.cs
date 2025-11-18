using System.Linq;
using System.Collections.Generic;
using System;
using Assimp;
using Mesh = Bearing.Mesh3D;
using OpenTK.Mathematics;

namespace Bearing;

public static class ModelLoader
{
    /// <summary>
    /// This function only creates a mesh from the provided mesh id, this means that if your file has multiple objects, only one will be loaded.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>

    public static Mesh3D FileToMesh3D(string filepath, int meshID)
    {
        AssimpContext importer = new AssimpContext();
        var fStream = Resources.Open(Resource.FromPath(filepath));
        Assimp.Scene model = importer.ImportFileFromStream(fStream, PostProcessSteps.Triangulate, Path.GetExtension(filepath));

        Mesh3D result = Mesh3D.CreateEmpty();
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

    public static Mesh3D FileToMesh3D(string filepath)
    {
        AssimpContext importer = new AssimpContext();
        var fStream = Resources.Open(Resource.FromPath(filepath));
        Assimp.Scene model = importer.ImportFileFromStream(fStream, PostProcessSteps.Triangulate, Path.GetExtension(filepath));

        List<MeshVertex3D> finalVerts = new List<MeshVertex3D>();
        List<uint> finalIndices = new List<uint>();
        uint numInds = 0;
        for (int i = 0; i < model.MeshCount; i++)
        {
            Mesh3D tempMesh = FileToMesh3D(filepath, i);
            finalVerts.AddRange(tempMesh.vertices);
            foreach (uint index in tempMesh.indices)
            {
                finalIndices.Add(index+numInds);
            }
            numInds = (uint)finalIndices.Count;
        }

        Mesh3D result = Mesh3D.FromData(finalVerts.ToArray(), finalIndices.ToArray());

        fStream.DisposeAsync();

        return result;
    }

    public static Mesh2D FileToMesh2D(string filepath)
    {
        AssimpContext importer = new AssimpContext();
        var fStream = Resources.Open(Resource.FromPath(filepath));
        Assimp.Scene model = importer.ImportFileFromStream(fStream, PostProcessSteps.Triangulate, Path.GetExtension(filepath));

        Mesh2D result = Mesh2D.CreateEmpty();
        Assimp.Mesh impMesh = model.Meshes[0];

        List<Vector3> vertices = new List<Vector3>();
        impMesh.Vertices.ForEach((i) => { Vector3 pos = new Vector3(i.X, i.Y, i.Z) / 2.0f; vertices.Add(pos); });
        List<MeshVertex2D> verts = new List<MeshVertex2D>();

        int g = 0;
        foreach (Vector3D tex in impMesh.TextureCoordinateChannels[0])
        {
            MeshVertex2D newV = new MeshVertex2D();
            newV.position = new Vector2(vertices[g].X, vertices[g].Y);
            newV.texCoord = new Vector2(tex.X, tex.Y);
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
}