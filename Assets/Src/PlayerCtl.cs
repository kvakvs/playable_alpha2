using UnityEngine;
using System.Collections;

public class PlayerCtl : MonoBehaviour {

	Rigidbody2D rb2d;
	const float MAX_SPEED = 0.5f;

	void Start () {
		rb2d = GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate () {
		float moveX = Input.GetAxis("Horizontal");

		rb2d.velocity = new Vector2(moveX * MAX_SPEED, rb2d.velocity.y);
	}
}
