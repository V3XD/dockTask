﻿using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiHand: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess;

	public GameObject index;
	public GameObject thumb;
	public GameObject palm;
	public GameObject trail;

	OptiCalibration calibration;
	public Material blue;
	Vector3 prevOrient = new Vector3();
	Vector3 prevOrientTarget = new Vector3();

	protected override void atAwake ()
	{
		optiManager = OptiTrackManager.Instance;
		selectLevel ();
		trialsType.setRealThing ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();

		calibration = OptiCalibration.Instance;

		setNewPositionAndOrientation();
		pointer.renderer.enabled = true;
		interaction = "Fingers";

		if (trialsType.currentGroup > trialsType.getRepetition())
		{
			nextLevel = "MainMenu";
			trialsType.currentGroup = 1;
		}
		else
			nextLevel = "optiHand";

		if (bSuccess) 
		{
			connectionMessage="connected";
		}
	}
	
	protected override void gameBehavior ()
	{
		if(bSuccess)
		{
			if(optiManager.getMarkerNum() == 2 && optiManager.getRigidBodyNum() >= 1)
			{
				thumb.renderer.material = blue;
				index.renderer.material = blue;
				palm.renderer.material = blue;
				pointer.renderer.material = blue;

				Quaternion currentOrient = optiManager.getOrientation(0);
				palm.transform.position = optiManager.getPosition(0);
				
				Vector3 thumbPos = optiManager.getMarkerPosition(0);
				Vector3 indexPos = optiManager.getMarkerPosition(1);

				float thumbToIndex = Vector3.Distance(thumbPos, 
				                                      indexPos);

				Vector3 currentPos = (thumbPos + indexPos)*0.5f;

				thumb.transform.position = thumbPos; 
				index.transform.position = indexPos; 

				float aveDist = thumbToIndex;

				Vector3 penOrient = currentOrient.eulerAngles;
				Vector3 fakeOrient = new Vector3 (penOrient.x, penOrient.y, 0f);
				
				pointer.transform.position = currentPos;
				pointer.transform.rotation = Quaternion.Euler(fakeOrient);//currentOrient;
				


				if( aveDist <= calibration.touchDist)
				{
					pointer.renderer.enabled = true;
					trail.GetComponent<TrailRenderer>().enabled = true;
					index.renderer.enabled = false;
					thumb.renderer.enabled = false;

					if(!action)
					{
						prevClutchTime = Time.time; 
						action = true;
					}
					else
					{
						Vector3 transVec = currentPos - prevPos;
						Vector3 rotVec = penOrient - prevOrient;
						cursor.transform.Translate (transVec, Space.World);
						cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
						                                         Mathf.Clamp(cursor.transform.position.y, yMin, yMax),
						                                         Mathf.Clamp(cursor.transform.position.z, zMin, zMax));

						
						Vector3 zAxis = pointer.transform.TransformDirection(Vector3.forward);
						cursor.transform.RotateAround(cursor.transform.position, zAxis, rotVec.z);
						Vector3 xAxis = pointer.transform.TransformDirection(Vector3.right);
						cursor.transform.RotateAround(cursor.transform.position, xAxis, rotVec.x);
						Vector3 yAxis = pointer.transform.TransformDirection(Vector3.up); 
						cursor.transform.RotateAround(cursor.transform.position, yAxis, rotVec.y);
						dominantAxis(rotVec, rotCntI);
						Vector3 rotVecTarget = cursor.transform.eulerAngles - prevOrientTarget;
						prevOrientTarget = rotVecTarget;
						dominantAxis(rotVecTarget, rotCntChair);
					}
					prevOrient = penOrient;
					prevPos = currentPos;
				}
				else
				{
					if(action)
					{
						action = false;
						clutchCn++;
						clutchTime = clutchTime + Time.time - prevClutchTime; 
						index.renderer.enabled = true;
						thumb.renderer.enabled = true;
						pointer.renderer.enabled = false;
						trail.GetComponent<TrailRenderer>().enabled = false;

						tapTime = Time.time - prevClutchTime;
						if(tapTime <= maxTapTime && isDocked)
							confirm = true;
					}

					if(isDocked && confirm)
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
			{
				thumb.renderer.material = red;
				index.renderer.material = red;
				palm.renderer.material = red;
				pointer.renderer.material = red;
				trail.GetComponent<TrailRenderer>().enabled = false;
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
