using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DynamicQuest : MonoBehaviour {

	private List<GameObject> enemyList = new List<GameObject>();
	private List<GameObject> resourceList = new List<GameObject>();
	private List<int> killList = new List<int>();
	private List<int> gatherList = new List<int>();
	private List<int> killQuestList = new List<int>();
	private List<int> gatherQuestList = new List<int>();
	private List<string> questStringList = new List<string>();

	private GameObject questTextHolder;
	private GameObject runeChild;
	private GameObject runeLight;
	private GameObject parentIsland;
	private GameObject playerHolder;
	private GameObject playerHUD;
	private GameObject runeIcon;
	public GameObject runeParticleHolder;

	public ParticleSystem runeParticle;

	private float invulnerableTime = 0.5f;

	private int dmgLimit;
	private int dmgDir;
	private int killPos;
	private int gatherPos;
	private int dmgLimitPos;

	private string dmgDirText;

	private bool complete;
	private bool failed;
	private bool invulnerable;

	// Use this for initialization
	void Start () {
		runeIcon = GameObject.FindGameObjectWithTag("RuneIcon");

		runeChild = transform.GetChild(0).GetChild(0).gameObject;
		runeLight = transform.GetChild(1).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//Spawns a new GameObject with a particle system component attached to it, according to the parameters.
	void CreateParticle(GameObject newParticle){
		Instantiate(newParticle, runeParticle.transform.position, newParticle.transform.rotation);
	}

	//Determines course of action when hit by the player character.
	public void HitByPWeapon(int attack){
		if(attack != 0 && !invulnerable){
			GetComponent<QuestSound>().PlayHitSound(0);
			StartCoroutine(InvulnerableWait());
			CreateParticle(runeParticleHolder);
			questTextHolder.GetComponent<QuestText>().ChangeQuestTextState(true, "QuestAdd");
		}
	}

	//Makes the dynamic quest beacon invulnerable for a set time.
	IEnumerator InvulnerableWait(){
		invulnerable = true;
		yield return new WaitForSeconds(invulnerableTime);
		invulnerable = false;
	}

	//Initializes the quest algorithm, this is called when the island has spawned.
	public void InitializeQuestAlgorithm(){
		playerHUD = GameObject.FindGameObjectWithTag("playerHUD");
		playerHolder = GameObject.FindGameObjectWithTag("Player");
		parentIsland = transform.parent.parent.parent.gameObject;
		DetermineGoal();
		DetermineCriteria();
	}

	//Determines the goal for the dymanic quest, depending on the current situation on the island.
	void DetermineGoal(){
		for(int listNr = 0; listNr<2; listNr++){
			killQuestList.Add(0);
		}
		for(int listNr = 0; listNr<2; listNr++){
			gatherQuestList.Add(0);
		}
		questTextHolder = GameObject.FindGameObjectWithTag("QuestText");
		//GOALS:
		//Kill enemies
		//Collect Resources
		if(enemyList.Count > 0){
			killQuestList[0] = Random.Range(1, enemyList.Count);
		}
		if(resourceList.Count > 0){
			gatherQuestList[0] = Random.Range(1, resourceList.Count);
		}
	}

	//Shows the quest that has been determined or updated.
	public void ShowQuest(string anim){
		if(!complete && !failed){
			string questText = "";
			for(int listPos = 0; listPos<questStringList.Count; listPos++){
				questText += questStringList[listPos];
			}
			questTextHolder.GetComponent<QuestText>().SetQuestText(questText, anim);
		}else if(complete){
			ShowCompleteText();
		}else{
			ShowFailedText();
		}
	}

	//Hides the quest.
	public void HideQuest(){
		questTextHolder.GetComponent<QuestText>().ChangeQuestTextState(false, "QuestAdd");
	}

	//Determines the sub criteria for the quest on the island.
	void DetermineCriteria(){
		//CRITERAI:
		if(killQuestList[0] > 0 && gatherQuestList[0] <= 0){
			AddKillText("KILL:  ");
			//Take less/more than X DMG
			DetermineDMGLimit();
			//Use/don't use "Y MOVE"
			//Within/after X time has passed
		}else if(gatherQuestList[0] > 0 && killQuestList[0] <= 0){
			AddGatherText("GATHER:  ");
			//Within/after X time has passed
		}else{
			AddKillText("KILL:  ");
			AddGatherText("\nGATHER:  ");
			//Kill/Collect Z before/after Y
			//Take less/more than X DMG
			DetermineDMGLimit();
			//Use/don't use "Y MOVE"
			//Within/after X time has passed
		}
	}

	//Adds the quest text regarding the kill requirement.
	void AddKillText(string newText){
		questStringList.Add(newText);
		questStringList.Add(killQuestList[0].ToString());
		killPos = questStringList.Count-1;
	}
	
	//Adds the quest text regarding the gather requirement.
	void AddGatherText(string newText){
		questStringList.Add(newText);
		questStringList.Add(gatherQuestList[0].ToString());
		gatherPos = questStringList.Count-1;
	}
	
	//Determines the limit for the damage requirement.
	void DetermineDMGLimit(){
		//DMG LIMIT
		dmgDir = Random.Range(0, 2);

		if(dmgDir != 0){
			List<GameObject> armorList = playerHUD.GetComponent<PlayerHud>().GetArmorHUDList();
			int randDMG = Random.Range(1, armorList.Count-1);
			dmgLimit = randDMG;

			questStringList.Add("\nDON'T LOSE HEALTH SHIELDS:  ");
			questStringList.Add(dmgLimit.ToString());
			dmgLimitPos = questStringList.Count-1;
		}
	}
	
	//Checks the completion of the dynamic quest.
	void CheckCompletion(){
		if(killQuestList[0] > 0 || gatherQuestList[0] > 0){
			CreateParticle(runeParticleHolder);
			ShowQuest("QuestAdd");
		}else{
			QuestComplete();
		}
	}

	//Checks the health criteria for the quest.
	public void CheckHealthCriteria(){
		if(!complete && !failed){
			//CHECK HEALTH
			if(dmgLimit > 0){
				dmgLimit --;
				questStringList[dmgLimitPos] = dmgLimit.ToString();
				ShowQuest("QuestAdd");
				if(dmgDir == 1 && dmgLimit <= 0){
					QuestFailed();
				}
			}
		}
	}
	
	//Shows the quest complete text.
	void ShowCompleteText(){
		questTextHolder.GetComponent<QuestText>().SetQuestText("QUEST COMPLETE!", "QuestAdd");
	}
	
	//Shows the quest failed text.
	void ShowFailedText(){
		questTextHolder.GetComponent<QuestText>().SetQuestText("QUEST FAILED.", "QuestAdd");
	}

	//Completes the quest.
	void QuestComplete(){
		runeIcon.GetComponent<RuneIcon>().SetCompleteSprite();
		GetComponent<QuestSound>().PlayQuestSound(0);
		runeParticle.loop = true;
		runeParticle.maxParticles = 1000;
		runeParticle.Play();
		Color newColor = new Color(5, 5, 5);
		runeChild.GetComponent<MeshRenderer>().GetComponent<Renderer>().material.color *= newColor;
		runeLight.SetActive(true);
		ShowCompleteText();
		parentIsland.GetComponent<IslandSystem>().IslandCompleted();
		complete = true;
	}

	//Fails the quest.
	void QuestFailed(){
		runeIcon.GetComponent<RuneIcon>().SetFailSprite();
		GetComponent<QuestSound>().PlayQuestSound(1);
		Color newColor = new Color(0.5f, 0.5f, 0.5f);
		runeChild.GetComponent<MeshRenderer>().GetComponent<Renderer>().material.color *= newColor;
		ShowFailedText();
		parentIsland.GetComponent<IslandSystem>().IslandFailed();
		failed = true;
	}

	//Removes an enemy from the quest tracker when killed.
	public void RemoveEnemy(){
		if(!complete && !failed){
			if(killQuestList[0] > 0){
				killQuestList[0] --;
				questStringList[killPos] = killQuestList[0].ToString();
				CheckCompletion();
			}
		}
	}

	//Removes a resource from the quest tracker when gathered.
	public void RemoveResource(){
		if(!complete && !failed){
			if(gatherQuestList[0] > 0){
				gatherQuestList[0] --;
				questStringList[gatherPos] = gatherQuestList[0].ToString();
				CheckCompletion();
			}
		}
	}
	
	//Adds an enemy to the quest tracker.
	public void AddEnemy(GameObject newEnemy){
		enemyList.Add(newEnemy);
		newEnemy.GetComponent<LandEnemyCombat>().AddQuestTracker(gameObject);
	}

	//Adds a resource to the quest tracker.
	public void AddResource(GameObject newResource){
		resourceList.Add(newResource);
		newResource.transform.GetComponent<ResourceObject>().AddQuestTracker(gameObject);
	}
}
