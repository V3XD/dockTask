using UnityEngine;
using System.Collections;

public class OptiCalibration : Singleton<OptiCalibration> 
{
	protected OptiCalibration () {} 
	
	public float touchDist;//distance between finger tips
	public float minDist;//when not touching

	void Awake () 
	{
		touchDist = 3f;
		minDist = 4.75f;
	}

	public void setTouchDist(float dist)
	{
		touchDist = dist+0.5f;
		minDist = dist + 1f;
	}
}
