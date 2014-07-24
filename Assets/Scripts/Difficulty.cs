using UnityEngine;
using System.Collections;

public class Difficulty : Singleton<Difficulty> 
{
	protected Difficulty () {} 

	public float angle;//angle between quaternions
	public float distance;//distance between target and cursor
	string level;//level of difficulty

	void Awake () 
	{
		setEasy ();
	}

	public void setEasy()
	{
		angle = 15f;
		distance = 3f;
		level = "easy";
	} 

	public void setNormal()
	{
		angle = 10f;
		distance = 2f;
		level = "normal";
	}

	public void setHard()
	{
		angle = 5f;
		distance = 1f;
		level = "hard";
	}

	public string getLevel()
	{
		return level;
	}

}
