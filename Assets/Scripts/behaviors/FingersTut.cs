using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class FingersTut: Game 
{
	OptiTrackManager optiManager;
	bool bSuccess;

	public GameObject index;
	public GameObject thumb;
	public GameObject ring;
	public GameObject axis;
	public GameObject trail;
	public GUIText completeText;
	public GUIText instructionsText;

	OptiCalibration calibration;
	bool translate = false;
	bool rotate = false;
	Vector3 prevPinch = new Vector3 ();
	bool isCalibrated = false;
	float maxDist = 0;

	protected override void atAwake ()
	{
		path = @"Log/tutorial/"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+difficulty.getLevel()+"_FingerTut.csv";
		File.AppendAllText(path, "Time,Distance,Angle"+ Environment.NewLine);//save to file
		optiManager = OptiTrackManager.Instance;
		difficulty.setEasy ();
	}
	
	protected override void atStart ()
	{
		bSuccess = optiManager.isConnected ();

		calibration = OptiCalibration.Instance;

		setNewPositionAndOrientationTut();
		pointer.renderer.enabled = true;
		instructionsText.material.color = Color.gray;

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

		if (completeText.enabled ) 
		{
			if( (int)(Time.time - prevTotalTime) > 2)
				completeText.enabled  = false;
		}
		
		if(translate)
		{
			rotate = false;
			//pointer.GetComponent<AudioSource>().mute = true;
			info = "translate";
			axis.transform.position = cursor.transform.position;
			foreach(Transform child in axis.transform) 
			{
				child.renderer.enabled = true;
			}
			index.renderer.enabled = false;
			thumb.renderer.enabled = false;
			ring.renderer.enabled = false;
		}
		else
		{	
			//pointer.GetComponent<AudioSource>().mute = false;
			foreach(Transform child in axis.transform) 
			{
				child.renderer.enabled = false;
			}
			
		}
		
		if(rotate)
		{
			info = "rotate";
			translate = false;
			pointer.renderer.enabled = true;
			trail.GetComponent<TrailRenderer>().enabled = true;
			index.renderer.enabled = false;
			thumb.renderer.enabled = false;
			ring.renderer.enabled = false;
			cursor.renderer.enabled = true;
			
		}
		else
		{	
			cursor.renderer.enabled = false;
			trail.GetComponent<TrailRenderer>().enabled = false;
			prevPinch = new Vector3 ();
			pointer.renderer.enabled = false;
		}

		
		if(bSuccess)
		{
			if(optiManager.getMarkerNum() == 3)
			{
				thumb.renderer.material = green;
				index.renderer.material = green;
				ring.renderer.material = green;
				pointer.renderer.material = green; 
				
				Vector3 thumbPos = optiManager.getMarkerPosition(0);
				Vector3 indexPos = optiManager.getMarkerPosition(1);
				Vector3 ringPos = optiManager.getMarkerPosition(2);
				
				float thumbToIndex = Vector3.Distance(thumbPos, 
				                                      indexPos);
				float thumbToRing = Vector3.Distance(thumbPos, 
				                                     ringPos);
				float indexToRing = Vector3.Distance(indexPos, 
				                                     ringPos);

				Vector3 currentPos = (thumbPos + indexPos + ringPos)*0.33f;
				Vector3 transVec = currentPos - prevPos;

				thumb.transform.position = thumbPos; 
				index.transform.position = indexPos; 
				ring.transform.position = ringPos;

				if(!isCalibrated)
				{
					int count = (int)(Time.time - prevTotalTime);
					instructionsText.text = "Grab "+ (10-count).ToString();
					if( count > 5)
					{
						instructionsText.material.color = Color.white;
						if(thumbToIndex > maxDist)
							maxDist = thumbToIndex;
						if(thumbToRing > maxDist)
							maxDist = thumbToRing;
						if(indexToRing > maxDist)
							maxDist = indexToRing;
					}
					
					if( count > 10)
					{
						instructionsText.enabled = false;
						isCalibrated = true;
						
						calibration.setTouchDist(maxDist);
						//Debug.Log("************ "+calibration.touchDist +" " + calibration.minDist);
						prevTotalTime = Time.time;
					}
				}
				else if(thumbToIndex < calibration.touchDist && thumbToRing < calibration.touchDist && indexToRing < calibration.touchDist)
				{
					translate = true;
					
					cursor.transform.Translate (transVec, Space.World);
					cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
					                                         Mathf.Clamp(cursor.transform.position.y, 3.0f, yMax),
					                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
				}
				else if((thumbToIndex < calibration.touchDist && thumbToRing > calibration.minDist && indexToRing > calibration.minDist) ||
				        (thumbToIndex > calibration.minDist && thumbToRing < calibration.touchDist && indexToRing > calibration.minDist) ||
				        (thumbToIndex > calibration.minDist && thumbToRing > calibration.minDist && indexToRing < calibration.touchDist))
				{
					rotate = true;
					Vector3 pointerPos = new Vector3();
					
					if(thumbToIndex < calibration.touchDist)
						pointerPos = (thumb.transform.position + index.transform.position)*0.5f;
					else if (thumbToRing < calibration.touchDist)
						pointerPos = (thumb.transform.position + ring.transform.position)*0.5f;
					else 
						pointerPos = (index.transform.position + ring.transform.position)*0.5f;

					pointer.transform.position = pointerPos;//closestPoint
					
					Vector3 to = pointerPos - cursor.transform.position;
					to.Normalize();
					Vector3 axisVec = Vector3.Cross(prevPinch, to);
					cursor.transform.RotateAround(cursor.transform.position, axisVec, Vector3.Angle(prevPinch, to));
					prevPinch = to;
				}
				else
				{
					translate = false;
					rotate = false;
					index.renderer.enabled = true;
					thumb.renderer.enabled = true;
					ring.renderer.enabled = true;
					info = "hold";
					pointer.transform.position = currentPos;
					
					if(isDocked)
					{
						newTask();
						setNewPositionAndOrientationTut();
						if(score == 5)
							completeText.enabled = true;
						
					}
				}
				prevPos = currentPos;
			}
			else
			{
				translate = false;
				rotate = false;

				thumb.renderer.material = yellow;
				index.renderer.material = yellow;
				ring.renderer.material = yellow;
				pointer.renderer.material = yellow;
				//Debug.Log("not tracked");
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
