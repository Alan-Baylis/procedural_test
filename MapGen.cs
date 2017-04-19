/* Zoilo Mercedes
 * Procedural Map Generation tutorial/test, credit Sebastian Lague
 * Uses cellular automata to generate basic cave shapes
 * Many ways to experiment with the outcome:
 *     change number of smoothing iterations
 *     change automata rules (SmoothMap method)
 *     do different rules based on # of iterations
 *     check different neighbors (GetSurroundingWallCount method)
 * Cool seeds:
 * 	   26.03741
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGen : MonoBehaviour {

	public int width, height;
	public string seed;
	public bool useRandomSeed;
	public int smoothingIterations = 5;
	public int wallThreshold = 50; // used in processmap, any wall region < this will be removed.
	public int roomThreshold = 50; // same as above for room regions
	public int passageRad = 1; // how wide passageways will be

	[Range(0,100)]
	public int randomFillPercent;

	int[,] map;

	void Start(){
		GenerateMap();
	}

	void Update(){
		if(Input.GetMouseButtonDown(0))
			GenerateMap();
	}

	void GenerateMap(){
		map = new int[width,height];
		RandomFillMap();

		for(int i = 0; i<smoothingIterations;i++) 
			SmoothMap();

		ProcessMap();

		int borderSize = 5; // sets border THICCness
		int[,] borderedMap = new int[width + borderSize*2,height + borderSize*2];

		for (int x = 0; x < borderedMap.GetLength(0); x++){
			for (int y = 0; y <  borderedMap.GetLength(1); y++){
				if(x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
					borderedMap[x,y] = map[x - borderSize,y - borderSize];
				else
					borderedMap[x,y] = 1;
			}
		}

		MeshGen meshGen = GetComponent<MeshGen>();
		meshGen.GenerateMesh(borderedMap, 1);
	}

	void RandomFillMap(){
		if(useRandomSeed)
			seed = Time.time.ToString();

		System.Random rng = new System.Random(seed.GetHashCode());
	
		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				if(x == 0 || x == width-1 || y == 0 || y == height-1)
					map[x,y] = 1;
				else
					map[x,y] = (rng.Next(0,100) < randomFillPercent) ? 1:0;
			}
		}
	}

	void SmoothMap() {
		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				int neighborWallTiles = GetSurroundingWallCount(x,y);

				if (neighborWallTiles > 4) // rules to mess with for diff shapes
					map[x,y] = 1;
				else if (neighborWallTiles < 4) // rules to mess with for diff shapes
					map[x,y] = 0;

			}
		}
	}

	// searches 3x3 grid surrounding a tile for its neighbors.
	// can change which/how many neighbors to look at for diff outcome
	int GetSurroundingWallCount(int gridX, int gridY){ 
		int wallCount = 0;
		for (int neighborX = gridX - 1; neighborX <= gridX+1 ; neighborX++){
			for (int neighborY = gridY-1; neighborY <= gridY+1; neighborY++){
				if(IsInMapRange(neighborX, neighborY)){
					if(neighborX != gridX || neighborY != gridY)
						wallCount += map[neighborX,neighborY];	
				} else 
					wallCount++;
			}
		}
		return wallCount;
	}

	// goes over map and removes regions smaller than wallThreshold
	void ProcessMap(){
		List<List<Coord>> wallRegions = GetRegions(1);

		foreach(List<Coord> wallRegion in wallRegions){
			if(wallRegion.Count < wallThreshold){
				foreach(Coord tile in wallRegion)
					map[tile.tileX,tile.tileY] = 0;
			}
		}

		List<List<Coord>> roomRegions = GetRegions(0);
		List<Room> survivingRooms = new List<Room>();

		foreach(List<Coord> roomRegion in roomRegions){
			if(roomRegion.Count < roomThreshold){
				foreach(Coord tile in roomRegion)
					map[tile.tileX,tile.tileY] = 1;
			} else {
				survivingRooms.Add(new Room(roomRegion, map));
			}
		}
		survivingRooms.Sort(); // will put biggest room at the start.
		survivingRooms[0].isMain = true; // biggest room is main room.
		survivingRooms[0].isAccessibleFromMain = true; // the main room is accessible from itself

		ConnectClosestRooms(survivingRooms);
	}

	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessFromMain = false){
		List<Room> roomListA = new List<Room>();
		List<Room> roomListB = new List<Room>();

		if(forceAccessFromMain){
			foreach(Room room in allRooms){
				if(room.isAccessibleFromMain)
					roomListB.Add(room);
				else
					roomListA.Add(room);
			}
		} else {
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = 0;
		Coord bestTileA = new Coord();
		Coord bestTileB = new Coord();
		Room bestRoomA = new Room();
		Room bestRoomB = new Room();
		bool possibleConnection = false;

		foreach(Room roomA in roomListA){
			if(!forceAccessFromMain){
				possibleConnection = false; // watch ep 7(~9:00) to understand this logic
				if(roomA.connectedRooms.Count > 0)
					continue;
			}

			foreach(Room roomB in roomListB){
				if(roomA == roomB || roomA.IsConnected(roomB))
					continue; // goes to next roomB

				for(int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++){
					for(int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++){
						Coord tileA = roomA.edgeTiles[tileIndexA];
						Coord tileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));
					
						if(distanceBetweenRooms < bestDistance || !possibleConnection){
							bestDistance = distanceBetweenRooms;
							possibleConnection = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}
			if(possibleConnection && !forceAccessFromMain)
				CreatePassage(bestRoomA,bestRoomB,bestTileA,bestTileB);
		}
		if(possibleConnection && forceAccessFromMain){
			CreatePassage(bestRoomA,bestRoomB,bestTileA,bestTileB);
			ConnectClosestRooms(allRooms, true);
		}

		if(!forceAccessFromMain){
			ConnectClosestRooms(allRooms, true);
		}
	}

	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB){
		Room.ConnectRooms(roomA, roomB);
		Debug.DrawLine(CoordToWorldPoint(tileA),CoordToWorldPoint(tileB), Color.green, 100);

		List<Coord> line = GetLine(tileA, tileB);
		foreach(Coord c in line)
			DrawCircle(c,passageRad);
	}

	void DrawCircle(Coord c, int r){
		for(int x = -r; x <= r; x++){
			for(int y = -r; y <= r; y++){
				if(x*x + y*y <= r*r){
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;
					if(IsInMapRange(drawX,drawY))
						map[drawX,drawY] = 0;
				}
			}
		}
	}

	List<Coord> GetLine(Coord from, Coord to){
		List<Coord> line = new List<Coord>();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;

		int step = Math.Sign(dx);
		int gradientStep = Math.Sign(dy);

		int longest = Mathf.Abs(dx);
		int shortest = Mathf.Abs(dy);

		if(longest < shortest){
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign(dy);
			gradientStep = Math.Sign(dx);
		}

		int gradientAcc = longest / 2;
		for(int i = 0; i < longest; i++){
			line.Add(new Coord(x,y));

			if(inverted)
				y += step;
			else
				x += step;

			gradientAcc += shortest;
			if(gradientAcc >= longest){
				if(inverted)
					x += gradientStep;
				else
					y += gradientStep;

				gradientAcc -= longest;
			}
		}
		return line;
	}

	// this method is adapted for 2d, might give errors
	Vector3 CoordToWorldPoint(Coord tile){
		return new Vector3(-width/2 + .5f + tile.tileX, -height/2 + .5f + tile.tileY,-1);
	}

	List<List<Coord>> GetRegions(int tileType){
		List<List<Coord>> regions = new List<List<Coord>>();
		int[,] mapFlags = new int[width,height];

		for(int x = 0; x < width; x++){
			for(int y = 0; y < height; y++){
				if(mapFlags[x,y] == 0 && map[x,y] == tileType){
					List<Coord> newRegion = GetRegionTiles(x,y);
					regions.Add(newRegion);

					foreach(Coord tile in newRegion){
						mapFlags[tile.tileX,tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	List<Coord> GetRegionTiles(int startX, int startY){
		List<Coord> tiles = new List<Coord>();
		int[,] mapFlags = new int[width,height]; // determine which tiles have been looked at
		int tileType = map[startX,startY];

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX,startY));
		mapFlags[startX,startY] = 1;

		while(queue.Count > 0){
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for(int x = tile.tileX -1; x <= tile.tileX +1; x++){
				for(int y = tile.tileY -1; y <= tile.tileY+1; y++){
					// checks for non diagonal tiles
					if(IsInMapRange(x,y) && (y == tile.tileY || x == tile.tileX)){
						if(mapFlags[x,y] == 0 && map[x,y] == tileType){
							mapFlags[x,y] = 1;
							queue.Enqueue(new Coord(x,y));
						}
					}
				}
			}
		}
		return tiles;
	}

	bool IsInMapRange(int x, int y){
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	Vector3 GetRandomCaveLocation(){
		Coord random;

		return CoordToWorldPoint(random);
	}

	public struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) { 
			tileX = x;
			tileY = y;
		}
	}
	
	public class Room : IComparable<Room> {
		public List<Coord> tiles; // tiles that belong to room
		public List<Coord> edgeTiles; // edges of room
		public List<Room> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMain;
		public bool isMain;

		public Room(){}

		public Room(List<Coord> roomTiles, int[,] map){
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();

			edgeTiles = new List<Coord>();
			foreach(Coord tile in tiles){
				for(int x = tile.tileX-1; x<= tile.tileX+1; x++){
					for(int y = tile.tileY-1; y<= tile.tileY+1; y++){
						if( x == tile.tileX || y == tile.tileY){ // if tile being checked isn't diagonal
							if(map[x,y] == 1){
								edgeTiles.Add(tile);
							}
						}
					}
				}
			}
		}

		// 
		public void SetAccessibleFromMainRoom(){
			if(!isAccessibleFromMain){
				isAccessibleFromMain = true;
				foreach(Room connectedRoom in connectedRooms)
					connectedRoom.SetAccessibleFromMainRoom();
			}
		}

		public static void ConnectRooms(Room roomA, Room roomB){
			if(roomA.isAccessibleFromMain)
				roomB.SetAccessibleFromMainRoom();
			else if(roomB.isAccessibleFromMain)
				roomA.SetAccessibleFromMainRoom();

			roomA.connectedRooms.Add(roomB);
			roomB.connectedRooms.Add(roomA);
		}
		public bool IsConnected(Room otherRoom){
			return connectedRooms.Contains(otherRoom);
		}
		
		// when using IComparable<obj> interface, must define CompareTo
		// primitives(?) have compareto defined already
		public int CompareTo(Room otherRoom){ 
			return otherRoom.roomSize.CompareTo(roomSize);
		}
	}

	void OnDrawGizmos(){/*
		if(map!= null){
			for (int x = 0; x < width; x++){
				for (int y = 0; y < height; y++){
					Gizmos.color = (map[x,y] == 1) ? Color.black : Color.white;
					Vector3 pos = new Vector3(-width/2 + x +.5f,-height/2 + y +.5f,0); // why /2?
					Gizmos.DrawCube(pos, Vector3.one); // (position, size)
				}
			}
		}*/
	}
}