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
	public Light roomLight;
	public GUIText pointText;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;
	public Camera secondCamera;
	public GameObject dummy;//the target changes orientation

	protected static float xMax = 15.0f;
	protected static float yMax = 15.0f;
	protected static float zMax = 15.0f;
	
	protected bool isDocked = false;
	protected int score = 0;
	protected string connectionMessage="not connected";
	protected string message="";
	protected string info="";
	protected float prevTime=0;
	protected float prevTotalTime;
	protected string path;
	protected Difficulty difficulty;
	protected Vector3 prevPos= new Vector3 ();
	bool mute = false;
	protected float distance = 0;
	protected float angle = 0;
	public bool window = false;
	protected Folders folders;
	protected string nextLevel = "MainMenu";

	void OnGUI ()
	{
		GUI.Box (new Rect (0,0,150,70), "<size=20>"+info + "\n" + message + "\n" +difficulty.getLevel () + "</size>");
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 120,0,120,80), "<size=20>Score: " + score +
		         "\nTime: " + (int)(Time.time - prevTotalTime) +"\nPrev: " + ((int)prevTime).ToString()+"</size>");
		GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 30), "<size=18>"+connectionMessage+"</size>");
		if (window)
			GUI.Window(0, new Rect(UnityEngine.Screen.height*0.5f, UnityEngine.Screen.height*0.5f, 210, 80), DoWindow, "<size=18>Complete</size>");
	}
	
	void Awake ()
	{
		folders = Folders.Instance;
		difficulty = Difficulty.Instance;
		UnityEngine.Screen.showCursor = false;
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
		
		if(Input.GetKeyUp (KeyCode.M))
		{
			if(!mute)
			{
				mute = true;
				ambientSource.volume = 0f;
				pointer.GetComponent<AudioSource>().volume = 0f;
			}
			else
				mute = false;
		}
		
		if (pointText.enabled) 
		{
			if( (int)(Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}

		gameBehavior ();

		evaluateDock ();
	}

	void LateUpdate()
	{
		Vector3 camPos = new Vector3();
		Vector3 axisVec = dummy.transform.TransformDirection (Vector3.forward);
		Vector3 pointerDir = target.transform.position - pointer.transform.position;
		pointerDir.Normalize();
		float angle = Vector3.Angle(axisVec, pointerDir);

//		Debug.Log (pointerDir+" "+angle+" "+axisVec);

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

	void OnDestroy () 
	{
		atEnd ();
	}
	
	protected void setNewPositionAndOrientation()
	{
		target.transform.rotation = UnityEngine.Random.rotation;
		cursor.transform.position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax),
		                                         UnityEngine.Random.Range(4.0F, yMax),
		                                         UnityEngine.Random.Range(-zMax, zMax));
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
				target.transform.Rotate(Vector3.right * 45f, Space.World);
				break;
				//learn to rotate around z
			case 3:
				target.transform.rotation = new Quaternion();
				target.transform.Rotate(Vector3.forward * 45f, Space.World);
				break;
				//practise docking
			default:
				target.transform.rotation = UnityEngine.Random.rotation;
				break;
		}
		cursor.transform.position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax),
		                                         UnityEngine.Random.Range(4.0F, yMax),
		                                         UnityEngine.Random.Range(-zMax, zMax));
	}

	void evaluateDock ()
	{
		Quaternion targetQ = target.transform.rotation;
		Quaternion cursorQ = cursor.transform.rotation;
		Vector3 targetV = target.transform.position;
		Vector3 cursorV = cursor.transform.position;
		distance = (targetV - cursorV).magnitude;
		angle = Quaternion.Angle(cursorQ, targetQ);
		if(!mute)
			ambientSource.volume = (1f-(angle / 180f))*0.75f;
		
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
			message= "";
		}
		
		if (angle <= difficulty.angle)
		{	
			cursor.renderer.material = green;
		}
		else
		{
			cursor.renderer.material = yellow;
		}
	}

	protected void newTask()
	{
		popSource.PlayOneShot(popSound);
		prevTime = Time.time - prevTotalTime;
		prevTotalTime = Time.time;
		pointText.enabled = true;
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
		if (GUI.Button (new Rect (10, 35, 90, 30), "<size=16>Continue</size>"))
		{
			window = false;
		}
		else if (GUI.Button(new Rect(110, 35, 95, 30), "<size=16>Next</size>"))
		{
			Application.LoadLevel(nextLevel);
		}
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

