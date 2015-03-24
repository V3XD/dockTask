using UnityEngine;
using System.Collections;

public class pichCalibration : Game {
	OptiTrackManager optiManager;
	bool bSuccess;
	
	public GameObject index;
	public GameObject thumb;
	public GameObject trail;
	
	OptiCalibration calibration;
	public Material blue;
	bool isCalibrated = false;
	float maxDist = 0;
	static int startNum = 6;

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
		interaction = "pinch";
		setNewPositionAndOrientationTut();
		pointer.GetComponent<Renderer>().enabled = false;
		nextLevel = "MainMenu";
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
			if(optiManager.getMarkerNum() == 2)
			{
				thumb.GetComponent<Renderer>().material = blue;
				index.GetComponent<Renderer>().material = blue;
				pointer.GetComponent<Renderer>().material = blue;

				Vector3 thumbPos = optiManager.getMarkerPosition(0);
				Vector3 indexPos = optiManager.getMarkerPosition(1);
				
				float thumbToIndex = Vector3.Distance(thumbPos, 
				                                      indexPos);
				
				Vector3 currentPos = (thumbPos + indexPos)*0.5f;

				thumb.transform.position = thumbPos; 
				index.transform.position = indexPos; 
				
				float aveDist = thumbToIndex;

				pointer.transform.position = currentPos;


				if(!isCalibrated)
				{
					int count = (int)(Time.time - prevTotalTime);
					
					instructionsText.text = "Pinch "+ (startNum-count).ToString();
					if( count > startNum-3)
					{
						instructionsText.material.color = Color.white;
						if(thumbToIndex > maxDist)
							maxDist = thumbToIndex;
					}
					
					if( count > startNum)
					{
						instructionsText.text = "Ready";
						isCalibrated = true;
						
						calibration.setTouchDist(maxDist);
						prevTotalTime = Time.time;
					}
					
					
				}
				else if( aveDist <= calibration.touchDist)
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
						index.GetComponent<Renderer>().enabled = true;
						thumb.GetComponent<Renderer>().enabled = true;
						pointer.GetComponent<Renderer>().enabled = false;
						trail.GetComponent<TrailRenderer>().enabled = false;

						tapTime = Time.time - prevClutchTime;
						if(tapTime <= maxTapTime && isDocked)
							confirm = true;
					}
					
					
					if(isDocked && confirm)
					{
						window = true;
					}
				}
			}
			else
			{
				thumb.GetComponent<Renderer>().material = red;
				index.GetComponent<Renderer>().material = red;
				pointer.GetComponent<Renderer>().material = red;
				trail.GetComponent<TrailRenderer>().enabled = false;
			}
			
			
		}
	}
	
	protected override void atEnd ()
	{
	}
}
