using UnityEngine;
using System.Collections;

public class OptiCalibration : Singleton<OptiCalibration> 
{
	protected OptiCalibration () {} 
	
	public float touchDist;//distance between finger tips
	public float minDist;//when not grabbing
	public float ave;//when grabbing

	void Awake () 
	{
		touchDist = 3.1f;
		ave = 2.6f;
		minDist = ave + 0.5f;
	}

	public void setTouchDist(float dist)
	{
		touchDist = dist+0.1f;

	}

	public void setAveDist(float dist)
	{
		ave = dist+0.1f;
		minDist = ave + 0.5f;
	}
}
