using UnityEngine;
using System.Collections;

public class OptiTrackBehavoir : MonoBehaviour {

	OptiTrackUDPClient udpClient;
	bool bSuccess;
	Skeleton skelPerformer = new Skeleton();

	void Start () 
	{
		udpClient = new OptiTrackUDPClient();
		bSuccess = udpClient.Connect();
		udpClient.skelTarget = skelPerformer;
	}
	
	void Update () 
	{
	
		if(bSuccess)
		{
			udpClient.RequestDataDescriptions();
				for(int i=0; i < udpClient.rigidTargets.Length; i++)
				{
				if(udpClient.rigidTargets[i] != null)
					Debug.Log(udpClient.rigidTargets[i].name + udpClient.rigidTargets[i].pos + udpClient.rigidTargets[i].ori);
				}
		}
	}

	void OnDestroy() 
	{
		udpClient.Close();
	}
}
