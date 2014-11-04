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
		angle = 11f;
		distance = 1f;
		level = "easy";
	} 

	public void setNormal()
	{
		angle = 7.6f;
		distance = 0.7f;
		level = "medium";
	}

	public void setHard()
	{
		angle = 3.9f;
		distance = 0.3f;
		level = "hard";
	}

	public string getLevel()
	{
		return level;
	}

}
