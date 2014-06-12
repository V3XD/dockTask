using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

public class PhantomTutorial : MonoBehaviour {
	
	public GameObject cursor;
	public GameObject target;
	public Light roomLight;
	public GUIText pointText;
	public GameObject j2;
	public GameObject j3;
	public GameObject j4;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;
	public Material green;
	public Material yellow;
	
	private Vector3 prevPosition;
	private Vector3 prevOrient;
	protected bool grab;
	protected bool isDocked;
	protected bool isConnected;
	private int score;
	private string connectionMessage="not connected";
	private string message="";
	private string info="not grabbed";
	private float prevTime;
	protected float prevTotalTime;
	protected static float xMax = 12.0f;
	protected static float yMax = 12.0f;
	protected static float zMax = 12.0f;
	protected static float scale = 0.10f;
	Difficulty difficulty;
	
	[DllImport("phantomDll")]
	private static extern bool initDevice();
	[DllImport("phantomDll")]
	private static extern void cleanup();
	[DllImport("phantomDll")]
	private static extern bool getData();
	[DllImport("phantomDll")]
	private static extern double getPosX();
	[DllImport("phantomDll")]
	private static extern double getPosY();
	[DllImport("phantomDll")]
	private static extern double getPosZ();
	[DllImport("phantomDll")]
	private static extern double getQuatX();
	[DllImport("phantomDll")]
	private static extern double getQuatY();
	[DllImport("phantomDll")]
	private static extern double getQuatZ();
	[DllImport("phantomDll")]
	private static extern double getQuatW();
	[DllImport("phantomDll")]
	private static extern bool isButtonADown();
	[DllImport("phantomDll")]
	private static extern bool isButtonBDown();
	[DllImport("phantomDll")]
	private static extern double gimbalX();
	[DllImport("phantomDll")]
	private static extern double gimbalY();
	[DllImport("phantomDll")]
	private static extern double gimbalZ();
	[DllImport("phantomDll")]
	private static extern double jointX();
	[DllImport("phantomDll")]
	private static extern double jointY();
	[DllImport("phantomDll")]
	private static extern double jointZ();
	
	void OnGUI()
	{
		if (isConnected)
			connectionMessage = "connected";
		GUI.Box (new Rect (0,0,150,70), "<size=20>" +info + "\n" + message+"</size>");
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 120,0,120,80), "<size=20>Score: " + score +
		         "\nTime: " + (int)(Time.time - prevTotalTime) +"\nPrev: " + (int)prevTime+"</size>");
		GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 30), "<size=18>"+connectionMessage+"</size>");
	}
	
	void Awake ()
	{
		UnityEngine.Screen.showCursor = false;
		difficulty = Difficulty.Instance;
		difficulty.setNormal();
	}
	
	void Start()
	{
		grab = false;
		isDocked = false;
		score = 0;
		prevTime = 0;
		prevTotalTime = (int)Time.time;
		setNewPositionAndOrientation ();
		isConnected = initDevice ();
		if(isConnected)
		{
			getData ();
			prevPosition = new Vector3 ( (float)getPosX()*scale, 
			                            (float)getPosY()*scale, 
			                            -(float)getPosZ()*scale);
			prevOrient = new Vector3((float)gimbalY()* Mathf.Rad2Deg,
			                         -(float)gimbalX()* Mathf.Rad2Deg, 
			                         -(float)gimbalZ()* Mathf.Rad2Deg);
		}
	}
	
	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.LoadLevel("MainMenu");
		}
		else if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
			prevTotalTime = (int)Time.time;
		}
		
		if (pointText.enabled) 
		{
			if( (int)(Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}
		
		isConnected = getData ();
		if(isConnected)
		{
			Vector3 newPos = new Vector3 ( (float)getPosX()*scale, 
			                              (float)getPosY()*scale, 
			                              -(float)getPosZ()*scale);
			
			Vector3 transVec = newPos - prevPosition;
			prevPosition = newPos;
			
			Vector3 joints = new Vector3 ((float)jointX()* Mathf.Rad2Deg, 
			                              (float)jointY()* Mathf.Rad2Deg, 
			                              (float)jointZ()* Mathf.Rad2Deg);
			
			Vector3 joints2 = new Vector3 ((float)gimbalX()* Mathf.Rad2Deg, 
			                               (float)gimbalY()* Mathf.Rad2Deg, 
			                               (float)gimbalZ()* Mathf.Rad2Deg);
			
			Quaternion rotation;
			rotation = Quaternion.Euler(joints.y, joints.x, 0f);
			j2.transform.rotation = rotation;
			rotation = Quaternion.Euler(joints.z, joints.x, 0f);
			j3.transform.rotation = rotation;
			
			Vector3 penOrient = new Vector3(joints2.y, -joints2.x, -joints2.z);
			rotation = Quaternion.Euler(joints2.y, -joints2.x, -joints2.z);
			j4.transform.localRotation = rotation;
			
			Vector3 rotVec = penOrient - prevOrient;
			prevOrient = penOrient;
			
			if(isButtonADown() || isButtonBDown())
			{
				grab = true;
				cursor.transform.Translate (transVec, Space.World);
				cursor.transform.Rotate(rotVec, Space.World);
				info = "grabbed";
			}
			else
			{
				grab = false;
				info = "not grabbed";
			}
			
			if(isDocked && !grab)
			{
				popSource.PlayOneShot(popSound);
				pointText.enabled = true;
				newGame();
			}
			
			evaluateDock();
		}
		
	}
	
	void OnDestroy() 
	{
		if(isConnected)
			cleanup ();
	}
	
	protected void setNewPositionAndOrientation()
	{
		switch (score)
		{
			//learn to translate
		case 0:
			cursor.transform.rotation = target.transform.rotation;
			break;
			//learn to rotate around y
		case 1:
			cursor.transform.rotation = target.transform.rotation;
			cursor.transform.Rotate(Vector3.up * 45f, Space.World);
			break;
			//learn to rotate around x
		case 2:
			cursor.transform.rotation = target.transform.rotation;
			cursor.transform.Rotate(Vector3.right * 45f, Space.World);
			break;
			//learn to rotate around z
		case 3:
			cursor.transform.rotation = target.transform.rotation;
			cursor.transform.Rotate(Vector3.forward * 45f, Space.World);
			break;
			//practise docking
		default:
			target.transform.rotation = UnityEngine.Random.rotation;
			cursor.transform.rotation = UnityEngine.Random.rotation;
			break;
		}
		cursor.transform.position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax), 
		                                         UnityEngine.Random.Range(4.0F, yMax), 
		                                         UnityEngine.Random.Range(-zMax, zMax));
	}
	
	protected void evaluateDock() 
	{
		Quaternion targetQ = target.transform.GetChild(0).rotation;
		Quaternion cursorQ = cursor.transform.GetChild(0).rotation;
		Vector3 targetV = target.transform.GetChild(0).position;
		Vector3 cursorV = cursor.transform.GetChild(0).position;
		float distance = (targetV - cursorV).magnitude;
		float angle = Quaternion.Angle(cursorQ, targetQ);
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
	
	protected void clampPosition()
	{
		cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax), 
		                                         Mathf.Clamp(cursor.transform.position.y, 2.0f, yMax), 
		                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
	}
	
	protected void newGame()
	{
		setNewPositionAndOrientation();
		prevTime = Time.time - prevTotalTime;
		prevTotalTime = Time.time;
		score++;
	}
}