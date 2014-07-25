using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	Difficulty difficulty;
	public bool isEasy;
	public bool isNormal;
	public bool isHard;

	void OnGUI() 
	{
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,(3*UnityEngine.Screen.height)/4,125,80),"<size=26>Phantom</size>"))
			Application.LoadLevel("phantomGrab");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,(3*UnityEngine.Screen.height)/4,125,80),"<size=26>Phantom\nPractice</size>"))
			Application.LoadLevel("phantomTutorial");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,(UnityEngine.Screen.height)/4,125,80),"<size=26>Minichair</size>"))
			Application.LoadLevel("optiChair");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,(UnityEngine.Screen.height)/4,125,80),"<size=26>Minichair\nPractice</size>"))
			Application.LoadLevel("optiChairTutorial");
		if (GUI.Button(new Rect(UnityEngine.Screen.width - 110,10,100,70),"<size=28>Quit</size>"))
			Application.Quit();
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,(UnityEngine.Screen.height)/2,125,80),"<size=26>Fingers</size>"))
			Application.LoadLevel("optiHand");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,(UnityEngine.Screen.height)/2,125,80),"<size=26>Fingers\nPractice</size>"))
			Application.LoadLevel("optiHandTut");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/4,10,125,80),"<size=26>AirPen</size>"))
			Application.LoadLevel("optiAirPen");
		if (GUI.Button(new Rect((UnityEngine.Screen.width)/2,10,125,80),"<size=26>AirPen\nPractice</size>"))
			Application.LoadLevel("optiAirPenTutorial");
			

		/*GUI.Label(new Rect((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2 - 100, 150, 50), "<size=20>Difficulty Level</size>");
		if (GUI.Toggle (new Rect ((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2 + 50, 100, 50), isEasy, "<size=20>Easy</size>"))
			isEasy = SetMeOnly ();

		if (GUI.Toggle (new Rect ((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2, 100, 50), isNormal, "<size=20>Normal</size>"))
			isNormal = SetMeOnly ();

		if (GUI.Toggle (new Rect ((3 * UnityEngine.Screen.width) / 4, (UnityEngine.Screen.height) / 2 - 50, 150, 50), isHard, "<size=20>Hard</size>"))
			isHard = SetMeOnly ();*/
	}

	void Awake()
	{
		difficulty = Difficulty.Instance;
		isEasy = false;
		isNormal = false;
		isHard = false;

		if(difficulty.angle == 20f)
			isEasy = true;
		else if (difficulty.angle == 15f)
			isNormal = true;
		else
			isHard = true;
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
		if(isEasy)
		{
			difficulty.setEasy();
		}
		else if(isNormal)
		{
			difficulty.setNormal();
		}
		else if(isHard)
		{
			difficulty.setHard();
		}
	}

	bool SetMeOnly()
	{
		isEasy = false;
		isNormal = false;
		isHard = false;

		return true;
	}
}
