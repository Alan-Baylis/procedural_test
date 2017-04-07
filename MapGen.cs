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
		//ProcessMap();

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

	void ProcessMap(){
		List<List<Coord>> wallRegions = GetRegions(1);

		foreach(List<Coord> wallRegion in wallRegions){
			if(wallRegion.Count < wallThreshold){
				foreach(Coord tile in wallRegion)
					map[tile.tileX,tile.tileY] = 0;
			}
		}
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
		int[,] mapFlags = new int[width,height];
		int tileType = map[startX,startY];

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX,startY));
		mapFlags[startX,startY] = 1;

		while(queue.Count > 0){
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for(int x = tile.tileX -1; x <= tile.tileX; x++){
				for(int y = tile.tileY -1; y <= tile.tileY; y++){
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

	struct Coord {
		public int tileX;
		public int tileY;

		public Coord(int x, int y) { 
			tileX = x;
			tileY = y;
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