using UnityEngine;
using System;
using System.Collections;
using Leap;
using System.IO;

public class LeapPinch : Game 
{
	public GameObject axis;
	public GameObject trail;

	public GameObject indexObj;
	public GameObject thumbObj;

	private Controller mController;
	private Frame mLastFrame;
	bool rotate;
	bool translate;
	private Frame frame;
	Vector3 fingerDir;

	Vector3 prevPinch = new Vector3 ();
	static float chairRadius = 5f;
	static float scale = 0.1f;

	protected override void atAwake ()
	{
		mController = new Controller();
		path = folders.getPath()+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+"_LeapPinch.csv";
		File.AppendAllText(path, "Time,Distance,Angle,Difficulty"+ Environment.NewLine);//save to file
		selectLevel ();
	}
	
	protected override void atStart ()
	{
		mLastFrame = new Frame();
		frame = new Frame();
		rotate = false;
		isDocked = false;
		translate = false;
		prevTotalTime = (int)Time.time;
		setNewPositionAndOrientation();

		if(mController.IsConnected)
		{
			connectionMessage = "Leap connected";
			mLastFrame = mController.Frame();
		}
	}
	
	protected override void gameBehavior ()
	{
	
		if (Input.GetKeyUp (KeyCode.S))
		{
			setNewPositionAndOrientation();
			prevTotalTime = (int)Time.time;
			skipWindow = false;
		}

		if(translate)
		{
			rotate = false;
			pointer.GetComponent<AudioSource>().mute = true;
			info = "translate";
			axis.transform.position = cursor.transform.position;
			foreach(Transform child in axis.transform) 
			{
				child.renderer.enabled = true;
			}
			indexObj.renderer.enabled = false;
			thumbObj.renderer.enabled = false;
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
			indexObj.renderer.enabled = false;
			thumbObj.renderer.enabled = false;
			cursor.renderer.enabled = true;
			targetSphere.renderer.enabled = true;
			
		}
		else
		{	
			cursor.renderer.enabled = false;
			trail.GetComponent<TrailRenderer>().enabled = false;
			prevPinch = new Vector3 ();
			pointer.renderer.enabled = false;
			targetSphere.renderer.enabled = false;
		}
		if (mController.IsConnected)
		{
			connectionMessage = "Connected";
			frame = mController.Frame ();
			if(frame.IsValid)
			{
				if(frame.Id != mLastFrame.Id)
				{
					InteractionBox iBox = frame.InteractionBox;
					/*Pointable finger = frame.Pointables.Frontmost;
					Vector normalizedPos = iBox.NormalizePoint(finger.StabilizedTipPosition);
					fingerObj.transform.position = new Vector3 ( ((normalizedPos.x*2.0f)-1.0f) * xMax,
					                                            normalizedPos.y * yMax,
					                                            ((normalizedPos.z*2.0f)-1.0f) * -zMax);
					Vector direction = frame.Translation (mLastFrame);*/
					Hand firstHand = frame.Hands[0];
					Finger thumb = firstHand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0];
					Vector normalizedThumbPos = iBox.NormalizePoint(thumb.StabilizedTipPosition);
					/*Vector3 thumbPos = new Vector3 ( ((normalizedThumbPos.x*2.0f)-1.0f) * xMax,
					                                normalizedThumbPos.y * yMax,
					                                ((normalizedThumbPos.z*2.0f)-1.0f) * -zMax);*/
					Vector3 thumbPos = new Vector3 ( ((normalizedThumbPos.x*2.0f)-1.0f) * xMax,
					                                ((normalizedThumbPos.y*2.0f)-1.0f) * yMax,
					                                ((normalizedThumbPos.z*2.0f)-1.0f) * -zMax);

					Finger index = firstHand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];
					Vector normalizedIndexPos = iBox.NormalizePoint(index.StabilizedTipPosition);
					/*Vector3 indexPos = new Vector3 ( ((normalizedIndexPos.x*2.0f)-1.0f) * xMax,
					                                normalizedIndexPos.y * yMax,
					                                ((normalizedIndexPos.z*2.0f)-1.0f) * -zMax);*/
					Vector3 indexPos = new Vector3 ( ((normalizedIndexPos.x*2.0f)-1.0f) * xMax,
					                                ((normalizedIndexPos.y*2.0f)-1.0f) * yMax,
					                                ((normalizedIndexPos.z*2.0f)-1.0f) * -zMax);

					float thumbToIndex = Vector3.Distance( thumbPos, indexPos);
					//Debug.Log(indexPos + " thumb "+thumbPos + " dist "+ thumbToIndex);



					fingerDir = new Vector3 (thumb.Direction.x, thumb.Direction.y, -thumb.Direction.z);
					fingerDir.Normalize();

					thumbPos = thumbPos + cursor.transform.position;
					indexPos = indexPos + cursor.transform.position;
	
					Vector3 clampedThumb = new Vector3 (Mathf.Clamp(thumbPos.x, cursor.transform.position.x-chairRadius, cursor.transform.position.x+chairRadius),
					                                   Mathf.Clamp(thumbPos.y, cursor.transform.position.y-chairRadius, cursor.transform.position.y+chairRadius),
					                                   Mathf.Clamp(thumbPos.z, cursor.transform.position.z-chairRadius, cursor.transform.position.z+chairRadius));
					thumbObj.transform.position = clampedThumb;

					Vector3 clampedIndex = new Vector3 (Mathf.Clamp(indexPos.x, cursor.transform.position.x-chairRadius, cursor.transform.position.x+chairRadius),
					                                    Mathf.Clamp(indexPos.y, cursor.transform.position.y-chairRadius, cursor.transform.position.y+chairRadius),
					                                    Mathf.Clamp(indexPos.z, cursor.transform.position.z-chairRadius, cursor.transform.position.z+chairRadius));
					indexObj.transform.position = clampedIndex;

					Vector3 avgPos = (thumbPos+indexPos)*0.5f;
					Vector3 to = avgPos - cursor.transform.position;
					to.Normalize();

					Debug.Log(firstHand.PinchStrength + " grab "+firstHand.GrabStrength + " dist " +thumbToIndex);
					pointer.transform.position = (clampedThumb+clampedIndex)*0.5f;

					if(firstHand.GrabStrength == 1f)//translation
					{
						rotate = false;
						translate = true;

						if(frame.TranslationProbability(mLastFrame) > 0.60)
						{
							Vector leapTransVec = frame.Translation (mLastFrame);
							Vector3 transVec = new Vector3 (leapTransVec.x * scale, leapTransVec.y * scale, -leapTransVec.z * scale);

							cursor.transform.Translate (transVec, Space.World);
							cursor.transform.position = new Vector3 (Mathf.Clamp(cursor.transform.position.x, -xMax, xMax),
							                                         Mathf.Clamp(cursor.transform.position.y, 2.0f, yMax),
							                                         Mathf.Clamp(cursor.transform.position.z, -zMax, zMax));
						}
						
					}
					else if(thumbToIndex < 3.5f)//~rotation//if(firstHand.PinchStrength > 0.2f)//~rotation
					{
						translate = false;	
						rotate = true;
						
						//pointer.renderer.material = green;
							
						//if(frame.TranslationProbability(mLastFrame) > 0.60)
						//{
							
							Vector3 axisVec = Vector3.Cross(prevPinch, to);
							cursor.transform.RotateAround(cursor.transform.position, axisVec, Vector3.Angle(prevPinch, to));
							
						//}
						
					}
					else //hold
					{

						info = "hold";
						indexObj.renderer.enabled = true;
						thumbObj.renderer.enabled = true;
						//indexObj.renderer.material = yellow;
						rotate = false;
						translate = false;

						if(isDocked)
						{
							newTask();
							setNewPositionAndOrientation();
							selectLevel();
							if(score == 9)
								window = true;
						}
					}

					prevPinch = to;
					mLastFrame = frame;
				}
			}
		}else
			connectionMessage = "Not connected";
	}
	
	protected override void atEnd ()
	{
		mController.Dispose();
	}
}