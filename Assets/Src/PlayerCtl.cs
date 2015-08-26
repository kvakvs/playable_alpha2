using UnityEngine;
using System.Collections;

public class PlayerCtl : MonoBehaviour {
	enum MovementDecision {
		Stay, Move, StepUp
	};

	const float VOXEL_SIZE = Terrain.VOXEL_SIZE;
	const float MAX_SPEED = VOXEL_SIZE * 10f; // 10 vox per sec
	const float WALK_SPEED = VOXEL_SIZE * 0.2f; // speed used in update
	const float FALL_SPEED = VOXEL_SIZE * 0.3f; // fall speed in update
	const float JUMP_SPEED = VOXEL_SIZE * 0.3f; // fall speed in update
	//float jumpForce; 
	public bool isStanding; // feet on the ground

	Animator anim;
	bool facingRight = true;
	bool isJumping = false;

	// Player can not move through the solid layer
	private const int COLLISION_MASK = TerrainChunk.LAYERMASK_SOLID;

	const int LAYERID_PLAYER = 8;
	public const int LAYERMASK_PLAYER = 1 << LAYERID_PLAYER;

	void Start () {
		anim = GetComponent<Animator>();
		//jumpForce = Terrain.VOXEL_SIZE * 48f * Mathf.Abs(Physics2D.gravity.y);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			isJumping = true;
		}

		// BUG BUG: When player is out of screen, colliders are deleted and player falls

		if (Input.GetMouseButton /*Down*/(0)) {
			//RaycastHit hitInfo;
			Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Terrain.instance.EditVoxels(p);
			anim.SetTrigger("Dig");
		}

		// Movement handling
		float moveX = Input.GetAxis("Horizontal");
		CheckWallsAndSteps(moveX);
		if (isJumping) {
			CheckCeiling();
		} else {
			// if we are not jumping, we may be falling
			CheckFloorAndFall();
		}
	}

	// Want to move left or right. Check walls, possibly 1-voxel steps
	void CheckWallsAndSteps (float moveX)
	{
		anim.SetFloat("Speed", Mathf.Abs(moveX));
		if (Mathf.Abs(moveX) < 0.1f) {
			return;
		}

		bool newFacingRight = facingRight;
		if (moveX < 0.1f) {
			newFacingRight = false;
		} else if (moveX > 0.1f) {
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

		float dirX = WALK_SPEED;
		if (moveX < 0) dirX = -WALK_SPEED;

		switch (CanMove (dirX)) {
		case MovementDecision.Move:
			transform.position += new Vector3(dirX, 0f, 0f);
			return;
		case MovementDecision.StepUp:
			transform.position += new Vector3(dirX * .5f, VOXEL_SIZE * .5f, 0f);
			return;
		case MovementDecision.Stay:
			return;
		}
	}

	MovementDecision CanMove(float dirX) {
		// Head forward
		var r1 = CastRay(new Vector2(transform.position.x, transform.position.y + VOXEL_SIZE * .5f), 
	                      new Vector2(dirX, 0));
		if (r1.collider != null) { return MovementDecision.Stay; }

		// Torso forward
		var r2 = CastRay(new Vector2(transform.position.x, transform.position.y - VOXEL_SIZE * .5f), 
		                  new Vector2(dirX, 0));
		if (r2.collider != null) { return MovementDecision.Stay; }

		// Feet forward
		var feet = CastRay(new Vector2(transform.position.x, transform.position.y - VOXEL_SIZE * 1.5f), 
		                    new Vector2(dirX, 0));		

		// If feet collide with the wall, but torso and head did not - we can try and step up
		if (feet.collider != null) {
			return MovementDecision.StepUp;
		}
		return MovementDecision.Move;
	}

	//
	// Possibly falling. Check voxels under the feet
	//
	void CheckFloorAndFall() {
		float feetY = transform.position.y - VOXEL_SIZE * 1.55f;
		var r1 = CastRay(new Vector2(transform.position.x - VOXEL_SIZE * .5f, feetY), Vector2.down);
		var r2 = CastRay(new Vector2(transform.position.x + VOXEL_SIZE * .5f, feetY), Vector2.down);

		// Shift values so that we have to deal with left collision, or both
		if (r1.collider == null) { r1 = r2; }
		this.isStanding = (r1.collider != null);

		float hitY = r2.collider != null ? Mathf.Max (r1.point.y, r2.point.y) : r1.point.y;
		float distanceToFloor = Mathf.Abs(hitY - feetY);

		float fall = Mathf.Min (distanceToFloor, FALL_SPEED);
		transform.position += new Vector3(0f, -fall, 0f);
	}

	//
	// Jumping (moving vertically up) so check the upper limit
	//
	void CheckCeiling() {
		float headY = transform.position.y + VOXEL_SIZE * .95f;
		var r1 = CastRay(new Vector2(transform.position.x - VOXEL_SIZE * .5f, headY), Vector2.up);
		var r2 = CastRay(new Vector2(transform.position.x + VOXEL_SIZE * .5f, headY), Vector2.up);
		
		// Shift values so that we have to deal with left collision, or both
		if (r1.collider == null) { r1 = r2; }

		float hitY = r2.collider != null ? Mathf.Min (r1.point.y, r2.point.y) : r1.point.y;
		float distanceToCeil = Mathf.Abs(hitY - headY);
		if (distanceToCeil < 0.5f * VOXEL_SIZE) {
			isJumping = false;
		} else {
			float jump = Mathf.Min (distanceToCeil, JUMP_SPEED);
			transform.position += new Vector3(0f, jump, 0f);
		}
	}

	RaycastHit2D CastRay(Vector2 origin, Vector2 direction) {
		Ray2D testRay = new Ray2D(origin, direction);		
		Debug.DrawRay(testRay.origin, testRay.direction);
		
		const float deltaX = VOXEL_SIZE;
		RaycastHit2D foundHit = Physics2D.Raycast(origin, direction, deltaX, COLLISION_MASK);
		if (foundHit.collider != null) {
			Debug.DrawRay(testRay.origin, testRay.direction, Color.white);
		} else {
			Debug.DrawRay(testRay.origin, testRay.direction, Color.red);
		}
		return foundHit;
	}
}
