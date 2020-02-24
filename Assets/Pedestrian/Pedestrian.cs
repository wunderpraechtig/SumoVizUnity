using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pedestrian : MonoBehaviour {
	
	Vector3 start;
	Vector3 target;
	float movement_time_total;
	float movement_time_elapsed;
	private float speed;
	int densityReload;
	int densityReloadInterval = 10;
    bool playingAnimation = false;

	int id;
    List<PedestrianPosition> positions = new List<PedestrianPosition>();
    int lastTimeIndex = 0;
    float timeEarliest = 0;
    float timeLatest = 0;
	Color myColor;
	bool trajectoryVisible;

    Animation componentAnimation = null;
    AnimationState animationWalking = null;
    Renderer tileRenderer;
    

    //private InfoText it;
	private PedestrianLoader pl;
	private PlaybackControl pc;
	private Renderer pedRenderer;
	private GeometryLoader gl;
	private Groundplane gp;

	GameObject tile;

    private void Awake()
    {
        pl = GameObject.Find("PedestrianLoader").GetComponent<PedestrianLoader>();
        pc = GameObject.Find("PlaybackControl").GetComponent<PlaybackControl>();
        gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();
        gp = gl.groundplane;
        pedRenderer = GetComponentInChildren<Renderer>();
        componentAnimation = GetComponent<Animation>();
        addTile();
    }

    // Use this for initialization
    void Start () {
        gameObject.AddComponent<BoxCollider>();
        transform.Rotate(0, 90, 0);
        


        //it = GameObject.Find ("InfoText").GetComponent<InfoText> ();
        if (componentAnimation != null)
        {
            myColor = new Color(Random.value, Random.value, Random.value);
            pedRenderer.materials[1].color = myColor;
            animationWalking = componentAnimation["walking"];
            componentAnimation.Stop();
        }
    }

	void OnMouseDown(){
		if (!Cursor.visible && !trajectoryVisible && !pc.drawLine && hideFlags!=HideFlags.HideInHierarchy) {
			showTrajectory();
		} else if (!Cursor.visible && trajectoryVisible && !pc.drawLine && hideFlags!=HideFlags.HideInHierarchy) {
			hideTrajectory();
		}
	}

	public void hideTrajectory() {
        GameObject myLine = GameObject.Find(name + " trajectory");
        Destroy(myLine);
		trajectoryVisible = false;
	}

    
    public void showTrajectory()
    {
        throw new System.NotImplementedException();
        /*
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < positions.Count - 1; i++)
        {
            PedestrianPosition a = (PedestrianPosition)positions.GetByIndex(i);
            points.Add(new Vector3(a.getX(), a.getZ() + 0.01f, a.getY()));
        }

        GameObject myLine = new GameObject(name + " trajectory");
        myLine.transform.position = points[1];
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        
        lr.material = new Material((Material)Resources.Load("LineMaterial", typeof(Material)));
        lr.material.SetColor("_EmissionColor", myColor);
        lr.material.SetColor("_Color", myColor);
        lr.startColor = myColor;
        lr.endColor = myColor;        
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

        pc.trajectoriesShown = true;
        trajectoryVisible = true;
        */
    }



    void addTile() {
		float side = 1.0f;
		tile = new GameObject ("tile"+id, typeof(MeshFilter), typeof(MeshRenderer));
		MeshFilter mesh_filter = tile.GetComponent<MeshFilter> ();
        tileRenderer = tile.GetComponent<Renderer>();
        tileRenderer.material = (Material) Resources.Load("Tilematerial", typeof(Material));
        tileRenderer.material.color = Color.red;
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[] {new Vector3 (-side/2, 0.01f, -side/2),new Vector3 (side/2, 0.01f, -side/2),new Vector3 (-side/2, 0.01f, side/2),new Vector3 (side/2, 0.01f, side/2)};
		mesh.triangles = new int[] {2,1,0,1,2,3};


		Vector2[] uvs = new Vector2[mesh.vertices.Length];
		int i = 0;
		while (i < uvs.Length) {
			uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
			i++;
		}
		mesh.uv = uvs;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh_filter.mesh = mesh;

		tile.transform.position = gameObject.transform.position;
		tile.transform.parent = gameObject.transform;
        tile.transform.localScale = Vector3.one;


    }
    
	void Update () {

		if (pc.playing) {
            if (!playingAnimation)
            {
                if (componentAnimation != null)
                    componentAnimation.Play();
                playingAnimation = true;
            }
		} else {
            if (playingAnimation)
            {
                if (componentAnimation != null)
                    componentAnimation.Stop();
                playingAnimation = false;
            }
		}

		int index = getIndexOfNewTime(pc.current_time);
		
		if (index<positions.Count-1 && index>-1){
			pedRenderer.enabled = true;
			PedestrianPosition pos = positions[index];
			PedestrianPosition pos2 = positions[index+1];
			start = new Vector3 (pos.getX(), pos.getZ(), pos.getY());
			target = new Vector3 (pos2.getX(), pos2.getZ(), pos2.getY());

			float movement_percentage = pc.current_time - pos.getTime();
			Vector3 newPosition = Vector3.Lerp(start,target,movement_percentage);

            // to keep pedestrians upright change in the Z Axis is excluded from rotation calculation
            Vector3 relativePos = new Vector3(pos2.getX(), 0, pos2.getY()) - new Vector3(pos.getX(), 0, pos.getY());
            speed = relativePos.magnitude;

            if (componentAnimation != null)
                animationWalking.speed = getSpeed ();
            

            if (start!=target) transform.localRotation = Quaternion.LookRotation(relativePos);

			//check if line is crossed

			if (gp.point1active && gp.point2active) {
				if (FasterLineSegmentIntersection(new Vector2(gp.point1.x,gp.point1.z), new Vector2(gp.point2.x,gp.point2.z), new Vector2(transform.position.x, transform.position.z), new Vector2(newPosition.x, newPosition.z))) {
					gp.lineCross(speed);
				}
			}

			//Tile coloring
			if (pc.tileColoringMode != TileColoringMode.TileColoringNone) {

                tileRenderer.enabled = true;

				if (pc.tileColoringMode == TileColoringMode.TileColoringSpeed) {
                    tileRenderer.material.color = ColorHelper.ColorForSpeed(getSpeed(), 1.5f);  //1.5 as average maximum walking speed when not hindered
					//it.updateSpeed(speed);
				} else if (pc.tileColoringMode == TileColoringMode.TileColoringDensity) {
					densityReload = (densityReload+1)%densityReloadInterval;
					if (densityReload==0) {
						getDensity();
					}
					float density = getDensity();
					if (density>=pc.threshold) {
                        tileRenderer.material.color = ColorHelper.ColorForDensity(density);
					} else {
                        tileRenderer.enabled = false;
					}
				}
			} else {
                tileRenderer.enabled = false;
			}

			transform.localPosition = newPosition;
			gameObject.hideFlags = HideFlags.None;

		} else {
			pedRenderer.enabled = false;
            tileRenderer.enabled = false;
			gameObject.hideFlags = HideFlags.HideInHierarchy;
		}
	}


	public float getDensity() {

		if (hideFlags==HideFlags.HideInHierarchy) return -1;
		int nearbys = 0;
		float radius = 2.0f;
		foreach (Pedestrian p in pl.pedestrians) {
			if (p!=this && Vector3.Distance(transform.localPosition,p.transform.localPosition) <radius && p.hideFlags!=HideFlags.HideInHierarchy) {
				nearbys++;
			}
		}
		float density = nearbys/(radius*radius*Mathf.PI);
		//it.updateDensity(density);
		return density;
	}

	public float getDensityF() {

		if (hideFlags==HideFlags.HideInHierarchy) return -1;
		List<float> nearbys = new List<float>();
		foreach (Pedestrian p in pl.pedestrians) {
			if (p!=this && p.hideFlags!=HideFlags.HideInHierarchy) {
				float distance = Vector3.Distance(transform.position,p.transform.position);
				if (nearbys.Count == 0) nearbys.Add(distance);
				else if (nearbys[0]>distance) {nearbys.Insert(0,distance);}
				else {nearbys.Add (distance);}
			}
		}
		float density = 8/(nearbys[7]*nearbys[7]*Mathf.PI);
		//it.updateDensity(density);

		return density;
	}

	

	public float getSpeed() {
		return speed*2;
	}

	// http://www.stefanbader.ch/faster-line-segment-intersection-for-unity3dc/
	bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {

		Vector2 a = p2 - p1;
		Vector2 b = p3 - p4;
		Vector2 c = p1 - p3;
		
		float alphaNumerator = b.y*c.x - b.x*c.y;
		float alphaDenominator = a.y*b.x - a.x*b.y;
		float betaNumerator  = a.x*c.y - a.y*c.x;
		float betaDenominator  = alphaDenominator; /*2013/07/05, fix by Deniz*/
		
		bool doIntersect = true;
		
		if (alphaDenominator == 0 || betaDenominator == 0) {
			doIntersect = false;
		} else {
			
			if (alphaDenominator > 0) {
				if (alphaNumerator < 0 || alphaNumerator > alphaDenominator) {
					doIntersect = false;
				}
			} else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator) {
				doIntersect = false;
			}
			
			if (doIntersect && betaDenominator > 0) {
				if (betaNumerator < 0 || betaNumerator > betaDenominator) {
					doIntersect = false;
				}
			} else if (betaNumerator > 0 || betaNumerator < betaDenominator) {
				doIntersect = false;
			}
		}
		
		return doIntersect;
	}

	private int getIndexOfNewTime(float time) {
        if (positions.Count < 1) return -1;
        float lastTime = positions[lastTimeIndex].getTime();
        if (time < timeEarliest) return -1;
        if (time > timeLatest) return -1;
        if (lastTime < time)
        {
            for (int i = lastTimeIndex; i < positions.Count; ++i)
            {
                if (positions[i].getTime() > time) {
                    lastTimeIndex = i - 1;
                    return i - 1;
                }
            }
        }
        else
        {
            for (int i = lastTimeIndex; i >= 0; --i)
            {
                if (positions[i].getTime() <= time) {
                    lastTimeIndex = i;
                    return i;
                }
            }
        }
        return -1;
		
		/*
		// Check to see if we need to search the list.
		if (thisList == null || thisList.Count <= 0) { return -1; }
		if (thisList.Count == 1) { return 0; }
		
		// Setup the variables needed to find the closest index
		int lower = 0;
		int upper = thisList.Count - 1;
		int index = (lower + upper) / 2;
		
		// Find the closest index (rounded down)
		bool searching = true;
		while (searching)
		{
			int comparisonResult = float.Compare(thisValue, (float) thisList.GetKey(index));
			if (comparisonResult == 0) { return index; }
			else if (comparisonResult < 0) { upper = index - 1; }
			else { lower = index + 1; }
			Debug.Log (thisValue + " : " + (float) thisList.GetKey(index));
			index = (lower + upper) / 2;
			if (lower > upper) { searching = false; }
		}

		// Check to see if we are under or over the max values.
		if (index >= thisList.Count - 1) { return thisList.Count - 1; }
		if (index < 0) { return 0; }
		
		// Check to see if we should have rounded up instead
		//if (thisList.Keys[index + 1] - thisValue < thisValue - (thisList.Keys[index])) { index++; }
		
		// Return the correct/closest string
		return index;*/
	}

	
	public void setID(int id) {
		this.id = id;
		densityReload = id % densityReloadInterval;
		this.name = "Pedestrian "+id;
        
    }

	public void setPositions(List<PedestrianPosition> p) {
        positions = p;
		PedestrianPosition pos = positions[0];
		transform.localPosition = new Vector3 (pos.getX(),pos.getZ(),pos.getY());
        timeEarliest = positions[0].getTime();
        timeLatest = positions[positions.Count - 1].getTime();
	}
}
