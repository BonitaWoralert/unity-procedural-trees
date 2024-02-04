using System;
using System.Collections;
using System.Collections.Generic;
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

    public void CreateGeometry(List<Branch> branches, float slices)
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

        float x, y, z;
        float theta = ((float)Mathf.PI * 2) / slices;

        Quaternion directionAdjust = Quaternion.FromToRotation(Vector3.down, branches[0].direction);
        
        for(int i = 0; i <= slices; ++i)
        {
            x = radius * Mathf.Cos(i*theta);
            y = 0;
            z = radius * Mathf.Sin(i*theta);

            Vector3 position = new Vector3(x, y, z);
            position = directionAdjust * position;
            position += branches[0].startPos;
            verticesList.Add(position);
        }

        verticesList.Add(branches[0].startPos); //center vertex
        int centerIndex = verticesList.Count-1;

        for (int j = 0; j < slices; ++j)
        {
            triangleList.Add(centerIndex);
            triangleList.Add(baseIndex + j + 1);
            triangleList.Add(baseIndex + j);
        }



        /*for(int i = 1; i < branches.Count; i++)
        {
            directionAdjust = Quaternion.FromToRotation(Vector3.up, branches[i].direction);
        }*/




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
        mesh.Optimize();
    }

    /*private void CreateGeometry()
    {
        *//*create circle with the following formula:
         * x = radius * cos(theta)
         * y = radius * sin(theta)
         * z = height
         * theta runs from 0 to 2 * PI
         *//*

        float numVerticesPerRow = slices + 1;
        float numVertices = numVerticesPerRow * 2 + 2;

        List<Vector3> verticesList = new List<Vector3>();
        List<int> triangleList = new List<int>();
        
        Vector3[] vertices;
        int[] triangles;

        //vertex buffer

        float theta = 0.0f;
        float horizontalAngularStride = ((float)Mathf.PI * 2) / (float)slices;
        Quaternion directionAdjust = Quaternion.FromToRotation(Vector3.up, direction);

        for (int verticalIt = 0; verticalIt < 2; verticalIt++)
        {
            for (int horizontalIt = 0; horizontalIt < numVerticesPerRow; horizontalIt++)
            {
                float x;
                float y;
                float z;

                theta = (horizontalAngularStride * horizontalIt);

                if (verticalIt == 0)
                {
                    // upper circle
                    x = radiusTop * (float)Math.Cos(theta);
                    y = radiusTop * (float)Math.Sin(theta);
                    z = height;
                }
                else
                {
                    // lower circle
                    x = radiusBottom * (float)Math.Cos(theta);
                    y = radiusBottom * (float)Math.Sin(theta);
                    z = 0;
                }

                Vector3 position = new Vector3(x, z, y);
                position = directionAdjust * position;
                verticesList.Add(position);
            }
        }

        verticesList.Add(directionAdjust * new Vector3(0, height, 0));
        verticesList.Add(directionAdjust * Vector3.zero);

        //index buffer

        float numIndices = slices * 2 * 6;

        for (int verticalIt = 0; verticalIt < 1; verticalIt++)
        {
            for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
            {
                short lt = (short)(horizontalIt + verticalIt * (numVerticesPerRow));
                short rt = (short)((horizontalIt + 1) + verticalIt * (numVerticesPerRow));

                short lb = (short)(horizontalIt + (verticalIt + 1) * (numVerticesPerRow));
                short rb = (short)((horizontalIt + 1) + (verticalIt + 1) * (numVerticesPerRow));

                triangleList.Add(lt);
                triangleList.Add(rt);
                triangleList.Add(lb);

                triangleList.Add(rt);
                triangleList.Add(rb);
                triangleList.Add(lb);
            }
        }

        for (int verticalIt = 0; verticalIt < 1; verticalIt++)
        {
            for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
            {
                short lt = (short)(horizontalIt + verticalIt * (numVerticesPerRow));
                short rt = (short)((horizontalIt + 1) + verticalIt * (numVerticesPerRow));

                short patchIndexTop = (short)(numVerticesPerRow * 2);

                triangleList.Add(lt);
                triangleList.Add(patchIndexTop);
                triangleList.Add(rt);
            }
        }

        for (int verticalIt = 0; verticalIt < 1; verticalIt++)
        {
            for (int horizontalIt = 0; horizontalIt < slices; horizontalIt++)
            {
                short lb = (short)(horizontalIt + (verticalIt + 1) * (numVerticesPerRow));
                short rb = (short)((horizontalIt + 1) + (verticalIt + 1) * (numVerticesPerRow));


                short patchIndexBottom = (short)(numVerticesPerRow * 2 + 1);
                triangleList.Add(lb);
                triangleList.Add(rb);
                triangleList.Add(patchIndexBottom);
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
    }*/
}
