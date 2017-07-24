using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;

public class FileLoader : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {

        GeometryLoader gl = GameObject.Find("GeometryLoader").GetComponent<GeometryLoader>();
        gl.setTheme(new LabThemingMode());

        var GeometryXML = EditorUtility.OpenFilePanel(
            "Load Geometry", "", "xml");

        var TrajectoryXML = EditorUtility.OpenFilePanel(
            "Load Trajectory", "", "xml");

        loadPedestrianFile(TrajectoryXML);
        loadGeometryFile(GeometryXML);
    }

    // Update is called once per frame
    void Update() { }

    void loadGeometryFile(string filename)
    {

        if (!System.IO.File.Exists(filename))
        {
            Debug.Log("Error: File " + filename + " not found.");
            return;
        }

        XmlDocument xmlDocGeo = new XmlDocument();
        xmlDocGeo.LoadXml(System.IO.File.ReadAllText(filename));

        int wallNr = 1;     //includes "Walls" and "Obsacles"
        int stairNr = 1;    //includes "Stairs" and "Escalators"
        int floorNr = 1;    //floors are generated for each subroom at this state

        XmlNode geometry = xmlDocGeo.SelectSingleNode("//geometry");
        foreach (XmlElement rooms in geometry.SelectNodes("//rooms"))
        {
            foreach (XmlElement room in rooms.SelectNodes("room"))
            {
                foreach (XmlElement subroom in room.SelectNodes("subroom[@class='subroom'" +
                													" or @class = 'Office' " +
                													" or @class='Corridor']"))
                {   
                    float height = 0.5f;
                    float zOffset = TryParseWithDefault.ToSingle(subroom.GetAttribute("C_z"), 0);       //EXCEPTION: sometimes "C_z" is referred to as "C"

                    if (zOffset == 0)
                    {
                        zOffset = TryParseWithDefault.ToSingle(subroom.GetAttribute("C"), 0);
                    }

                    foreach (XmlElement openWall in subroom.SelectNodes("polygon[@caption='wall']"))
                    {
                        WallExtrudeGeometry.create(openWall.GetAttribute("caption") + (" ") + wallNr, parsePoints(openWall), height, 0.2f, zOffset);
                        
                        wallNr++;
                    }
					foreach (XmlElement obstacle in subroom.SelectNodes("obstacle")) 
					{
						foreach (XmlElement Wall in obstacle.SelectNodes("polygon")) 
						{
							ObstacleExtrudeGeometry.create(Wall.GetAttribute ("caption") + (" ") + wallNr, parsePoints (Wall), height, zOffset);
							wallNr++;
						}
					}
				
//
//					foreach (XmlElement area in subroom.SelectNodes("polygon[@caption='origin'" +						                                                "or @caption='destination' " +
//                                                                    "or @caption='scaledArea' " +
//                                                                    "or @caption='waitingZone' " +
//                                                                    "or @caption='beamExit' " +
//                                                                    "or @caption='eofWall' " +
//                                                                    "or @caption='oben' " +
//                                                                    "or @caption='unten']"))
//                    {
//                        AreaGeometry.create(area.GetAttribute("caption") + (" ") + wallNr, parsePoints(area));
//                        wallNr++;
//                    }  
//					foreach (XmlElement area in subroom.SelectNodes("Transition[]"))
//					{
//						AreaGeometry.create(area.GetAttribute("caption") + (" ") + wallNr, parsePoints(area));
//						wallNr++;
//					} 



					//FloorExtrudeGeometry.create("floor " + floorNr, parsePoints_floor(subroom), zOffset);//here is creating floor?
                    floorNr++;
                }
                foreach (XmlElement subroom in room.SelectNodes("subroom[@class='stair' or @class ='Stair'"
                                                              + "or @class='idle_escalator']"))
                {
                    float A_x = TryParseWithDefault.ToSingle(subroom.GetAttribute("A_x"), 0);
                    float B_y = TryParseWithDefault.ToSingle(subroom.GetAttribute("B_y"), 0);
                    float C_z = TryParseWithDefault.ToSingle(subroom.GetAttribute("C_z"), 0);  //EXCEPTION: sometimes "C_z" is referred to as "C"
                    if (C_z == 0)
                    {
                        C_z = TryParseWithDefault.ToSingle(subroom.GetAttribute("C"), 0);
                    }

                    float pxUp = 0;     //Variables need to be assigned
                    float pyUp = 0;
                    float pxDown = 0;
                    float pyDown = 0;

                    foreach (XmlElement up in subroom.SelectNodes("up"))
                    {
                        pxUp = TryParseWithDefault.ToSingle(up.GetAttribute("px"), 0);
                        pyUp = TryParseWithDefault.ToSingle(up.GetAttribute("py"), 0);
                    }
                    foreach (XmlElement down in subroom.SelectNodes("down"))
                    {
                        pxDown = TryParseWithDefault.ToSingle(down.GetAttribute("px"), 0);
                        pyDown = TryParseWithDefault.ToSingle(down.GetAttribute("py"), 0);
                    }

                    bool lowerPart = true;
                    List<Vector2> coordFirst = new List<Vector2>();
                    List<Vector2> coordSecond = new List<Vector2>();
                    foreach (XmlElement stair in subroom.SelectNodes("polygon[@caption='wall']"))
                    {   
                        if (lowerPart == true)
                        {
                            coordFirst = parsePoints(stair);
                            lowerPart = false;
                        } else
                        {
                            coordSecond = parsePoints(stair);
                            lowerPart = true;
                        }
                    }


                    StairExtrudeGeometry.create(subroom.GetAttribute("class") + (" ") + stairNr, coordFirst, coordSecond, A_x, B_y, C_z, pxUp, pyUp, pxDown, pyDown);
                    stairNr++;
                } 
            }
        }

    }

    static List<Vector2> parsePoints(XmlElement polyPoints)
    {
        List<Vector2> list = new List<Vector2>();

        foreach (XmlElement vertex in polyPoints.SelectNodes("vertex"))
        {
            float x;
            float y;
            if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
            {
                list.Add(new Vector2(x, y));
            }
        }

        foreach (XmlElement openWall in polyPoints.SelectNodes("polygon[@caption='wall']"))  //used to list vertices of all walls in a subroom to create the floor
        {
            foreach (XmlElement vertex in openWall.SelectNodes("vertex"))
            {
                float x;
                float y;
                if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
                {
                    list.Add(new Vector2(x, y));
                }
            }
        }



        return list;
    }

	static List<Vector2> parsePoints_floor(XmlElement polyPoints)
	{
		List<Vector2> list = new List<Vector2>();

		foreach (XmlElement vertex in polyPoints.SelectNodes("vertex"))
		{
			float x;
			float y;
			if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
			{
				list.Add(new Vector2(x, y));
			}
		}

		foreach (XmlElement openWall in polyPoints.SelectNodes("polygon[@caption='wall']"))  //used to list vertices of all walls in a subroom to create the floor
		{
			foreach (XmlElement vertex in openWall.SelectNodes("vertex"))
			{
				float x;
				float y;
				if (float.TryParse(vertex.GetAttribute("px"), out x) && float.TryParse(vertex.GetAttribute("py"), out y))
				{
					list.Add(new Vector2(x, y));
				}
			}
		}

		SortNodes(list);

		return list;
	}

    void loadPedestrianFile(string filename)
    {
        if (!System.IO.File.Exists(filename))
        {
            Debug.Log("Error: File " + filename + " not found.");
            return;
        }

        XmlDocument xmlDocTraj = new XmlDocument();
        xmlDocTraj.LoadXml(System.IO.File.ReadAllText(filename));

        PedestrianLoader pl = GameObject.Find("PedestrianLoader").GetComponent<PedestrianLoader>();

        XmlNode trajectories = xmlDocTraj.SelectSingleNode("//trajectories");

        int fps = 8, FrameID;  //8 is used as a default framerate. Actual value is read from xmlDocTraj

        foreach (XmlElement header in trajectories.SelectNodes("header"))
        {
            
            XDocument doc = XDocument.Load(filename);
            var fpsInput = doc.Descendants("frameRate");

            foreach (var fpsVal in fpsInput)
            {
                if (float.Parse(fpsVal.Value) != 0)
                    fps = (int) float.Parse(fpsVal.Value);
            }  
        }

        foreach (XmlElement frame in trajectories.SelectNodes("frame"))
        {
            int.TryParse(frame.GetAttribute("ID"), out FrameID);

            if (FrameID % fps == 0)
            {
                foreach (XmlElement agent in frame)
                {
                    decimal time;
                    int id;
                    float x;
                    float y;
                    float z;     
                    decimal.TryParse(frame.GetAttribute("ID"), out time);
                    int.TryParse(agent.GetAttribute("ID"), out id);
                    float.TryParse(agent.GetAttribute("x"), out x);
                    float.TryParse(agent.GetAttribute("y"), out y);
                    float.TryParse(agent.GetAttribute("z"), out z);
                    pl.addPedestrianPosition(new PedestrianPosition(id, time / fps, x, y, z));
                }
            }
        }
        pl.createPedestrians();
    }
		
	static List<Vector2> SortNodes(List<Vector2> PointSet)//, List<Node> ch
	{

		int i,j,k = 0;//top = 2;

		for ( i = 0; i < PointSet.Count; i++)  //外循环是循环的次数
		{
			for ( j = PointSet.Count - 1 ; j > i; j--)  //内循环是 外循环一次比较的次数
			{

				if (PointSet[i].x == PointSet[j].x&&PointSet[i].y==PointSet[j].y)
				{
					PointSet.RemoveAt(j);
				}

			}
		}






		Vector2 tmp; int n = PointSet.Count;
		for (i = 0; i < n; i++)
		{
			for (j = i + 1; j < n; j++)
				if (PointSet[i].x == PointSet[j].x && PointSet[i].y == PointSet[j].y)
				{
					PointSet.Remove(PointSet[i]);
					n = PointSet.Count;
				}


		}


		// 选取PointSet中y坐标最小的点PointSet[k]，如果这样的点有多个，则取最左边的一个   
		for (i = 1; i < n; i++)
			if (PointSet[i].y < PointSet[k].y || (PointSet[i].y == PointSet[k].y)
				&& (PointSet[i].x < PointSet[k].x))
				k = i;
		tmp = PointSet[0];
		PointSet[0] = PointSet[k];
		PointSet[k] = tmp; // 现在PointSet中y坐标最小的点在PointSet[0]   
		//  对顶点按照相对PointSet[0]的极角从小到大进行排序，  
		//  极角相同的按照距离PointSet[0]从近到远进行排序  
		for (i = 1; i < n - 1; i++)
		{
			k = i;
			for (j = i + 1; j < n; j++)
				if (multiply(PointSet[j], PointSet[k], PointSet[0]) > 0 ||  // 极角更小      
					(multiply(PointSet[j], PointSet[k], PointSet[0]) == 0) && /* 极角相等，距离更短 */
					distance(PointSet[0], PointSet[j]) < distance(PointSet[0], PointSet[k]))
					k = j;
			tmp = PointSet[i];
			PointSet[i] = PointSet[k];
			PointSet[k] = tmp;
		}
		//ch[0] = PointSet[0];
		//ch[1] = PointSet[1];
		//ch[2] = PointSet[2];
		//for (i = 3; i < n; i++)
		//{
		//    while (multiply(PointSet[i].Location, ch[top].Location, ch[top - 1].Location) >= 0)
		//        top--;
		//    ch[++top] = PointSet[i];
		//}
		return PointSet;
	}
	static double multiply(Vector2 begPnt, Vector2 endPnt, Vector2 nextPnt)
	{
		return ((nextPnt.x - begPnt.x) * (endPnt.y - begPnt.y) - (endPnt.x - begPnt.x) * (nextPnt.y - begPnt.y));
	}
	static double distance(Vector2 pnt1, Vector2 pnt2)
	{
		return System.Math.Sqrt((pnt2.x - pnt1.x) * (pnt2.x - pnt1.x) + (pnt2.y - pnt1.y) * (pnt2.y - pnt1.y));
	}


}