using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class PointsCircle
{
    public double radius;
    public int pointCount;
    public int[] pointIds;
}

public class CircleMeshGenerator
{
    public static GameObject createCircleMesh(float radius, int radiusSegments = 5, int angleSegments = 10)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        generateCircleOnGO(go, radius, radiusSegments, angleSegments);

        return go;
    }

    public static unsafe void generateCircleOnGO(GameObject go, float radius, int radiusSegments = 5, int angleSegments = 10)
    {
        bool wireframe = false;

        var meshFilter = go.GetComponent<MeshFilter>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int id = 0;

        Vector3* sides = stackalloc Vector3[4];

        float radialStep = radius / radiusSegments;

        float dAngle = 360.0f / angleSegments;
        for (int i = radiusSegments - 1; i >= 0; --i)
        {
            float startR = radialStep * (i);
            float endR = radialStep * (i + 1);

            for (int j = 0; j < angleSegments; ++j)
            {
                Quaternion q1 = Quaternion.AngleAxis(dAngle * j, Vector3.up);
                Quaternion q2 = Quaternion.AngleAxis(dAngle * (j + 1), Vector3.up);

                Vector3 start = Vector3.forward * startR;
                Vector3 end = Vector3.forward * endR;

                sides[0] = q1 * start;
                sides[1] = q1 * end;
                sides[2] = q2 * end;
                sides[3] = q2 * start;

                uvs.Add(new Vector2((float) i / radiusSegments, (float) (j + 0) / angleSegments));
                uvs.Add(new Vector2((float) (i + 1) / radiusSegments, (float) (j + 0) / angleSegments));
                uvs.Add(new Vector2((float) (i + 1) / radiusSegments, (float) (j + 1) / angleSegments));
                uvs.Add(new Vector2((float) i / radiusSegments, (float) (j + 1) / angleSegments));

                addQuad(sides, vertices, triangles, ref id, wireframe);
            }
        }

        setMaterial(go, meshFilter, vertices, triangles, wireframe);
        meshFilter.mesh.SetUVs(0, uvs);
    }

    private static void addTriangles(List<int> triangles, ref int id, bool lines, bool revers = false)
    {
        if (!lines)
        {
            for (int i = !revers ? 0 : 3; !revers ? i < 4 : i >= 0; i+= !revers? 1 : -1)
            {
                triangles.Add(id + i);
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                triangles.Add(id + i);
                triangles.Add(i < 3 ? id + i + 1 : id);
            }
        }
    }
    static unsafe void addQuad(Vector3* corners, List<Vector3> vertices, List<int> triangles, ref int id,
        bool lines = false)
    {
        vertices.Add(corners[0]);
        vertices.Add(corners[1]);
        vertices.Add(corners[2]);
        vertices.Add(corners[3]);

        addTriangles(triangles, ref id, lines);
        //addTriangles(triangles, ref id, lines, true);

        id += 4;

    }

    private static void setMaterial(GameObject go, MeshFilter meshFilter, List<Vector3> vertices, List<int> triangles,
        bool wireframe = false)
    {
        meshFilter.mesh.Clear(true);
        //meshFilter.mesh.
        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.SetIndices(triangles, wireframe ? MeshTopology.Lines : MeshTopology.Quads, 0);
        //meshFilter.mesh.SetUVs();
        
        if (!wireframe)
            meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();

        var mat = Resources.Load<Material>("def");

        go.GetComponent<Renderer>().material = mat;
    }

}
