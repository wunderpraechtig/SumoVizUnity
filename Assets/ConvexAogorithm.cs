using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



	public class ConvexAogrithm
	{
		public ConvexAogrithm ()
		{
			
		}

 
			private List<Vector2> nodes;  
			private Stack<Vector2> sortedNodes;  
			public Vector2[] sor_nodes;  
		public ConvexAogrithm(List<Vector2> points)
		{  
			nodes = points;  
		}  
			private double DistanceOfNodes(Vector2 p0, Vector2 p1)
		{  
			if (p0.Equals(null) || p1.Equals(null))
				return 0.0;  
			return Math.Sqrt ((p1.x - p0.x) * (p1.x - p0.x) + (p1.y - p0.y) * (p1.y - p0.y));  
		}  
			public void GetNodesByAngle( out Vector2 p0)
		{  
			LinkedList<Vector2> list_node = new LinkedList<Vector2> ();  
			p0 = GetMinYPoint ();  
			LinkedListNode<Vector2> node = new LinkedListNode<Vector2> (nodes [0]);  
			list_node.AddFirst (node);  
			for (int i = 1; i < nodes.Count; i++) {  
				int direct = IsClockDirection (p0, node.Value, nodes [i]);  
				if (direct == 1) {  
					list_node.AddLast (nodes [i]);  
					node = list_node.Last;  
					//node.Value = nodes[i];  

				} else if (direct == -10) {  
					list_node.Last.Value = nodes [i];  
					//node = list_node.Last  
					//node.Value = nodes[i];  
				} else if (direct == 10)
					continue;
				else if (direct == -1) {  
					LinkedListNode<Vector2> temp = node.Previous;  
					while (temp != null && IsClockDirection (p0, temp.Value, nodes [i]) == -1) {  
						temp = temp.Previous;  
					}  
					if (temp == null) {  
						list_node.AddFirst (nodes [i]);  
						continue;  
					}  
					if (IsClockDirection (p0, temp.Value, nodes [i]) == -10)
						temp.Value = nodes [i];
					else if (IsClockDirection (p0, temp.Value, nodes [i]) == 10)
						continue;
					else
						list_node.AddAfter (temp, nodes [i]);  
				}  
			}  
			sor_nodes = list_node.ToArray ();  
			sortedNodes = new Stack<Vector2> ();  
			sortedNodes.Push (p0);  
			sortedNodes.Push (sor_nodes [0]);  
			sortedNodes.Push (sor_nodes [1]);  
			for (int i = 2; i < sor_nodes.Length; i++) {  

				Vector2 p2 = sor_nodes [i];  
				Vector2 p1 = sortedNodes.Pop ();  
				Vector2 p0_sec = sortedNodes.Pop ();  
				sortedNodes.Push (p0_sec);  
				sortedNodes.Push (p1);  

				if (IsClockDirection1 (p0_sec, p1, p2) == 1) {  
					sortedNodes.Push (p2);  
					continue;  
				}  
				while (IsClockDirection1 (p0_sec, p1, p2) != 1) {  
					sortedNodes.Pop ();  
					p1 = sortedNodes.Pop ();  
					p0_sec = sortedNodes.Pop ();  
					sortedNodes.Push (p0_sec);  
					sortedNodes.Push (p1);  
				}  
				sortedNodes.Push (p2);  
			}  


		}  
			private int IsClockDirection1(Vector2 p0, Vector2 p1, Vector2 p2)
		{  
			Vector2 p0_p1 = new Vector2 (p1.x - p0.x, p1.y - p0.y);  
			Vector2 p0_p2 = new Vector2 (p2.x - p0.x, p2.y - p0.y);  
			return (p0_p1.x * p0_p2.y - p0_p2.x * p0_p1.y) > 0 ? 1 : -1;  
		}  
			private Vector2 GetMinYPoint()
		{  
			Vector2 succNode;  
			float miny = nodes.Min (r => r.y);  
			IEnumerable<Vector2> pminYs = nodes.Where (r => r.y == miny);  
			Vector2[] ps = pminYs.ToArray ();  
			if (pminYs.Count () > 1) {  
				succNode = pminYs.Single (r => r.x == pminYs.Min (t => t.x));  
				nodes.Remove (succNode);  
				return succNode;  
			} else {  
				nodes.Remove (ps [0]);  
				return ps [0];  
			}  

		} 
			private int IsClockDirection(Vector2 p0, Vector2 p1, Vector2 p2)  
			{  
				Vector2 p0_p1 = new Vector2(p1.x-p0.x,p1.y-p0.y) ;  
				Vector2 p0_p2 = new Vector2(p2.x - p0.x, p2.y - p0.y);  
				if ((p0_p1.x * p0_p2.y - p0_p2.x * p0_p1.y) != 0)  
					return (p0_p1.x * p0_p2.y - p0_p2.x * p0_p1.y) > 0 ? 1 : -1;  
				else  
					return DistanceOfNodes(p0, p1) > DistanceOfNodes(p0, p2) ? 10 : -10;  

		}
			public Stack<Vector2> SortedNodes {  
			get { return sortedNodes; }  
		} 

	}



