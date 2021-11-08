using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


class PointsCircle
{
    public double radius;
    public int pointCount;
    public int[] pointIds;
}

static class Utilz
{
    public static bool HasValue(this double value)
    {
        return !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
    public static double Propagate(this double value)
    {
        return (!Double.IsNaN(value) && !Double.IsInfinity(value)) ? value : 0;
    }

    public static double Clamp(this double value)
    {
        return (value.HasValue() && value >= 0 && value <= 1) ? value : 0;
    }
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
        meshFilter.mesh.indexFormat = IndexFormat.UInt32;

        setMaterial(go, meshFilter, vertices, triangles, wireframe);
        meshFilter.mesh.SetUVs(0, uvs);

        
    }
    public static unsafe void generateCircleHeightOnGO(GameObject go, double[,] map, double min, double max, float radius, int radiusSegments = 5, int angleSegments = 10)
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
        float ratio = (float) (1.0 / (max - min));

        float height = 3;
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


                sides[0].y = (float) clamp((map[i, j] - min) * ratio) * height;
                sides[1].y = (float) clamp((map[i + 1, j] - min) * ratio) * height;
                sides[2].y = (float)clamp((map[i + 1, (j + 1)% angleSegments] - min) * ratio) * height;
                sides[3].y = (float)clamp((map[i, (j + 1) % angleSegments] - min) * ratio) * height;

                uvs.Add(new Vector2((float)i / radiusSegments, (float)(j + 0) / angleSegments));
                uvs.Add(new Vector2((float)(i + 1) / radiusSegments, (float)(j + 0) / angleSegments));
                uvs.Add(new Vector2((float)(i + 1) / radiusSegments, (float)(j + 1) / angleSegments));
                uvs.Add(new Vector2((float)i / radiusSegments, (float)(j + 1) / angleSegments));

                addQuad(sides, vertices, triangles, ref id, wireframe);
            }
        }
        meshFilter.mesh.indexFormat = IndexFormat.UInt32;

        setMaterial(go, meshFilter, vertices, triangles, wireframe);
        meshFilter.mesh.SetUVs(0, uvs);


    }

    static double clamp(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return 0;
        if (value > 1)
            value = 1;
        else if (value < 0)
            value = 0;
        return value;
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
