using UnityEngine;
using System.Collections;

public class Difficulty : Singleton<Difficulty> {
	protected Difficulty () {} 

	public float angle;//angle between quaternions
	public float distance;//distance between target and cursor

	void Awake () 
	{
		setNormal ();
	}

	public void setNormal()
	{
		angle = 20f;
		distance = 3f;
	} 

	public void setDifficult()
	{
		angle = 15f;
		distance = 2f;
	}

	public void setVeryDifficult()
	{
		angle = 5f;
		distance = 1f;
	}

}
