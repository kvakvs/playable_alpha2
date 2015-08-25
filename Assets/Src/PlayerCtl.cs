using UnityEngine;
using System.Collections;

public class PlayerCtl : MonoBehaviour {

	const float MAX_SPEED = Terrain.VOXEL_SIZE * 10f; // 10 vox per sec
	const float JUMP_FORCE = 160f;

	Rigidbody2D rb2d;
	Animator anim;
	bool facingRight = true;

	void Start () {
		rb2d = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
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

		anim.SetFloat("Speed", Mathf.Abs(moveX));

		bool newFacingRight = facingRight;
		if (moveX < 0f) {
			newFacingRight = false;
		} else if (moveX > 0f) {
			newFacingRight = true;
		}
		if (newFacingRight != facingRight) {
			facingRight = newFacingRight;
			if (facingRight) {
				transform.localScale = new Vector3(1f, 1f, 1f);
			} else {
				transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}
	}
}
