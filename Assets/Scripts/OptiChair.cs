using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiChair : MonoBehaviour {
	
	OptiTrackUDPClient udpClient;
	bool bSuccess;
	Skeleton skelPerformer = new Skeleton();

	public GameObject cursor;
	public GameObject target;
	public Material green;
	public Material yellow;
	public Material red;
	public Light roomLight;
	public GUIText pointText;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;

	static float xMax = 15.0f;
	static float yMax = 15.0f;
	static float zMax = 15.0f;

	bool isDocked;
	private int score;
	private string connectionMessage="not connected";
	private string message="";
	private string info="";
	private float prevTime;
	private float prevTotalTime;
	string path;
	Difficulty difficulty;
	static float chairRadius = 5f;
	float distance = 0;
	float angle = 0;
	bool confirm = false;
	private Vector3 prevOrient;
	Vector3 prevPos;
	bool mute = false;

	void OnGUI()
	{
		GUI.Box (new Rect (0,0,150,60), "<size=20>"+info + "\n" + message + "\n" +"</size>");
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 120,0,120,80), "<size=20>Score: " + score +
		         "\nTime: " + (int)(Time.time - prevTotalTime) +"\nPrev: " + ((int)prevTime).ToString()+"</size>");
		GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 30), "<size=18>"+connectionMessage+"</size>");
	}
	
	void Awake ()
	{
		difficulty = Difficulty.Instance;
		path = @"Log/"+difficulty.getLevel()+"/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_OptiChairTutorial.csv";
		UnityEngine.Screen.showCursor = false;
		File.AppendAllText(path, "Time,Distance,Angle"+ Environment.NewLine);//save to file
	}
	
	void Start () 
	{
		udpClient = new OptiTrackUDPClient();
		bSuccess = udpClient.Connect();
		udpClient.skelTarget = skelPerformer;
		prevPos = new Vector3 ();
		isDocked = false;
		score = 0;
		prevTime = 0;
		prevTotalTime = Time.time;
		setNewPositionAndOrientation();
		prevPos = new Vector3 ();
		prevOrient = new Vector3 ();

		if (bSuccess) 
		{
			udpClient.RequestDataDescriptions ();
			if(udpClient.rigidTargets[0] != null)
			{
				prevPos = new Vector3(udpClient.rigidTargets[0].pos.x, udpClient.rigidTargets[0].pos.y, -udpClient.rigidTargets[0].pos.z);

			}
			connectionMessage="connected";
		}
	}
	
	void Update () 
	{
		if (Input.GetKeyUp(KeyCode.Escape))
			Application.LoadLevel("MainMenu");
		if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
			prevTotalTime = Time.time;
		}
		if (Input.GetKeyUp (KeyCode.Space))
		{
			confirm = true;
		}


		if(Input.GetKeyUp (KeyCode.P))
		{
			Application.CaptureScreenshot(@"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Screenshot.png");
			Debug.Log("print");
		}

		if(Input.GetKeyUp (KeyCode.M))
		{
			if(!mute)
			{
				mute = true;
				ambientSource.volume = 0f;
				cursor.GetComponent<AudioSource>().volume = 0f;
			}
			else
				mute = false;
		}

		if (pointText.enabled) 
		{
			if( (int)(Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}

		if(bSuccess)
		{
			udpClient.RequestDataDescriptions();

			if(udpClient.numTrackables >= 1)
			{
				info = "";
				Vector3 currentPos = new Vector3(udpClient.rigidTargets[0].pos.x,
				                               	udpClient.rigidTargets[0].pos.y,
				                              	 -udpClient.rigidTargets[0].pos.z);

				Debug.Log(currentPos);
				Quaternion currentOrient = udpClient.rigidTargets[0].ori;
				Vector3 euler = currentOrient.eulerAngles;

				Quaternion rotation = Quaternion.Euler(-euler.x, -euler.y, euler.z);

				Vector3 transVec = currentPos - prevPos;

				cursor.transform.Translate (transVec, Space.World);
				//cursor.transform.position = currentPos;
				cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
				                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
				                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
				cursor.transform.rotation = rotation;

				if(confirm)
				{
					if(isDocked)
					{
						popSource.PlayOneShot(popSound);
						setNewPositionAndOrientation();
						prevTime = Time.time - prevTotalTime;
						prevTotalTime = Time.time;
						pointText.enabled = true;
						score++;
						File.AppendAllText(path, prevTime.ToString()+","+distance.ToString()+","+angle.ToString()+ Environment.NewLine);//save to file
					}
					confirm = false;
				}

				prevPos = currentPos;
			}
			else
			{
				info = "not tracked";
				Debug.Log("not tracked");
			}
			
			
		}
		evaluateDock();
	}
	
	void OnDestroy() 
	{
		udpClient.Close();
	}
	
	void setNewPositionAndOrientation()
	{
		target.transform.rotation = UnityEngine.Random.rotation;
		cursor.transform.position = new Vector3 (UnityEngine.Random.Range(-xMax, xMax),
		                                         UnityEngine.Random.Range(4.0F, yMax),
		                                         UnityEngine.Random.Range(-zMax, zMax));
	}
	
	void evaluateDock()
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
}
