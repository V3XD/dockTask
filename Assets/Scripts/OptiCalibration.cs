using UnityEngine;
using System.Collections;

public class OptiCalibration : Singleton<OptiCalibration> 
{
	protected OptiCalibration () {} 
	
	public float touchDist;//distance between finger tips
	public float minDist;//when not touching

	void Awake () 
	{
		touchDist = 3.1f;
		minDist = 3.6f;
	}

	public void setTouchDist(float dist)
	{
		touchDist = dist+0.5f;
		minDist = dist + 1f;
	}
}
