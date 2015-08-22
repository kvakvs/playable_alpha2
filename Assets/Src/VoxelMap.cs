using UnityEngine;

public class VoxelMap : MonoBehaviour {

	private static string[] fillTypeNames = {"Empty", "Dirt", "Stone", "Iron"};
	private static string[] radiusNames = {"0", "1", "2", "3", "4", "5"};
	private static string[] stencilNames = {"Square", "Circle"};

	public float visibleSize = 2f;

	public int voxelsPerChunk = 8;
	public int chunksVisible = 2;

	public VoxelChunk voxelGridPrefab;

	//private Voxel[] voxels;

	// Currently visible X x Y chunks
	private VoxelChunk[] chunks;
	
	private float chunkSize, voxelSize, halfSize;

	private int fillTypeIndex, radiusIndex, stencilIndex;

	private VoxelStencil[] stencils = {
		new VoxelStencil(),
		new VoxelStencilCircle()
	};
	
	private void Awake () {
		halfSize = visibleSize * 0.5f;
		chunkSize = visibleSize / chunksVisible;
		voxelSize = chunkSize / voxelsPerChunk;
		
		chunks = new VoxelChunk[chunksVisible * chunksVisible];
		for (int i = 0, y = 0; y < chunksVisible; y++) {
			for (int x = 0; x < chunksVisible; x++, i++) {
				CreateChunk(i, x, y);
			}
		}
		BoxCollider box = gameObject.AddComponent<BoxCollider>();
		box.size = new Vector3(visibleSize, visibleSize);
	}

	private void CreateChunk (int i, int x, int y) {
		VoxelChunk chunk = Instantiate(voxelGridPrefab) as VoxelChunk;
		chunk.Initialize(voxelsPerChunk, chunkSize, x * voxelsPerChunk, y * voxelsPerChunk);
		chunk.transform.parent = transform;
		chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
		chunks[i] = chunk;
		if (x > 0) {
			chunks[i - 1].xNeighbor = chunk;
		}
		if (y > 0) {
			chunks[i - chunksVisible].yNeighbor = chunk;
			if (x > 0) {
				chunks[i - chunksVisible - 1].xyNeighbor = chunk;
			}
		}
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
		int centerX = (int)((point.x + halfSize) / voxelSize);
		int centerY = (int)((point.y + halfSize) / voxelSize);

		int xStart = (centerX - radiusIndex - 1) / voxelsPerChunk;
		if (xStart < 0) {
			xStart = 0;
		}
		int xEnd = (centerX + radiusIndex) / voxelsPerChunk;
		if (xEnd >= chunksVisible) {
			xEnd = chunksVisible - 1;
		}
		int yStart = (centerY - radiusIndex - 1) / voxelsPerChunk;
		if (yStart < 0) {
			yStart = 0;
		}
		int yEnd = (centerY + radiusIndex) / voxelsPerChunk;
		if (yEnd >= chunksVisible) {
			yEnd = chunksVisible - 1;
		}

		VoxelStencil activeStencil = stencils[stencilIndex];
		activeStencil.Initialize((VoxelType)fillTypeIndex, radiusIndex);

		int voxelYOffset = yEnd * voxelsPerChunk;
		for (int y = yEnd; y >= yStart; y--) {
			int i = y * chunksVisible + xEnd;
			int voxelXOffset = xEnd * voxelsPerChunk;
			for (int x = xEnd; x >= xStart; x--, i--) {
				activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
				chunks[i].Apply(activeStencil);
				voxelXOffset -= voxelsPerChunk;
			}
			voxelYOffset -= voxelsPerChunk;
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