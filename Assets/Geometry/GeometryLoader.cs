using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeometryLoader : MonoBehaviour {
    
	public ThemingMode theme;
	public Groundplane groundplane;

	public void setTheme(ThemingMode mode) {
		theme = mode;
		groundplane = theme.getTerrain().GetComponent<Groundplane> ();
	}
}
