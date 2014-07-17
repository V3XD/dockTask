using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Chair: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess = false;

	bool action = false;

	protected override void atAwake ()
	{
		path = folders.getPath()+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_Chair.csv";
		File.AppendAllText(path, "Time,Distance,Angle,Difficulty"+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
		selectLevel ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		setNewPositionAndOrientation();
		pointer.renderer.enabled = true;
		info = "hold";

		if (bSuccess) 
		{
			connectionMessage="connected";
		}
	}
	
	protected override void gameBehavior ()
	{
		if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
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
				
				if(currentPos != Vector3.zero)
				{
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
						if(score == 9)
							window = true;
					}

					cursor.transform.rotation = currentOrient;
					prevPos = currentPos;
				}
			}
			/*else
			{
				info = "not tracked";
				//Debug.Log("not tracked");
			}*/
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
