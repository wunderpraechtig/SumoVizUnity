﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallExtrudeGeometry : ExtrudeGeometry  {

    private static GeometryLoader gl = null;

    public static void create  (string name, List<Vector2> verticesList, float height, float width, float zOffset) {

		int wallpoints = verticesList.Count;
		float wall = width;
		
		//extrude the wall
		for (int i = wallpoints-1;i>=0;i--) {
			
			Vector2 extrudePoint;
			
			if (i == wallpoints-1) {
				
				Vector2 lot = verticesList[i-1]-verticesList[i];
				lot = new Vector2(-lot.y,lot.x);
				lot.Normalize();
				extrudePoint = verticesList[i]+lot*wall;
			} else if (i == 0) {
				
				Vector2 lot = verticesList[0]-verticesList[1];
				lot = new Vector2(-lot.y,lot.x);
				lot.Normalize();
				extrudePoint = verticesList[0]+lot*wall;
			} else {
				float angle = Vector2.Angle(verticesList[i]-verticesList[i-1],verticesList[i]-verticesList[i+1]);
				
				float thickness;
				
				if (Mathf.Cos(angle/2)!=0) {
					thickness = wall/Mathf.Sin(angle*Mathf.Deg2Rad/2);
				} else {
					thickness = 100;
				}
				
				Vector3 cross = Vector3.Cross(verticesList[i]-verticesList[i-1],verticesList[i]-verticesList[i+1]);
				if (cross.z > 0) angle = 360 -angle;
				
				Vector2 lot = verticesList[i]-verticesList[i+1];
				lot = Quaternion.AngleAxis(angle/2, Vector3.forward) * lot;
				lot.Normalize();
				
				extrudePoint = verticesList[i]+lot*thickness;
			}
			
			verticesList.Add(extrudePoint);
		}

		if(gl == null)
            gl = GameObject.Find ("GeometryLoader").GetComponent<GeometryLoader> ();
		ExtrudeGeometry.create (name, verticesList, height, zOffset, gl.theme.getWallsMaterial(), gl.theme.getWallsMaterialST(), 12);
		
	}

    public static void CreateMeshes(string name, List<Vector2> verticesList, float height, float width, float zOffset, ref List<Mesh> sideMeshes, ref List<Mesh> topMeshes)
    {

        int wallpoints = verticesList.Count;
        float wall = width;

        //extrude the wall
        for (int i = wallpoints - 1; i >= 0; i--)
        {

            Vector2 extrudePoint;

            if (i == wallpoints - 1)
            {

                Vector2 lot = verticesList[i - 1] - verticesList[i];
                lot = new Vector2(-lot.y, lot.x);
                lot.Normalize();
                extrudePoint = verticesList[i] + lot * wall;
            }
            else if (i == 0)
            {

                Vector2 lot = verticesList[0] - verticesList[1];
                lot = new Vector2(-lot.y, lot.x);
                lot.Normalize();
                extrudePoint = verticesList[0] + lot * wall;
            }
            else
            {
                float angle = Vector2.Angle(verticesList[i] - verticesList[i - 1], verticesList[i] - verticesList[i + 1]);

                float thickness;

                if (Mathf.Cos(angle / 2) != 0)
                {
                    thickness = wall / Mathf.Sin(angle * Mathf.Deg2Rad / 2);
                }
                else
                {
                    thickness = 100;
                }

                Vector3 cross = Vector3.Cross(verticesList[i] - verticesList[i - 1], verticesList[i] - verticesList[i + 1]);
                if (cross.z > 0) angle = 360 - angle;

                Vector2 lot = verticesList[i] - verticesList[i + 1];
                lot = Quaternion.AngleAxis(angle / 2, Vector3.forward) * lot;
                lot.Normalize();

                extrudePoint = verticesList[i] + lot * thickness;
            }

            verticesList.Add(extrudePoint);
        }

        if (gl == null)
            gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();
        ExtrudeGeometry.CreateMeshes(name, verticesList, height, zOffset, 12 , ref sideMeshes, ref topMeshes);

    }
}

