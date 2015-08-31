using UnityEngine;

public class Terrain : MonoBehaviour {

	const int MAP_NUM_COLS  = 1024;
	const int MAP_NUM_ROWS  = 512;
	public const float MAP_WIDTH = MAP_NUM_COLS * VOXEL_SIZE - CHUNK_SIZE;
	public const float MAP_HEIGHT = MAP_NUM_ROWS * VOXEL_SIZE - CHUNK_SIZE;

	public const int CHUNK_VOXELS_DIM = 16;
	const int VIS_CHUNKS_DIM  = 3;
	const int VIS_CHUNKS = VIS_CHUNKS_DIM * VIS_CHUNKS_DIM;
	const int VOXELS_PER_CHUNK = CHUNK_VOXELS_DIM * CHUNK_VOXELS_DIM;

	// how many chunk blocks are there in map dimensions
	const int X_CHUNK_COUNT = MAP_NUM_COLS / CHUNK_VOXELS_DIM;
	const int Y_CHUNK_COUNT = MAP_NUM_ROWS / CHUNK_VOXELS_DIM;
	const int TOTAL_CHUNKS  = X_CHUNK_COUNT * Y_CHUNK_COUNT;

	public const float VOXEL_SIZE   = 0.6f; // x3 = 1.8 human height
	public const float CHUNK_SIZE   = CHUNK_VOXELS_DIM * VOXEL_SIZE;
	const        float VISIBLE_SIZE = CHUNK_SIZE * VIS_CHUNKS_DIM;

	public TerrainChunk voxelGridPrefab;

	// Two-dimensional array of whole world, split into chunks

	private Voxel[][] voxels;

	// Currently visible X x Y chunks
	private TerrainChunk[] visibleChunks;
	
	//private int fillTypeIndex, radiusIndex, stencilIndex;
	public static Terrain instance;

	//private VoxelStencil[] stencils = {
	//	new VoxelStencil(),
	//	new VoxelStencilCircle()
	//};
	
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
				// Find in the remaining chunks if any of them uses voxels we plan to use
				var vox = voxels[y * X_CHUNK_COUNT + x];
				int j = VIS_CHUNKS - 1;
				while (j > i) {
					// If found one chunk who uses this mesh, just swap it into this position
					if (visibleChunks[j].IsUsingVoxels(vox)) {
						if (i != j) { Swap(ref visibleChunks[i], ref visibleChunks[j]); }
						break;
					}
					j--;
				}
				var c = visibleChunks[j];
				c.transform.position = new Vector3(x * CHUNK_SIZE, y * CHUNK_SIZE);
				c.UseVoxels(vox, x, y);
				c.enabled = true;
				i++;
			}
		}
		for (; i < VIS_CHUNKS; i++) {
			visibleChunks[i].enabled = false;
		}
	}

	public static void Swap<T> (ref T lhs, ref T rhs) {
		T temp = lhs;
		lhs = rhs;
		rhs = temp;
	}
	
	private void Awake () {
		Terrain.instance = this;

		this.voxels = new Voxel[TOTAL_CHUNKS][];
		for (int i = 0, y = 0; y < Y_CHUNK_COUNT; y++) {
			for (int x = 0; x < X_CHUNK_COUNT; x++, i++) {
				voxels[i] = new Voxel[VOXELS_PER_CHUNK];
				InitChunkTerrain(voxels[i], x * CHUNK_VOXELS_DIM, y * CHUNK_VOXELS_DIM);
			}
		}

		visibleChunks = new TerrainChunk[VIS_CHUNKS];
		for (int i = 0; i < VIS_CHUNKS; i++) {
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
		float x_offs2 = x_offs1 + MAP_NUM_COLS * VOXEL_SIZE;
		float y_offs2 = y_offs1 + MAP_NUM_ROWS * VOXEL_SIZE;
		// more density for ore
		float x_offs_ore = x_offs1 * 0.6f + 2 * MAP_NUM_COLS * VOXEL_SIZE;
		float y_offs_ore = y_offs1 * 0.6f + 2 * MAP_NUM_ROWS * VOXEL_SIZE;

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

	const float PERLIN_DIRT_SCALE = 1 / (10f * VOXEL_SIZE);
	const float PERLIN_ORE_SCALE = 1 / (6f * VOXEL_SIZE);
	const float PERLIN_VOID_SCALE = 1 / (10f * VOXEL_SIZE);

	private VoxelType GenerateRandomStoneDirt(float x, float y) {
		double rnd = Mathf.Min (Mathf.PerlinNoise(x * PERLIN_DIRT_SCALE, y * PERLIN_DIRT_SCALE), 1f);
		if (rnd < 0.7f) { return VoxelType.Dirt; }
		return VoxelType.Stone;
	}
	private VoxelType GenerateRandomOre(VoxelType vt, float x, float y) {
		double rnd = Mathf.Min (Mathf.PerlinNoise(x * PERLIN_ORE_SCALE, y * PERLIN_ORE_SCALE), 1f);
		if (rnd < 0.15f) { return VoxelType.IronOre; }
		return vt; // else do not change
	}
	private VoxelType GenerateRandomVoid(VoxelType vt, float x, float y) {
		double rnd = Mathf.Min (Mathf.PerlinNoise(x * PERLIN_VOID_SCALE, y * PERLIN_VOID_SCALE), 1f);
		if (rnd < 0.25f) { return VoxelType.Empty; }
		return vt; // else do not change
	}

	private void CreateChunk (int i) {
		TerrainChunk chunk = Instantiate(voxelGridPrefab) as TerrainChunk;
		chunk.Initialize();
		chunk.transform.parent = transform;
		visibleChunks[i] = chunk;
	}

/*	private void Update () {
		if (Input.GetMouseButtonDown(0)) {
			//RaycastHit hitInfo;
			Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			EditVoxels(p);
		}
	}*/

	public void FindVoxel(Vector3 point, ref Voxel vox, ref TerrainChunk tc) {
		int clickX = (int)(point.x / VOXEL_SIZE);
		int clickY = (int)(point.y / VOXEL_SIZE);
		
		int chunkX = clickX / CHUNK_VOXELS_DIM;
		int chunkY = clickY / CHUNK_VOXELS_DIM;

		int chunkIndex = chunkY * X_CHUNK_COUNT + chunkX;
		int voxelIndex = (clickY % CHUNK_VOXELS_DIM) * CHUNK_VOXELS_DIM + (clickX % CHUNK_VOXELS_DIM);
		vox = voxels[chunkIndex][voxelIndex];
		for (int i = 0; i < VIS_CHUNKS; i++) {
			if (visibleChunks[i].IsUsingVoxels(voxels[chunkIndex])) {
				tc = visibleChunks[i];
			}
		}
	}

	public void EditVoxels (Vector3 point) {
		int clickX = (int)(point.x / VOXEL_SIZE);
		int clickY = (int)(point.y / VOXEL_SIZE);

		int chunkX = clickX / CHUNK_VOXELS_DIM;
		int chunkY = clickY / CHUNK_VOXELS_DIM;

		int voxIndex = chunkY * X_CHUNK_COUNT + chunkX;
		if (voxIndex < 0 || voxIndex >= TOTAL_CHUNKS) {
			return; 
		}
		var vox = voxels[voxIndex];
		for (int i = 0; i < VIS_CHUNKS; i++) {
			if (visibleChunks[i].IsUsingVoxels(vox)) {
				visibleChunks[i].Edit(clickX % CHUNK_VOXELS_DIM, clickY % CHUNK_VOXELS_DIM);
				return;
			}
		}

	}

	private void OnGUI () {
	}
}