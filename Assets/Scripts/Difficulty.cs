using UnityEngine;
using System.Collections;

public class Difficulty : Singleton<Difficulty> {
	protected Difficulty () {} 

	public float angle;//angle between quaternions
	public float distance;//distance between target and cursor
	string level;//level of difficulty

	void Awake () 
	{
		setNormal ();
	}

	public void setNormal()
	{
		angle = 20f;
		distance = 3f;
		level = "normal";
	} 

	public void setDifficult()
	{
		angle = 15f;
		distance = 2f;
		level = "difficult";
	}

	public void setVeryDifficult()
	{
		angle = 5f;
		distance = 1f;
		level = "veryDifficult";
	}

	public string getLevel()
	{
		return level;
	}

}
