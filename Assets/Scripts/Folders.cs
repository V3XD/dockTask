using UnityEngine;
using System.Collections;

public class Folders : Singleton<Folders> 
{
	protected Folders () {} 

	string folderPath;

	void Awake () 
	{
		folderPath = @"Log\"+System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss")+@"\";
		System.IO.Directory.CreateDirectory(folderPath);
		System.IO.Directory.CreateDirectory(folderPath+@"tutorial\");
	}

	public string getPath ()
	{
		return folderPath;
	}

}
