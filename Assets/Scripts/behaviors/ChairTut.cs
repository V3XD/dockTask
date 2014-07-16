using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class ChairTut: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess;

	bool confirm = false;
	public GUIText completeText;

	protected override void atAwake ()
	{
		path = @"Log/tutorial/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_Chair.csv";
		File.AppendAllText(path, "Time,Distance,Angle"+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
		difficulty.setEasy ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();

		setNewPositionAndOrientationTut();
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
			setNewPositionAndOrientationTut();
			prevTotalTime = Time.time;
		}
		
		if (Input.GetKeyUp (KeyCode.Space))
		{
			confirm = true;
		}

		if (completeText.enabled ) 
		{
			if( (int)(Time.time - prevTotalTime) > 2)
				completeText.enabled  = false;
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
						setNewPositionAndOrientationTut();
						if(score == 5)
							completeText.enabled = true;
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

	}
}
