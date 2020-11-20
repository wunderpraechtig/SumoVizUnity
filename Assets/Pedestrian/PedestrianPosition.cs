using System;

public class PedestrianPosition
{
	private int id;
	private float time;
	private float x;
	private float y;
    private float z;
    private float zOffset = 0.3f;
	public PedestrianPosition (int id, float time, float x, float y, float z) {
		this.id = id;
		this.time = time;
		this.x = x;
		this.y = y;
        this.z = z - zOffset;
	}

	public int getID() {return this.id;}
	public float getTime() {return this.time;}
	public float getX() {return this.x;}
	public float getY() {return this.y;}
    public float getZ() {return this.z;}
}

