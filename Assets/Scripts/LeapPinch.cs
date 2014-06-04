using UnityEngine;
using System;
using System.Collections;
using Leap;
using System.IO;

public class LeapPinch : MonoBehaviour 
{
	
	public GameObject fingerObj;
	public GameObject cursor;
	public GameObject target;
	public Material green;
	public Material yellow;
	public Material red;
	public Light camLight;
	public GameObject axis;
	public GameObject sphere;
	public GameObject trail;
	public GUIText pointText;
	public Camera secondCamera;
	public GameObject rotAxis;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;
	//public GUIText keysText;

	private Controller mController;
	private Frame mLastFrame;
	static float scale = 0.10F;
	bool rotate;
	bool translate;
	bool isDocked;
	private int score;
	private string connectionMessage="not connected";
	private string message="";
	private string info="";
	private int prevTime;
	private int prevTotalTime;
	private Frame frame;
	static float xMax = 15.0f;
	static float yMax = 15.0f;
	static float zMax = 15.0f;
	string path;
	bool updateCam;
	Vector3 fingerDir;
	Difficulty difficulty;
	bool locked;
	bool mute;
	Vector3 prevPinch = new Vector3 ();

	void Awake ()
	{
		difficulty = Difficulty.Instance;
		mController = new Controller();
		path = @"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_LeapPinch.csv";
		UnityEngine.Screen.showCursor = false;
	}
	
	void Start ()
	{
		locked = true;
		mLastFrame = new Frame();
		frame = new Frame();
		rotate = false;
		isDocked = false;
		translate = false;
		updateCam = false;
		score = 0;
		prevTime = 0;
		prevTotalTime = (int)Time.time;
		setNewPositionAndOrientation();
		mute = false;

		if(mController.IsConnected)
		{
			connectionMessage = "Leap connected";
			mLastFrame = mController.Frame();
		}
	}
	
	void OnGUI()
	{
		GUI.Box (new Rect (0,0,150,60), "<size=20>"+info + "\n" + message + "\n" +"</size>");
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 120,0,120,80), "<size=20>Score: " + score +
		         "\nTime: " + ((int)Time.time - prevTotalTime) +"\nPrev: " + prevTime+"</size>");
		GUI.Box (new Rect (UnityEngine.Screen.width - 150,UnityEngine.Screen.height - 30, 150, 30), "<size=18>"+connectionMessage+"</size>");
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
		/*else if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl))
		{
			rotate = false;
			info = "hold";
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			rotate = true;
			translate = false;
		}*/

		/*if(Input.GetKeyUp (KeyCode.L))
		{
			locked = !locked;
			if(locked)
				keysText.text = "[CTRL] rotate [S] skip [L] unlock";
			else			
				keysText.text = "[CTRL] rotate [S] skip [L] lock";
		}*/

		if(Input.GetKeyUp (KeyCode.M))
		{
			if(!mute)
			{
				mute = true;
				ambientSource.volume = 0f;
			}
			else
				mute = false;
		}

		if (pointText.enabled) 
		{
			if( ((int)Time.time - prevTotalTime) > 1)
				pointText.enabled = false;
		}
		
		if(translate)
		{
			info = "translate";
			axis.transform.position = cursor.transform.position;
			foreach(Transform child in axis.transform) 
			{
				child.renderer.enabled = true;
			}
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
		}
		else
		{	
			sphere.renderer.enabled = false;
			trail.GetComponent<TrailRenderer>().enabled = false;
		}
		
		updateCam = false;
		if (mController.IsConnected)
		{
			connectionMessage = "Connected";
			frame = mController.Frame ();
			if(frame.IsValid)
			{
				if(frame.Id != mLastFrame.Id)
				{
					InteractionBox iBox = frame.InteractionBox;
					/*Pointable finger = frame.Pointables.Frontmost;
					Vector normalizedPos = iBox.NormalizePoint(finger.StabilizedTipPosition);
					fingerObj.transform.position = new Vector3 ( ((normalizedPos.x*2.0f)-1.0f) * xMax,
					                                            normalizedPos.y * yMax,
					                                            ((normalizedPos.z*2.0f)-1.0f) * -zMax);
					Vector direction = frame.Translation (mLastFrame);*/
					Hand firstHand = frame.Hands[0];
					Finger thumb = firstHand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0];
					Vector normalizedThumbPos = iBox.NormalizePoint(thumb.StabilizedTipPosition);
					Vector3 thumbPos = new Vector3 ( ((normalizedThumbPos.x*2.0f)-1.0f) * xMax,
					                                normalizedThumbPos.y * yMax,
					                                ((normalizedThumbPos.z*2.0f)-1.0f) * -zMax);
					Vector3 to = thumbPos - cursor.transform.position;
					to.Normalize();

					fingerDir = new Vector3 (thumb.Direction.x, thumb.Direction.y, -thumb.Direction.z);
					fingerDir.Normalize();
					fingerObj.transform.position = thumbPos;

					Debug.Log(firstHand.PinchStrength + " grab "+firstHand.GrabStrength);

					if(firstHand.GrabStrength == 1f)//translation
					{
						fingerObj.renderer.enabled = false;
						rotate = false;

						if(frame.TranslationProbability(mLastFrame) > 0.60)
						{
							translate = true;
							Vector leapTransVec = frame.Translation (mLastFrame);
							Vector3 transVec = new Vector3 (leapTransVec.x * scale, leapTransVec.y * scale, -leapTransVec.z * scale);

							cursor.transform.Translate (transVec, Space.World);
							cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
							                                         Mathf.Clamp(cursor.transform.position.y, 2.0f, yMax),
							                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
						}
						
					}
					else if(firstHand.PinchStrength > 0.2f)//~rotation
					{
						fingerObj.renderer.enabled = true;
						
						fingerObj.renderer.material = yellow;
						translate = false;	
						updateCam = true;
						rotate = true;
						
						fingerObj.renderer.material = green;
						sphere.transform.position = cursor.transform.position;
						sphere.renderer.enabled = true;
						
						if(frame.TranslationProbability(mLastFrame) > 0.60)
						{
							trail.GetComponent<TrailRenderer>().enabled = true;
							
							
							Vector3 axisVec = Vector3.Cross(prevPinch, to);
							cursor.transform.RotateAround(cursor.transform.position, axisVec, Vector3.Angle(prevPinch, to));
							
						}
						
					}
					else //hold
					{

						info = "hold";

						updateCam = true;
						fingerObj.renderer.material = yellow;
						fingerObj.renderer.enabled = true;
						rotate = false;
						translate = false;

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

					prevPinch = to;
					mLastFrame = frame;
				}
			}
		}else
			connectionMessage = "Not connected";
		evaluateDock();
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
	
	void OnDestroy() 
	{
		mController.Dispose();
	}
	
	void setNewPositionAndOrientation()
	{
		cursor.transform.rotation = UnityEngine.Random.rotation;
		//target.transform.rotation = UnityEngine.Random.rotation;
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

		if(!mute)
			ambientSource.volume = 1f-(angle / 180f);
		
		if ((angle <= difficulty.angle) && (distance < difficulty.distance)) 
		{	
			isDocked = true;
			camLight.intensity = 4.0f;
			message= "Target docked!";
		}
		else
		{
			isDocked=false;
			camLight.intensity = 1.0f;
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
}