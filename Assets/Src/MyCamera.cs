using UnityEngine;
using System.Collections;

public class MyCamera : MonoBehaviour {
	public float mouseSensitivity = 0.01f;
	private Vector3 lastPosition;

	const float CAMERA_Z = -10;
	const float CAMERA_SCROLL_SPEED = 0.04f;

	const float MAP_WIDTH = VoxelMap.MAP_WIDTH;
	const float MAP_HEIGHT = VoxelMap.MAP_HEIGHT;

	// Use this for initialization
	void Start () {
		transform.position = new Vector3(MAP_WIDTH / 2, MAP_HEIGHT);
		var pl = GameObject.Find ("Player");
		pl.transform.position = transform.position + new Vector3(-5f * VoxelMap.VOXEL_SIZE, 2f, 0);
		VoxelMap.instance.OnCameraPosChanged();
	}
	
	// Update is called once per frame
	void Update () {
		/*var pl = GameObject.Find ("Player");
		Vector3 dist = transform.localPosition - pl.transform.position;
		if (dist.magnitude > VoxelMap.VOXEL_SIZE) {
			transform.localPosition = pl.transform.position;
			VoxelMap.instance.OnCameraPosChanged();
		}*/

		// Free scroll with Middle Mouse
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
