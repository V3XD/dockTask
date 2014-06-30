using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Chair: Game 
{
	
	OptiTrackUDPClient udpClient;
	bool bSuccess;
	Skeleton skelPerformer = new Skeleton();
	
	bool confirm = false;

	protected override void atAwake ()
	{
		path = @"Log/"+difficulty.getLevel()+"/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_Chair.csv";
		File.AppendAllText(path, "Time,Distance,Angle"+ Environment.NewLine);//save to file
	}
	
	protected override void atStart ()
	{
		udpClient = new OptiTrackUDPClient();
		bSuccess = udpClient.Connect();
		udpClient.skelTarget = skelPerformer;
		
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
			udpClient.RequestDataDescriptions();
			
			if(udpClient.numTrackables >= 1)
			{
				info = "";
				Vector3 currentPos = new Vector3(udpClient.rigidTargets[0].pos.x,
				                                 udpClient.rigidTargets[0].pos.y,
				                                 -udpClient.rigidTargets[0].pos.z);
				
				Debug.Log(currentPos);
				Quaternion currentOrient = udpClient.rigidTargets[0].ori;
				Vector3 euler = currentOrient.eulerAngles;
				
				Quaternion rotation = Quaternion.Euler(-euler.x, -euler.y, euler.z);
				
				Vector3 transVec = currentPos - prevPos;
				
				if(currentPos != Vector3.zero)
				{
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
					cursor.transform.rotation = rotation;
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
				Debug.Log("not tracked");
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
		udpClient.Close();
	}
}
