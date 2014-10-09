using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Folders : Singleton<Folders> 
{
	protected Folders () {} 

	string folderPath;
	string columns = "Time,Distance,Angle,Difficulty,initDistance,initAngle,clutchTime,clutchCount,interaction";
	string columnsSkip = "Time,Difficulty,autoSkip,skip,initDistance,initAngle,clutchTime,clutchCount,targetX,targetY,targetZ,targetW,interaction";

	void Awake () 
	{
		folderPath = @"Log\"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+@"\";
		System.IO.Directory.CreateDirectory(folderPath);
		File.AppendAllText(folderPath+"Trials.csv", columns+ Environment.NewLine);
		File.AppendAllText(folderPath+"Tutorials.csv", columns+ Environment.NewLine);
		File.AppendAllText(folderPath+"Skip.csv", columnsSkip+ Environment.NewLine);
	}

	public string getPath ()
	{
		return folderPath;
	}

}
