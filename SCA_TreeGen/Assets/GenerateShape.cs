using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mesh;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class GenerateShape : MonoBehaviour
{
    private bool finishedGenerating = false;

    [Header("Info")]
    [SerializeField] private int iterationCount = 0;
    [SerializeField] private int maxIterations = 100;
    [SerializeField] private int numOfBranches = 0;
    [SerializeField] private float timeBetweenIterations = 1.0f;
    private float nextIteration = 0.0f;
    [SerializeField] private int randomSeed = 0;

    [Header("Gizmos")]
    [SerializeField] private bool TogglePointsView = false;
    [SerializeField] private bool ShowOnlyDuplicates = false;
    [SerializeField] private Material branchMat;

    [Header("Crown Variables (sphere)")]
    [SerializeField][Range(0,25)] private float radius = 8;
    [SerializeField][Range(0,100)] private int sliceCount = 10, stackCount = 10;
    Mesh mesh;
    List<Vector3> verticesList = new List<Vector3>();

    //branches

    [Header("Branches")]
    private List<Branch> branches = new List<Branch>();
    private List<Branch> newBranches = new List<Branch>();
    private List<Branch> duplicateBranches = new List<Branch>();
    private int numOfBranchesAtBeginning = 0;

    class Branch
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector3 direction;
        public List<Vector3> pointsInRange = new List<Vector3>();
        public float branchLength = 0.5f;
        private Branch parent;
        public List<Branch> children = new List<Branch>();

        public Branch(Branch parent, Vector3 direction)
        {
            startPos = parent.endPos; this.direction = direction; this.parent = parent;
            endPos = startPos + (direction * branchLength);
            parent.children.Add(this);
        }
        public Branch(Vector3 startPos, Vector3 direction)
        {
            this.startPos = startPos; this.direction = direction;
            endPos = startPos + (direction * branchLength);
            parent = null;
        }
    }

    //attraction points
    private List<AttractionPoints> attractionPoints = new List<AttractionPoints>();
    [Header("Attraction Points")]
    [SerializeField] private bool displayKillDistance = false;
    [SerializeField][Range(0, 10)] private float killDistance = 1f;
    [SerializeField] private bool displayInfluenceDistance = false;
    [SerializeField][Range(0, 10)] private float influenceDistance = 2f;
    [SerializeField] private List<AttractionPoints> currentAttractionPoints = new List<AttractionPoints>();
    [SerializeField] private int numOfAttractionPoints = 100;

    class AttractionPoints
    {
        public Branch closestBranch;
        public Vector3 position;

        public AttractionPoints(Vector3 position)
        { this.position = position; }
    }


    private void Start()
    {
        Random.InitState(randomSeed);
        //DrawSphere();
        GenerateAttractionPoints();

       /* MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        meshFilter.mesh.CombineMeshes(combine, true, true);
        gameObject.SetActive(true);*/


        /*
                mesh = new Mesh();
                mesh.CombineMeshes(combine, true, true);
                transform.GetComponent<MeshFilter>().sharedMesh = mesh;
                transform.gameObject.SetActive(true);*/
        ///

        Branch firstBranch = new Branch(new Vector3(0, -radius - Random.Range(5.0f, 7.5f), 0), Vector3.up);
        branches.Add(firstBranch);
    }

    private void Update()
    {
        if (!finishedGenerating)
            GenerateTree();
        else
        {
            //Debug.Log("Finished!");
            Debug.Log("Time from start to end = " + Time.time);
            DrawGeometry();
            Debug.Log("Time from start to end = " + Time.time);
            Debug.Break();
            //Debug.Log("Num of duplicate branches = " + duplicateBranches.Count);
        }
    }

    private void DrawBranch(float topRadius, float bottomRadius, float height, Vector3 position, Vector3 direction)
    {
        GameObject testCylinder = new GameObject("branch", typeof(CreateCylinder));
        testCylinder.transform.SetParent(this.transform);
        testCylinder.GetComponent<MeshRenderer>().material = branchMat;
        CreateCylinder thing = testCylinder.GetComponent<CreateCylinder>();
        thing.radiusTop = topRadius;
        thing.radiusBottom = bottomRadius;
        thing.transform.position = position;
        thing.transform.rotation = Quaternion.LookRotation(direction) * new Quaternion(0,90,90,0);
        thing.height = height;
    }

    private void DrawGeometry()
    {
        foreach(Branch branch in branches)
        {
            DrawBranch(0.1f, 0.1f, branch.branchLength, Vector3.Lerp(branch.endPos, branch.startPos, 0.5f), branch.direction);
        }
    }

    private void GenerateTree()
    {
        if (Time.time > nextIteration) //to watch tree grow slowly
        {
            nextIteration = Time.time + timeBetweenIterations;

            //if not reached max iterations + there are still points
            if (iterationCount < maxIterations && attractionPoints.Count > 0)
            {
                iterationCount++;
                numOfBranches = branches.Count;
                numOfBranchesAtBeginning = numOfBranches;

                //attraction points pick nearest branch node 
                foreach (var a in attractionPoints)
                {
                    foreach (Branch b in branches)
                    {
                        //find distance between the point and the branch
                        float distance = Vector3.Distance(b.endPos, a.position);
                        //if its in influence distance and closer than current closest branch (or if theres no closest branch) then this branch is now closest
                        if (distance < influenceDistance && a.closestBranch == null)
                        {
                            a.closestBranch = b;
                            //Debug.Log("attraction point at " + a.position + " closest branch = " + b.endPos);
                        }
                        else if (distance < influenceDistance && distance < Vector3.Distance(a.position, a.closestBranch.endPos))
                        {
                            a.closestBranch = b;
                            //Debug.Log("attraction point at " + a.position + " closest branch = " + b.endPos);
                        }
                    }
                }

                //tell branches which points are affecting them
                foreach (var a in attractionPoints)
                {
                    if (a.closestBranch != null)
                    {
                        currentAttractionPoints.Add(a);
                        a.closestBranch.pointsInRange.Add(a.position);
                    }
                }

                if (currentAttractionPoints.Count == 0 && iterationCount < 15) //if we cant reach a point in the first 10 iterations
                {
                    //grow upwards
                    newBranches.Add(new Branch(branches.Last(), Vector3.up));
                    //Debug.Log("No point in range, growing upwards");

                    //later introduce randomisation so it doesn't just suddenly grow up
                }
                else if (currentAttractionPoints.Count == 0 && iterationCount > 15) //if we can't reach point after first 5, stop trying
                {
                    finishedGenerating = true;
                }

                foreach (Branch b in branches)
                {
                    //Debug.Log(b.children.Count);
                    if (b.pointsInRange.Count > 0 && b.children.Count <= 3) //if the branch has influence points and less than 5 children
                    {
                        //Debug.Log("Branch at " + b.endPos + " has " + b.pointsInRange.Count + " points in range");
                        Vector3 newBranchDir = Vector3.zero;
                        Vector3 branchToPoint;
                        for (int i = 0; i < b.pointsInRange.Count; i++)
                        {
                            //get normalised vector from tree node to each influencing point
                            branchToPoint = new Vector3(
                                b.pointsInRange[i].x - b.endPos.x,
                                b.pointsInRange[i].y - b.endPos.y,
                                b.pointsInRange[i].z - b.endPos.z);

                            branchToPoint.Normalize();
                            //add together
                            newBranchDir += branchToPoint;
                        }
                        //then normalize again
                        newBranchDir.Normalize();

                        //add new branches
                        newBranches.Add(new Branch(b, newBranchDir));

                        //test code
                        /*if (branches.Exists(x => x.endPos == newBranches.Last().endPos))
                        {
                            duplicateBranches.Add(new Branch(b, newBranchDir));
                            newBranches.Remove(newBranches.Last());
                            //Debug.Log("not this branch!!");
                        }*/

                        //Debug.Log("New branch is being added: startPos = " + b.endPos + " \nDirection = " +  newBranchDir + " endPos = " + newBranches.Last().endPos);
                    }
                }

                //add these new nodes, remove points, repeat.
                branches.AddRange(newBranches);

                List<int> pointsToDelete = new List<int>();
                //remove attraction points that have been reached
                //for (int i = 0; i < attractionPoints.Count; i++)
                for (int i = attractionPoints.Count - 1; i >= 0; i--)
                {
                    //also reset closest branch
                    attractionPoints[i].closestBranch = null;

                    foreach (Branch branch in newBranches)
                    {
                        if (Vector3.Distance(branch.endPos, attractionPoints[i].position) < killDistance) //if a branch is in the kill distance
                        {
                            //Debug.Log("attraction point at " + attractionPoints[i].position + " has been removed.");
                            pointsToDelete.Add(i);
                            //attractionPoints.Remove(attractionPoints[i]); //remove that point
                        }
                    }
                }
                for (int i = 0; i < pointsToDelete.Count; i++)
                    attractionPoints.RemoveAt(pointsToDelete[i]);

                //clean up after iteration 

                foreach (Branch b in branches)
                {
                    b.pointsInRange.Clear();
                }
                newBranches.Clear();
                currentAttractionPoints.Clear();


            }
            else
            {
                finishedGenerating = true;
            }
        }
        if (numOfBranchesAtBeginning == branches.Count)
        {
            finishedGenerating = true;
            //iterationCount = 9999;
        }
    }
    /*
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
    */
    private void GenerateAttractionPoints()
    {
        //Vertex method: Attraction points are placed at each vertex of the crown

        /*for (int i = 0; i < verticesList.Count; i++)
        {
            attractionPoints.Add(new AttractionPoints(verticesList[i]));
        }*/

        //Random method
        for (int i = 0; i < numOfAttractionPoints; i++)
        {
            attractionPoints.Add(new AttractionPoints(Random.insideUnitSphere * radius));
        }
    }

    private void OnDrawGizmos()
    {
        if(TogglePointsView == false)
        {
            //draw crown
            Gizmos.color = Color.magenta;
            //Gizmos.DrawWireMesh(mesh, transform.InverseTransformPoint(transform.position));

            //draw attraction points
            foreach (var attraction in attractionPoints)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.InverseTransformPoint(attraction.position), 0.5f);

                //draw influence + kill distance
                if (displayKillDistance == true)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.InverseTransformPoint(attraction.position), killDistance);
                }
            
                if (displayInfluenceDistance == true) 
                { 
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.InverseTransformPoint(attraction.position), influenceDistance);
                }
            }
        }


        //draw branches
        if(ShowOnlyDuplicates == false)
        {
            foreach(Branch branch in branches)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(transform.InverseTransformPoint(branch.startPos), transform.InverseTransformPoint(branch.endPos));
            }
        }
        foreach(Branch duplicate in duplicateBranches)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.InverseTransformPoint(duplicate.startPos), transform.InverseTransformPoint(duplicate.endPos));
        }
    }
}