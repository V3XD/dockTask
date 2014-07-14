/**
 * Adapted from johny3212
 * Written by Matt Oskamp
 */
using UnityEngine;
using System;
using System.Collections;
using OptitrackManagement;

public class OptiTrackManager : Singleton<OptiTrackManager> 
{
	protected  OptiTrackManager () {}

	public float scale = 100.0f;
	public Vector3 origin = Vector3.zero; // set this to wherever you want the center to be in your scene

	void Awake ()
	{
		//Debug.Log("Initializing");
		
		OptitrackManagement.DirectMulticastSocketClient.Start();
		Application.runInBackground = true;
	}

	public OptiTrackRigidBody getOptiTrackRigidBody(int index)
	{
		// only do this if you want the raw data
		if(OptitrackManagement.DirectMulticastSocketClient.IsInit())
		{
			DataStream networkData = OptitrackManagement.DirectMulticastSocketClient.GetDataStream();
			return networkData.getRigidbody(index);
		}
		else
		{
			OptitrackManagement.DirectMulticastSocketClient.Start();
			return getOptiTrackRigidBody(index);
		}
	}

	public Vector3 getPosition(int rigidbodyIndex)
	{
		if(OptitrackManagement.DirectMulticastSocketClient.IsInit())
		{
			DataStream networkData = OptitrackManagement.DirectMulticastSocketClient.GetDataStream();
			Vector3 pos = origin + networkData.getRigidbody(rigidbodyIndex).position * scale;
			pos.z = -pos.z;
			return pos;
		}
		else
		{
			return Vector3.zero;
		}
	}

	public Quaternion getOrientation(int rigidbodyIndex)
	{
		// should add a way to filter it
		if(OptitrackManagement.DirectMulticastSocketClient.IsInit())
		{
			DataStream networkData = OptitrackManagement.DirectMulticastSocketClient.GetDataStream();
			Quaternion rot = networkData.getRigidbody(rigidbodyIndex).orientation;

			// Invert pitch and yaw
			Vector3 euler = rot.eulerAngles;
			rot.eulerAngles = new Vector3(-euler.x, -euler.y, euler.z); // these may change depending on your calibration

			return rot;
		}
		else
		{
			return Quaternion.identity;
		}
	}

	public bool isConnected ()
	{
		return OptitrackManagement.DirectMulticastSocketClient.IsInit ();
	}

	public int getRigidBodyNum ()
	{
		return OptitrackManagement.DirectMulticastSocketClient.GetDataStream ()._nRigidBodies;
	}

	public Vector3 getMarkerPosition(int markerIndex)
	{
		if(OptitrackManagement.DirectMulticastSocketClient.IsInit())
		{
			DataStream networkData = OptitrackManagement.DirectMulticastSocketClient.GetDataStream();
			Vector3 pos = origin + networkData.getMarker(markerIndex).position * scale;
			pos.z = -pos.z;
			return pos;
		}
		else
		{
			return Vector3.zero;
		}
	}

	public int getMarkerNum ()
	{
		return OptitrackManagement.DirectMulticastSocketClient.GetDataStream ()._nMarkers;
	}

	void OnDestroy ()
	{
		//Debug.Log("OptitrackManager: Destruct");
		OptitrackManagement.DirectMulticastSocketClient.Close();
	}
}