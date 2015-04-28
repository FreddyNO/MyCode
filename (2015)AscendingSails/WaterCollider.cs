using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterCollider : MonoBehaviour {

	public List<GameObject> waterList = new List<GameObject>();

	public GameObject shipHolder;

	public bool leadOcean;
	private bool normalWaves;

	private Vector3[] vertices;

	private float wAmplitudeRate = 0.1f;
	private float wAmplitude = 1.0f;
	private float wSpeed = 1.5f;
	private float wFrequency = 0.0f;
	private float wFrequencyAdd = 1.25661f;

	private int vertXOffset;
	private int maxWAmplitude = 2;
	private int minWAmplitude = 1;

	// Use this for initialization
	void Start () {
		normalWaves = true;
		vertices = waterList[3].GetComponent<MeshFilter>().sharedMesh.vertices;
	}

	// Update is called once per frame
	void Update() {
		CheckShipState();
		WaveCalculation();
	}

	//Checks if the ship is close to an island, or if it's on the open ocean.
	void CheckShipState(){
		if(normalWaves && PlayerControls.modeShip && !shipHolder.GetComponent<ShipSystem>().GetCloseToIslandState()){
			StopCoroutine("DecreaseWAmplitude");
			StartCoroutine("IncreaseWAmplitude");
		}else if(!normalWaves && (!PlayerControls.modeShip || shipHolder.GetComponent<ShipSystem>().GetCloseToIslandState())){
			StopCoroutine("IncreaseWAmplitude");
			StartCoroutine("DecreaseWAmplitude");
		}
	}
	
	//Sets the new value for the wAmplitude variable.
	public void SetWAmplitude(float newAmp){
		wAmplitude = newAmp;
	}

	//Increases the height of waves on the open ocean.
	public IEnumerator IncreaseWAmplitude(){
		while(wAmplitude < maxWAmplitude){
			wAmplitude += wAmplitudeRate*Time.deltaTime;
			yield return null;
		}
		normalWaves = false;
	}
	
	//Decreases the height of waves when close to an island.
	public IEnumerator DecreaseWAmplitude(){
		while(wAmplitude > minWAmplitude){
			wAmplitude -= wAmplitudeRate*Time.deltaTime;
			yield return null;
		}
		normalWaves = true;
	}

	//Calculates the cosnus wave algorithm
	void WaveCalculation(){
		//The vertices y-axis(height) position calculation.
		if(leadOcean){
			wFrequency = 0.0f;
			for(int vertPosZ = 0; vertPosZ<Mathf.Sqrt(vertices.Length); vertPosZ++){
				for(int vertPosX = vertPosZ; vertPosX< Mathf.Sqrt(vertices.Length)+vertPosZ; vertPosX++){
					float wH = 0.0f;
					wH = wAmplitude *Mathf.Cos(wFrequency+1*Time.time*wSpeed);
					vertices[vertPosZ*10+vertPosX] = new Vector3(vertices[vertPosZ*10+vertPosX].x,wH,vertices[vertPosZ*10+vertPosX].z);
				}
				wFrequency += wFrequencyAdd;
			}

			wFrequency = 0.0f;
			for(int vertPosX = 0; vertPosX<(int)Mathf.Sqrt(vertices.Length); vertPosX++){
				vertXOffset = 0;
				for(int vertPosZ = 0; vertPosZ<(int)Mathf.Sqrt(vertices.Length); vertPosZ++){
					float wH = 0.0f;
					wH = wAmplitude *Mathf.Cos(wFrequency+1*Time.time*wSpeed);
					vertices[vertPosX+vertXOffset+(vertPosZ*10)] += new Vector3(0,wH,0);
					vertXOffset ++;
				}
				wFrequency += wFrequencyAdd;
			}

			//Updates actual mesh vertices/bounds/normals
			waterList[3].GetComponent<MeshFilter>().sharedMesh.vertices = vertices;
			waterList[3].GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
			waterList[3].GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();

		}
		for(int waterNr = 0; waterNr<waterList.Count; waterNr++){
			waterList[waterNr].transform.parent.GetComponent<MeshCollider>().enabled = false;
			waterList[waterNr].transform.parent.GetComponent<MeshCollider>().enabled = true;
		}
	}

	//Returns the normals of the ocean plane mesh.
	public Vector3[] ReturnNormal(){
		return waterList[3].GetComponent<MeshFilter>().sharedMesh.normals;
	}

	//Returns the vertices array of the ocean plane mesh.
	public Vector3[] ReturnArray(){
		return vertices;
	}
}
