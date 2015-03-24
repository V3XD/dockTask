using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiAirPenTut : Game 
{
	
	OptiTrackManager optiManager;
	bool bSuccess;
	
	OptiCalibration calibration;
	Vector3 prevOrient = new Vector3();
	Vector3 prevOrientTarget = new Vector3();

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
		nextLevel = "optiAirPen";
		setNewPositionAndOrientationTut();
		pointer.GetComponent<Renderer>().enabled = true;
		interaction = "AirPen";
		
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

				Vector3 transVec = currentPos - prevPos;
				
				Vector3 penOrient = currentOrient.eulerAngles;
				Vector3 fakeOrient = new Vector3 (penOrient.x, penOrient.y, 0f);
				
				pointer.transform.position = currentPos;
				pointer.transform.rotation = Quaternion.Euler(fakeOrient);//currentOrient;
				
				Vector3 rotVec = penOrient - prevOrient;
				prevOrient = penOrient;
				
				if(thumbToIndex <= calibration.touchDist)
				{
					pointer.GetComponent<Renderer>().material = trueGreen; 

					if(!action)
					{
						prevClutchTime = Time.time; 
						action = true;
					}
					else
					{
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
					pointer.GetComponent<Renderer>().material = yellow;
					if(isDocked && confirm)
					{
						newTask();
						setNewPositionAndOrientationTut();
						if(score == trialsType.getTrialNum())
							window = true;
					}
				}
			}
		}
	}
	
	protected override void atEnd ()
	{
	}
}
