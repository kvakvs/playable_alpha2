using UnityEngine;
using System.Collections;

public class PlayerCtl : MonoBehaviour {

	Rigidbody2D rb2d;
	const float MAX_SPEED = 0.6f;
	const float JUMP_FORCE = 160f;

	void Start () {
		rb2d = GetComponent<Rigidbody2D>();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			rb2d.AddForce(new Vector2(0, JUMP_FORCE));
		}

		// BUG BUG: When player is out of screen, colliders are deleted and player falls
	}

	void FixedUpdate () {
		float moveX = Input.GetAxis("Horizontal");

		rb2d.velocity = new Vector2(moveX * MAX_SPEED, rb2d.velocity.y);
	}
}
