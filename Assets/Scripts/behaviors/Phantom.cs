using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

public class Phantom : Game 
{
	Vector3 prevOrient = new Vector3();
	Vector3 prevOrientTarget = new Vector3();
	bool isConnected = false;
	static float scale = 0.10f;

	public GameObject j2;
	public GameObject j3;
	public GameObject j4;

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
		/*path = folders.getPath()+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Phantom.csv";
		File.AppendAllText(path, columns+ Environment.NewLine);//save to file*/
		selectLevel ();
		trialsType.setRealThing ();
	}

	protected override void atStart ()
	{

		setNewPositionAndOrientation();
		pointer.renderer.enabled = true;
		interaction = "Phantom";
		isConnected = initDevice ();
		if(isConnected)
		{
			getData ();
			connectionMessage = "connected";
			prevPos = new Vector3 ( (float)getPosX()*scale, 
			                            (float)getPosY()*scale, 
			                            -(float)getPosZ()*scale);
			prevOrient = new Vector3((float)gimbalY()* Mathf.Rad2Deg,
			                         -(float)gimbalX()* Mathf.Rad2Deg, 
			                         -(float)gimbalZ()* Mathf.Rad2Deg);
		}

		if (trialsType.currentGroup > trialsType.getRepetition())
		{
			nextLevel = "MainMenu";
			trialsType.currentGroup = 1;
		}
		else
			nextLevel = "phantomGrab";
	}

	protected override void gameBehavior ()
	{
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

			if(isButtonBDown())
			{
				if(!action)
				{
					prevClutchTime = Time.time; 
					action = true;
				}

				cursor.transform.Translate (transVec, Space.World);
				cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
				                                         Mathf.Clamp(cursor.transform.position.y, yMin, yMax),
				                                         Mathf.Clamp(cursor.transform.position.z, zMin, zMax));

				Vector3 zAxis = j4.transform.TransformDirection(Vector3.forward);
				cursor.transform.RotateAround(cursor.transform.position, zAxis, rotVec.z);
				Vector3 xAxis = j4.transform.TransformDirection(Vector3.right);
				cursor.transform.RotateAround(cursor.transform.position, xAxis, rotVec.x);
				Vector3 yAxis = j4.transform.TransformDirection(Vector3.up);
				cursor.transform.RotateAround(cursor.transform.position, yAxis, rotVec.y);
				j4.renderer.material = green;
				dominantAxis(rotVec, rotCntI);
				Vector3 rotVecTarget = cursor.transform.eulerAngles - prevOrientTarget;
				prevOrientTarget = rotVecTarget;
				dominantAxis(rotVecTarget, rotCntChair);
			}
			else if (action)
			{
				clutchTime = clutchTime + Time.time - prevClutchTime; 
				action = false;
				clutchCn++;
				j4.renderer.material = yellow;
			}
			
			if(isDocked)
			{
				if(isButtonADown())
					confirm = true;
				if(!action && confirm)
				{
					newTask();
					setNewPositionAndOrientation();
					selectLevel();
					if(score == trialsType.getTrialNum())
					{
						trialsType.currentGroup++;
						window = true;
					}
				}
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
