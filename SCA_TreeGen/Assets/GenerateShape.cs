using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mesh;
using UnityEngine.UIElements;

public class GenerateShape : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] private bool TogglePointsView = false;

    [Header("Crown Variables (sphere)")]
    [SerializeField][Range(0,25)] private float radius = 8;
    [SerializeField][Range(0,100)] private int sliceCount = 10, stackCount = 10;
    Mesh mesh;
    List<Vector3> verticesList = new List<Vector3>();

    //branches

    [Header("Branches")]
    private List<Branch> branches = new List<Branch>();
    private List<Branch> newBranches = new List<Branch>();
    [SerializeField] int numOfBranches;

    class Branch
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector3 direction;
        public List<Vector3> pointsInRange = new List<Vector3>();
        public float branchLength = 1.5f;

        public Branch(Vector3 startPos, Vector3 direction)
        {
            this.startPos = startPos; this.direction = direction;
            endPos = startPos + (direction * branchLength);
        }
    }

    //attraction points
    //private List<Vector3> attractionPoints = new List<Vector3>();
    private List<AttractionPoints> attractionPoints = new List<AttractionPoints>();
    [Header("Attraction Points")]
    [SerializeField] private bool displayKillDistance = false;
    [SerializeField][Range(0, 10)] private float killDistance = 1f;
    [SerializeField] private bool displayInfluenceDistance = false;
    [SerializeField][Range(0, 10)] private float influenceDistance = 2f;

    class AttractionPoints
    {
        public Branch closestBranch;
        public Vector3 position;

        public AttractionPoints(Vector3 position)
        { this.position = position; }
    }
    

    private void Start()
    {
        DrawSphere();
        GenerateAttractionPoints();

        //Branch firstBranch = new Branch(new Vector3(0,-9,0), new Vector3(0, branchLength-9, 0), Vector3.up);
        Branch firstBranch = new Branch(new Vector3(0, -11, 0), Vector3.up);
        branches.Add(firstBranch);
    }

    private void Update()
    {
        numOfBranches = branches.Count;

        //attraction points pick nearest branch node 
        foreach(var a in attractionPoints)
        {
            foreach(Branch b in branches)
            {
                //find distance between the point and the branch
                float distance = Vector3.Distance(b.endPos, a.position);
                //if its in influence distance and closer than current closest branch (or if theres no closest branch) then this branch is now closest
                if(distance < influenceDistance && a.closestBranch == null)
                {
                    a.closestBranch = b;
                    //Debug.Log("attraction point at " + a.position + " closest branch = " + b.endPos);
                }
                else if(distance < influenceDistance && distance < Vector3.Distance(a.position, a.closestBranch.endPos))
                {
                    a.closestBranch = b;
                    //Debug.Log("attraction point at " + a.position + " closest branch = " + b.endPos);
                }
            }
        }

        //tell branches which points are affecting them
        foreach(var a in attractionPoints)
        {
            if(a.closestBranch != null)
            {
                //a.closestBranch.pointsInRange.Clear(); //clear points in range
                a.closestBranch.pointsInRange.Add(a.position);
            }
        }

        foreach(Branch b in branches)
        {
            if(b.pointsInRange.Count > 0) //if the branch has influence points
            {
                //Debug.Log("Branch at " + b.endPos + " has " + b.pointsInRange.Count + " points in range");
                Vector3 newBranchDir = Vector3.zero;
                Vector3 branchToPoint = Vector3.zero;
                for(int i = 0; i < b.pointsInRange.Count;i++)
                {
                    //get normalised vector from tree node to each influencing point
                    branchToPoint = b.pointsInRange[i] - b.endPos;
                    branchToPoint.Normalize();
                    //add together
                    newBranchDir += branchToPoint;
                }
                //then normalize again
                newBranchDir.Normalize();

                //add new branches
                newBranches.Add(new Branch(b.endPos, newBranchDir));
                Debug.Log("New branch is being added: startPos = " + b.endPos + " \nDirection = " +  newBranchDir);
            }
        }

        //add these new nodes, remove points, repeat.
        branches.AddRange(newBranches);

        foreach (Branch b in branches)
        {
            b.pointsInRange.Clear(); //clear points in range
            //Debug.Log("points in range cleared");
        }
        newBranches.Clear();


        //remove attraction points that have been reached
        for (int i = 0; i < attractionPoints.Count; i++)
        {
            //also reset closest branch
            attractionPoints[i].closestBranch = null;
            foreach (Branch branch in branches)
            {
                if (Vector3.Distance(branch.endPos, attractionPoints[i].position) < killDistance) //if a branch is in the kill distance
                {
                    Debug.Log("attraction point at " + attractionPoints[i].position + " has been removed.");
                    attractionPoints.Remove(attractionPoints[i]); //remove that point
                    break;
                }
            }
        }

        //stop when all points removed / not in range

    }

    private void DrawSphere()
    {
        //crown
        List<int> triangleList = new List<int>();
        List<Vector3> normalList = new List<Vector3>();

        Vector3[] vertices;
        int[] triangles;
        Vector3[] normals;

    GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "sphere";

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
        for(int i = 0; i < verticesList.Count;  i++)
        {
            attractionPoints.Add(new AttractionPoints(verticesList[i]));
        }
    }

    private void OnDrawGizmos()
    {
        if(TogglePointsView == false)
        {
            //draw crown
            Gizmos.color = Color.green;
            Gizmos.DrawWireMesh(mesh);

            //draw attraction points
            foreach (var attraction in attractionPoints)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(attraction.position, 0.5f);

                //draw influence + kill distance
                if (displayKillDistance == true)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(attraction.position, killDistance);
                }
            
                if (displayInfluenceDistance == true) 
                { 
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(attraction.position, influenceDistance);
                }
            }
        }

        //draw branches
        foreach (Branch branch in branches)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(branch.startPos, 0.2f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(branch.endPos, 0.2f);
        }
    }
}