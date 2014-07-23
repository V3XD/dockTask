using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiAirPenTut : Game 
{
	
	OptiTrackManager optiManager;
	bool bSuccess;

	bool action = false;
	Vector3 prevOrient = new Vector3();
	public GameObject pen;
	public Material brown;

	protected override void atAwake ()
	{
		path = folders.getPath()+@"tutorial/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_AirPenTut.csv";
		File.AppendAllText(path, "Time,Distance,Angle,Difficulty"+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
		difficulty.setEasy ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		nextLevel = "optiAirPen";
		setNewPositionAndOrientationTut();
		pointer.renderer.enabled = true;
		
		if (bSuccess) 
		{
			connectionMessage="connected";
		}
	}
	
	protected override void gameBehavior ()
	{
		if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientationTut();
			prevTotalTime = Time.time;
		}
		
		if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl))
		{
			action = false;
			info = "hold";
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			action = true;
			info = "free";
		}

		if(bSuccess)
		{
			if(optiManager.getRigidBodyNum() >= 1)
			{
				Vector3 currentPos = optiManager.getPosition(0);
				
				Quaternion currentOrient = optiManager.getOrientation(0);
				
				//if(currentPos != Vector3.zero)
				//{
					Vector3 transVec = currentPos - prevPos;
					
					Vector3 penOrient = currentOrient.eulerAngles;
					Vector3 fakeOrient = new Vector3 (penOrient.x, penOrient.y, 0f);
					
					pointer.transform.position = currentPos;
					pointer.transform.rotation = Quaternion.Euler(fakeOrient);//currentOrient;
					
					Vector3 rotVec = penOrient - prevOrient;
					prevOrient = penOrient;
					
					if(action)
					{
						pointer.renderer.material = green; 
						cursor.transform.Translate (transVec, Space.World);
						cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
						                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
						                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
						
						Vector3 zAxis = pointer.transform.TransformDirection(Vector3.forward);
						cursor.transform.RotateAround(cursor.transform.position, zAxis, rotVec.z);
						Vector3 xAxis = pointer.transform.TransformDirection(Vector3.right);
						cursor.transform.RotateAround(cursor.transform.position, xAxis, rotVec.x);
						Vector3 yAxis = pointer.transform.TransformDirection(Vector3.up); 
						cursor.transform.RotateAround(cursor.transform.position, yAxis, rotVec.y);
					}
					else
					{
						pointer.renderer.material = yellow;
						if(isDocked)
						{
							newTask();
							setNewPositionAndOrientationTut();
							if(score == 5)
								window = true;
						}
					}
					prevPos = currentPos;
				//}
			}
			else
			{
				pen.renderer.material = brown;
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
