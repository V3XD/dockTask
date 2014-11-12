using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Folders : Singleton<Folders> 
{
	protected Folders () {} 

	string folderPath;
	string columns = "Time,Distance,Angle,Difficulty,initDistance,initAngle,clutchTime,clutchCount,tab,xRotI,yRotI,zRotI," +
					"xRotChair,yRotChair,zRotChair,easyTime,mediumTime,hardTime,interaction";
	string columnsSkip = "Time,Difficulty,autoSkip,skip,initDistance,initAngle,clutchTime,clutchCount,targetX,targetY,targetZ,targetW,interaction";
	string columnsRaw = "Time,Distance,Angle,Difficulty,trialNum, group, trialType,action,interaction";

	void Awake () 
	{
		folderPath = @"Log\"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+@"\";
		System.IO.Directory.CreateDirectory(folderPath);
		File.AppendAllText(folderPath+"Trials.csv", columns+ Environment.NewLine);
		File.AppendAllText(folderPath+"Tutorials.csv", columns+ Environment.NewLine);
		File.AppendAllText(folderPath+"Skip.csv", columnsSkip+ Environment.NewLine);
		File.AppendAllText(folderPath+"Raw.csv", columnsRaw+ Environment.NewLine);
	}

	public string getPath ()
	{
		return folderPath;
	}

}
