using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Game : MonoBehaviour 
{
	public GameObject cursor;
	public GameObject target;
	public GameObject pointer;
	public Material green;
	public Material yellow;
	public Material red;
	public Material transGreen;
	public Material transYellow;
	public Light roomLight;
	public GUIText pointText;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource bassSource;
	public AudioSource drumsSource;
	public Camera secondCamera;
	public GameObject dummy;//the target changes orientation
	public GameObject targetSphere;
	public GUIText instructionsText;
	public GameObject posCube;
	public Material trueGreen;

	protected static float xMax = 15.0f;
	protected static float yMax = 15.0f;
	protected static float zMax = 15.0f;
	protected static float yMin = 4.0f;
	protected static float zMin = -12.5f;

	protected bool isDocked = false;
	protected int score = 0;
	protected string connectionMessage="not connected";
	protected string message=" ";
	protected string info="";
	protected float prevTime=0;
	protected float prevTotalTime;
	protected float clutchTime=0;
	protected float prevClutchTime=0;
	protected Difficulty difficulty;
	protected Vector3 prevPos= new Vector3 ();
	protected float distance = 0;
	protected float angle = 0;
	protected bool window = false;
	protected bool skipWindow = false;
	protected Folders folders;
	protected string nextLevel = "MainMenu";
	protected bool action = false;
	float maxTime = 60f; //max time before the trial is skipped
	protected Type trialsType;
	float minDistance = 5f; //min distance between target and cursor
	protected int clutchCn=0;
	float initDistance = 0;
	float initAngle = 0;
	protected string interaction = "";
	Quaternion initTarget;
	Vector3 camPosL 
		= new Vector3(-15f,8.5f,0f);
	Vector3 camPosR = new Vector3(15f,8.5f,0f);
	int cntTab=0;
	protected int [] rotCntI = new int[3];
	protected int [] rotCntChair = new int[3];
	float soundAngle = 25f;
	float soundDistance = 3f;
	protected bool confirm = false;
	float[] accuracyTimes = new float[3];

	void OnGUI ()
	{
		GUI.Box (new Rect (0,0,265,100), "<size=36>"+ message + "</size>");//+ "\n" +"Level: "+difficulty.getLevel ()
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 200,0,200,100), "<size=36>Trial: " + score +"/"+trialsType.getTrialNum()+
		         "\nTime: " + (int)(Time.time - prevTotalTime)+"</size>");
		if (window)
			GUI.Window(0, new Rect((UnityEngine.Screen.width*0.5f)-105, (UnityEngine.Screen.height*0.5f)-50, 210, 100), DoWindow, "<size=28>Complete</size>");
		if (skipWindow)
			GUI.Window(1, new Rect((UnityEngine.Screen.width*0.5f)-105, (UnityEngine.Screen.height*0.5f)-50, 210, 100), DoWindow, "<size=28>Skip this trial</size>");
	}
	
	void Awake ()
	{
		folders = Folders.Instance;
		difficulty = Difficulty.Instance;
		trialsType = Type.Instance;
		UnityEngine.Screen.showCursor = false;
		soundAngle = difficulty.angle;
		soundDistance = difficulty.distance;

		if(trialsType.mute)
		{
			bassSource.volume = 0f;
			pointer.GetComponent<AudioSource>().volume = 0f;
		}

		atAwake ();
	}
	
	void Start () 
	{
		prevTotalTime = Time.time;
		atStart ();
	}
	
	void Update () 
	{
		if (Input.GetKeyUp (KeyCode.Escape))
				Application.LoadLevel ("MainMenu");

		if (Input.GetKeyUp (KeyCode.P)) 
		{
				Application.CaptureScreenshot (folders.getPath () + System.DateTime.Now.ToString ("MM-dd-yy_hh-mm-ss") + "_Screenshot.png");
		}

		if (window && (Input.GetKeyUp (KeyCode.Return) || Input.GetKeyUp (KeyCode.KeypadEnter)) )
			Application.LoadLevel (nextLevel);
		if (skipWindow && (Input.GetKeyUp (KeyCode.Return) || Input.GetKeyUp (KeyCode.KeypadEnter)) ) 
		{
			setNewPositionAndOrientation();
			skipWindow = false;
			prevTotalTime = Time.time;
		}


		if(Input.GetKeyUp (KeyCode.Tab))
		{
			trialsType.updateCam = !trialsType.updateCam;
			cntTab++;
		}

		if(Input.GetKeyUp (KeyCode.M))
		{
			if(!trialsType.mute)
			{
				trialsType.mute = true;
				bassSource.volume = 0f;
				drumsSource.volume = 0f;
			}
			else
				trialsType.mute = false;
		}

		float tmpTime = Time.time - prevTotalTime;
		if (Input.GetKeyUp (KeyCode.S))
		{

			prevTotalTime = Time.time;
			skipWindow = false;
			File.AppendAllText(folders.getPath()+"Skip.csv", tmpTime.ToString()+","+difficulty.getLevel()+ ","+"0"+","+"1"+
			                   ","+initDistance.ToString()+","+initAngle.ToString()+","+clutchTime.ToString()+","+clutchCn.ToString()+
			                   ","+initTarget.x+","+initTarget.y+","+initTarget.z+","+initTarget.w+","+interaction+Environment.NewLine);//save to file
			if(trialsType.getType() == "Trials")
				setNewPositionAndOrientation();
			else
				setNewPositionAndOrientationTut();
		}

		if( (Input.GetKeyUp (KeyCode.Z) || Input.GetKeyUp (KeyCode.Slash) || Input.GetKeyUp (KeyCode.Space)) && isDocked)
		{
			confirm = true;
		}

		if (instructionsText.enabled) 
		{
			if(action)
			{
				instructionsText.enabled = false;
				prevTotalTime = Time.time;
			}
		}

		if (pointText.enabled) 
		{
			if( (int)(Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}

		if (window || skipWindow)
			UnityEngine.Screen.showCursor = true;
		else
		{
			UnityEngine.Screen.showCursor = false;
			gameBehavior ();
		}

		tmpTime = Time.time - prevTotalTime;
		if(tmpTime > maxTime && !window && !instructionsText.enabled)
		{
			if(!skipWindow)
			{
				prevTotalTime = Time.time;
				File.AppendAllText(folders.getPath()+"Skip.csv", tmpTime.ToString()+","+difficulty.getLevel()+ ","+"1"+","+"0"+
				                   ","+initDistance.ToString()+","+initAngle.ToString()+","+clutchTime.ToString()+","+clutchCn.ToString()+
				                   ","+initTarget.x+","+initTarget.y+","+initTarget.z+","+initTarget.w+","+interaction+Environment.NewLine);//save to file
				skipWindow = true;
			}
		}

		evaluateDock ();

		//"Time,Distance,Angle,Difficulty,trialNum, group, type,interaction";
		if(action || interaction=="MiniChair")
			File.AppendAllText(folders.getPath()+"Raw"+".csv", tmpTime.ToString()+","+distance.ToString()+","+angle.ToString()+ 
		                   ","+difficulty.getLevel()+ ","+score.ToString()+","+trialsType.currentGroup.ToString()+
			                   ","+trialsType.getType()+","+action.ToString()+","+interaction+Environment.NewLine);//save to file

	}

	void LateUpdate()
	{
		if (trialsType.updateCam) 
		{
			secondCamera.transform.position = camPosR;
			secondCamera.rect = new Rect (0.75f, 0, 0.25f, 0.25f);
		}
		else
		{
			secondCamera.transform.position = camPosL;
			secondCamera.rect = new Rect (0, 0, 0.25f, 0.25f);
		}
		secondCamera.transform.LookAt(target.transform.position);
	}

	void OnDestroy () 
	{
		transGreen.color = trueGreen.color;
		green.color = trueGreen.color;
		atEnd ();
	}
	
	protected void setNewPositionAndOrientation()
	{
		setRotation ();
		setPosition ();
		resetVariables ();
	}

	protected void setNewPositionAndOrientationTut()
	{
		switch (score)
		{
			//learn to translate
			case 0:
				target.transform.rotation = new Quaternion();
				break;
			//learn to rotate around y
			case 1:
				target.transform.rotation = new Quaternion();
				target.transform.Rotate(Vector3.up * 45f, Space.World);
				break;
			//learn to rotate around x
			case 2:
				target.transform.rotation = new Quaternion();
				target.transform.Rotate(Vector3.right * -45f, Space.World);
				break;
			//learn to rotate around z
			case 3:
				target.transform.rotation = new Quaternion();
				target.transform.Rotate(Vector3.forward * 45f, Space.World);
				break;
			//practise docking
			default:
				setRotation ();
				break;
		}
		initTarget = target.transform.rotation;
		initAngle = Quaternion.Angle(cursor.transform.rotation, initTarget);
		setPosition ();
		resetVariables ();
	}

	void evaluateDock ()
	{
		Quaternion targetQ = target.transform.rotation;
		Quaternion cursorQ = cursor.transform.rotation;
		Vector3 targetV = target.transform.position;
		Vector3 cursorV = cursor.transform.position;
		distance = Vector3.Distance (targetV, cursorV);
		angle = Quaternion.Angle(cursorQ, targetQ);
		float distRatio = 1f - distance / difficulty.distance;
		float angleRatio = 1f - angle/difficulty.angle;

		if(!trialsType.mute)
		{
			if(angle > soundAngle)
				bassSource.volume = 0;
			else
				bassSource.volume = 1f-(angle / soundAngle);
			if(distance > soundDistance)
				drumsSource.volume = 0;
			else
				drumsSource.volume = 1f-(distance / soundDistance);
		}
		
		if ((angle <= difficulty.angle) && (distance < difficulty.distance)) 
		{	
			isDocked = true;
			float intense = (distRatio*0.5f + angleRatio*0.5f)*4f+1f;
			roomLight.intensity = intense;//4.0f;
			//Debug.Log(intense + " "+distRatio+ " "+angleRatio);
			message= "Target docked!";
		}
		else
		{
			isDocked=false;
			roomLight.intensity = 1.0f;
			message= " ";
		}


		if (angle <= difficulty.angle)
		{	
			green.color = new Vector4(0, angleRatio, 0, 1);
			cursor.renderer.material = green;
			targetSphere.renderer.material = green;
		}
		else
		{
			cursor.renderer.material = yellow;
			targetSphere.renderer.material = transYellow;
		}

		if (distance <= difficulty.distance)
		{	
			transGreen.color = new Vector4(0, distRatio, 0, 1);
			posCube.renderer.material = transGreen;
		}
		else
		{
			posCube.renderer.material = transYellow;
		}

		if ( (angle > difficulty.angles [0]) || (distance > difficulty.distances[0]) )
		{
			accuracyTimes = new float[3];
		}
		else
		{
			if(accuracyTimes[0] == 0)
				accuracyTimes[0] = Time.time - prevTotalTime;
			if( (angle < difficulty.angles [2]) && (distance < difficulty.distances[2]))
			{
				if(accuracyTimes[2] == 0)
					accuracyTimes[2] = Time.time - prevTotalTime;
			}
			else if ( (angle < difficulty.angles [1]) && (distance < difficulty.distances[1]) )
			{
				accuracyTimes[2] = 0;
				if(accuracyTimes[1] == 0)
					accuracyTimes[1] = Time.time - prevTotalTime;
			}
			else
				accuracyTimes[1] = 0;

		}
	}

	protected void newTask()
	{
		float tmpTime = Time.time - prevTotalTime;
		popSource.PlayOneShot(popSound);
		prevTotalTime = Time.time;
		pointText.enabled = true;
		File.AppendAllText(folders.getPath()+trialsType.getType()+".csv", tmpTime.ToString()+","+distance.ToString()+","+angle.ToString()+ 
		                   ","+difficulty.getLevel()+ ","+initDistance.ToString()+","+initAngle.ToString()+","+clutchTime.ToString()
		                   +","+clutchCn.ToString()+","+cntTab.ToString()+","+rotCntI[0].ToString()+","+rotCntI[1].ToString()+","+rotCntI[2].ToString()+
		                   ","+rotCntChair[0].ToString()+","+rotCntChair[1].ToString()+","+rotCntChair[2].ToString()+
		                   ","+accuracyTimes[0].ToString()+","+accuracyTimes[1].ToString()+","+accuracyTimes[2].ToString()+
		                   ","+interaction+Environment.NewLine);//save to file
		score++;
	}

	protected void selectLevel()
	{
		switch (score)
		{
			/*case 0:
				difficulty.setEasy ();
				break;
			case 1:
				difficulty.setNormal ();
				break;
			case 2:
				difficulty.setHard ();
				break;
			case 3:
				difficulty.setNormal ();
				break;
			case 4:
				difficulty.setEasy ();
				break;
			case 5:
				difficulty.setHard ();
				break;
			case 6:
				difficulty.setNormal ();
				break;
			case 7:
				difficulty.setHard ();
				break;
			case 8:
				difficulty.setEasy ();
				break;*/
			default:
				difficulty.setEasy ();
				break;
		}
	}
	void DoWindow(int windowID)
	{
		if(windowID == 0)
		{
			if (GUI.Button(new Rect(50, 45, 95, 30), "<size=20>Next</size>"))
			{
				Application.LoadLevel(nextLevel);
			}
		}
		else
		{
			if (GUI.Button(new Rect(50, 45, 95, 30), "<size=20>Skip</size>"))
			{
				setNewPositionAndOrientation();
				skipWindow = false;
				prevTotalTime = Time.time;
			}
		}
	}

	void setRotation ()
	{ 
		float angle = 0f;
		do
		{
			target.transform.rotation = UnityEngine.Random.rotationUniform;
			angle = Vector3.Angle(target.transform.up, Vector3.down);
		}while(angle < 75f);
		initTarget = target.transform.rotation;
		initAngle = Quaternion.Angle(cursor.transform.rotation, initTarget);
	}

	void setPosition ()
	{ 
		Vector3 position = new Vector3();
		do
		{
			position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax),
	                                 UnityEngine.Random.Range(yMin, yMax),
	                                 UnityEngine.Random.Range(zMin, zMax));
		}while(Vector3.Distance(position, target.transform.position) < minDistance);
		cursor.transform.position = position;
		initDistance = Vector3.Distance (target.transform.position, cursor.transform.position);
		
	}

	protected virtual void gameBehavior ()
	{
		
	}

	protected virtual void atAwake ()
	{
		
	}

	protected virtual void atStart ()
	{
		
	}

	protected virtual void atEnd ()
	{
		
	}

	protected void dominantAxis(Vector3 rotAngles, int [] counter)
	{
		if (rotAngles.x > rotAngles.y && rotAngles.x > rotAngles.z)
			counter[0]++;
		else if(rotAngles.y > rotAngles.z)
			counter[1]++;
		else
			counter[2]++;
	}

	void resetVariables()
	{
		clutchTime = 0;
		clutchCn = 0;
		cntTab = 0;
		rotCntI = new int[3];
		rotCntChair = new int[3];
		confirm = false;
		accuracyTimes = new float[3];		
	}
}

