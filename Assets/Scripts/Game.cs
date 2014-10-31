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
	public AudioSource ambientSource;
	public Camera secondCamera;
	public GameObject dummy;//the target changes orientation
	public GameObject targetSphere;
	public GUIText instructionsText;
	public GameObject posCube;

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
	float maxTime = 20f; //max time before the trial is skipped
	protected bool updateCam = true;
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
	
	void OnGUI ()
	{
		GUI.Box (new Rect (0,0,265,100), "<size=36>"+ message + "</size>");//+ "\n" +"Level: "+difficulty.getLevel ()
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 200,0,200,100), "<size=36>Trial: " + score +"/"+trialsType.getTrialNum()+
		         "\nTime: " + (int)(Time.time - prevTotalTime)+"</size>");
		//GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 36), "<size=18>"+connectionMessage+"</size>");
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

		if(trialsType.mute)
		{
			ambientSource.volume = 0f;
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
		if (Input.GetKeyUp(KeyCode.Escape))
			Application.LoadLevel("MainMenu");

		if(Input.GetKeyUp (KeyCode.P))
		{
			Application.CaptureScreenshot(folders.getPath()+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Screenshot.png");
		}

		/*if(Input.GetKeyUp (KeyCode.F))
		{
			updateCam = !updateCam;
		}*/

		if(Input.GetKeyUp (KeyCode.Tab))
		{
			updateCam = !updateCam;
			cntTab++;
		}

		if(Input.GetKeyUp (KeyCode.M))
		{
			if(!trialsType.mute)
			{
				trialsType.mute = true;
				ambientSource.volume = 0f;
				pointer.GetComponent<AudioSource>().volume = 0f;
			}
			else
				trialsType.mute = false;
		}

		if (Input.GetKeyUp (KeyCode.S))
		{
			float tmpTime = Time.time - prevTotalTime;
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

		if((Time.time - prevTotalTime) > maxTime && !window && !instructionsText.enabled)
		{
			if(!skipWindow)
			{
				float tmpTime = Time.time - prevTotalTime;
				prevTotalTime = Time.time;
				File.AppendAllText(folders.getPath()+"Skip.csv", tmpTime.ToString()+","+difficulty.getLevel()+ ","+"1"+","+"0"+
				                   ","+initDistance.ToString()+","+initAngle.ToString()+","+clutchTime.ToString()+","+clutchCn.ToString()+
				                   ","+initTarget.x+","+initTarget.y+","+initTarget.z+","+initTarget.w+","+interaction+Environment.NewLine);//save to file
				skipWindow = true;
			}
		}

		evaluateDock ();
	}

	void LateUpdate()
	{
		if (updateCam) 
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

		/*if (updateCam)
		{
			Vector3 camPos = new Vector3();
			Vector3 axisVec = dummy.transform.TransformDirection (Vector3.forward);
			Vector3 pointerDir = target.transform.position - pointer.transform.position;
			pointerDir.Normalize();
			float angle = Vector3.Angle(axisVec, pointerDir);

			if(angle <= 45f)
			{
				camPos = Vector3.forward;
				camPos = camPos*-15f;
				camPos.y = 10f;
			}
			else
			{
				axisVec = dummy.transform.TransformDirection (Vector3.right);
				angle = Vector3.Angle(axisVec, pointerDir);
				if(angle <= 45f)
				{
					camPos = Vector3.right;
					camPos = camPos*-15f;
					camPos.y = 10f;
				}
				else
				{
					axisVec = dummy.transform.TransformDirection (Vector3.left);
					angle = Vector3.Angle(axisVec, pointerDir);
					if(angle <= 45f)
					{
						camPos = Vector3.left;
						camPos = camPos*-15f;
						camPos.y = 10f;
					}
					else
					{
						axisVec = dummy.transform.TransformDirection (Vector3.back);
						angle = Vector3.Angle(axisVec, pointerDir);
						if(angle <= 45f)
						{
							camPos = Vector3.back;
							camPos = camPos*-15f;
							camPos.y = 10f;
						}
						else
						{
							axisVec = dummy.transform.TransformDirection (Vector3.down);
							angle = Vector3.Angle(axisVec, pointerDir);
							if(angle <= 45f)
							{
								camPos = Vector3.down;
								camPos = camPos*-15f;
							}
							else
							{
								axisVec = dummy.transform.TransformDirection (Vector3.up);
								angle = Vector3.Angle(axisVec, pointerDir);
								camPos = Vector3.up;
							}
						}
					}
				}
			}

			secondCamera.transform.position = camPos;
			secondCamera.transform.LookAt(target.transform.position);
		}*/
	}

	void OnDestroy () 
	{
		atEnd ();
	}
	
	protected void setNewPositionAndOrientation()
	{
		setRotation ();
		setPosition ();
		clutchTime = 0;
		clutchCn = 0;
		cntTab = 0;
		updateCam = true;
		rotCntI = new int[3];
		rotCntChair = new int[3];
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
	}

	void evaluateDock ()
	{
		Quaternion targetQ = target.transform.rotation;
		Quaternion cursorQ = cursor.transform.rotation;
		Vector3 targetV = target.transform.position;
		Vector3 cursorV = cursor.transform.position;
		distance = Vector3.Distance (targetV, cursorV);
		angle = Quaternion.Angle(cursorQ, targetQ);
		if(!trialsType.mute)
		{
			if(angle > 40f)
				ambientSource.volume = 0;
			else
				ambientSource.volume = 1f-(angle / 40f);
		}
		
		if ((angle <= difficulty.angle) && (distance < difficulty.distance)) 
		{	
			isDocked = true;
			roomLight.intensity = 4.0f;
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
			cursor.renderer.material = green;
			targetSphere.renderer.material = transGreen;
		}
		else
		{
			cursor.renderer.material = yellow;
			targetSphere.renderer.material = transYellow;
		}

		if (distance <= difficulty.distance)
		{	
			posCube.renderer.material = transGreen;
		}
		else
		{
			posCube.renderer.material = transYellow;
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
		                   ","+interaction+Environment.NewLine);//save to file
		score++;
	}

	protected void selectLevel()
	{
		switch (score)
		{
			case 0:
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
				break;
			default:
				difficulty.setEasy ();
				break;
		}
		targetSphere.transform.localScale = Vector3.one;
		targetSphere.transform.localScale *= difficulty.distance+0.5f;
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
		}while(angle < 60f);
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
}

