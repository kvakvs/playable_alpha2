using UnityEngine;
using System;

public enum VoxelType {
	Empty, Dirt, Stone, IronOre
};

[Serializable]
public class Voxel {
	static int[] sprite_col = {0, 14, 0,  14};
	static int[] sprite_row = {0, 17, 16, 19};

	public VoxelType vtype;

	public Vector2 position, xEdgePosition, yEdgePosition;

	public Voxel (int x, int y, float size) {
		vtype = VoxelType.Empty;

		position.x = (x + 0.5f) * size;
		position.y = (y + 0.5f) * size;

		xEdgePosition = position;
		xEdgePosition.x += size * 0.5f;
		yEdgePosition = position;
		yEdgePosition.y += size * 0.5f;
	}

	public Voxel () {}

	public Color GetColor() {
		// optimizeme
		switch (vtype) {
		case VoxelType.Empty: return Color.black;
		case VoxelType.Dirt: return Color.Lerp (Color.yellow, Color.black, 0.5f); // optimizeme
		case VoxelType.Stone: return Color.gray;
		}
		return Color.blue;
	}

	public void GetUV(Vector2[] uv) {
		const int spr_cols = 16;
		const int spr_rows = 22;
		int spr_col = sprite_col[(int)vtype];
		int spr_row = sprite_row[(int)vtype];
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

	public void BecomeXDummyOf (Voxel other, float offset) {
		vtype = other.vtype;
		position = other.position;
		xEdgePosition = other.xEdgePosition;
		yEdgePosition = other.yEdgePosition;
		position.x += offset;
		xEdgePosition.x += offset;
		yEdgePosition.x += offset;
	}

	public void BecomeYDummyOf (Voxel other, float offset) {
		vtype = other.vtype;
		position = other.position;
		xEdgePosition = other.xEdgePosition;
		yEdgePosition = other.yEdgePosition;
		position.y += offset;
		xEdgePosition.y += offset;
		yEdgePosition.y += offset;
	}

	public void BecomeXYDummyOf (Voxel other, float offset) {
		vtype = other.vtype;
		position = other.position;
		xEdgePosition = other.xEdgePosition;
		yEdgePosition = other.yEdgePosition;
		position.x += offset;
		position.y += offset;
		xEdgePosition.x += offset;
		xEdgePosition.y += offset;
		yEdgePosition.x += offset;
		yEdgePosition.y += offset;
	}
}