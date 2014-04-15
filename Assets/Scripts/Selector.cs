using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//only one task is true
public class Selector : Task 
{
	List<Task> children;

	public Selector (List<Task> tasks)
	{
		children = tasks;
	}

	public override bool run ()
	{
		bool result = false;
		foreach (Task child in children) 
		{
			if(child.run())
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
