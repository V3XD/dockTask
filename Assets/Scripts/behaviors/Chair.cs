using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Chair: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess = false;

	public GameObject trackedObj;

	protected override void atAwake ()
	{
		path = folders.getPath()+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Chair.csv";
		File.AppendAllText(path, "Time,Distance,Angle,Difficulty"+ Environment.NewLine);//save to file
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
		if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
			prevTotalTime = Time.time;
			skipWindow = false;
		}

		if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl))
		{
			action = false;
			info = "";
			
		}
		else if(Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl))
		{
			action = true;
			info = "translate";
		}
		
		if(bSuccess)
		{

			if(optiManager.getRigidBodyNum() >= 1)
			{
				//info = "";

				Vector3 currentPos = optiManager.getPosition(0);

				Quaternion currentOrient = optiManager.getOrientation(0);

				Vector3 transVec = currentPos - prevPos;
				//Debug.Log(currentPos);

				//if(currentPos != Vector3.zero)
				//{
					trackedObj.renderer.enabled = false;
					if(action)
					{
						cursor.transform.Translate (transVec, Space.World);
						cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
						                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
						                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
					}
					else if(isDocked)
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
				/*}
				else
				{
					trackedObj.renderer.enabled = true;
				}*/
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
