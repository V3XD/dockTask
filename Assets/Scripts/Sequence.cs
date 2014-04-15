using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//run until a task is false
public class Sequence : Task
{
	List<Task> children;

	public Sequence (List<Task> tasks)
	{
		children = tasks;
	}

	public override bool run ()
	{
		bool result = false;
		foreach (Task child in children) 
		{
			if(child.run())
				result = true;
			else
				break;
		}
		return result;
	}
}
