using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mesh;
using UnityEngine.UIElements;

public class GenerateShape : MonoBehaviour
{
    //crown
    private Mesh mesh;
    private List<Vector3> verticesList = new List<Vector3>();
    private List<int> triangleList= new List<int>();
    private List<Vector3> normalList = new List<Vector3>();

    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] normals;


    [Header("Crown Variables (sphere)")]
    [SerializeField][Range(0,25)] private float radius = 8;
    [SerializeField][Range(0,100)] private int sliceCount = 10, stackCount = 10;

    //attraction points
    private List<Vector3> attractionPoints = new List<Vector3>();
    [Header("Attraction Points")]
    [SerializeField][Range(0, 10)] private float killDistance = 1f;
    [SerializeField][Range(0,10)]private float influenceDistance = 2f;

    //branches
    class Branch
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector3 direction;
        public List<Vector3> pointsInRange;

        public Branch(Vector3 startPos, Vector3 endPos, Vector3 direction)
        {
            this.startPos = startPos; this.endPos = endPos; this.direction = direction;
        }
    }

    [Header("Branches")]
    [SerializeField] private float branchLength = 0.2f;
    [SerializeField] private List<Branch> branches = new List<Branch>();
    [SerializeField] private List<Branch> newBranches = new List<Branch>();

    

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "sphere";

        DrawSphere();
        GenerateAttractionPoints();

        Branch firstBranch = new Branch(Vector3.zero, new Vector3(0, branchLength, 0), Vector3.up);
        branches.Add(firstBranch);
    }

    private void DrawSphere()
    {
        var phiStep = MathF.PI / stackCount;
        var thetaStep = 2.0f * MathF.PI / sliceCount;

        verticesList.Add(new Vector3(0, radius, 0));
        normalList.Add(new Vector3(0, 1, 0));

        for (int i = 1; i <= stackCount - 1; i++)
        {
            var phi = i * phiStep;
            for (int j = 0; j <= sliceCount; j++)
            {
                var theta = j * thetaStep;
                var p = new Vector3(
                    (radius * MathF.Sin(phi) * MathF.Cos(theta)),
                    (radius * MathF.Cos(phi)),
                    (radius * MathF.Sin(phi) * MathF.Sin(theta))
                    );

                var n = p;
                n.Normalize();

                verticesList.Add(p);
                normalList.Add(n);
            }
        }

        verticesList.Add(new Vector3(0, -radius, 0));
        normalList.Add(new Vector3(0, -1, 0));

        //indices / triangles

        for (int i = 1; i <= sliceCount; i++)
        {
            triangleList.Add(0);
            triangleList.Add(i + 1);
            triangleList.Add(i);
        }
        var baseIndex = 1;
        var ringVertexCount = sliceCount + 1;
        for (int i = 0; i < stackCount - 2; i++)
        {
            for (int j = 0; j < sliceCount; j++)
            {
                triangleList.Add(baseIndex + i * ringVertexCount + j);
                triangleList.Add(baseIndex + i * ringVertexCount + j + 1);
                triangleList.Add(baseIndex + (i + 1) * ringVertexCount + j);

                triangleList.Add(baseIndex + (i + 1) * ringVertexCount + j);
                triangleList.Add(baseIndex + i * ringVertexCount + j + 1);
                triangleList.Add(baseIndex + (i + 1) * ringVertexCount + j + 1);
            }
        }
        var southPoleIndex = verticesList.Count - 1;
        baseIndex = southPoleIndex - ringVertexCount;
        for (int i = 0; i < sliceCount; i++)
        {
            triangleList.Add(southPoleIndex);
            triangleList.Add(baseIndex + i);
            triangleList.Add(baseIndex + i + 1);
        }

        //add to mesh
        //verts
        vertices = new Vector3[verticesList.Count];
        for (int i = 0; i < verticesList.Count; i++)
        {
            vertices[i] = verticesList[i];
        }
        //tris
        triangles = new int[triangleList.Count];
        for (int i = 0; i < triangleList.Count; i++)
        {
            triangles[i] = triangleList[i];
        }
        //normals
        normals = new Vector3[normalList.Count];
        for (int i = 0; i < normalList.Count; i++)
            normals[i] = normalList[i];

        //set it on the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
    }

    private void GenerateAttractionPoints()
    {
        //Vertex method: Attraction points are placed at each vertex of the crown
        attractionPoints = verticesList;
    }

    private void OnDrawGizmos()
    {
        //draw crown
        Gizmos.color = Color.green;
        Gizmos.DrawWireMesh(mesh);

        //draw attraction points
        foreach (var attraction in attractionPoints)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(attraction, 0.5f);
            
            //draw influence + kill distance
            /*Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attraction, killDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attraction, influenceDistance);*/
        }

        //draw branches
        foreach (Branch branch in branches)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(branch.startPos, branch.endPos);
            Gizmos.DrawWireSphere(branch.startPos, 0.2f);
            Gizmos.DrawWireSphere(branch.endPos, 0.2f);
        }
    }
}