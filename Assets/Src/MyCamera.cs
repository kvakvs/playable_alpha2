using UnityEngine;
using System.Collections;

public class MyCamera : MonoBehaviour {
	public float mouseSensitivity = 0.01f;
	private Vector3 lastPosition;
	const float CAMERA_Z = -10;

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
			transform.position = new Vector3(ms_pos.x, ms_pos.y, CAMERA_Z);
			VoxelMap.instance.OnCameraPosChanged();
		}
	}
}
