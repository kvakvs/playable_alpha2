using UnityEngine;

public class VoxelStencilCircle : VoxelStencil {
	
	private int sqrRadius;
	
	public override void Initialize (VoxelType fillType, int radius) {
		base.Initialize (fillType, radius);
		sqrRadius = radius * radius;
	}
	
	public override VoxelType Apply (int x, int y, VoxelType voxel) {
		x -= centerX;
		y -= centerY;
		if (x * x + y * y <= sqrRadius) {
			return fillType;
		}
		return voxel;
	}
}