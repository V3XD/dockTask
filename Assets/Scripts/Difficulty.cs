using UnityEngine;
using System.Collections;

public class Difficulty : Singleton<Difficulty> 
{
	protected Difficulty () {} 

	public float angle;//angle between quaternions
	public float distance;//distance between target and cursor
	string level;//level of difficulty
	public float [] angles = new float[3] {15f, 10f, 5f};
	public float [] distances = new float[3] {1.5f, 1f, 0.5f};

	void Awake () 
	{
		setEasy ();
	}

	public void setEasy()
	{
		angle = angles [0];//15f;
		distance = distances[0];//1.5f;
		level = "easy";
	} 

	public void setNormal()
	{
		angle = angles [1];//10f;
		distance = distances[1];//1f;
		level = "medium";
	}

	public void setHard()
	{
		angle = angles [2];//5f;
		distance = distances[2];//0.5f;
		level = "hard";
	}

	public string getLevel()
	{
		return level;
	}

}
