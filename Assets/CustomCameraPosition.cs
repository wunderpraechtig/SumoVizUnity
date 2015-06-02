using UnityEngine;
using System.Collections;

public class CustomCameraPosition : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//Camera.current.transform.position += new Vector3(0, -10, 10);
		//transform.position += transform.
		// transform.position += new Vector3(0, 10, -10);

		//TeleportRandomly();
		InvokeRepeating("TeleportRandomly", 2, 1);


	}
	
	// Update is called once per frame
	void Update () {
		// Camera.current.transform.position += new Vector3(0, -1, 1);
	}

	public void TeleportRandomly() {
		Vector3 direction = Random.onUnitSphere;
		direction.y = Mathf.Clamp(direction.y, 0.5f, 1f);
		float distance = 2 * Random.value + 1.5f;
		transform.localPosition = direction * distance;
	}
}
