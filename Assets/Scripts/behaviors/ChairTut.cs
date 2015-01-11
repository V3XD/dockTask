using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class ChairTut: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess;
	Vector3 prevOrient = new Vector3();
	Vector3 prevOrientTarget = new Vector3();

	public GameObject trackedObj;
	
	OptiCalibration calibration;

	protected override void atAwake ()
	{
		optiManager = OptiTrackManager.Instance;
		difficulty.setEasy ();
		trialsType.setTutorial ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		calibration = OptiCalibration.Instance;
		interaction = "MiniChair";
		setNewPositionAndOrientationTut();
		pointer.renderer.enabled = true;
		nextLevel = "optiChair";

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

				Vector3 currentPos = optiManager.getPosition(0);
				
				Quaternion currentOrient = optiManager.getOrientation(0);

				Vector3 thumbPos = optiManager.getMarkerPosition(0);
				Vector3 indexPos = optiManager.getMarkerPosition(1);
				
				float thumbToIndex = Vector3.Distance(thumbPos, 
				                                      indexPos);
				
				trackedObj.renderer.enabled = false;

				Vector3 penOrient = currentOrient.eulerAngles;
				Vector3 rotVec = penOrient - prevOrient;
				prevOrient = penOrient;
				dominantAxis(rotVec, rotCntI);
				Vector3 rotVecTarget = cursor.transform.eulerAngles - prevOrientTarget;
				prevOrientTarget = rotVecTarget;
				dominantAxis(rotVecTarget, rotCntChair);

				if(thumbToIndex <= calibration.touchDist)
				{
					if(!action)
					{
						prevClutchTime = Time.time; 
						action = true;
					}
					else
					{
						Vector3 transVec = currentPos - prevPos;
						cursor.transform.Translate (transVec, Space.World);
						cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
						                                         Mathf.Clamp(cursor.transform.position.y, yMin, yMax),
						                                         Mathf.Clamp(cursor.transform.position.z, zMin, zMax));
					}
					prevPos = currentPos;
				}
				else 
				{
					if(action)
					{
						action = false;
						clutchCn++;
						clutchTime = clutchTime + Time.time - prevClutchTime; 

						tapTime = Time.time - prevClutchTime;
						if(tapTime <= maxTapTime && isDocked)
							confirm = true;
					}
					if(isDocked && confirm)
					{
						newTask();
						setNewPositionAndOrientationTut();
						if(score == trialsType.getTrialNum())
							window = true;
					}
				}
				
				cursor.transform.rotation = currentOrient;
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
