using UnityEngine;
using System.Collections;

public class Type : Singleton<Type>
{
	protected Type () {} 
	
	int trialNum;//number of trials
	int repetition;//times to repeat the group of trials
	public int currentGroup;
	public bool mute = false;
	string trialType = "Tutorials";
	public bool updateCam = true;

	void Awake () 
	{
		setTutorial();
		currentGroup = 2;
	}
	
	public void setTutorial()
	{
		trialNum = 4;
		repetition = 1;
		trialType = "Tutorials";
	}

	public void setRealThing()
	{
		trialNum = 6;
		repetition = 2;
		trialType = "Trials";
	} 

	public int getTrialNum()
	{
		return trialNum;
	}

	public int getRepetition()
	{
		return repetition;
	}

	public string getType()
	{
		return trialType;
	}


}
