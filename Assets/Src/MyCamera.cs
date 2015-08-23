using UnityEngine;
using System.Collections;

public class MyCamera : MonoBehaviour {
	public float mouseSensitivity = 0.01f;
	private Vector3 lastPosition;

	const float CAMERA_Z = -10;
	const float CAMERA_SCROLL_SPEED = 0.04f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(2))
		{
		}

		if (Input.GetMouseButton(2))
		{
			var ms_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			var diff = ms_pos - transform.position;
			diff.Normalize();
			diff *= CAMERA_SCROLL_SPEED;
			diff += transform.position;
			transform.position = new Vector3(diff.x, diff.y, CAMERA_Z);
				//new Vector3(ms_pos.x, ms_pos.y, CAMERA_Z);
			VoxelMap.instance.OnCameraPosChanged();
		}
	}
}
