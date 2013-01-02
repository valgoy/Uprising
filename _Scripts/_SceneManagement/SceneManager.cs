using Holoville.HOTween;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour {
	
	public static int IDServer = 0;
	
	private MapManager C_mapManager;
	
	public static bool isServer = true;
	public static bool isSingleplayer = true;
	public static bool isNetworking = false;
	public static bool isReady4Map = false;
	public static bool isLogVisible = false;
	
    private static SceneManager _instance;
    
    public static SceneManager Instance {
		get {
			if (_instance == null) {
				_instance = GameObject.Find("_SceneManager").GetComponent<SceneManager>();
			}
			return _instance;
		}
	}
	
	public static void setNetworking(bool Pb_networking) {
		isSingleplayer = !Pb_networking;
		isNetworking = Pb_networking;
	}

	
	void OnApplicationQuit() {
		_instance = null;
	}
    	
	// Use this for initialization
	void Start () {
	
		HOTween.Init( true);
		_instance = this;
	    Application.RegisterLogCallback(HandleLog);
	    
		initScene();
	 }
	 
	 public void initScene() {
	    StartCoroutine(waitAndStartMap());
	 }
	  
	 protected IEnumerator waitAndStartMap() {
	 
		while (Application.isLoadingLevel) {
			Debug.Log("waiting for level");
			yield return 0f;
		}
		
		
		C_mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
		C_mapManager.initializeBeforeStart();
		
		if (isSingleplayer) {
			C_mapManager.startMap();
			
			PlayerLife F_gamer = null;
			if (F_gamer == null || !isSingleplayer) {
				F_gamer = PrefabManager.CreateLife("Player", Vector3.zero, Quaternion.identity, PrefabMode.Everyone, null) as PlayerLife;			
			}
			
			C_mapManager.setPlayerMain(F_gamer);
			SpawnManager.Instance.SpawnLifeform(F_gamer);
			
		}
		isReady4Map = true;
	 }
	 
	 public void initPlayer(PlayerLife P_mainPlayer) {
		if (!isReady4Map) {
			
			C_mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
			C_mapManager.initializeBeforeStart();
			C_mapManager.startMap();
			isReady4Map = true;
		} 
		P_mainPlayer.StartLifeform();
		C_mapManager.setPlayerMain(P_mainPlayer);
		
	 }
	 
	 void Awake() {
		
	 }
    
	public void whatToDoWithMeAfterKilled(PlayerLife P_obj) {
		SpawnManager.Instance.SpawnLifeform(P_obj);
	}
	
	
    void HandleLog(string logString, string stackTrace, LogType type) {
		if (logString.StartsWith(">>")) { //solche einträge ignorieren wir mal
			return;
		}
	    myLog +="\n"+logString;
	}
	 
    private string myLog = "";
	
    private void OnGUI() {
		if (isLogVisible) {
			GUI.TextArea (new Rect (0, 30, 500, 600), myLog);
			if (Input.GetKeyUp(KeyCode.K)) {
				isLogVisible = !isLogVisible;
			}
		} else {
			if (Input.GetKeyUp(KeyCode.L)) {
				isLogVisible = !isLogVisible;
			}
		}
	}
	
}
