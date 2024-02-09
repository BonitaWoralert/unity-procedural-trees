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

public class Branch
{
    public Vector3 startPos;
    public Vector3 endPos;
    public Vector3 direction;
    public List<Vector3> pointsInRange = new List<Vector3>();
    public float branchLength;
    public Branch parent;
    public List<Branch> children = new List<Branch>();
    public float radius; //default radius

    //mesh info
    public int startingIndex;

    public Branch(Branch parent, Vector3 direction, float length, float radius)
    {
        startPos = parent.endPos; this.direction = direction; this.parent = parent;
        this.radius = radius; this.branchLength = length;
        endPos = startPos + (direction * branchLength);
        parent.children.Add(this);
    }
    public Branch(Vector3 startPos, Vector3 direction, float length, float radius)
    {
        this.startPos = startPos; this.direction = direction;
        this.radius = radius; this.branchLength = length;
        endPos = startPos + (direction * branchLength);
        parent = this;
    }
}

class AttractionPoints
{
    public Branch closestBranch;
    public Vector3 position;

    public AttractionPoints(Vector3 position)
    { this.position = position; }
}

public class GenerateShape : MonoBehaviour
{
    private bool finishedGenerating = false;

    [Header("Info")]
    [SerializeField] private int iterationCount = 0;
    [SerializeField] private int numOfBranches = 0;
    [SerializeField] private int randomSeed = 0;

    [Header("Gizmos")]
    [SerializeField] private bool ShowPoints = false;

    [Header("Crown Variables (sphere)")]
    [SerializeField][Range(0,15)] private float radius = 7.5f;

    [Header("Branches")]
    [SerializeField] private int sliceCount = 8;
    [SerializeField] private Material branchMat;
    [SerializeField][Range(0.4f, 1.0f)] private float branchLength = 0.5f;
    [SerializeField][Range(0.0f, 1.0f)] private float branchRadius = 0.03f;
    [SerializeField][Range(0.01f, 0.05f)] private float branchRadiusIncrease = 0.02f;
    private List<Branch> branches = new List<Branch>();
    private List<Branch> newBranches = new List<Branch>();
    private int numOfBranchesAtBeginning = 0;

    //attraction points
    [Header("Attraction Points")]
    private List<AttractionPoints> attractionPoints = new List<AttractionPoints>();
    [SerializeField] private bool displayKillDistance = false;
    [SerializeField][Range(0, 10)] private float killDistance = 1f;
    [SerializeField] private bool displayInfluenceDistance = false;
    [SerializeField][Range(0, 10)] private float influenceDistance = 2f;
    [SerializeField] private List<AttractionPoints> currentAttractionPoints = new List<AttractionPoints>();
    [SerializeField] private int numOfAttractionPoints = 600;

    private void Start()
    {
        if(randomSeed !=0) //set to 0 if we want actual random
            Random.InitState(randomSeed);
        GenerateAttractionPoints();

        Branch firstBranch = new Branch(new Vector3(0, -radius - Random.Range(5.0f, 7.5f), 0), Vector3.up, branchLength, branchRadius);
        branches.Add(firstBranch);
    }

    private void Update()
    {
        if (!finishedGenerating)
        {
            GenerateTree();
        }
        else
        {
            DrawGeometry();
            Debug.Log("Time from start to end = " + Time.time);
            Debug.Break();
        }
    }

    private void DrawGeometry()
    {
        //geometry
        GameObject treeGeometry = new GameObject("treeMesh", typeof(CreateCylinder)); //create geometry gameobject
        treeGeometry.transform.SetParent(this.transform); //set it as a child of this object
        treeGeometry.GetComponent<MeshRenderer>().material = branchMat; //set material
        treeGeometry.GetComponent<CreateCylinder>().CreateGeometry(branches, sliceCount, branchRadiusIncrease); //create geometry with branches + specified slice count
    }

    private void GenerateTree()
    {
        //if there are still points
        if (attractionPoints.Count > 0)
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
                    }
                    else if (distance < influenceDistance && distance < Vector3.Distance(a.position, a.closestBranch.endPos))
                    {
                        a.closestBranch = b;
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

            if (currentAttractionPoints.Count == 0 && iterationCount < 15) //if we cant reach a point in the first 15 iterations
            {
                //grow upwards
                newBranches.Add(new Branch(branches.Last(), Vector3.up, branchLength, branchRadius));

                //later, randomisation can be introduces so it doesn't just suddenly grow up
            }
            else if (currentAttractionPoints.Count == 0 && iterationCount > 15) //if we can't reach point after first 15, stop trying
            {
                finishedGenerating = true;
            }

            foreach (Branch b in branches)
            {
                if (b.pointsInRange.Count > 0 && b.children.Count <= 3) //if the branch has influence points and less than 5 children
                {
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
                    newBranches.Add(new Branch(b, newBranchDir, branchLength, branchRadius));
                }
            }

            //add these new nodes, remove points, repeat.
            branches.AddRange(newBranches);

            List<int> pointsToDelete = new List<int>();
            //remove attraction points that have been reached
            for (int i = attractionPoints.Count - 1; i >= 0; i--)
            {
                //also reset closest branch
                attractionPoints[i].closestBranch = null;

                foreach (Branch branch in newBranches)
                {
                    if (Vector3.Distance(branch.endPos, attractionPoints[i].position) < killDistance) //if a branch is in the kill distance
                    {
                        pointsToDelete.Add(i);
                    }
                }
            }
            for (int i = 0; i < pointsToDelete.Count; i++)
                attractionPoints.RemoveAt(pointsToDelete[i]);

            //clean up after each iteration 
            foreach (Branch b in branches)
            {
                b.pointsInRange.Clear();
            }
            newBranches.Clear();
            currentAttractionPoints.Clear();

            if (numOfBranchesAtBeginning == branches.Count) //if branch amount has not changed
            {
                finishedGenerating = true;
            }
        }
        else
        {
            finishedGenerating = true;
        }
    }

    private void GenerateAttractionPoints()
    {
        //Random method
        for (int i = 0; i < numOfAttractionPoints; i++)
        {
            attractionPoints.Add(new AttractionPoints(Random.insideUnitSphere * radius));
        }
    }

    private void OnDrawGizmos()
    {
        if(ShowPoints == true)
        {
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
    }
}