using UnityEngine;
using System;
using System.Collections;
using Leap;
using System.IO;
using System.Collections.Generic;

public class LeapTutorial : MonoBehaviour 
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
	public GUIText completeText;
	public GUIText repeatText;
	public GameObject hand;
	public GameObject pointHand;
	public GameObject pointHandTrail;
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
	bool updateCam;
	Vector3 fingerDir;
	Difficulty difficulty;
	bool auto;
	bool inView;
	bool isOriented;
	Vector3 orientDir;
	bool inPos;
	bool zOrient;
	bool yOrient;
	bool xOrient;
	bool isClose;
	Quaternion savedRot;
	Vector3 savedPos;
	int moveCount;
	Vector3 transVec;
	Vector3 targetPos;
	Sequence behavoirTree;
	bool isRotating;
	bool mute;
	bool locked;

	void Awake ()
	{
		difficulty = Difficulty.Instance;
		difficulty.setNormal();
		mController = new Controller();
		UnityEngine.Screen.showCursor = false;
		score = 0;
		List<Task> layer3 = new List<Task>();
		layer3.Add (new Leaf (orientCursorZ));
		layer3.Add (new Leaf (orientCursorY));
		layer3.Add (new Leaf (orientCursorX));
		Selector orientCursorS = new Selector (layer3);
		List<Task> layer2 = new List<Task>();
		layer2.Add (new Leaf (bringToView));
		layer2.Add (orientCursorS);
		layer2.Add (new Leaf (bringToTarget));
		Selector dock = new Selector (layer2);
		List<Task> layer = new List<Task>();
		layer.Add (new Leaf (renderFullHand));
		layer.Add (dock);
		behavoirTree = new Sequence (layer);
	}
	
	void Start ()
	{
		locked = false;
		mute = false;
		auto = false;
		inView = false;
		isOriented = false;
		inPos = false;
		Vector3 orientDir = new Vector3();
		zOrient = false;
		yOrient = false;
		xOrient = false;
		mLastFrame = new Frame();
		frame = new Frame();
		rotate = false;
		isDocked = false;
		translate = false;
		updateCam = false;
		moveCount = 0;
		completeText.enabled = false;
		repeatText.enabled = false;
		prevTime = 0;
		isClose = false;
		prevTotalTime = (int)Time.time;
		setNewPositionAndOrientation();
		isRotating = false;

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
			moveCount = 0;
		}
		else if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl))
		{
			rotate = false;
			moveCount++;
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			rotate = true;
			translate = false;
		}
		else if(Input.GetKeyUp (KeyCode.H))
		{
			if(!auto)
			{
				savedRot = cursor.transform.rotation;
				savedPos = cursor.transform.position;
				prevTotalTime = (int)Time.time;
			}
			auto = true;
		}

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
		if (repeatText.enabled ) 
		{
			if( ((int)Time.time - prevTotalTime) > 1)
				repeatText.enabled  = false;
		}
		if (completeText.enabled ) 
		{
			if( ((int)Time.time - prevTotalTime) > 2)
				completeText.enabled  = false;
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
			hand.renderer.enabled = false;
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
			pointHand.renderer.enabled = false;
			pointHandTrail.renderer.enabled = false;
		}
		
		updateCam = false;
		if (auto)
		{
			behavoirTree.run ();
		}
		else if (mController.IsConnected)
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
						pointHand.transform.rotation = Quaternion.LookRotation(fingerDir);
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
								Vector3 axisVec;

								Vector3 swipeVec = new Vector3(direction.x, direction.y, -direction.z);
								axisVec = Vector3.Cross(swipeVec, fingerDir);
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

								cursor.transform.RotateAround(cursor.transform.position, 
								                              axisVec, 
								                              direction.Magnitude);

							}
						}
					}
					else if(frame.Pointables.Count > 2)//full hand
					{
						if(frame.TranslationProbability(mLastFrame) > 0.60)
						{
							translate = true;
							Vector leapTransVec = frame.Translation (mLastFrame);
							transVec = new Vector3 (leapTransVec.x * scale, 
							                        leapTransVec.y * scale, 
							                         -leapTransVec.z * scale);
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
						//check if finger is too close to the screen
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
							prevTime = (int)Time.time - prevTotalTime;
							prevTotalTime = (int)Time.time;
							//check if has made one move
							if(moveCount<2 || score > 3)
							{
								pointText.enabled = true;
								score++;
							}
							else
							{
								repeatText.enabled = true;
								message = "Solve with one movement";
							}
							moveCount = 0;
							if(score == 5)
								completeText.enabled = true;
							setNewPositionAndOrientation();
						}
					}
					
					mLastFrame = frame;
				}
			}
		}else
			connectionMessage = "Not connected";
		evaluateDock();
	}

	//update camera depending on finger's position
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
				cursor.transform.rotation = UnityEngine.Random.rotation;
				break;
		}
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
		//increase volume as the cursor gets closer(orientation) to the target
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

	float clockDirection(float angleA, float angleB)
	{
		float angle = angleA;
		float result = 1f;
		if(angleA < angleB)
		{
			angle = angleA+360f;
		}
		if( (angle - angleB) > 180f)
			result = -1f;
		return result;
	}

	//render the hand if needed
	bool renderFullHand()
	{
		if(translate)
		{
			hand.transform.position = cursor.transform.position;
			hand.transform.Translate (new Vector3(0f,-4f,-3f), Space.World);
			hand.renderer.enabled = true;
		}
		return true;
	}

	//brings the cursor closer
	bool bringToView()
	{
		bool result = true;
		if(!inView && (score>0))
		{
			fingerObj.renderer.enabled = false;
			pointerObj.renderer.enabled = false;
			translate = true;
			warning = "";
			targetPos = new Vector3(0f,8.5f,-9f);
			transVec = targetPos - cursor.transform.position;
			transVec.Normalize();
			cursor.transform.Translate (transVec*scale, Space.World);
			if(Vector3.Distance(targetPos, cursor.transform.position) < 1f)
			{
				translate = false;
				fingerDir = new Vector3(-1f, 0f, 0f);
				sphere.renderer.enabled = true;
				rotate = true;
				pointHand.transform.rotation = Quaternion.LookRotation(fingerDir);
				pointHand.transform.position = new Vector3(10f,5f,0f);
				updateCam = true;
				orientDir = new Vector3 (0f, 0f, 1f);
				Quaternion targetQ = target.transform.rotation;
				Quaternion cursorQ = cursor.transform.rotation;
				
				orientDir.z = clockDirection(targetQ.eulerAngles.z, cursorQ.eulerAngles.z);
				if(cursor.transform.forward.z <0)
				{
					orientDir.z = -orientDir.z;
				}
				pointHand.renderer.enabled = true;
				pointHandTrail.renderer.enabled = true;

				inView = true;
			}
		}
		else
			result = false;
		return result;
	}

	//orient the cursor around the Z axis
	bool orientCursorZ()
	{
		bool result = true;
		if(!isOriented)
		{

			Quaternion targetQ = target.transform.rotation;
			Quaternion cursorQ = cursor.transform.rotation;
			sphere.transform.position = cursor.transform.position;
			
			transVec = Vector3.Cross(fingerDir, orientDir);
			transVec.Normalize();

			if(!isRotating)
			{
				isRotating = true;
				StartCoroutine(rotateSlowly());
			}

			if(!zOrient )
			{
				bool stop = false;
				if(cursorQ.eulerAngles.z < (targetQ.eulerAngles.z +10f) || cursorQ.eulerAngles.z > (targetQ.eulerAngles.z+360f -10f))
				{
					isClose = true;
				}
				else if(isClose)
				{
					stop = true;
				}
				if(cursorQ.eulerAngles.z < (targetQ.eulerAngles.z +3f) || cursorQ.eulerAngles.z > (targetQ.eulerAngles.z+360f -3f) || stop)
				{
					zOrient = true;
					orientDir = new Vector3(0f,1f,0f);
					fingerDir = new Vector3(0f, 0f, 1f);
					pointHand.transform.rotation = Quaternion.LookRotation(fingerDir);
					updateCam = true;
					pointHand.transform.position = new Vector3(0f,5f,-15f);
					orientDir.y = clockDirection(targetQ.eulerAngles.y, cursorQ.eulerAngles.y);
					isClose = false;
				}
			}
			else
				result = false;
		}
		else
			result = false;
		return result;
	}

	//orient the cursor around the Y axis
	bool orientCursorY()
	{
		bool result = true;
		if(!isOriented)
		{
			Quaternion targetQ = target.transform.rotation;
			Quaternion cursorQ = cursor.transform.rotation;
			sphere.transform.position = cursor.transform.position;
			
			transVec = Vector3.Cross(fingerDir, orientDir);
			transVec.Normalize();

			if(!isRotating)
			{
				isRotating = true;
				StartCoroutine(rotateSlowly());
			}

			if( !yOrient)
			{
				if( cursorQ.eulerAngles.y < (targetQ.eulerAngles.y +3f) || cursorQ.eulerAngles.y > (targetQ.eulerAngles.y+360f -3f))
				{
					yOrient = true;
					orientDir = new Vector3(1f,0f,0f);
					pointHand.transform.position = new Vector3(0f,5f,-15f);
					orientDir.x = clockDirection(targetQ.eulerAngles.x, cursorQ.eulerAngles.x);
				}
			}
			else
				result = false;
		}
		else
			result = false;
		return result;
	}

	//orient the cursor around the X axis
	bool orientCursorX()
	{
		bool result = true;
		if(!isOriented)
		{
			Quaternion targetQ = target.transform.rotation;
			Quaternion cursorQ = cursor.transform.rotation;
			sphere.transform.position = cursor.transform.position;
			
			transVec = Vector3.Cross(fingerDir, orientDir);
			transVec.Normalize();

			if(!isRotating)
			{
				isRotating = true;
				StartCoroutine(rotateSlowly());
			}

			if(!xOrient)
			{
				if( cursorQ.eulerAngles.x < (targetQ.eulerAngles.x +3f) || cursorQ.eulerAngles.x > (targetQ.eulerAngles.x+360f -3f))
				{
					xOrient = true;
					isOriented = true;
					fingerObj.renderer.enabled = false;
					pointerObj.renderer.enabled = false;
					pointHand.renderer.enabled = false;
					pointHandTrail.renderer.enabled = false;
					rotate = false;
				}	
			}
			else
				result = false;
		}
		else
			result = false;
		return result;
	}

	//translates cursor to target
	bool bringToTarget()
	{
		bool result = true;
		if(!inPos)
		{
			translate = true;
			warning = "";
			targetPos = target.transform.position;
			transVec = targetPos - cursor.transform.position;
			transVec.Normalize();
			cursor.transform.Translate (transVec*scale, Space.World);
			if(Vector3.Distance(targetPos, cursor.transform.position) < 0.5f)
			{
				inPos = true;
				auto = false;
				inView = false;
				isOriented = false;
				inPos = false;
				zOrient = false;
				yOrient = false;
				xOrient = false;
				isClose = false;
				cursor.transform.rotation = savedRot;
				cursor.transform.position = savedPos;
				prevTime = (int)Time.time - prevTotalTime;
				prevTotalTime = (int)Time.time;
				translate = false;
				info = "hold";
			}
		}
		else
			result = false;
		return result;
	}

	IEnumerator rotateSlowly() 
	{
		pointHand.transform.Translate (transVec* scale, Space.World);
		cursor.transform.Rotate(orientDir, Space.World);
		yield return new WaitForSeconds(0.01f);
		isRotating = false;
	}
}