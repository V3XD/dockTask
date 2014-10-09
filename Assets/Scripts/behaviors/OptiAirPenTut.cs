﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiAirPenTut : Game 
{
	
	OptiTrackManager optiManager;
	bool bSuccess;

	Vector3 prevOrient = new Vector3();

	protected override void atAwake ()
	{
		//path = folders.getPath()+@"tutorial/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_AirPenTut.csv";
		//File.AppendAllText(path, columns+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
		difficulty.setEasy ();
		trialsType.setTutorial ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		nextLevel = "optiAirPen";
		setNewPositionAndOrientationTut();
		pointer.renderer.enabled = true;
		interaction = "AirPen";
		
		if (bSuccess) 
		{
			connectionMessage="connected";
		}
	}
	
	protected override void gameBehavior ()
	{

		if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl))
		{
			action = false;
			info = "hold";
			clutchTime = clutchTime + Time.time - prevClutchTime; 
			clutchCn++;
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			if(!action)
			{
				prevClutchTime = Time.time; 
				action = true;
				info = "free";
			}
		}

		if(bSuccess)
		{
			if(optiManager.getRigidBodyNum() >= 1)
			{
				Vector3 currentPos = optiManager.getPosition(0);
				
				Quaternion currentOrient = optiManager.getOrientation(0);

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
					if(updateCam)
					{
						cursor.transform.Translate (transVec, Space.World);
						cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
						                                         Mathf.Clamp(cursor.transform.position.y, yMin, yMax),
						                                         Mathf.Clamp(cursor.transform.position.z, zMin, zMax));
					}
					
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
						if(score == trialsType.getTrialNum())
							window = true;
					}
				}
				prevPos = currentPos;
			}
		}
	}
	
	protected override void atEnd ()
	{
	}
}
