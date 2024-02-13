using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.Mesh;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class CreateGeometry : MonoBehaviour
{
    Mesh mesh;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "cylinder";
    }

    public void CreateMesh(List<Branch> branches, int slices, float radiusIncrease)
    {
        SetRadii(branches, radiusIncrease);
        CalculateMesh(branches, slices);
    }

    private void CalculateMesh(List<Branch> branches, int slices)
    {
        /*create circle with the following formula:
         *x = radius * cos(theta)
         * y = radius * sin(theta)
         * z = height
         * theta runs from 0 to 2 * PI
         */

        List<Vector3> verticesList = new List<Vector3>();
        List<int> triangleList = new List<int>();
        Vector3[] vertices;
        int[] triangles;

        int baseIndex = verticesList.Count;
        int centerIndex;
        Quaternion directionAdjust;

        float x, y, z;
        float theta = ((float)Mathf.PI * 2) / slices; //theta

        //for each branch, create circle at the end point
        for (int i = 0; i < branches.Count; i++)
        {
            baseIndex = verticesList.Count;
            //adjust by branch direction
            directionAdjust = Quaternion.FromToRotation(Vector3.up, branches[i].GetDir());

            //store the starting index of each branch
            branches[i].SetStartingIndex(verticesList.Count);

            for (int j = 0; j <= slices; j++)
            {
                x = branches[i].GetRadius() * Mathf.Cos(j * theta);
                y = 0;
                z = branches[i].GetRadius() * Mathf.Sin(j * theta);

                Vector3 position = new Vector3(x, y, z);
                position = directionAdjust * position; //rotate accordingly
                position += branches[i].GetEndPos(); //move to correct location
                verticesList.Add(position); //add to list
            }

            //cylinder caps
            centerIndex = verticesList.Count; //store index of center vertex
            verticesList.Add(branches[i].GetEndPos()); //center vertex


            for (int j = 0; j < slices; j++) //set triangles
            {
                triangleList.Add(centerIndex);
                triangleList.Add(baseIndex + j + 1);
                triangleList.Add(baseIndex + j);
            }
        }

        //connect together to make a cylinder!
        //b and t are base and top circles starting indices

        //for first branch node
        int b = 0;
        int t = b + slices + 1;
        for (int j = 0; j < slices; j++)
        {
            triangleList.Add(b + j);
            triangleList.Add(t + j);
            triangleList.Add(b + j + 1);

            triangleList.Add(b + j + 1);
            triangleList.Add(t + j);
            triangleList.Add(t + j + 1);
        }

        //rest of the branches
        for (int i = 1; i < branches.Count; i++)
        {
            b = branches[i].GetParent().GetStartingIndex();
            t = branches[i].GetStartingIndex();

            for (int j = 0; j < slices; j++)
            {
                triangleList.Add(b + j);
                triangleList.Add(t + j);
                triangleList.Add(b + j + 1);

                triangleList.Add(b + j + 1);
                triangleList.Add(t + j);
                triangleList.Add(t + j + 1);
            }
        }

        //add to arrays
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

        //set it on the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //optimise and create normals
        mesh.RecalculateNormals();
        mesh.Optimize();
    }

    private void SetRadii(List<Branch> branches, float increaseValue)
    {
        float newRadius;
        for (int i = branches.Count - 1; i >= 0; i--)
        {
            newRadius = branches[i].GetRadius() + increaseValue; // new radius is current one + increase value
            if (branches[i].GetParent().GetRadius() < newRadius) //if parent branch is smaller than this value
                branches[i].GetParent().SetRadius(newRadius); //set the value
        }
    }
}