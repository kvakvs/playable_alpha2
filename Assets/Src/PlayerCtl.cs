using UnityEngine;
using System.Collections;

public class PlayerCtl : MonoBehaviour {
	enum MovementDecision {
		Stay, Move, StepOnABlock
	};

	const float VOXEL_SIZE = Terrain.VOXEL_SIZE;
	//const float MAX_SPEED = VOXEL_SIZE * 10f; // 10 vox per sec
	const float WALK_SPEED = VOXEL_SIZE * 12f; // speed (per second)
	const float FALL_SPEED = VOXEL_SIZE * 20f; // fall speed (per second)
	const float JUMP_SPEED = VOXEL_SIZE * 20f; // jump speed (per second)
	const float JUMP_DURATION = 0.2f;
	const float TOUCH_DISTANCE = 0.2f * VOXEL_SIZE; // how near can come to the wall
	const float HALF_CHAR_WIDTH = VOXEL_SIZE * 0.7f;
	const float HALF_FEET_WIDTH = VOXEL_SIZE * 0.7f;
	const float STEP_UP_SPEED = VOXEL_SIZE * 20f;
	const float PLAYER_CENTER_TO_FEET = VOXEL_SIZE * 1.55f;

	//float jumpForce; 
	public bool isGrounded; // feet on the ground
	float jumpStartTime = 0f;
	float stepLevel; // when step on a block detected, here will be new level Y

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
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			isJumping = true;
			jumpStartTime = Time.time;
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

		if (isJumping && Time.time - jumpStartTime >= JUMP_DURATION) {
			isJumping = false;
		}

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

		float wallTouchDist;
		switch (CanMove (moveX, out wallTouchDist)) {
		case MovementDecision.Move: {
				float dirX = Mathf.Min (wallTouchDist * .95f, 
			                        	WALK_SPEED * Time.deltaTime) * Mathf.Sign(moveX);
				transform.position += new Vector3(dirX, 0f, 0f);
				return; 
			}
		case MovementDecision.StepOnABlock: {
				float dirX = Mathf.Min (wallTouchDist * .95f, 
				                        WALK_SPEED * Time.deltaTime) * Mathf.Sign(moveX);
				transform.position = new Vector3(transform.position.x + dirX, 
			                                 stepLevel + PLAYER_CENTER_TO_FEET, 0f);
				return; 
			}
		case MovementDecision.Stay:
			return;
		}
	}

	MovementDecision CanMove(float moveX, out float wallTouchDist) {
		Vector2 dirVector = moveX > 0 ? Vector2.right : Vector2.left;
		float halfCharacter = dirVector.x * HALF_CHAR_WIDTH;
		float frontX = transform.position.x + halfCharacter;
		wallTouchDist = 99f;

		// Head forward (top)
		var r0 = CastRay(new Vector2(frontX, transform.position.y + VOXEL_SIZE * 1.45f), 
		                 dirVector);
		if (r0.collider != null) {
			wallTouchDist = Mathf.Abs (r0.point.x - frontX);
			if (wallTouchDist < TOUCH_DISTANCE) { 
				return MovementDecision.Stay;
			}
		} 

		// Head forward (neck)
		var r1 = CastRay(new Vector2(frontX, transform.position.y + VOXEL_SIZE * .5f), 
		                 dirVector);
		if (r1.collider != null) {
			wallTouchDist = Mathf.Abs (r1.point.x - frontX);
			if (wallTouchDist < TOUCH_DISTANCE) { 
				return MovementDecision.Stay;
			}
		}

		// Torso forward
		var r2 = CastRay(new Vector2(frontX, transform.position.y - VOXEL_SIZE * .5f), 
		                 dirVector);
		if (r2.collider != null) {
			wallTouchDist = Mathf.Abs (r2.point.x - frontX);
			if (wallTouchDist < TOUCH_DISTANCE) { 
				return MovementDecision.Stay;
			}
		}

		// Feet forward
		var feet = CastRay(new Vector2(frontX, transform.position.y - VOXEL_SIZE * 1.45f), 
		                   dirVector);

		// If feet collide with the wall, but torso and head did not - we can try and step up
		if (feet.collider != null) {
			wallTouchDist = Mathf.Abs (feet.point.x - frontX);
			if (wallTouchDist < TOUCH_DISTANCE) {
				stepLevel = feet.collider.bounds.max.y;
				return MovementDecision.StepOnABlock;
			}
		}
		return MovementDecision.Move;
	}

	float GetFeetY() {
		return transform.position.y - PLAYER_CENTER_TO_FEET;
	}

	//
	// Possibly falling. Check voxels under the feet
	//
	void CheckFloorAndFall() {
		float feetY = GetFeetY();
		var r1 = CastRay(new Vector2(transform.position.x - HALF_FEET_WIDTH, feetY),
		                 Vector2.down);
		var r2 = CastRay(new Vector2(transform.position.x + HALF_FEET_WIDTH, feetY),
		                 Vector2.down);

		// Shift values so that we have to deal with left collision, or both
		if (r1.collider == null) { r1 = r2; }
		this.isGrounded = (r1.collider != null);

		float hitY = r2.collider != null ? Mathf.Max (r1.point.y, r2.point.y) : r1.point.y;
		float distanceToFloor = Mathf.Abs(hitY - feetY);

		float fall = Mathf.Min (distanceToFloor, FALL_SPEED * Time.deltaTime);
		transform.position += new Vector3(0f, -fall, 0f);
	}

	//
	// Jumping (moving vertically up) so check the upper limit
	//
	void CheckCeiling() {
		float headY = transform.position.y + VOXEL_SIZE * 1.45f;
		var r1 = CastRay(new Vector2(transform.position.x - HALF_CHAR_WIDTH, headY), 
		                 Vector2.up);
		var r2 = CastRay(new Vector2(transform.position.x + HALF_CHAR_WIDTH, headY), 
		                 Vector2.up);
		
		// Shift values so that we have to deal with left collision, or both
		if (r1.collider == null) { r1 = r2; }

		float hitY = r2.collider != null ? Mathf.Min (r1.point.y, r2.point.y) : r1.point.y;
		float distanceToCeil = Mathf.Abs(hitY - headY);
		if (distanceToCeil < TOUCH_DISTANCE) {
			isJumping = false;
		} else {
			float jump = Mathf.Min (distanceToCeil, JUMP_SPEED * Time.deltaTime);
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
