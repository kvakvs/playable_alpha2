using UnityEngine;

public class VoxelMap : MonoBehaviour {

	private static string[] fillTypeNames = {"Empty", "Dirt", "Stone", "Iron"};
	private static string[] radiusNames = {"0", "1", "2", "3", "4", "5"};
	private static string[] stencilNames = {"Square", "Circle"};

	const int MAP_WIDTH  = 1024;
	const int MAP_HEIGHT = 512;
	public const int CHUNK_VOXELS_DIM = 16;
	const int VIS_CHUNKS_DIM   = 3;
	const int VOXELS_PER_CHUNK = CHUNK_VOXELS_DIM * CHUNK_VOXELS_DIM;

	// how many chunk blocks are there in map dimensions
	const int X_CHUNK_COUNT = MAP_WIDTH / CHUNK_VOXELS_DIM;
	const int Y_CHUNK_COUNT = MAP_HEIGHT / CHUNK_VOXELS_DIM;
	const int TOTAL_CHUNKS  = X_CHUNK_COUNT * Y_CHUNK_COUNT;

	public const float VOXEL_SIZE   = 0.075f; //CHUNK_SIZE / CHUNK_VOXELS_DIM;
	public const float CHUNK_SIZE   = CHUNK_VOXELS_DIM * VOXEL_SIZE; //VISIBLE_SIZE / VIS_CHUNKS_DIM;
	const        float VISIBLE_SIZE = CHUNK_SIZE * VIS_CHUNKS_DIM;

	public VoxelChunk voxelGridPrefab;

	// Two-dimensional array of whole world, split into chunks

	private Voxel[][] voxels;

	// Currently visible X x Y chunks
	private VoxelChunk[] visibleChunks;
	
	private int fillTypeIndex, radiusIndex, stencilIndex;
	public static VoxelMap instance;

	private VoxelStencil[] stencils = {
		new VoxelStencil(),
		new VoxelStencilCircle()
	};

	//private Vector2 m_camera_pos;
	public Vector2 cameraPos {
		get { return Camera.main.transform.position; }
		set { 
			Camera.main.transform.position = value; 
			OnCameraPosChanged(); 
		}
	}

	public void OnCameraPosChanged() {
		//Camera.main.transform.position = new Vector3(pos.x, pos.y, -10); 
		//Camera.main.transform.LookAt(new Vector3(pos.x, pos.y, 0)); 
		ShowVisibleChunks();
	}

	static int ClampLower( int value, int min ) {
		return (value < min) ? min : value;
	}
	static int ClampUpper( int value, int max ) {
		return (value > max) ? max : value;
	}
	static int Clamp( int value, int min, int max ) {
		return (value > max) ? max : ((value < min) ? min : value);
	}

	// Around camera center, create map chunks with mesh
	private void ShowVisibleChunks() {
		int xbegin = Clamp ((int)(cameraPos.x / CHUNK_SIZE - VIS_CHUNKS_DIM/2), 
		                    0, X_CHUNK_COUNT - 1);
		int xend = Clamp (xbegin + VIS_CHUNKS_DIM, 
		                  0, X_CHUNK_COUNT - 1);

		int ybegin = Clamp ((int)(cameraPos.y / CHUNK_SIZE - VIS_CHUNKS_DIM/2), 
		                    0, Y_CHUNK_COUNT - 1);
		int yend = Clamp (ybegin + VIS_CHUNKS_DIM, 
		                  0, Y_CHUNK_COUNT - 1);
		
		//Debug.Log ("create vis x=" + xbegin.ToString() + ".." + xend.ToString()
		//           + "; y=" + ybegin.ToString() + ".." + yend.ToString());
		int i = 0;
		for (int y = ybegin; y < yend; y++) {
			for (int x = xbegin; x < xend; x++) {
				var c = visibleChunks[i];
				c.transform.position = new Vector3(x * CHUNK_SIZE, y * CHUNK_SIZE);
				c.UseVoxels(voxels[y * X_CHUNK_COUNT + x]);
				c.enabled = true;
				i++;
			}
		}
		//for (; i < VIS_CHUNKS_DIM * VIS_CHUNKS_DIM; i++) {
		//	visibleChunks[i].enabled = false;
		//}
	}
	
	private void Awake () {
		VoxelMap.instance = this;

		this.voxels = new Voxel[TOTAL_CHUNKS][];
		for (int i = 0, y = 0; y < Y_CHUNK_COUNT; y++) {
			for (int x = 0; x < X_CHUNK_COUNT; x++, i++) {
				voxels[i] = new Voxel[VOXELS_PER_CHUNK];
				InitChunkTerrain(voxels[i], x * CHUNK_VOXELS_DIM, y * CHUNK_VOXELS_DIM);
			}
		}

		visibleChunks = new VoxelChunk[VIS_CHUNKS_DIM * VIS_CHUNKS_DIM];
		for (int i = 0; i < VIS_CHUNKS_DIM * VIS_CHUNKS_DIM; i++) {
			CreateChunk(i);
		}
		cameraPos = new Vector2(0f, 0f);

		BoxCollider box = gameObject.AddComponent<BoxCollider>();
		box.size = new Vector3(VISIBLE_SIZE, VISIBLE_SIZE);
	}

	// Fills a square of voxels 
	private void InitChunkTerrain(Voxel[] vv, int basex, int basey) {
		float x_offs1 = basex * VOXEL_SIZE;
		float y_offs1 = basey * VOXEL_SIZE;
		float x_offs2 = x_offs1 + MAP_WIDTH * VOXEL_SIZE;
		float y_offs2 = y_offs1 + MAP_HEIGHT * VOXEL_SIZE;
		// more density for ore
		float x_offs_ore = x_offs1 * 0.6f + 2 * MAP_WIDTH * VOXEL_SIZE;
		float y_offs_ore = y_offs1 * 0.6f + 2 * MAP_HEIGHT * VOXEL_SIZE;

		// Init stone wavy pattern, place void pattern over it
		for (int i = 0, y = 0; y < CHUNK_VOXELS_DIM; y++) {
			for (int x = 0; x < CHUNK_VOXELS_DIM; x++, i++) {
				vv[i] = new Voxel(x, y);
				var vt = GenerateRandomStoneDirt(x_offs1 + vv[i].position.x, y_offs1 + vv[i].position.y);
				vt = GenerateRandomOre(vt, x_offs_ore + vv[i].position.x, y_offs_ore + vv[i].position.y);
				if (vt != VoxelType.Empty) {
					vt = GenerateRandomVoid(vt, x_offs2 + vv[i].position.x, y_offs2 + vv[i].position.y);
				}
				vv[i].vtype = vt;
			}
		}
	}

	private VoxelType GenerateRandomStoneDirt(float x, float y) {
		double rnd = Mathf.Min (Mathf.PerlinNoise(x, y), 1f);
		if (rnd < 0.7f) { return VoxelType.Dirt; }
		return VoxelType.Stone;
	}
	private VoxelType GenerateRandomOre(VoxelType vt, float x, float y) {
		double rnd = Mathf.Min (Mathf.PerlinNoise(x, y), 1f);
		if (rnd < 0.15f) { return VoxelType.IronOre; }
		return vt; // else do not change
	}
	private VoxelType GenerateRandomVoid(VoxelType vt, float x, float y) {
		double rnd = Mathf.Min (Mathf.PerlinNoise(x, y), 1f);
		if (rnd < 0.25f) { return VoxelType.Empty; }
		return vt; // else do not change
	}

	private void CreateChunk (int i) {
		VoxelChunk chunk = Instantiate(voxelGridPrefab) as VoxelChunk;
		chunk.Initialize();
		chunk.transform.parent = transform;
		visibleChunks[i] = chunk;
	}

	private void Update () {
		if (Input.GetMouseButton(0)) {
			RaycastHit hitInfo;
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
				if (hitInfo.collider.gameObject == gameObject) {
					EditVoxels(transform.InverseTransformPoint(hitInfo.point));
				}
			}
		}
	}

	private void EditVoxels (Vector3 point) {
		int centerX = (int)(point.x / VOXEL_SIZE);
		int centerY = (int)(point.y / VOXEL_SIZE);

		int xStart = (centerX - radiusIndex - 1) / CHUNK_VOXELS_DIM;
		if (xStart < 0) {
			xStart = 0;
		}
		int xEnd = (centerX + radiusIndex) / CHUNK_VOXELS_DIM;
		if (xEnd >= VIS_CHUNKS_DIM) {
			xEnd = VIS_CHUNKS_DIM - 1;
		}
		int yStart = (centerY - radiusIndex - 1) / CHUNK_VOXELS_DIM;
		if (yStart < 0) {
			yStart = 0;
		}
		int yEnd = (centerY + radiusIndex) / CHUNK_VOXELS_DIM;
		if (yEnd >= VIS_CHUNKS_DIM) {
			yEnd = VIS_CHUNKS_DIM - 1;
		}

		VoxelStencil activeStencil = stencils[stencilIndex];
		activeStencil.Initialize((VoxelType)fillTypeIndex, radiusIndex);

		int voxelYOffset = yEnd * CHUNK_VOXELS_DIM;
		for (int y = yEnd; y >= yStart; y--) {
			int i = y * VIS_CHUNKS_DIM + xEnd;
			int voxelXOffset = xEnd * CHUNK_VOXELS_DIM;
			for (int x = xEnd; x >= xStart; x--, i--) {
				activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
				visibleChunks[i].Apply(activeStencil);
				voxelXOffset -= CHUNK_VOXELS_DIM;
			}
			voxelYOffset -= CHUNK_VOXELS_DIM;
		}
	}

	private void OnGUI () {
		GUILayout.BeginArea(new Rect(4f, 4f, 200f, 500f));
		GUILayout.Label("Fill Type");
		fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames, 4);
		GUILayout.Label("Radius");
		radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
		GUILayout.Label("Stencil");
		stencilIndex = GUILayout.SelectionGrid(stencilIndex, stencilNames, 2);
		GUILayout.EndArea();
	}
}