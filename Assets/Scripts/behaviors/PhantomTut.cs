using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

public class PhantomTut : Game 
{
	Vector3 prevOrient = new Vector3();
	bool grab = false;
	bool isConnected = false;
	static float scale = 0.10f;

	public GameObject j2;
	public GameObject j3;
	public GameObject j4;
	public GUIText completeText;

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
	
	protected override void atAwake ()
	{
		path = folders.getPath()+@"tutorial/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Phantom.csv";
		File.AppendAllText(path, "Time,Distance,Angle,Difficulty"+ Environment.NewLine);//save to file
		difficulty.setEasy ();
	}

	protected override void atStart ()
	{
		nextLevel = "phantomGrab";
		setNewPositionAndOrientationTut();
		pointer.renderer.enabled = true;

		isConnected = initDevice ();
		if(isConnected)
		{
			connectionMessage = "connected";
			getData ();
			prevPos = new Vector3 ( (float)getPosX()*scale, 
			                            (float)getPosY()*scale, 
			                            -(float)getPosZ()*scale);
			prevOrient = new Vector3((float)gimbalY()* Mathf.Rad2Deg,
			                         -(float)gimbalX()* Mathf.Rad2Deg, 
			                         -(float)gimbalZ()* Mathf.Rad2Deg);
		}
	}

	protected override void gameBehavior ()
	{
		if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
			prevTotalTime = Time.time;
		}

		if (completeText.enabled ) 
		{
			if( (int)(Time.time - prevTotalTime) > 2)
				completeText.enabled  = false;
		}

		isConnected = getData ();
		if(isConnected)
		{
			connectionMessage = "connected";
			Vector3 newPos = new Vector3 ( (float)getPosX()*scale, 
			                              (float)getPosY()*scale, 
			                              -(float)getPosZ()*scale);
			
			Vector3 transVec = newPos - prevPos;
			prevPos = newPos;
			
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
			
			Vector3 fakeOrient = new Vector3 (penOrient.x, penOrient.y, 0f);
			rotation = Quaternion.Euler(fakeOrient);
			j4.transform.localRotation = rotation;
			
			Vector3 rotVec = penOrient - prevOrient;
			prevOrient = penOrient;
			
			if(isButtonADown() || isButtonBDown())
			{
				grab = true;
				cursor.transform.Translate (transVec, Space.World);
				Vector3 zAxis = j4.transform.TransformDirection(Vector3.forward);
				cursor.transform.RotateAround(cursor.transform.position, zAxis, rotVec.z);
				Vector3 xAxis = j4.transform.TransformDirection(Vector3.right);
				cursor.transform.RotateAround(cursor.transform.position, xAxis, rotVec.x);
				Vector3 yAxis = j4.transform.TransformDirection(Vector3.up);
				cursor.transform.RotateAround(cursor.transform.position, yAxis, rotVec.y);
				info = "grabbed";
				j4.renderer.material = green;
			}
			else
			{
				grab = false;
				info = "not grabbed";
				j4.renderer.material = yellow;
			}
			
			if(isDocked && !grab)
			{
				newTask();
				setNewPositionAndOrientationTut();
				if(score == 5)
					window = true;
			}
		}
		else
			connectionMessage = "not connected";
	}
	
	protected override void atEnd ()
	{
		if(isConnected)
			cleanup ();
	}
}
