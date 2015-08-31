using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using Assert = UnityEngine.Assertions.Assert;

public class PlayerCtl : MonoBehaviour 
{
	const float VOXEL_SIZE = Terrain.VOXEL_SIZE;
	//const float MAX_SPEED = VOXEL_SIZE * 10f; // 10 vox per sec
	const float WALK_SPEED = VOXEL_SIZE * 12f; // speed (per second)
	const float FALL_SPEED = VOXEL_SIZE * 20f; // fall speed (per second)
	const float JUMP_SPEED = VOXEL_SIZE * 20f; // jump speed (per second)
	const float JUMP_DURATION = 0.2f;
	const float TOUCH_DISTANCE = 0.05f * VOXEL_SIZE; // how near can come to the wall
	const float HALF_CHAR_WIDTH = VOXEL_SIZE * 0.75f;

	const float STEP_UP_SPEED = VOXEL_SIZE * 20f;
	const float PLAYER_CENTER_TO_FEET = VOXEL_SIZE * 1.5f;

	//float jumpForce; 
	public bool isGrounded; // feet on the ground
	float jumpStartTime = 0f;
	float stepLevel; // when step on a block detected, here will be new level Y

	Animator anim;
	bool facingRight = true;
	bool isJumping = false;
	//public GameObject pfLoot; // prefab to clone for loot drop

	// Player can not move through the solid layer
	private const int COLLISION_MASK = TerrainChunk.LAYERMASK_SOLID;

	const int LAYERID_PLAYER = 8;
	public const int LAYERMASK_PLAYER = 1 << LAYERID_PLAYER;

	//
	// Stuff player owns
	//
	public Inventory inventory;
	public GuiInventory guiInventory;

	void Start () {
		anim = GetComponent<Animator>();

		inventory = new Inventory ();
		inventory.AddNoStack (Item.Create (Item.PresetId.OldStonePick));

		Assert.IsNotNull (guiInventory);
		guiInventory.UpdateGuiElements (inventory);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			isJumping = true;
			isGrounded = false;
			jumpStartTime = Time.time;
		}

		// BUG BUG: When player is out of screen, colliders are deleted and player falls

		if (Input.GetMouseButton /*Down*/(0)) {
			//RaycastHit hitInfo;
			Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Dig(p);
			anim.SetTrigger("Dig");
		}

		// Movement handling
		float moveX = Input.GetAxis("Horizontal");
		Vector3 pos = CheckWallsAndSteps(transform.position, moveX);

		if (isJumping && Time.time - jumpStartTime >= JUMP_DURATION) {
			isJumping = false;
		}

		if (isJumping) {
			pos = CheckCeiling(pos);
		} else {
			// if we are not jumping, we may be falling
			pos = CheckFloorAndFall(pos);
		}

		transform.position = pos;
	}

	// Find targeted voxel, check its toughness points, remove voxel and create loot object
	void Dig(Vector3 p) {
		Voxel vox = null;
		TerrainChunk chunk = null;
		Terrain.instance.FindVoxel(p, ref vox, ref chunk);

		if (vox.IsSolid() && chunk != null) {
			/*
			GameObject loot = Instantiate (pfLoot);
			loot.transform.parent = chunk.transform;
			loot.transform.localPosition = new Vector3(vox.position.x, vox.position.y, 0f)
										+ new Vector3(VOXEL_SIZE * .5f, VOXEL_SIZE * .5f, 0f);
			loot.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 90f));
			*/
			var itemPresetId = vox.GetBlockItemPresetId ();
			if (itemPresetId != Item.PresetId.Void) {
				inventory.AddAndStack (Item.Create (itemPresetId));
				guiInventory.UpdateGuiElements (inventory);
			}
			Terrain.instance.EditVoxels(p);
		}
	}

	// Want to move left or right. Check walls, possibly 1-voxel steps
	Vector3 CheckWallsAndSteps (Vector3 pos, float moveX)
	{
		anim.SetFloat("Speed", Mathf.Abs(moveX));
		if (Mathf.Abs(moveX) < 0.1f) {
			return pos;
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

		return TryWalk (pos, moveX, true);
	}
	
	float GetY_OverHead(Vector3 pos)  { return pos.y + VOXEL_SIZE * 1.3f; }
	float GetY_Neck(Vector3 pos)      { return pos.y + VOXEL_SIZE * .4f; }
	float GetY_Chest(Vector3 pos)     { return pos.y - VOXEL_SIZE * .5f; }
	float GetY_Feet(Vector3 pos)      { return pos.y - VOXEL_SIZE * 1.5f; }
	//float GetY_UnderFeet() { return transform.position.y - PLAYER_CENTER_TO_FEET; } // V*1.5f

	// Try to move in the direction moveX, if stepUp is true will also try to step on a stone
	Vector3 TryWalk(Vector3 pos, float moveX, bool stepUp) {
		float[] wallDistance = new float[4];
		GetDistanceToWall(pos, moveX, wallDistance);

		if (wallDistance[0] > TOUCH_DISTANCE &&
		    wallDistance[1] > TOUCH_DISTANCE &&
		    wallDistance[2] > TOUCH_DISTANCE) 
		{
			float shortestDist = Mathf.Min(Mathf.Min(Mathf.Min(
				wallDistance[0], wallDistance[1]), wallDistance[2]), wallDistance[3]);

			// So we can walk, but there may be step or not
			if (wallDistance[3] > TOUCH_DISTANCE) {
				// We can walk freely, all probes show free space
				float dirX = Mathf.Min (shortestDist * .95f, 
				                        WALK_SPEED * Time.deltaTime) * Mathf.Sign(moveX);
				pos += new Vector3(dirX, 0f, 0f);
				return pos;
			}

			// Body can walk forward but feet are touching the stepstone
			// Check over head if we can step up
			if (stepUp && GetDistanceToCeiling() >= VOXEL_SIZE && isGrounded) {
				// Shift up and try if we fit
				//float stairHeight = VOXEL_SIZE;

				// Using Mathf.Floor to calculate floor!
				var pos1 = pos;
				pos1.y = pos1.y + VOXEL_SIZE;
				return TryWalk(pos1, moveX, false);
			}
		}
		return pos;
	}

	// Returns distances for 4 probes:
	// probe over head level (bodyDistances[0]), neck level (bodyDistance[1])
	// chest level probe (bodyDistance[2]) and feet level (bodyDistance[3])
	void GetDistanceToWall(Vector3 pos, float moveX, float[] bodyDistances) {
		Vector2 dirVector = moveX > 0 ? Vector2.right : Vector2.left;
		float halfCharacter = dirVector.x * HALF_CHAR_WIDTH;
		float frontX = pos.x + halfCharacter;

		// Head forward (top)
		var r0 = CastRay(new Vector2(frontX, GetY_OverHead(pos)), dirVector);
		bodyDistances[0] = (r0.collider != null) 
							? Mathf.Abs (r0.point.x - frontX) : float.MaxValue;

		// Head forward (neck)
		var r1 = CastRay(new Vector2(frontX, GetY_Neck(pos)), dirVector);
		bodyDistances[1] = (r1.collider != null) 
							? Mathf.Abs (r1.point.x - frontX) : float.MaxValue;
		
		// Torso forward
		var r2 = CastRay(new Vector2(frontX, GetY_Chest(pos)), dirVector);
		bodyDistances[2] = (r2.collider != null)
							? Mathf.Abs (r2.point.x - frontX) : float.MaxValue;
		
		// Feet forward
		var r3 = CastRay(new Vector2(frontX, GetY_Feet(pos)), dirVector);
		bodyDistances[3] = (r3.collider != null)
							? Mathf.Abs (r3.point.x - frontX) : float.MaxValue;
	}

	//
	// Possibly falling. Check voxels under the feet
	//
	Vector3 CheckFloorAndFall(Vector3 pos) {
		float distanceToFloor = GetDistanceToFloor(pos);
		this.isGrounded = distanceToFloor < TOUCH_DISTANCE;
		float fall = Mathf.Min (distanceToFloor, FALL_SPEED * Time.deltaTime);
		pos += new Vector3(0f, -fall, 0f);
		return pos;
	}
	
	float GetDistanceToFloor(Vector3 pos) {
		float feetY = GetY_Feet(pos) - 0.05f * VOXEL_SIZE; // slightly under feet
		var r1 = CastRay(new Vector2(pos.x - HALF_CHAR_WIDTH, feetY), Vector2.down);
		var r2 = CastRay(new Vector2(pos.x + HALF_CHAR_WIDTH, feetY), Vector2.down);

		// Shift values so that we have to deal with left collision, or both
		if (r1.collider == null) { r1 = r2; }

		float hitY = r2.collider != null ? Mathf.Max (r1.point.y, r2.point.y) : r1.point.y;
		return Mathf.Abs(hitY - feetY);
	}

	//
	// Jumping (moving vertically up) so check the upper limit
	//
	Vector3 CheckCeiling(Vector3 pos) {
		float distanceToCeil = GetDistanceToCeiling();
		if (distanceToCeil < TOUCH_DISTANCE) {
			isJumping = false;
		} else {
			float jump = Mathf.Min (distanceToCeil, JUMP_SPEED * Time.deltaTime);
			pos += new Vector3(0f, jump, 0f);
		}
		return pos;
	}

	float GetDistanceToCeiling() {
		float headY = transform.position.y + VOXEL_SIZE * 1.45f;
		var r1 = CastRay(new Vector2(transform.position.x - HALF_CHAR_WIDTH, headY), 
		                 Vector2.up);
		var r2 = CastRay(new Vector2(transform.position.x + HALF_CHAR_WIDTH, headY), 
		                 Vector2.up);
		
		// Shift values so that we have to deal with left collision, or both
		if (r1.collider == null) { r1 = r2; }
		
		float hitY = r2.collider != null ? Mathf.Min (r1.point.y, r2.point.y) : r1.point.y;
		return Mathf.Abs(hitY - headY);
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
