using UnityEngine;
using System.Collections.Generic;

[SelectionBase]
public class VoxelChunk : MonoBehaviour {

	public int chunk_size;

	// Offset where this chunk starts in the world
	private int chunk_base_x, chunk_base_y;

	public GameObject voxelPrefab;

	public VoxelChunk xNeighbor, yNeighbor, xyNeighbor;

	private Voxel[] voxels;

	private float voxelSize, gridSize;

	//private Material[] voxelMaterials;

	private Mesh mesh;

	private List<Vector3> gen_vertices;
	private List<int>     gen_triangles;

	private List<Vector2> gen_uv;
	private Vector2[] vox_uv;

	private Voxel dummyX, dummyY, dummyT;
	private Noise noise;

	public void Initialize (int chunk_sz, float size, int basex, int basey) {
		this.chunk_size = chunk_sz;
		this.chunk_base_x = basex;
		this.chunk_base_y = basey;
		gridSize = size;
		voxelSize = size / (float)chunk_sz;
		voxels = new Voxel[chunk_sz * chunk_sz];
		//voxelMaterials = new Material[voxels.Length];

		dummyX = new Voxel();
		dummyY = new Voxel();
		dummyT = new Voxel();

		//noise = new Noise(Noise.DEFAULT_SEED);
		for (int i = 0, y = 0; y < chunk_sz; y++) {
			for (int x = 0; x < chunk_sz; x++, i++) {
				voxels[i] = CreateVoxel(x, y);
				voxels[i].SetVType(RandomVType((basex + x) * voxelSize, (basey + y) * voxelSize));
				//voxels[i].SetVType((VoxelType)Random.Range (0f, 4f));
			}
		}

		mesh = new Mesh();
		mesh.name = "VoxelGrid Mesh";
		GetComponent<MeshFilter>().mesh = mesh;

		// TODO: optimize this with preallocated array
		gen_vertices  = new List<Vector3>();
		gen_uv        = new List<Vector2>();
		vox_uv        = new Vector2[4];
		gen_triangles = new List<int>();
		RebuildMesh();	
	}

	private Voxel CreateVoxel (int x, int y) {
		// Create white dots on voxel positions
		/*
		GameObject              white_dot = Instantiate(voxelPrefab) as GameObject;
		white_dot.transform.parent        = transform;
		white_dot.transform.localPosition = new Vector3(x * voxelSize, y * voxelSize, -0.01f);
		white_dot.transform.localScale    = Vector3.one * voxelSize * 0.1f;
		 */

		// Create map cell
		return new Voxel(x, y, voxelSize);
	}

	private VoxelType RandomVType(float x, float y) {
		//double vtype_norm = ((noise.eval(x, y) + 1f) / 2f);
		double vtype_norm = Mathf.PerlinNoise(x, y);
		int vtype = (int)(vtype_norm * (int)VoxelType.VoxelType_MaxValue);
		//Debug.Log ("x=" + x + " y=" + y + " vtype=" + vtype + " vtnorm=" + vtype_norm);
		return (VoxelType)vtype;
	}

	
	private void RebuildMesh () {
		gen_vertices.Clear();
		gen_uv.Clear();
		gen_triangles.Clear();
		mesh.Clear();

		TriangulateCellRows();

		mesh.vertices  = gen_vertices.ToArray();
		mesh.triangles = gen_triangles.ToArray();
		mesh.uv        = gen_uv.ToArray();
		mesh.RecalculateNormals();
	}

	private void TriangulateCellRows () {
		for (int i = 0, y = 0; y < chunk_size; y++) {
			for (int x = 0; x < chunk_size; x++, i++) {
				TriangulateCell(voxels[i]);
			}
		}
	}

	private void TriangulateCell (Voxel a) {

		if (a.vtype != VoxelType.Empty) {
			AddQuad(a);

			a.GetUV(vox_uv);
			gen_uv.Add(vox_uv[0]);
			gen_uv.Add(vox_uv[1]);
			gen_uv.Add(vox_uv[2]);
			gen_uv.Add(vox_uv[3]);
		}
	}

	private void AddQuad (Voxel vx) {
		Vector2 a = vx.tl;
		Vector2 b = new Vector2(vx.br.x, a.y);
		Vector2 c = new Vector2(a.x, vx.br.y);
		Vector2 d = vx.br;
		//Debug.Log ("a=" + a.ToString() + " d=" + d.ToString());

		int vertexIndex = gen_vertices.Count;
		gen_vertices.Add(a);
		gen_vertices.Add(b);
		gen_vertices.Add(c);
		gen_vertices.Add(d);

		gen_triangles.Add(vertexIndex);
		gen_triangles.Add(vertexIndex + 2);
		gen_triangles.Add(vertexIndex + 3);
		gen_triangles.Add(vertexIndex);
		gen_triangles.Add(vertexIndex + 3);
		gen_triangles.Add(vertexIndex + 1);
	}

	public void Apply (VoxelStencil stencil) {
		int xStart = stencil.XStart;
		if (xStart < 0) {
			xStart = 0;
		}
		int xEnd = stencil.XEnd;
		if (xEnd >= chunk_size) {
			xEnd = chunk_size - 1;
		}
		int yStart = stencil.YStart;
		if (yStart < 0) {
			yStart = 0;
		}
		int yEnd = stencil.YEnd;
		if (yEnd >= chunk_size) {
			yEnd = chunk_size - 1;
		}

		//VoxelType current_vtype = VoxelType.Dirt;
		for (int y = yStart; y <= yEnd; y++) {
			int i = y * chunk_size + xStart;
			for (int x = xStart; x <= xEnd; x++, i++) {
				voxels[i].SetVType(stencil.Apply(x, y, voxels[i].vtype));
			}
		}
		RebuildMesh();
	}
}