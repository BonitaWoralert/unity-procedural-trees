using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.Mesh;

//https://apparat-engine.blogspot.com/2013/04/procdural-meshes-cylinder.html
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class CreateCylinder : MonoBehaviour
{
    //[SerializeField] public float slices = 16, height = 7, radiusTop = 2, radiusBottom = 4;
    float radius = 0.2f;
    Mesh mesh;
    //public Vector3 direction;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "cylinder";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CreateGeometry(List<Branch> branches, int slices)
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

        /*//create circle for the start node
        directionAdjust = Quaternion.FromToRotation(Vector3.down, branches[0].direction);
        for (int j = 0; j <= slices; j++)
        {
            x = radius * Mathf.Cos(j * theta);
            y = 0;
            z = radius * Mathf.Sin(j * theta);

            Vector3 position = new Vector3(x, y, z);
            position = directionAdjust * position; //rotate accordingly
            position += branches[0].startPos; //move to correct location
            verticesList.Add(position); //add to list
        }

        verticesList.Add(branches[0].startPos); //center vertex
        centerIndex = verticesList.Count - 1;

        for (int j = 0; j < slices; j++) //set triangles
        {
            triangleList.Add(centerIndex);
            triangleList.Add(baseIndex + j + 1);
            triangleList.Add(baseIndex + j);
        }*/

        //for each branch, create circle at the end point
        for (int i = 0; i < branches.Count; i++) 
        {
            baseIndex = verticesList.Count;
            directionAdjust = Quaternion.FromToRotation(Vector3.up, branches[i].direction);
            for (int j = 0; j <= slices; j++)
            {
                x = radius * Mathf.Cos(j * theta);
                y = 0;
                z = radius * Mathf.Sin(j * theta);

                Vector3 position = new Vector3(x, y, z); 
                position = directionAdjust * position; //rotate accordingly
                position += branches[i].endPos; //move to correct location
                verticesList.Add(position); //add to list
            }

            verticesList.Add(branches[i].endPos); //center vertex
            centerIndex = verticesList.Count- 1;

            for (int j = 0; j < slices; j++) //set triangles
            {
                triangleList.Add(centerIndex);
                triangleList.Add(baseIndex + j + 1);
                triangleList.Add(baseIndex + j);
            }
        }

        int b = 0, t = 0;
        //connect together to make a cylinder!
        for (int i = 0; i < 11; i++)
        {
            if(i == 0) //test with just one
            {
                b = 0;
                t = b + slices+2;
                for(int j=0;j<slices;j++)
                {
                    triangleList.Add(b + j);
                    triangleList.Add(t + j);
                    triangleList.Add(b + j + 1);

                    triangleList.Add(b + j + 1);
                    triangleList.Add(t + j);
                    triangleList.Add(t + j + 1);
                }
            }
            else if (branches[i].children.Count != 0)
            {
                b += slices + 2;
                t = b + slices + 2;
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

        //optimise ?
        mesh.RecalculateNormals();
        mesh.Optimize();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireMesh(mesh);
    }
}