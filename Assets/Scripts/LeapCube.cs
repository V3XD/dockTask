using UnityEngine;
using System;
using System.Collections;
using Leap;
using System.IO;

public class LeapCube : MonoBehaviour 
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
	public GameObject pointerObj;
	public GameObject rotAxis;
	public AudioClip popSound;
	public AudioSource popSource;
	public AudioSource ambientSource;
	public GUIText keysText;

	private Controller mController;
	private Frame mLastFrame;
	static float scale = 0.10F;
	bool rotate;
	bool translate;
	bool isDocked;
	private int score;
	private string connectionMessage="not connected";
	private string message="";
	private string warning="";
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

	void Awake ()
	{
		difficulty = Difficulty.Instance;
		mController = new Controller();
		path = @"Log/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_Leap.csv";
		UnityEngine.Screen.showCursor = false;
	}
	
	void Start ()
	{
		locked = false;
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

		if(mController.IsConnected)
		{
			connectionMessage = "Leap connected";
			mLastFrame = mController.Frame();
		}
	}
	
	void OnGUI()
	{
		GUI.Box (new Rect (0,0,100,70), info + "\n" + message + "\n" + warning);
		
		GUI.Box (new Rect (UnityEngine.Screen.width - 100,0,100,50), "Score: " + score +
		         "\nTime: " + ((int)Time.time - prevTotalTime) +"\nPrev: " + prevTime);
		GUI.Box (new Rect (UnityEngine.Screen.width - 100,UnityEngine.Screen.height - 25, 100, 25), connectionMessage);
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
		else if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl))
		{
			rotate = false;
			
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			rotate = true;
			translate = false;
		}

		if(Input.GetKeyUp (KeyCode.L))
		{
			locked = !locked;
			if(locked)
				keysText.text = "[CTRL] rotate [S] skip [L] unlock";
			else			
				keysText.text = "[CTRL] rotate [S] skip [L] lock";
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
					InteractionBox iBox= frame.InteractionBox;
					Pointable finger=frame.Pointables.Frontmost;
					Vector normalizedPos = iBox.NormalizePoint(finger.StabilizedTipPosition);
					fingerObj.transform.position = new Vector3 ( ((normalizedPos.x*2.0f)-1.0f) * xMax,
					                                            normalizedPos.y * yMax,
					                                            ((normalizedPos.z*2.0f)-1.0f) * -zMax);
					Vector direction = frame.Translation (mLastFrame);
					
					fingerDir = new Vector3(finger.Direction.x, finger.Direction.y, -finger.Direction.z);
					
					if(frame.Pointables.Count > 0 && frame.Pointables.Count < 3 && finger.Length > 30.0f)//~one finger
					{
						fingerObj.transform.rotation = Quaternion.LookRotation(fingerDir);
						fingerObj.renderer.enabled = true;
						pointerObj.renderer.enabled = true;
						fingerObj.renderer.material = yellow;
						translate = false;	
						updateCam = true;
						
						if(rotate)
						{
							fingerObj.renderer.material = green;
							sphere.transform.position = cursor.transform.position;
							sphere.renderer.enabled = true;

							if(frame.TranslationProbability(mLastFrame) > 0.60)
							{
								trail.GetComponent<TrailRenderer>().enabled = true;
								Vector3 swipeVec = new Vector3(direction.x, direction.y, -direction.z);
								Vector3 axisVec = Vector3.Cross(swipeVec, fingerDir);
								axisVec.Normalize();

								Vector3 tmpAxis = new Vector3(); 
								if(locked)
								{
									tmpAxis.x = Mathf.Round(axisVec.x);
									tmpAxis.y = Mathf.Round(axisVec.y);
									tmpAxis.z = Mathf.Round(axisVec.z);
									
									if( (Mathf.Abs(tmpAxis.x) + Mathf.Abs(tmpAxis.y) + Mathf.Abs(tmpAxis.z)) > 1f)
									{
										if(Mathf.Abs(tmpAxis.x) == 1f)
										{
											if(Mathf.Abs(tmpAxis.y) == 1f)
											{
												if(Mathf.Abs(axisVec.x) >= Mathf.Abs(axisVec.y))
													tmpAxis.y = 0f;
												else
													tmpAxis.x = 0f;
											}
											else
											{
												if(Mathf.Abs(axisVec.x) >= Mathf.Abs(axisVec.z))
													tmpAxis.z = 0f;
												else
													tmpAxis.x = 0f;
												
											}
										}
										else if(Mathf.Abs(tmpAxis.y) == 1f)
										{
											if(Mathf.Abs(axisVec.y) >= Mathf.Abs(axisVec.z))
												tmpAxis.z = 0f;
											else
												tmpAxis.y = 0f;
										}
									}
									axisVec = tmpAxis;
								}

								cursor.transform.RotateAround(cursor.transform.position, axisVec, direction.Magnitude);
							}
						}
					}
					else if(frame.Pointables.Count > 2)//full hand
					{
						if(frame.TranslationProbability(mLastFrame) > 0.60)
						{
							translate = true;
							Vector leapTransVec = frame.Translation (mLastFrame);
							Vector3 transVec = new Vector3 (leapTransVec.x * scale, leapTransVec.y * scale, -leapTransVec.z * scale);
							fingerObj.renderer.enabled = false;
							pointerObj.renderer.enabled = false;
							info = "translate";
							warning = "";
							cursor.transform.Translate (transVec, Space.World);
							cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
							                                         Mathf.Clamp(cursor.transform.position.y, 2.0f, yMax),
							                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
						}
						
					}
					else //fist
					{
						if(fingerObj.transform.position.z > zMax-3)
						{
							warning="too close\nto the screen";
							info = "";
						}
						else
						{
							warning = "";
							info = "hold";
						}
						
						fingerObj.renderer.enabled = false;
						pointerObj.renderer.enabled = false;

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