using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiTrackBehavoir : MonoBehaviour {

	OptiTrackUDPClient udpClient;
	bool bSuccess;
	Skeleton skelPerformer = new Skeleton();

	public GameObject fingerObj;
	public GameObject cursor;
	public GameObject target;
	public Material green;
	public Material yellow;
	public Material red;
	public Light roomLight;
	public GameObject axis;
	//public GameObject sphere;
	public GameObject trail;
	public GUIText pointText;
	public Camera secondCamera;
	public GameObject rotAxis;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;
	//public GUIText keysText;
	public GameObject index;
	public GameObject thumb;
	public GameObject ring;

	static float xMax = 15.0f;
	static float yMax = 15.0f;
	static float zMax = 15.0f;
	Vector3 prevPos;
	bool rotate;
	bool translate;
	bool isDocked;
	private int score;
	private string connectionMessage="not connected";
	private string message="";
	private string info="";
	private float prevTime;
	private float prevTotalTime;
	string path;
	bool updateCam;
	Vector3 fingerDir;
	Difficulty difficulty;
	bool locked;
	Vector3 prevPinch;
	/*Vector3 thumbPos;
	Vector3 indexPos;
	Vector3 pinkyPos;*/
	static float chairRadius = 5f;
	AudioSource hum;
	float distance = 0;
	float angle = 0;
	OptiCalibration calibration;

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
		calibration = OptiCalibration.Instance;
		path = @"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_Opti.csv";
		UnityEngine.Screen.showCursor = false;
		hum = fingerObj.GetComponent<AudioSource>();
		File.AppendAllText(path, "Time,Distance,Angle"+ Environment.NewLine);//save to file
	}

	void Start () 
	{
		udpClient = new OptiTrackUDPClient();
		bSuccess = udpClient.Connect();
		udpClient.skelTarget = skelPerformer;
		prevPos = new Vector3 ();
		rotate = false;
		isDocked = false;
		translate = false;
		updateCam = false;
		score = 0;
		prevTime = 0;
		prevTotalTime = Time.time;
		setNewPositionAndOrientation();
		prevPinch = new Vector3 ();
		fingerObj.renderer.enabled = true;
		//chairCollider = cursor.transform.GetChild (0).GetComponent<Collider> ();

		if (bSuccess) 
		{
			udpClient.RequestDataDescriptions ();
			if(udpClient.rigidTargets[0] != null)
				prevPos = new Vector3(udpClient.rigidTargets[0].pos.x, udpClient.rigidTargets[0].pos.y, -udpClient.rigidTargets[0].pos.z);
			connectionMessage="connected";
		}
	}
	
	void Update () 
	{
		if (Input.GetKey(KeyCode.Escape))
			Application.LoadLevel("MainMenu");
		else if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
			prevTotalTime = Time.time;
		}
		else if(Input.GetKeyDown (KeyCode.P))
		{
			Application.CaptureScreenshot(@"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Screenshot.png");
			Debug.Log("print");
		}

		if(translate)
		{
			rotate = false;
			hum.mute = true;
			info = "translate";
			axis.transform.position = cursor.transform.position;
			foreach(Transform child in axis.transform) 
			{
				child.renderer.enabled = true;
			}
			index.renderer.enabled = false;
			thumb.renderer.enabled = false;
			ring.renderer.enabled = false;
		}
		else
		{	
			hum.mute = false;
			foreach(Transform child in axis.transform) 
			{
				child.renderer.enabled = false;
			}

		}
		
		if(rotate)
		{
			info = "rotate";
			translate = false;
			fingerObj.renderer.enabled = true;
			trail.GetComponent<TrailRenderer>().enabled = true;
			index.renderer.enabled = false;
			thumb.renderer.enabled = false;
			ring.renderer.enabled = false;
			cursor.renderer.enabled = true;
	
		}
		else
		{	
			cursor.renderer.enabled = false;
			trail.GetComponent<TrailRenderer>().enabled = false;
			prevPinch = new Vector3 ();
			fingerObj.renderer.enabled = false;
		}

		if (pointText.enabled) 
		{
			if( (int)(Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}

		if(bSuccess)
		{
			udpClient.RequestDataDescriptions();

			if(udpClient.numMarkers == 3)
			{
				thumb.renderer.material = green;
				index.renderer.material = green;
				ring.renderer.material = green;
				fingerObj.renderer.material = green; 

				Vector3 thumbPos = new Vector3(udpClient.markers[0].x,
		                                       udpClient.markers[0].y,
		                                       -udpClient.markers[0].z);
				Vector3 indexPos = new Vector3(udpClient.markers[1].x,
		                                       udpClient.markers[1].y,
		                                       -udpClient.markers[1].z);
				Vector3 ringPos = new Vector3(udpClient.markers[2].x,
	                                        udpClient.markers[2].y,
	                                        -udpClient.markers[2].z);

				float thumbToIndex = Vector3.Distance(thumbPos, 
				                                      indexPos);
				float thumbToRing = Vector3.Distance(thumbPos, 
				                                     ringPos);
				float indexToRing = Vector3.Distance(indexPos, 
				                                     ringPos);

				Vector3 currentPos = (thumbPos + indexPos + ringPos)*0.33f;
				Vector3 transVec = currentPos - prevPos;
				fingerDir = Vector3.zero - fingerObj.transform.position;
				fingerDir.Normalize();

				thumb.transform.position = thumbPos; 
				index.transform.position = indexPos; 
				ring.transform.position = ringPos;

				//Debug.Log(thumb.transform.position + " "+ index.transform.position+" "+ring.transform.position);

				//Debug.Log(thumbToIndex + " "+ thumbToRing+" "+indexToRing);

				if(thumbToIndex < calibration.touchDist && thumbToRing < calibration.touchDist && indexToRing < calibration.touchDist)
				{
					translate = true;
					
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
				}
				else if((thumbToIndex < calibration.touchDist && thumbToRing > calibration.minDist && indexToRing > calibration.minDist) ||
				        (thumbToIndex > calibration.minDist && thumbToRing < calibration.touchDist && indexToRing > calibration.minDist) ||
				        (thumbToIndex > calibration.minDist && thumbToRing > calibration.minDist && indexToRing < calibration.touchDist))
				{
					rotate = true;
					updateCam = true;
					Vector3 pointerPos = new Vector3();
		
					if(thumbToIndex < calibration.touchDist)
						pointerPos = (thumb.transform.position + index.transform.position)*0.5f;
					else if (thumbToRing < calibration.touchDist)
						pointerPos = (thumb.transform.position + ring.transform.position)*0.5f;
					else 
						pointerPos = (index.transform.position + ring.transform.position)*0.5f;

					/*Vector3 closestPoint = chairCollider.ClosestPointOnBounds(pointerPos);
					Debug.Log( Vector3.Distance(closestPoint, pointerPos));*/
					fingerObj.transform.position = pointerPos;//closestPoint

					Vector3 to = pointerPos - cursor.transform.position;
					to.Normalize();
					Vector3 axisVec = Vector3.Cross(prevPinch, to);
					cursor.transform.RotateAround(cursor.transform.position, axisVec, Vector3.Angle(prevPinch, to));
					prevPinch = to;
				}
				else
				{
					translate = false;
					rotate = false;
					index.renderer.enabled = true;
					thumb.renderer.enabled = true;
					ring.renderer.enabled = true;
					info = "hold";
					updateCam = true;
					fingerObj.transform.position = currentPos;

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
				}
				prevPos = currentPos;
			}
			else
			{
				translate = false;
				rotate = false;
				info = "not tracked";
				thumb.renderer.material = yellow;
				index.renderer.material = yellow;
				ring.renderer.material = yellow;
				fingerObj.renderer.material = yellow;
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
		cursor.transform.rotation = UnityEngine.Random.rotation;
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

	void LateUpdate()
	{
		bool vertical = false;

		if(updateCam)
		{
			Vector3 camPos = new Vector3();
			Vector3 axisVec = new Vector3(0f, 0f, 1f);
			float angle = Vector3.Angle(axisVec, fingerDir);
			if(angle <= 45f)
			{
				camPos = axisVec;
			}
			else
			{
				axisVec = new Vector3(-1f, 0f, 0f);
				angle = Vector3.Angle(axisVec, fingerDir);
				if(angle <= 45f)
				{
					camPos = axisVec;
				}
				else
				{
					axisVec = new Vector3(1f, 0f, 0f);
					angle = Vector3.Angle(axisVec, fingerDir);
					if(angle <= 45f)
					{
						camPos = axisVec;
					}
					else
					{
						axisVec = new Vector3(0f, 0f, -1f);
						angle = Vector3.Angle(axisVec, fingerDir);
						if(angle <= 45f)
						{
							camPos = axisVec;
						}
						else
						{
							vertical = true;
							axisVec = new Vector3(0f, 1f, 0f);
							angle = Vector3.Angle(axisVec, fingerDir);
							if(angle <= 45f)
							{
								camPos = axisVec;
							}
							else
								axisVec = new Vector3(0f, -1f, 0f);
						}
					}
				}
			}

			if(!vertical)
			{
				camPos = camPos*-15f;
				camPos.y = 10f;
			}
			else
				camPos = camPos*5f;
			
			secondCamera.transform.position = camPos;
			secondCamera.transform.LookAt(target.transform.position);
		}
	}
}
