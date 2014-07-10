using UnityEngine;
using System.Collections;

public class InterSenseUDPTranslate : MonoBehaviour {
	
	InterSenseUdp client = null;

	// Use this for initialization
	void Start () {
		client = new InterSenseUdp (22222);
	}
	
	// Update is called once per frame
	void Update () {
		foreach (InterSenseUdp.StationData s in client.Data)
		{
			if (s.NewData)
			{
				Vector3 position = new Vector3(s.X, s.Y, s.Z);
				Debug.Log(position);
				s.NewData = false;
			}
		}
	}
}
