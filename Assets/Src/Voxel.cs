using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum VoxelType {
	Empty, Dirt, Stone, IronOre,
	VoxelType_MaxValue
};

[Serializable]
public class Voxel {
	const int SPR_ROWS = 32;
	const int SPR_COLS = 32;
	const float U_STEP = 1f / SPR_COLS;
	const float V_STEP = 1f / SPR_ROWS;

	static uint[] sprite_col    = {0,  0,  0,  0};
	static uint[] sprite_row    = {0, 31, 30, 29};
	static uint[] num_varieties = {0,  3,  3,  3}; 

	private VoxelType m_vtype;
	public VoxelType vtype {
		set { 
			m_vtype = value; 
			variety = (uint)Random.Range(0f, (float)num_varieties[(int)value]);
		}
		get { return m_vtype; }
	}
	public uint      variety;

	public Vector2 position; // Bottom-left (origin)
	public Vector2 opposite; // Top-right (opposite origin)

	const float VOXEL_SIZE = VoxelMap.VOXEL_SIZE;

	public Voxel (int x, int y) {
		vtype = VoxelType.Empty;
		variety = 0;

		position.x = x * VOXEL_SIZE;
		position.y = y * VOXEL_SIZE;

		opposite = new Vector2(position.x + VOXEL_SIZE, position.y + VOXEL_SIZE);
	}

	public Voxel () {}

	public bool IsSolid() {
		return m_vtype != VoxelType.Empty;
	}

	public void GetUV(Vector2[] uv) {
		uint spr_col = sprite_col[(uint)vtype] + variety;
		uint spr_row = sprite_row[(uint)vtype];
		float u = spr_col * U_STEP;
		float v = spr_row * V_STEP;

		// c(2)...d(3)
		// ..
		// a(0)...b(1)

		uv[0].Set(u,          v);
		uv[1].Set(u + U_STEP, v);
		uv[2].Set(u,          v + V_STEP);
		uv[3].Set(u + U_STEP, v + V_STEP);
	}
}