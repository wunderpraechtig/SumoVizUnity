using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleExtrudeGeometry : ExtrudeGeometry  {

	public static void create  (string name, List<Vector2> verticesList, float height, float zOffset) {

		Material topMaterial;
		Material sideMaterial;
		
		GeometryLoader gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();

		if (height<=4.0) {
			sideMaterial = gl.theme.getBoxMaterial();
			topMaterial =  gl.theme.getBoxMaterial();
		} else {
			topMaterial = gl.theme.getRoofMaterial();
			sideMaterial = gl.theme.getHouseMaterial();
			sideMaterial.SetTextureScale("_MainTex",gl.theme.getTextureScaleForHeight((float)height));
		}

		ExtrudeGeometry.create (name, verticesList, height, zOffset, topMaterial, sideMaterial, 13);
	}

    public static void CreateMeshes(string name, List<Vector2> verticesList, float height, float zOffset, ref List<Mesh> sideMeshes, ref List<Mesh> topMeshes)
    {
        ExtrudeGeometry.CreateMeshes(name, verticesList, height, zOffset, 13, ref sideMeshes, ref topMeshes);
    }
}

