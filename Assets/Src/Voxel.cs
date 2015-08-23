using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum VoxelType {
	Empty, Dirt, Stone, IronOre,
	VoxelType_MaxValue
};

[Serializable]
public class Voxel {
	static uint[] sprite_col    = {0, 13,  0, 13};
	static uint[] sprite_row    = {0, 17, 16, 19};
	static uint[] num_varieties = {0,  3,  1,  3}; 

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

	public void GetUV(Vector2[] uv) {
		const int spr_cols = 16;
		const int spr_rows = 22;
		uint spr_col = sprite_col[(uint)vtype] + variety;
		uint spr_row = sprite_row[(uint)vtype];
		float u_step = 1f / (spr_cols);
		float v_step = 1f / (spr_rows);
		float u = spr_col * u_step;
		float v = spr_row * v_step;

		// c(2)...d(3)
		// ..
		// a(0)...b(1)

		uv[0].Set(u,          v);
		uv[1].Set(u + u_step, v);
		uv[2].Set(u,          v + v_step);
		uv[3].Set(u + u_step, v + v_step);
	}
}