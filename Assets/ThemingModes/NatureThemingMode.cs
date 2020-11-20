﻿using UnityEngine;
using System.Collections;

public class NatureThemingMode : ThemingMode {
	#region ThemingMode implementation

	public override string getTerrainName()
	{
		return "TerrainNature";
	}

    public override Material getFloorMaterial()
    {
        return (Material)Resources.Load("WallSeeThrough", typeof(Material));
    }

    public override Material getWallsMaterial ()
	{
		return (Material) Resources.Load("WallNature", typeof(Material));
	}

    public override Material getWallsMaterialST()
    {
        return (Material)Resources.Load("WallNature", typeof(Material));
    }

    public override Material getWallsGlow()
    {
        return (Material)Resources.Load("TopGlow", typeof(Material));
    }

    public override Material getBoxMaterial ()
	{
		return (Material) Resources.Load("Woodbox", typeof(Material));
	}

	public override Material getHouseMaterial ()
	{
		return (Material) Resources.Load("House", typeof(Material));
	}

	public override Material getRoofMaterial ()
	{
		return (Material) Resources.Load("Roof", typeof(Material));
	}

	public override Vector2 getTextureScaleForHeight (float height)
	{
		float y = 0;
		if (height < 5) y=height/0.44f;
		else if (height < 7) y=height/0.67f;
		else {
			int fulltextures = (int)height/7;
			y=height/fulltextures;
		}
		return new Vector2 (0.5f, 1/y);
	}

	#endregion


}
