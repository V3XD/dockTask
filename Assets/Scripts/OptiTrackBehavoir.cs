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
	public GameObject sphere;
	public GameObject trail;
	public GUIText pointText;
	public Camera secondCamera;
	public GameObject rotAxis;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;
	public GUIText keysText;
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
	private int prevTime;
	private int prevTotalTime;
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

	void OnGUI()
	{
		GUI.Box (new Rect (0,0,150,60), "<size=20>"+info + "\n" + message + "\n" +"</size>");
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 120,0,120,80), "<size=20>Score: " + score +
		         "\nTime: " + ((int)Time.time - prevTotalTime) +"\nPrev: " + prevTime+"</size>");
		GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 30), "<size=18>"+connectionMessage+"</size>");
	}

	void Awake ()
	{
		difficulty = Difficulty.Instance;
		path = @"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_Opti.csv";
		UnityEngine.Screen.showCursor = false;
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
		prevTotalTime = (int)Time.time;
		setNewPositionAndOrientation();
		prevPinch = new Vector3 ();
		fingerObj.renderer.enabled = true;

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
			prevTotalTime = (int)Time.time;
		}
		else if(Input.GetKeyDown (KeyCode.P))
		{
			Application.CaptureScreenshot(@"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Screenshot.png");
			Debug.Log("print");
		}

		if(translate)
		{
			rotate = false;

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
			sphere.renderer.enabled = true;
			/*index.renderer.enabled = true;
			thumb.renderer.enabled = true;
			ring.renderer.enabled = true;
			index.renderer.material = green;
			thumb.renderer.material = green;
			ring.renderer.material = green;*/
		}
		else
		{	
			sphere.renderer.enabled = false;
			fingerObj.renderer.material = yellow;
			trail.GetComponent<TrailRenderer>().enabled = false;
			prevPinch = new Vector3 ();
			fingerObj.renderer.enabled = false;
		}

		if (pointText.enabled) 
		{
			if( ((int)Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}

		if(bSuccess)
		{
			udpClient.RequestDataDescriptions();

			if(udpClient.numMarkers == 3)
			{

				Vector3 thumbPos = new Vector3(udpClient.markers[0].x,
		                                       udpClient.markers[0].y,
		                                       -udpClient.markers[0].z);
				Vector3 indexPos = new Vector3(udpClient.markers[1].x,
		                                       udpClient.markers[1].y,
		                                       -udpClient.markers[1].z);
				Vector3 ringPos = new Vector3(udpClient.markers[2].x,
	                                        udpClient.markers[2].y,
	                                        -udpClient.markers[2].z);

				thumb.transform.position = thumbPos;// - cursor.transform.position;
				index.transform.position = indexPos;// - cursor.transform.position;
				ring.transform.position = ringPos;// - cursor.transform.position;


				float thumbToIndex = Vector3.Distance(thumb.transform.position, 
				                                      index.transform.position);
				float thumbToRing = Vector3.Distance(thumb.transform.position, 
				                                     ring.transform.position);
				float indexToRing = Vector3.Distance(index.transform.position, 
				                                     ring.transform.position);

				/*thumb.transform.position = new Vector3 (Mathf.Clamp(thumb.transform.position, cursor.transform.position.x-chairRadius, cursor.transform.position.x+chairRadius),
				                                        Mathf.Clamp(thumb.transform.position, cursor.transform.position.y-chairRadius, cursor.transform.position.y+chairRadius),
				                                        Mathf.Clamp(thumb.transform.position, cursor.transform.position.z-chairRadius, cursor.transform.position.z+chairRadius));

				index.transform.position = new Vector3 (Mathf.Clamp(index.transform.position, cursor.transform.position.x-chairRadius, cursor.transform.position.x+chairRadius),
				                                        Mathf.Clamp(index.transform.position, cursor.transform.position.y-chairRadius, cursor.transform.position.y+chairRadius),
				                                        Mathf.Clamp(index.transform.position, cursor.transform.position.z-chairRadius, cursor.transform.position.z+chairRadius));

				index.transform.position = new Vector3 (Mathf.Clamp(index.transform.position, cursor.transform.position.x-chairRadius, cursor.transform.position.x+chairRadius),
				                                        Mathf.Clamp(index.transform.position, cursor.transform.position.y-chairRadius, cursor.transform.position.y+chairRadius),
				                                        Mathf.Clamp(index.transform.position, cursor.transform.position.z-chairRadius, cursor.transform.position.z+chairRadius));*/

				//Debug.Log(thumbToIndex + " " + thumbToRing +" "+indexToRing );

				Debug.Log(thumb.transform.position + " "+ index.transform.position+" "+ring.transform.position);


				Vector3 currentPos = (thumb.transform.position + index.transform.position + ring.transform.position)*0.33f;
				Vector3 transVec = currentPos - prevPos;
				if(thumbToIndex < 3f && thumbToRing < 3f && indexToRing < 3f)
				{
					translate = true;
					
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
				}
				else if(thumbToIndex < 3f || thumbToRing < 3f || indexToRing < 3f)
				{
					rotate = true;
					sphere.transform.position = cursor.transform.position;
					Vector3 pointerPos = new Vector3();
					if(thumbToIndex < 3f)
						pointerPos = (thumb.transform.position + index.transform.position)*0.5f;
					else if (thumbToRing < 3f)
						pointerPos = (thumb.transform.position + ring.transform.position)*0.5f;
					else 
						pointerPos = (index.transform.position + ring.transform.position)*0.5f;
					fingerObj.transform.position = pointerPos;

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
					
					if(isDocked)
					{
						popSource.PlayOneShot(popSound);
						setNewPositionAndOrientation();
						prevTime = (int)Time.time - prevTotalTime;
						prevTotalTime = (int)Time.time;
						pointText.enabled = true;
						score++;
						File.AppendAllText(path, prevTime.ToString()+ Environment.NewLine);//save to file
					}
				}
				prevPos = currentPos;
			}
			else
				Debug.Log("not tracked");

			/*if(udpClient.rigidTargets[0] != null && udpClient.rigidTargets[1]!= null && udpClient.rigidTargets[2]!= null)
			{


				Vector3 newthumbPos = new Vector3(udpClient.rigidTargets[0].pos.x, udpClient.rigidTargets[0].pos.y, -udpClient.rigidTargets[0].pos.z);
				Vector3 newindexPos = new Vector3(udpClient.rigidTargets[1].pos.x, udpClient.rigidTargets[1].pos.y, -udpClient.rigidTargets[1].pos.z);
				Vector3 newpinkyPos = new Vector3(udpClient.rigidTargets[2].pos.x, udpClient.rigidTargets[2].pos.y, -udpClient.rigidTargets[2].pos.z);


				if(newthumbPos == Vector3.zero)
				{
					Debug.Log("thumb untracked");
				}
				else
					thumbPos = newthumbPos;

				if(newindexPos == Vector3.zero)
				{
					Debug.Log("index untracked");
				}
				else
					indexPos = newindexPos;
				if(newpinkyPos == Vector3.zero)
				{
					Debug.Log("pinky untracked");
				}
				else
					pinkyPos = newpinkyPos;

				float thumbIndex = Vector3.Distance(thumbPos, indexPos);
				float thumbPinky =  Vector3.Distance(thumbPos, pinkyPos);


				fingerObj.transform.position = indexPos;
				Vector3 transVec = indexPos - prevPos;


				Debug.Log("index " +thumbIndex +" pinky " +thumbPinky);
				if(thumbIndex < 8f && thumbPinky > 8f) 
				{
	
					rotate = true;
					trail.GetComponent<TrailRenderer>().enabled = true;
					sphere.transform.position = cursor.transform.position;
					sphere.renderer.enabled = true;

					Vector3 to = indexPos - cursor.transform.position;
					to.Normalize();
					Vector3 axisVec = Vector3.Cross(prevPinch, to);
					cursor.transform.RotateAround(cursor.transform.position, axisVec, Vector3.Angle(prevPinch, to));
					prevPinch = to;
	
				}
				else if(thumbPinky < 7f && thumbPinky > 0f) 
				{
					translate = true;

					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));

				}
				*/
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
		float distance = (targetV - cursorV).magnitude;
		float angle = Quaternion.Angle(cursorQ, targetQ);
		ambientSource.volume = 1f-(angle / 180f);
		
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
			sphere.renderer.material = green;
		}
		else
		{
			sphere.renderer.material = yellow;
		}
	}

	void LateUpdate()
	{
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
							camPos = new Vector3(0f, 0f, 1f);
					}
				}
			}
			camPos = camPos*-15f;
			camPos.y = 10f;
			
			secondCamera.transform.position = camPos;
			secondCamera.transform.LookAt(target.transform.position);
		}
	}
}
