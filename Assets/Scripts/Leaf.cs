using UnityEngine;
using System.Collections;
using System;

//actual action
public class Leaf : Task 
{
	Func<bool> action;

	public Leaf(Func<bool> actionMethod)
	{
		action = actionMethod;
	}

	public override bool run ()
	{
		return action();
	}
}
