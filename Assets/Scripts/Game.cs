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
	
	protected bool isDocked = false;
	protected int score = 0;
	protected string connectionMessage="not connected";
	protected string message=" ";
	protected string info="";
	protected float prevTime=0;
	protected float prevTotalTime;
	protected string path;
	protected Difficulty difficulty;
	protected Vector3 prevPos= new Vector3 ();
	protected float distance = 0;
	protected float angle = 0;
	protected bool window = false;
	protected bool skipWindow = false;
	protected Folders folders;
	protected string nextLevel = "MainMenu";
	protected bool action = false;
	float maxTime = 60f;
	bool updateCam = true;
	protected Type trialsType;

	void OnGUI ()
	{
		GUI.Box (new Rect (0,0,265,100), "<size=36>"+ message + "\n" +"Level: "+difficulty.getLevel () + "</size>");
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 200,0,200,100), "<size=36>Trial: " + score +"/"+trialsType.getTrialNum()+
		         "\nTime: " + (int)(Time.time - prevTotalTime)+"</size>");// +"\nPrev: " + ((int)prevTime).ToString()+"</size>");
		GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 36), "<size=18>"+connectionMessage+"</size>");
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

		if(Input.GetKeyUp (KeyCode.F))
		{
			updateCam = !updateCam;
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

		if((Time.time - prevTotalTime) > maxTime && !window)
			skipWindow = true;


		evaluateDock ();
	}

	void LateUpdate()
	{
		if (updateCam)
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
		}
	}

	void OnDestroy () 
	{
		atEnd ();
	}
	
	protected void setNewPositionAndOrientation()
	{
		setRotation ();
		//target.transform.rotation = UnityEngine.Random.rotation;
		cursor.transform.position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax),
		                                         UnityEngine.Random.Range(4.0F, yMax),
		                                         UnityEngine.Random.Range(-zMax+2.5f, zMax));
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
				//target.transform.rotation = UnityEngine.Random.rotation;
				break;
		}

		cursor.transform.position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax),
		                                         UnityEngine.Random.Range(4.0F, yMax),
		                                         UnityEngine.Random.Range(-zMax+2.5f, zMax));
	}

	void evaluateDock ()
	{
		Quaternion targetQ = target.transform.rotation;
		Quaternion cursorQ = cursor.transform.rotation;
		Vector3 targetV = target.transform.position;
		Vector3 cursorV = cursor.transform.position;
		distance = (targetV - cursorV).magnitude;
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
		prevTime = tmpTime;
		File.AppendAllText(path, prevTime.ToString()+","+distance.ToString()+","+angle.ToString()+ ","+difficulty.getLevel()+Environment.NewLine);//save to file
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
	}
	void DoWindow(int windowID)
	{
		/*if (GUI.Button (new Rect (10, 40, 95, 30), "<size=20>Continue</size>"))
		{
			window = false;
			prevTotalTime = Time.time;
		}
		else if (GUI.Button(new Rect(110, 40, 95, 30), "<size=20>Next</size>"))
		{
			Application.LoadLevel(nextLevel);
		}*/
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
			target.transform.rotation = UnityEngine.Random.rotation;
			angle = Vector3.Angle(target.transform.up, Vector3.down);
		}while(angle < 60f);

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
}

