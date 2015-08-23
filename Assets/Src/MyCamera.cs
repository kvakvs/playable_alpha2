using UnityEngine;
using System.Collections;

public class MyCamera : MonoBehaviour {
	public float mouseSensitivity = 0.01f;
	private Vector3 lastPosition;
	const float CAMERA_Z = -10;

	// Use this for initialization
	void Start () {
		//main = this;
	}
	
	// Update is called once per frame
	void Update () {
		//Camera.main.ScreenToWorldPoint(Input.mousePosition) 

		if (Input.GetMouseButtonDown(2))
		{
			var ms_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			transform.position = new Vector3(ms_pos.x, ms_pos.y, CAMERA_Z);
			VoxelMap.instance.OnCameraPosChanged();
			//lastPosition = Input.mousePosition;
		}

		if (Input.GetMouseButton(2))
		{
			//Vector3 delta = lastPosition - Input.mousePosition;
			//transform.Translate(delta.x * mouseSensitivity, delta.y * mouseSensitivity, 0);
			//lastPosition = Input.mousePosition;
			//transform.LookAt(ms_pos);
		}
	}
}
