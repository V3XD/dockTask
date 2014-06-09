using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	Difficulty difficulty;
	public bool isNormal;
	public bool isHard;
	public bool isVeryHard;

	void OnGUI() 
	{
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,(3*UnityEngine.Screen.height)/4,100,50),"<size=20>Phantom</size>"))
			Application.LoadLevel("phantomGrab");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,(3*UnityEngine.Screen.height)/4,100,60),"<size=20>Phantom\nPractice</size>"))
			Application.LoadLevel("phantomTutorial");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,(UnityEngine.Screen.height)/4,100,50),"<size=20>Leap</size>"))
			Application.LoadLevel("leapCube");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,(UnityEngine.Screen.height)/4,100,60),"<size=20>Leap\nTutorial</size>"))
			Application.LoadLevel("leapTutorial");
		if (GUI.Button(new Rect(UnityEngine.Screen.width - 100,10,80,50),"<size=20>Quit</size>"))
			Application.Quit();
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,(UnityEngine.Screen.height)/2,100,50),"<size=20>Opti</size>"))
			Application.LoadLevel("optiTrack");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,(UnityEngine.Screen.height)/2,100,60),"<size=20>Opti\nTutorial</size>"))
			Application.LoadLevel("optiTrack");
			

		GUI.Label(new Rect((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2 - 100, 150, 50), "<size=20>Difficulty Level</size>");
		if (GUI.Toggle (new Rect ((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2 + 50, 100, 50), isNormal, "<size=20>Normal</size>"))
			isNormal = SetMeOnly ();

		if (GUI.Toggle (new Rect ((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2, 100, 50), isHard, "<size=20>Difficult</size>"))
			isHard = SetMeOnly ();

		if (GUI.Toggle (new Rect ((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2 - 50, 150, 50), isVeryHard, "<size=20>Very difficult</size>"))
			isVeryHard = SetMeOnly ();
	}

	void Awake()
	{
		difficulty = Difficulty.Instance;
		isNormal = false;
		isHard = false;
		isVeryHard = false;

		if(difficulty.angle == 20f)
			isNormal = true;
		else if (difficulty.angle == 15f)
			isHard = true;
		else
			isVeryHard = true;
	}

	void Start()
	{
		UnityEngine.Screen.showCursor = true; 
	}

	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
		else if(Input.GetKeyDown (KeyCode.P))
		{
			Application.CaptureScreenshot(@"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Screenshot.png");
			Debug.Log("print");
		}
		if(isNormal)
		{
			difficulty.setNormal();
		}
		else if(isHard)
		{
			difficulty.setDifficult();
		}
		else if(isVeryHard)
		{
			difficulty.setVeryDifficult();
		}
	}

	bool SetMeOnly()
	{
		isNormal = false;
		isHard = false;
		isVeryHard = false;

		return true;
	}
}
