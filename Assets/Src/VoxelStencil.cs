using UnityEngine;

public class VoxelStencil {

	protected VoxelType fillType;

	protected int centerX, centerY, radius;

	public int XStart {
		get {
			return centerX - radius;
		}
	}
	
	public int XEnd {
		get {
			return centerX + radius;
		}
	}
	
	public int YStart {
		get {
			return centerY - radius;
		}
	}
	
	public int YEnd {
		get {
			return centerY + radius;
		}
	}

	public virtual void Initialize (VoxelType fillType, int radius) {
		this.fillType = fillType;
		this.radius = radius;
	}

	public virtual void SetCenter (int x, int y) {
		centerX = x;
		centerY = y;
	}

	public virtual VoxelType Apply (int x, int y, VoxelType voxel) {
		return fillType;
	}
}