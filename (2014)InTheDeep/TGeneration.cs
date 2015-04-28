using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TGeneration : MonoBehaviour {

	private int myListPos;
	private int myList;
	
	private Terrain terrain;
	
	public float[,] terrainHeight;
	private int xRes;
	private int zRes;
	protected int terrainRes;
	
	private int squareSize;

	private GameObject player;

	//Objects
	private List<GameObject> objectsList = new List<GameObject>();

	//Variables to change
	private int sourcePoints = 8;
	protected int keepRandOdds = 1;
	protected float initialRandomH = 0.65f;
	protected float initialRandomL = 0.35f;
	protected float averageRandHigh = 0.13f;
	protected float averageRandLow = 0.0f;
	protected float randHMultiplier = 0.65f;
	protected float randLMultiplier = 0.65f;
	protected float randHLimit = 0.001f;
	protected float randLLimit = -0.001f;
	protected bool randSquare = true;
	protected bool randDiamond = true;
	private float addingValue = -123f;



	// Use this for initialization
	void Start () {
		
	}
	
	//Assigns the player variable according to the parameter.
	public void GetPlayer(GameObject currentPlayer){
		player = currentPlayer;
	}

	//Adds the parameter GameObject to the objectList.
	public void GetGameObject(GameObject gObject){
		objectsList.Add(gObject);
	}
	
	//Sets the myListPos variable according to the int parameter.
	public void GetListPos(int myPos){
		myListPos = myPos;
	}

	//Sets the myList variable according to the int parameter.
	public void GetList(int list){
		myList = list;
	}

	//Spawns objects in the world, depending on the detail resolution of the terrain map.
	public void SpawnObjects(){
		for(int zA = 0; zA<=terrainRes-1; zA+= (int)terrainRes/50){
			for(int xA = 0; xA<=terrainRes-1; xA+= (int)terrainRes/50){
				if(Random.Range(0, 200) == 0){
					GameObject newObject = (GameObject)Network.Instantiate(objectsList[0], new Vector3((int)(myListPos*1000)+xA*1000/terrainRes, 
                                                                           (ReturnHeightValue(xA,zA)*1000)+1, 
                                                                   			(int)(myList*1000)+zA*1000/terrainRes), objectsList[0].transform.rotation, 0);
					newObject.SendMessage("GetPlayer", player);
				}
			}
		}
	}

	//Starts the terrain generation, based on the Diamond Square Algorithm
	public void StartGeneration(int seed){
		Random.seed = seed;
		terrain = gameObject.GetComponent<Terrain>();
		xRes = terrain.terrainData.heightmapWidth;
		zRes = terrain.terrainData.heightmapHeight;
		terrainHeight = terrain.terrainData.GetHeights(0,0, xRes, zRes);
		terrainRes = terrain.terrainData.heightmapResolution;

		InitialPattern();
	}

	//Creates the initial pattern for the terrain generation.
	private void InitialPattern(){
		squareSize = (terrainRes-1)/sourcePoints;
		for(int zA = 0; zA<=terrainRes-1; zA+= squareSize){
			for(int xA = 0; xA<=terrainRes-1; xA+= squareSize){
				if(zA <= terrainRes-1 && xA <= terrainRes-1){
					if(zA == 0 && myList > 0){//take from below plane
						terrainHeight[zA, xA] = InGameMenu.terrainList[myList-1][myListPos]
						.GetComponent<TGeneration>().ReturnHeightValue(xA, terrainRes-1);
					}else if(xA == 0 && myListPos > 0){//take from left plane
						terrainHeight[zA, xA] = InGameMenu.terrainList[myList][myListPos-1]
						.GetComponent<TGeneration>().ReturnHeightValue(terrainRes-1,zA);
					}else if(zA == terrainRes-1 && myList == InGameMenu.terrainList.Count-1){//take from bottom plane
						terrainHeight[zA, xA] = InGameMenu.terrainList[0][myListPos]
						.GetComponent<TGeneration>().ReturnHeightValue(xA,0);
					}else if(xA == terrainRes-1 && myListPos == InGameMenu.terrainList[myList].Count-1){//take from most left plane
						terrainHeight[zA, xA] = InGameMenu.terrainList[myList][0]
						.GetComponent<TGeneration>().ReturnHeightValue(0,zA);
					}
					else{
						terrainHeight[zA, xA] = Random.Range(initialRandomL, initialRandomH);
					}
				}
			}
		}
	}

	//Runs the square algorithm sequence.
	private void CreateSquare(){
		if(squareSize / 2 >= 1){
			squareSize /= 2;
		}
		for(int zA = squareSize; zA<=terrainRes-1-squareSize; zA += squareSize*2){
			for(int xA = squareSize; xA<=terrainRes-1-squareSize; xA+= squareSize*2){
				if(zA <= terrainRes-1 && xA <= terrainRes-1 && zA >= 0 && xA >= 0){
					float terrainFloat = 0f;
					int neighbours = 0;
					for(int zN = zA-squareSize; zN<=zA+squareSize; zN+= squareSize*2){
						for(int xN = xA-squareSize; xN<=xA+squareSize; xN+= squareSize*2){
							if(zN <= terrainRes-1 && xN <= terrainRes-1 && zN >= 0 && xN >= 0){
								terrainFloat += terrainHeight[zN,xN];
								neighbours ++;
							}
						}
					}
					terrainFloat /= neighbours;
					if(randSquare){
						if(Random.Range(0, keepRandOdds) == 0 || addingValue == -123f){
							addingValue = Random.Range(averageRandLow, averageRandHigh);
						}
						terrainFloat += addingValue;
					}
					terrainHeight[zA, xA] = terrainFloat;
				}
			}
		}
		if(averageRandHigh > randHLimit){
			averageRandHigh *= randHMultiplier;
		}
		if(averageRandLow < randLLimit){
			averageRandLow *= randLMultiplier;
		}
	}
	
	//Runs the diamond algorithm sequence.
	private void CreateDiamond(){
		int posBooster = 0;

		for(int zA = 0; zA<=terrainRes-1; zA += squareSize){
			for(int xA = (-squareSize)+posBooster; xA<=terrainRes-1+squareSize; xA+= squareSize*2){
				if(xA <= terrainRes-1 && zA <= terrainRes-1 && xA >= 0){
					posBooster = 0;
					float terrainFloat = 0f;
					int neighbours = 0;
					for(int zN = zA-squareSize; zN<=zA+squareSize; zN+= squareSize){
						for(int xN = xA-squareSize; xN<=xA+squareSize; xN+= squareSize){
							if((zN != zA && xN == xA) || (zN == zA && xN != xA)){
								if(zN < 0 && myList > 0){//take from below plane
									terrainFloat += InGameMenu.terrainList[myList-1][myListPos]
									.GetComponent<TGeneration>().ReturnHeightValue(xN,(terrainRes-1)-squareSize);
									neighbours ++;
								}if(zN < 0 && myList == 0){//take from top plane
									terrainFloat += InGameMenu.terrainList[InGameMenu.terrainList.Count-1][myListPos]
									.GetComponent<TGeneration>().ReturnHeightValue(xN,(terrainRes-1)-squareSize);
									neighbours ++;
								}else if(xN < 0 && myListPos > 0){//take from left plane
									terrainFloat += InGameMenu.terrainList[myList][myListPos-1]
									.GetComponent<TGeneration>().ReturnHeightValue((terrainRes-1)-squareSize,zN);
									neighbours ++;
								}else if(xN < 0 && myListPos == 0){//take from right most plane
									terrainFloat += InGameMenu.terrainList[myList][InGameMenu.terrainList[myList].Count-1]
									.GetComponent<TGeneration>().ReturnHeightValue((terrainRes-1)-squareSize,zN);
									neighbours ++;
								}else if(zN > terrainRes-1 && myList < InGameMenu.terrainList.Count-1){//take from above plane
									terrainFloat += InGameMenu.terrainList[myList+1][myListPos]
									.GetComponent<TGeneration>().ReturnHeightValue(xN,squareSize);
									neighbours ++;
								}else if(zN > terrainRes-1 && myList == InGameMenu.terrainList.Count-1){//take from bottom plane
									terrainFloat += InGameMenu.terrainList[0][myListPos]
									.GetComponent<TGeneration>().ReturnHeightValue(xN,squareSize);
									neighbours ++;
								}else if(xN > terrainRes-1 && myListPos < InGameMenu.terrainList[myList].Count-1){//take from right plane
									terrainFloat += InGameMenu.terrainList[myList][myListPos+1]
									.GetComponent<TGeneration>().ReturnHeightValue(squareSize ,zN);
									neighbours ++;
								}else if(xN > terrainRes-1 && myListPos == InGameMenu.terrainList[myList].Count-1){//take most left plane
									terrainFloat += InGameMenu.terrainList[myList][0]
									.GetComponent<TGeneration>().ReturnHeightValue(squareSize ,zN);
									neighbours ++;
								}else if(zN >= 0 && xN >= 0 && zN <= terrainRes-1 && xN <= terrainRes-1){
									terrainFloat += terrainHeight[zN,xN];
									neighbours ++;
								}
							}
						}
					}
					terrainFloat /= neighbours;
					if(zA > 0 && zA < terrainRes-1 && xA > 0 && xA < terrainRes-1 && randDiamond){
						if(Random.Range(0, keepRandOdds) == 0 || addingValue == -123f){
							addingValue = Random.Range(averageRandLow, averageRandHigh);
						}
						terrainFloat += addingValue;
					}
					terrainHeight[zA, xA] = terrainFloat;
				}else if(xA > terrainRes-1){
					posBooster = squareSize;
				}
			}
		}
		if(averageRandHigh > randHLimit){
			averageRandHigh *= randHMultiplier;
		}
		if(averageRandLow < randLLimit){
			averageRandLow *= randLMultiplier;
		}
	}

	//Returns the height value according to the parameters.
	public float ReturnHeightValue(int x, int z){
		return terrainHeight[z, x];
	}

	//Sets the terrainHeight.
	public void SetGeneratedHeights(){
		terrain.terrainData.SetHeights(0, 0, terrainHeight);
	}
}
