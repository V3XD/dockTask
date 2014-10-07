﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class ChairTut: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess;

	public GameObject trackedObj;

	protected override void atAwake ()
	{
		/*path = folders.getPath()+@"tutorial/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_ChairTut.csv";
		File.AppendAllText(path, columns+ Environment.NewLine);//save to file*/
		optiManager = OptiTrackManager.Instance;
		difficulty.setEasy ();
		trialsType.setTutorial ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		interaction = "MiniChair";
		setNewPositionAndOrientationTut();
		pointer.renderer.enabled = true;
		info = "";
		nextLevel = "optiChair";

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
			clutchTime = clutchTime + Time.time - prevClutchTime; 
			info = "";
			
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			if(!action)
			{
				prevClutchTime = Time.time; 
				action = true;
				info = "translate";
			}
		}


		if(bSuccess)
		{
			if(optiManager.getRigidBodyNum() >= 1)
			{

				Vector3 currentPos = optiManager.getPosition(0);
				
				Quaternion currentOrient = optiManager.getOrientation(0);
				
				Vector3 transVec = currentPos - prevPos;

				trackedObj.renderer.enabled = false;
				if(action)
				{
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, yMin, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, zMin, zMax));
				}
				else if(isDocked)
				{
					newTask();
					setNewPositionAndOrientationTut();
					if(score == trialsType.getTrialNum())
						window = true;
				}
				
				cursor.transform.rotation = currentOrient;
				prevPos = currentPos;

			}
			else
			{
				trackedObj.renderer.enabled = true;
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{

	}
}
