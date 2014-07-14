using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Chair: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess;

	bool confirm = false;

	protected override void atAwake ()
	{
		path = @"Log/"+difficulty.getLevel()+"/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_Chair.csv";
		File.AppendAllText(path, "Time,Distance,Angle"+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();
		setNewPositionAndOrientation();
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
			setNewPositionAndOrientation();
			prevTotalTime = Time.time;
		}
		
		if (Input.GetKeyUp (KeyCode.Space))
		{
			confirm = true;
		}
		
		if(bSuccess)
		{

			if(optiManager.getRigidBodyNum() >= 1)
			{
				info = "";
				Vector3 currentPos = optiManager.getPosition(0);

				Quaternion currentOrient = optiManager.getOrientation(0);

				Vector3 transVec = currentPos - prevPos;
				
				if(currentPos != Vector3.zero)
				{
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
					cursor.transform.rotation = currentOrient;
					prevPos = currentPos;
				}
				
				if(confirm)
				{
					if(isDocked)
					{
						newTask();
						setNewPositionAndOrientation();
					}
					confirm = false;
				}
			}
			else
			{
				info = "not tracked";
				//Debug.Log("not tracked");
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
