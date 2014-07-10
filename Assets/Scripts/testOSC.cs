//Unity/Qualisys simple example using Bespoke OSC library
//https://bitbucket.org/pvarcholik/bespoke.osc

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Bespoke.Common;
using Bespoke.Common.Osc;
using Bespoke.Common.Net;

public class testOSC : MonoBehaviour
{
	public GameObject rod;

	OscServer receiver;
	int commandPort = 22225;
	int dataPort = 45454;
	IPEndPoint destination;
	OscMessage message;

	private static Vector3 position = new Vector3 ();
	private static Vector3 orientation = new Vector3 ();
	private static int sBundlesReceivedCount;
	private static int sMessagesReceivedCount;
	private static float scale = 0.1f;

	void Start () 
	{
		destination = new IPEndPoint(IPAddress.Loopback, commandPort);
		receiver = new OscServer(TransportType.Udp, IPAddress.Loopback, dataPort);
	
		receiver.FilterRegisteredMethods = false;
		receiver.RegisterMethod("/qtm");
		receiver.BundleReceived += new EventHandler<OscBundleReceivedEventArgs>(oscServer_BundleReceived);
		receiver.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(oscServer_MessageReceived);
		receiver.ReceiveErrored += new EventHandler<ExceptionEventArgs>(oscServer_ReceiveErrored);
		receiver.ConsumeParsingExceptions = false;

		receiver.Start ();

		message = new OscMessage (destination, "/qtm", "Connect");
		message.Append (dataPort);
		message.Send (destination);

		message.ClearData ();
		message.Append ("StreamFrames");
		message.Append ("AllFrames");
		message.Append ("6DEuler");
		message.Send (destination);
	}

	void Update () 
	{
		rod.transform.position = position;
		rod.transform.eulerAngles = orientation;
	}
	
	void OnDestroy()
	{
		message.ClearData ();
		message.Append ("Disconnect");
		message.Send (destination);
		receiver.Stop ();
		Debug.Log ("Disconnected");
	}

	private static void oscServer_BundleReceived(object sender, OscBundleReceivedEventArgs e)
	{
		sBundlesReceivedCount++;
		
		OscBundle bundle = e.Bundle;
		/*Debug.Log(String.Format("\nBundle Received [{0}:{1}]: Nested Bundles: {2} Nested Messages: {3}", 
		                        bundle.SourceEndPoint.Address.ToString(),
		                        bundle.TimeStamp.ToString(), 
		                        bundle.Bundles.Count.ToString(), 
		                        bundle.Messages.Count.ToString()));
		Debug.Log(String.Format("Total Bundles Received: {0}", sBundlesReceivedCount.ToString()));*/
	}
	
	private static void oscServer_MessageReceived(object sender, OscMessageReceivedEventArgs e)
	{
		sMessagesReceivedCount++;
		
		OscMessage message = e.Message;
		
		/*Debug.Log(String.Format("\nMessage Received [{0}]: {1}", 
		                        message.SourceEndPoint.Address.ToString(), 
		                        message.Address.ToString()));
		Debug.Log(String.Format("Message contains {0} objects.", 
		                        message.Data.Count.ToString()));*/
		
		for (int i = 0; i < message.Data.Count; i++)
		{
			string dataString;
			
			if (message.Data[i] == null)
			{
				dataString = "Nil";
			}
			else
			{
				dataString = (message.Data[i] is byte[] ? BitConverter.ToString((byte[])message.Data[i]) : message.Data[i].ToString());
			}
			//Debug.Log(String.Format("[{0}]: {1}", i.ToString(), dataString));
			if(message.Address.ToString().Equals("/qtm/6d_euler/pen"))
			{
				//needs to match Unity's axis frame
				switch (i)
				{
					case 0:
						position.x = float.Parse(dataString)*scale;
						break;
					case 1:
						position.y = float.Parse(dataString)*scale;
						break;
					case 2:
						position.z = -float.Parse(dataString)*scale;
						break;
					case 3:
						orientation.x = -float.Parse(dataString);
						break;
					case 4:
						orientation.y = -float.Parse(dataString);
						break;
					case 5:
						orientation.z = float.Parse(dataString);
						break;
				}
				Debug.Log(position+" "+orientation);
			}

		}
		
		//Debug.Log(String.Format("Total Messages Received: {0}", sMessagesReceivedCount.ToString()));
	}
	
	private static void oscServer_ReceiveErrored(object sender, ExceptionEventArgs e)
	{
		//Debug.Log(String.Format("Error during reception of packet: {0}", e.Exception.Message));
	}
}
