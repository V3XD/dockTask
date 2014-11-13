﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Chair: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess = false;
	Vector3 prevOrient = new Vector3();
	Vector3 prevOrientTarget = new Vector3();

	public GameObject trackedObj;

	protected override void atAwake ()
	{
		//path = folders.getPath()+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Chair.csv";
		//File.AppendAllText(path, columns+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
		selectLevel ();
		trialsType.setRealThing ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		setNewPositionAndOrientation();
		pointer.renderer.enabled = true;
		info = "";
		interaction = "MiniChair";

		if (bSuccess) 
		{
			connectionMessage="connected";
		}

		if (trialsType.currentGroup > trialsType.getRepetition())
		{
			nextLevel = "MainMenu";
			trialsType.currentGroup = 1;
		}
		else
			nextLevel = "optiChair";
	}
	
	protected override void gameBehavior ()
	{
		if (Input.GetKeyUp (KeyCode.Q) || Input.GetKeyUp (KeyCode.P))
		{
			action = false;
			clutchTime = clutchTime + Time.time - prevClutchTime; 
			clutchCn++;
		}
		else if(Input.GetKeyDown (KeyCode.Q) || Input.GetKeyDown (KeyCode.P))
		{
			if(!action)
			{
				prevClutchTime = Time.time; 
				action = true;
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

				Vector3 penOrient = currentOrient.eulerAngles;
				Vector3 rotVec = penOrient - prevOrient;
				prevOrient = penOrient;
				dominantAxis(rotVec, rotCntI);
				Vector3 rotVecTarget = cursor.transform.eulerAngles - prevOrientTarget;
				prevOrientTarget = rotVecTarget;
				dominantAxis(rotVecTarget, rotCntChair);

				if(action)
				{
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, yMin, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, zMin, zMax));
				}
				else if(isDocked && confirm)
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
