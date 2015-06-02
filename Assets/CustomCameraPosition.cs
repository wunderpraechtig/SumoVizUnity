using UnityEngine;
using System.Collections;

public class CustomCameraPosition : MonoBehaviour {

	// Use this for initialization
	void Start () {
		InvokeRepeating("TeleportRandomly", 2, 3);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void TeleportRandomly() {
		Vector3 direction = Random.onUnitSphere;
		direction.y = Mathf.Clamp(direction.y, 0.5f, 1f);
		float distance = 2 * Random.value + 15f;
		transform.localPosition = direction * distance;
		transform.position = direction * distance;
	}
}
