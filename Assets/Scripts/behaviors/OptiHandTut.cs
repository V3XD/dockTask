using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class OptiHandTut: Game 
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
	Vector3 prevTrans = new Vector3();

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
		interaction = "Fingers";
		setNewPositionAndOrientationTut();
		pointer.GetComponent<Renderer>().enabled = false;
		nextLevel = "optiHand";
		instructionsText.material.color = Color.gray;

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
				thumb.GetComponent<Renderer>().material = blue;
				index.GetComponent<Renderer>().material = blue;
				palm.GetComponent<Renderer>().material = blue;
				pointer.GetComponent<Renderer>().material = blue;

				Quaternion currentOrient = optiManager.getOrientation(0);
				palm.transform.position = optiManager.getPosition(0);
				
				Vector3 thumbPos = optiManager.getMarkerPosition(0);
				Vector3 indexPos = optiManager.getMarkerPosition(1);

				float thumbToIndex = Vector3.Distance(thumbPos, 
				                                      indexPos);

				Vector3 currentPos = (thumbPos + indexPos)*0.5f;
				Vector3 currentPosPalm = optiManager.getPosition(0);

				thumb.transform.position = thumbPos; 
				index.transform.position = indexPos; 

				float aveDist = thumbToIndex;

				Vector3 penOrient = currentOrient.eulerAngles;
				Vector3 fakeOrient = new Vector3 (penOrient.x, penOrient.y, 0f);
				
				pointer.transform.position = currentPos;
				pointer.transform.rotation = Quaternion.Euler(fakeOrient);//currentOrient;

				if( aveDist <= calibration.touchDist)
				{
					pointer.GetComponent<Renderer>().enabled = true;
					trail.GetComponent<TrailRenderer>().enabled = true;
					index.GetComponent<Renderer>().enabled = false;
					thumb.GetComponent<Renderer>().enabled = false;

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
						prevTrans = transVec;

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
					prevOrient = penOrient;
				}
				else
				{
					if(action)
					{
						action = false;
						clutchCn++;
						clutchTime = clutchTime + Time.time - prevClutchTime; 
						index.GetComponent<Renderer>().enabled = true;
						thumb.GetComponent<Renderer>().enabled = true;
						pointer.GetComponent<Renderer>().enabled = false;
						trail.GetComponent<TrailRenderer>().enabled = false;

						tapTime = Time.time - prevClutchTime;
						if(tapTime <= maxTapTime && isDocked)
						{
							//cursor.transform.Translate (-prevTrans, Space.World);
							confirm = true;
						}
					}


					if(isDocked && confirm)
					{
						newTask();
						setNewPositionAndOrientationTut();
						if(score == trialsType.getTrialNum())
							window = true;
					}
				}
			}
			else
			{
				thumb.GetComponent<Renderer>().material = red;
				index.GetComponent<Renderer>().material = red;
				palm.GetComponent<Renderer>().material = red;
				pointer.GetComponent<Renderer>().material = red;
				trail.GetComponent<TrailRenderer>().enabled = false;
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
