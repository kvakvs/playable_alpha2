using UnityEngine;

public class VoxelMap : MonoBehaviour {

	private static string[] fillTypeNames = {"Empty", "Dirt", "Stone", "Iron"};
	private static string[] radiusNames = {"0", "1", "2", "3", "4", "5"};
	private static string[] stencilNames = {"Square", "Circle"};

	const float VISIBLE_SIZE = 1f;

	const int MAP_WIDTH = 1024;
	const int MAP_HEIGHT = 512;
	const int CHUNK_VOXELS_DIM = 8;
	const int VIS_CHUNKS_DIM = 3;
	const int VOXELS_PER_CHUNK = CHUNK_VOXELS_DIM * CHUNK_VOXELS_DIM;
	// how many chunk blocks are there in map dimensions
	const int X_CHUNK_COUNT = MAP_WIDTH / VOXELS_PER_CHUNK;
	const int Y_CHUNK_COUNT = MAP_HEIGHT / VOXELS_PER_CHUNK;
	const int TOTAL_CHUNKS = X_CHUNK_COUNT * Y_CHUNK_COUNT;
	//const float HALF_SIZE = VISIBLE_SIZE * 0.5f;
	const float CHUNK_SIZE = VISIBLE_SIZE / VIS_CHUNKS_DIM;
	const float VOXEL_SIZE = CHUNK_SIZE / CHUNK_VOXELS_DIM;

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

	private Vector2 m_camera_pos;
	public Vector2 cameraPos {
		get { return m_camera_pos; }
		set { m_camera_pos = value; OnCameraPosChanged(value); }
	}

	private void OnCameraPosChanged(Vector2 pos) {
		//Camera.main.transform.position = new Vector3(pos.x, pos.y, -10); 
		//Camera.main.transform.LookAt(new Vector3(pos.x, pos.y, 0)); 
		CreateVisibleChunks();
	}

	static int ClampLower( int value, int min )
	{
		return (value < min) ? min : value;
	}

	static int ClampUpper( int value, int max )
	{
		return (value > max) ? max : value;
	}

	// Around camera center, create map chunks with mesh
	private void CreateVisibleChunks() {
		int xbegin = ClampLower ((int)(cameraPos.x / CHUNK_SIZE), 0);
		int xend = ClampUpper (xbegin + VIS_CHUNKS_DIM, X_CHUNK_COUNT - 1);

		int ybegin = ClampLower ((int)(cameraPos.y / CHUNK_SIZE), 0);
		int yend = ClampUpper (ybegin + VIS_CHUNKS_DIM, Y_CHUNK_COUNT - 1);
		
		Debug.Log ("create vis x=" + xbegin.ToString() + ".." + xend.ToString()
		           + "; y=" + ybegin.ToString() + ".." + yend.ToString());
		for (int i = 0, y = ybegin; y < yend; y++) {
			for (int x = xbegin; x < xend; x++, i++) {
				var c = visibleChunks[i];
				c.transform.localPosition = new Vector3(x * CHUNK_SIZE, y * CHUNK_SIZE);
				c.UseVoxels(voxels[y * X_CHUNK_COUNT + x], x, y);
			}
		}
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
		for (int i = 0, y = 0; y < VIS_CHUNKS_DIM; y++) {
			for (int x = 0; x < VIS_CHUNKS_DIM; x++, i++) {
				CreateChunk(i, x, y);
			}
		}
		cameraPos = new Vector2(0f, 0f);

		BoxCollider box = gameObject.AddComponent<BoxCollider>();
		box.size = new Vector3(VISIBLE_SIZE, VISIBLE_SIZE);
	}

	// Fills a square of voxels 
	private void InitChunkTerrain(Voxel[] vv, int basex, int basey) {
		//Noise noise;
		//noise = new Noise(Noise.DEFAULT_SEED);
		for (int i = 0, y = 0; y < CHUNK_VOXELS_DIM; y++) {
			for (int x = 0; x < CHUNK_VOXELS_DIM; x++, i++) {
				vv[i] = new Voxel(basex + x, basey + y, VOXEL_SIZE);
				vv[i].SetVType(GenerateRandomVType(vv[i].tl.x, vv[i].tl.y));
				//vv[i].SetVType((VoxelType)Random.Range (0f, 4f));
			}
		}
	}

	private VoxelType GenerateRandomVType(float x, float y) {
		//double vtype_norm = ((noise.eval(x, y) + 1f) / 2f);
		double vtype_norm = Mathf.PerlinNoise(x, y);
		int vtype = (int)(vtype_norm * (int)VoxelType.VoxelType_MaxValue);
		//Debug.Log ("x=" + x + " y=" + y + " vtype=" + vtype + " vtnorm=" + vtype_norm);
		return (VoxelType)vtype;
	}

	private void CreateChunk (int vis_chunk_index, int x, int y) {
		VoxelChunk chunk = Instantiate(voxelGridPrefab) as VoxelChunk;
		chunk.Initialize(CHUNK_VOXELS_DIM, CHUNK_SIZE); 
		                 //x * chunkVoxelsDimension, y * chunkVoxelsDimension);
		chunk.transform.parent = transform;
		//chunk.transform.localPosition = new Vector3(x * CHUNK_SIZE - HALF_SIZE, y * CHUNK_SIZE - HALF_SIZE);
		visibleChunks[vis_chunk_index] = chunk;
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