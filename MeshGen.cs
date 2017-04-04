/* Zoilo Merdedes
 * Marching Squares Mesh Generator, credit Sebastian Lague
 */ 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour {

	public SquareGrid squareGrid;
	List<Vector3> vertices;
	List<int> triangles;

	public void GenerateMesh(int[,] map, float squareSize){
		squareGrid = new SquareGrid(map, squareSize);

		vertices = new List<Vector3>();
		triangles = new List<int>();

		for(int x = 0;x < squareGrid.squares.GetLength(0);x++){
			for(int y = 0;y < squareGrid.squares.GetLength(1);y++){
					TriangulateSquare(squareGrid.squares[x,y]);
			}
		}

		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
	}
	
	void TriangulateSquare(Square square){
		switch (square.configuration){
			case 0:
				break;
			case 1: // bottom left (0001)
				MeshFromPoints(square.centerBottom,square.bottomLeft,square.centerLeft);
				break;
			case 2: // bottom right (0010)
				MeshFromPoints(square.centerRight,square.bottomRight,square.centerBottom);
				break;
			case 4: // top right (0100)
				MeshFromPoints(square.centerTop,square.topRight,square.centerRight);
				break;
			case 8: // top left (1000)
				MeshFromPoints(square.topLeft,square.centerTop,square.centerLeft);
				break;

			case 3: // bottom right, bottom left (0011)
				MeshFromPoints(square.centerRight,square.bottomRight,square.bottomLeft, square.centerLeft);
				break;
			case 6: // top right, bottom right (0110)
				MeshFromPoints(square.centerTop,square.topRight,square.bottomRight, square.centerBottom);
				break;
			case 12: // top left, top right (1100)
				MeshFromPoints(square.topLeft,square.topRight,square.centerRight, square.centerLeft);
				break;
			case 9: // top left, bottom left (1001)
				MeshFromPoints(square.topLeft,square.centerTop,square.centerBottom, square.bottomLeft);
				break;
			case 5: // bottom left, top right (0101)
				MeshFromPoints(square.centerTop,square.topRight,square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
				break;
			case 10: // top left, bottom right (1010)
				MeshFromPoints(square.topLeft,square.centerTop,square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
				break;

			case 7: // top right, bottom right, bottom left (0111)
				MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
				break;
			case 11: // top left, bottom right, bottom left (1011)
				MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
				break; 
			case 13: // top left, top right, bottom left (1101)
				MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.bottomRight, square.bottomLeft);
				break;
			case 14: // top right, top left, bottom left (1110)
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
				break;

			case 15: // all control points (1111)
				MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
				break;
		}
	}
	
	void MeshFromPoints(params Node[] points){
		AssignVertices(points);

		if(points.Length >= 3)
			CreateTriangle(points[0],points[1],points[2]);
		if(points.Length >= 4)
			CreateTriangle(points[0],points[2],points[3]);
		if(points.Length >= 5)
			CreateTriangle(points[0],points[3],points[4]);
		if(points.Length >= 6)
			CreateTriangle(points[0],points[4],points[5]);
	}

	void AssignVertices(Node[] points){
		foreach(Node node in points){
			if(node.vertexIndex == -1){
				node.vertexIndex = vertices.Count;
				vertices.Add(node.position);
			}
		}
	}

	void CreateTriangle(Node a, Node b, Node c){
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);
	}

	void OnDrawGizmos(){/*
		if(squareGrid != null){
			for(int x = 0;x < squareGrid.squares.GetLength(0);x++){
				for(int y = 0;y < squareGrid.squares.GetLength(1);y++){
					// control nodes
					Gizmos.color = (squareGrid.squares[x,y].topLeft.active) ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].topLeft.position, Vector3.one * .4f);

					Gizmos.color = (squareGrid.squares[x,y].topRight.active) ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].topRight.position, Vector3.one * .4f);

					Gizmos.color = (squareGrid.squares[x,y].bottomRight.active) ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].bottomRight.position, Vector3.one * .4f);

					Gizmos.color = (squareGrid.squares[x,y].bottomLeft.active) ? Color.black : Color.white;
					Gizmos.DrawCube(squareGrid.squares[x,y].bottomLeft.position, Vector3.one * .4f);
				
					Gizmos.color = Color.grey; // midpoint nodes
					Gizmos.DrawCube(squareGrid.squares[x,y].centerTop.position, Vector3.one * .15f);
					Gizmos.DrawCube(squareGrid.squares[x,y].centerRight.position, Vector3.one * .15f);
					Gizmos.DrawCube(squareGrid.squares[x,y].centerBottom.position, Vector3.one * .15f);
					Gizmos.DrawCube(squareGrid.squares[x,y].centerLeft.position, Vector3.one * .15f);
				}
			}
		}*/
	}

	public class SquareGrid {
		public Square[,] squares;

		public SquareGrid(int[,] map, float squareSize) {
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength(1);
			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;

			ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];

			for(int x = 0;x < nodeCountX;x++){
				for(int y = 0;y < nodeCountY;y++){
					Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2,-mapHeight/2 + y * squareSize + squareSize/2,0);
					controlNodes[x,y] = new ControlNode(pos,map[x,y] == 1,squareSize);
				}
			}

			squares = new Square[nodeCountX -1,nodeCountY -1];
			// initialize squares in squares[,]
			for(int x = 0;x < nodeCountX-1;x++){
				for(int y = 0;y < nodeCountY-1;y++){
					squares[x,y] = new Square(controlNodes[x,y+1],controlNodes[x+1,y+1],controlNodes[x+1,y], controlNodes[x,y]);
				}
			}
		}
	}

	public class Square {
		public ControlNode topLeft, topRight, bottomLeft, bottomRight;
		public Node centerTop, centerRight, centerBottom, centerLeft;
		public int configuration;

		public Square(ControlNode _tL,ControlNode _tR,ControlNode _bR,ControlNode _bL) {
			topLeft = _tL;
			topRight = _tR;
			bottomLeft = _bL;
			bottomRight = _bR;

			centerTop = topLeft.right;
			centerRight = bottomRight.above;
			centerLeft = bottomLeft.above;
			centerBottom = bottomLeft.right;


			if(topLeft.active)
				configuration += 8;
			if(topRight.active)
				configuration += 4;
			if(bottomRight.active)
				configuration += 2;
			if(bottomLeft.active)
				configuration += 1;

		}
	}

	public class Node {
		public Vector3 position;
		public int vertexIndex = -1;

		public Node(Vector3 _pos){
			position = _pos;
		}
	}

	public class ControlNode : Node {

		public bool active;
		public Node above, right;

		public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
			active = _active;
			above = new Node(position + Vector3.forward * squareSize/2f);
			right = new Node(position + Vector3.right * squareSize/2f);
		}
	}
}