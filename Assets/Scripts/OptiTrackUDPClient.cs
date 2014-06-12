using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyStateObject
{
	public Socket workSocket = null;
	public const int BUFFER_SIZE = 65507;
	public byte[] buffer = new byte[BUFFER_SIZE];
}

// OptiTrackUDPClient is a class for connecting to OptiTrack Arena Skeleton data
// and storing in a general Skeleton class object for access by Unity characters
public class OptiTrackUDPClient
{
	public int dataPort = 1511;
	public int commandPort = 1510;
	public string multicastIPAddress = "239.255.42.99";
	public string localIPAddress = "132.206.74.217";//"192.168.53.22";
	
	public bool bNewData = false;
	public Skeleton skelTarget = null;
	public RigidBody[] rigidTargets = new RigidBody[10];
	public int numTrackables = 0;
	public Vector3[] markers = new Vector3[10];
	Socket sockData = null;
	Socket sockCommand = null;
	String strFrame = "";
	String[] trackerNames = new string[10];
	public int numMarkers = 0;
	
	public OptiTrackUDPClient ()
	{
	}
	
	public bool Connect()
	{
		IPEndPoint ipep;
		MyStateObject so;
		
		Debug.Log("[UDPClient] Connecting.");
		rigidTargets [0] = new RigidBody ();
		rigidTargets [1] = new RigidBody ();
		rigidTargets [2] = new RigidBody ();
		markers [0] = new Vector3 ();
		markers [1] = new Vector3 ();
		markers [2] = new Vector3 ();
		// create data socket
		sockData = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		sockData.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		ipep = new IPEndPoint(IPAddress.Parse(localIPAddress), dataPort);
		try
		{
			sockData.Bind(ipep);
		}
		catch (Exception ex)
		{
			Debug.Log("bind exception : " + ex.Message);
		}
		
		// connect socket to multicast group
		IPAddress ip = IPAddress.Parse(multicastIPAddress);
		sockData.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Parse(localIPAddress)));
		
		so = new MyStateObject();
		#if true
		// asynch - begin listening
		so.workSocket = sockData;
		sockData.BeginReceive(so.buffer, 0, MyStateObject.BUFFER_SIZE, 0, new AsyncCallback(AsyncReceiveCallback), so);
		#else
		// synch - read 1 frame
		int nBytesRead = s.Receive(so.buffer);
		strFrame = String.Format("Received Bytes : {0}\n", nBytesRead);
		if(nBytesRead > 0)
			ReadPacket(so.buffer);
		textBox1.Text = strFrame;
		#endif
		
		
		// create command socket
		sockCommand = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		ipep = new IPEndPoint(IPAddress.Parse(localIPAddress), 0);
		try
		{
			sockCommand.Bind(ipep);
		}
		catch (Exception ex)
		{
			Debug.Log("bind exception : " + ex.Message);
		}
		// asynch - begin listening
		so = new MyStateObject();
		so.workSocket = sockCommand;
		sockCommand.BeginReceive(so.buffer, 0, MyStateObject.BUFFER_SIZE, 0, new AsyncCallback(AsyncReceiveCallback), so);
		
		return true;
	}
	
	public bool RequestDataDescriptions()
	{
		if(sockCommand != null)
		{
			Byte[] message = new Byte[100];
			int offset = 0;
			ushort[] val = new ushort[1];
			val[0] = 4;
			Buffer.BlockCopy(val, 0, message, offset, 1*sizeof(ushort));
			offset += sizeof(ushort);
			val[0] = 0;
			Buffer.BlockCopy(val, 0, message, offset, 1*sizeof(ushort));
			offset += sizeof(ushort);
			
			IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(localIPAddress), commandPort);
			int iBytesSent = sockCommand.SendTo(message, ipep);
			//Debug.Log("[UDPClient] sent RequestDescription . (Bytes sent:" + iBytesSent + ")");
		}
		
		return true;
	}
	
	public bool RequestFrameOfData()
	{
		if(sockCommand != null)
		{
			Byte[] message = new Byte[100];
			int offset = 0;
			ushort[] val = new ushort[1];
			val[0] = 6;
			Buffer.BlockCopy(val, 0, message, offset, 1*sizeof(ushort));
			offset += sizeof(ushort);
			val[0] = 0;
			Buffer.BlockCopy(val, 0, message, offset, 1*sizeof(ushort));
			offset += sizeof(ushort);
			
			IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(localIPAddress), commandPort);
			int iBytesSent = sockCommand.SendTo(message, ipep);
			//Debug.Log("[UDPClient] Sent RequestFrameOfData. (Bytes sent:" + iBytesSent + ")");
		}
		
		return true;
	}
	
	// Async socket reader callback - called by .net when socket async receive procedure receives a message
	private void AsyncReceiveCallback(IAsyncResult ar)
	{
		MyStateObject so = (MyStateObject)ar.AsyncState;
		Socket s = so.workSocket;
		int read = s.EndReceive(ar);
		//Debug.Log("[UDPClient] Received Packet (" + read + " bytes)");   
		if (read > 0)
		{
			// unpack the data
			ReadPacket(so.buffer);
			if(s == sockData)
				bNewData = true;   // indicate to update character
			
			// listen for next frame
			s.BeginReceive(so.buffer, 0, MyStateObject.BUFFER_SIZE, 0, new AsyncCallback(AsyncReceiveCallback), so);
		}
		
	}
	
	private void ReadPacket(Byte[] b)
	{
		int offset = 0;
		int nBytes = 0;
		int[] iData = new int[100];
		float[] fData = new float[500];
		char[] cData = new char[500];
		
		Buffer.BlockCopy(b, offset, iData, 0, 2); offset += 2;
		int messageID = iData[0];
		
		Buffer.BlockCopy(b, offset, iData, 0, 2); offset += 2;
		nBytes = iData[0];
		
		//Debug.Log("[UDPClient] Processing Received Packet (Message ID : " + messageID + ")");
		if(messageID == 5)      // Data descriptions
		{
			strFrame = ("[UDPClient] Read DataDescriptions");
			
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			strFrame += String.Format("Dataset Count: {0}\n", iData[0]);
			int nDatasets = iData[0];

			for(int i=0; i < nDatasets; i++)
			{
				//print("Dataset %d\n", i);

				Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
				strFrame += String.Format("Dataset # {0} (type: {1})\n", i, iData[0]);
				int type = iData[0];

				if(type == 0)   // markerset
				{
					// name
					string strName = "";
					while(b[offset] != '\0')
					{
						Buffer.BlockCopy(b, offset, cData, 0, 1); offset += 1;
						strName += cData[0];
					}
					offset += 1;
					strFrame += String.Format("MARKERSET (Name: {0})\n", strName);
					
					// marker data
					Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
					strFrame += String.Format("marker count: {0}\n", iData[0]);
					int nMarkers = iData[0];
					
					for(int j=0; j < nMarkers; j++)
					{
						strName = "";
						while(b[offset] != '\0')
						{
							Buffer.BlockCopy(b, offset, cData, 0, 1); offset += 1;
							strName += cData[0];
						}
						offset +=1;
						strFrame += String.Format("Name : {0}\n", strName);
					}
				}
				else if(type ==1)   // rigid body
				{
					// name
					string strName = "";
					while(b[offset] != '\0')
					{
						Buffer.BlockCopy(b, offset, cData, 0, 1); 
						offset++;
						strName = strName + cData[0].ToString();
					}
					offset++;
					trackerNames[i] = strName;

					offset +=4;
					offset +=4;
					offset +=4;
					offset +=4;
					offset +=4;
                
				}
				else if(type ==2)   // skeleton
				{
					InitializeSkeleton(b, offset);
					
				}
				
			}   // next dataset
			
			//Debug.Log(strFrame);
			
		}
		else if (messageID == 7)   // Frame of Mocap Data
		{

			strFrame = "[UDPClient] Read FrameOfMocapData\n";
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			strFrame += String.Format("Frame # : {0}\n", iData[0]);
			
			// MarkerSets
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			int nMarkerSets = iData[0];

			strFrame += String.Format("MarkerSets # : {0}\n", iData[0]);
			for (int i = 0; i < nMarkerSets; i++)
			{
				String strName = "";
				int nChars = 0;
				while (b[offset + nChars] != '\0')
				{
					nChars++;
				}
				strName = System.Text.Encoding.ASCII.GetString(b, offset, nChars);
				offset += nChars + 1;
				
				
				Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
				strFrame += String.Format("Marker Count : {0}\n", iData[0]);
				
				nBytes = iData[0] * 3 * 4;
				Buffer.BlockCopy(b, offset, fData, 0, nBytes); offset += nBytes;
			}
			
			// Other Markers
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			int nOtherMarkers = iData[0];
			strFrame += String.Format("Other Markers : {0}\n", iData[0]);
			numMarkers = iData[0];

			for (int i = 0; i < nOtherMarkers; i++)
			{
				Buffer.BlockCopy(b, offset, fData, 0, 4 * 3); offset += 4 * 3;
				markers[i].x = fData[0]*100; markers[i].y = fData[1]*100; markers[i].z = fData[2]*100;
			}

			// Rigid Bodies
			RigidBody rb = new RigidBody();
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			int nRigidBodies = iData[0];
			strFrame += String.Format("Rigid Bodies : {0}\n", iData[0]);

			for (int i = 0; i < nRigidBodies; i++)
			{
				ReadRB(b, ref offset, rb);
				numTrackables = nRigidBodies;
				rb.name = trackerNames[i];
				RigidBody trackable = new RigidBody();
				trackable = new RigidBody();
				trackable.name = rb.name;
				trackable.pos = rb.pos;
				trackable.ori = rb.ori;
				rigidTargets[i] = trackable;
			}

			// Skeletons
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			int nSkeletons = iData[0];
			strFrame += String.Format("Skeletons : {0}\n", iData[0]);
			for (int i = 0; i < nSkeletons; i++)
			{
				// ID
				Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
				skelTarget.ID = iData[0];
				// # rbs (bones) in skeleton
				Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
				skelTarget.nBones = iData[0];
				for (int j = 0; j < skelTarget.nBones; j++)
				{
					ReadRB(b, ref offset, skelTarget.bones[j]);
				}
			}
			
			// frame latency
			Buffer.BlockCopy(b, offset, fData, 0, 4); offset += 4;
			
			// end of data (EOD) tag
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			
			//Debug.Log(strFrame);
			
			// debug
			String str = String.Format("Skel ID : {0}", skelTarget.ID);
			for (int i = 0; i < skelTarget.nBones; i++)
			{
				String st = String.Format(" Bone {0}: ID: {1}    raw pos ({2:F2},{3:F2},{4:F2})  raw ori ({5:F2},{6:F2},{7:F2},{8:F2})",
				                          i, skelTarget.bones[i].ID,
				                          skelTarget.bones[i].pos[0], skelTarget.bones[i].pos[1], skelTarget.bones[i].pos[2],
				                          skelTarget.bones[i].ori[0], skelTarget.bones[i].ori[1], skelTarget.bones[i].ori[2], skelTarget.bones[i].ori[3]);
				str += "\n" + st;
			}
			//Debug.Log(str);
			
			if(skelTarget.bNeedBoneLengths)
				skelTarget.UpdateBoneLengths();
			
		}
		else if(messageID == 100)
		{
			Debug.Log("Packet Read: Unrecognized Request.");
		}
		
	}
	
	// Unpack RigidBody data
	private void ReadRB(Byte[] b, ref int offset, RigidBody rb)
	{
		int[] iData = new int[100];
		float[] fData = new float[100];
		
		// RB ID
		Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
		int iSkelID = iData[0] >> 16;           // hi 16 bits = ID of bone's parent skeleton
		int iBoneID = iData[0] & 0xffff;       // lo 16 bits = ID of bone
		rb.ID = iData[0]; // already have it from data descriptions
		
		// RB pos
		float[] pos = new float[3];
		Buffer.BlockCopy(b, offset, pos, 0, 4 * 3); offset += 4 * 3;
		rb.pos.x = pos[0]*100; rb.pos.y = pos[1]*100; rb.pos.z = pos[2]*100;

		// RB ori
		float[] ori = new float[4];
		Buffer.BlockCopy(b, offset, ori, 0, 4 * 4); offset += 4 * 4;
		rb.ori.x = ori[0]; rb.ori.y = ori[1]; rb.ori.z = ori[2]; rb.ori.w = ori[3];
		
		// RB's markers
		Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
		int nMarkers = iData[0];
		Buffer.BlockCopy(b, offset, fData, 0, 4 * 3 * nMarkers); offset += 4 * 3 * nMarkers;
		
		// RB's marker ids
		Buffer.BlockCopy(b, offset, iData, 0, 4 * nMarkers); offset += 4 * nMarkers;
		
		// RB's marker sizes
		Buffer.BlockCopy(b, offset, fData, 0, 4 * nMarkers); offset += 4 * nMarkers;
		
		// RB mean error
		Buffer.BlockCopy(b, offset, fData, 0, 4); offset += 4;
		
	}
	
	void InitializeSkeleton(Byte[] b, int offset)
	{
		int[] iData = new int[100];
		float[] fData = new float[500];
		char[] cData = new char[500];
		
		string strName = "";
		while(b[offset] != '\0')
		{
			Buffer.BlockCopy(b, offset, cData, 0, 1); offset += 1;
			strName += cData[0];
		}
		offset += 1;
		strFrame += String.Format("SKELETON (Name: {0})\n", strName);
		skelTarget.name = strName;
		
		Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
		strFrame += String.Format("SkeletonID: {0}\n", iData[0]);
		skelTarget.ID = iData[0];
		
		Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
		strFrame += String.Format("nRigidBodies: {0}\n", iData[0]);
		skelTarget.nBones = iData[0];
		
		for(int j=0; j< skelTarget.nBones; j++)
		{
			// RB name
			string strRBName = "";
			while(b[offset] != '\0')
			{
				Buffer.BlockCopy(b, offset, cData, 0, 1); offset += 1;
				strRBName += cData[0];
			}
			offset += 1;
			strFrame += String.Format("RBName: {0}\n", strRBName);
			skelTarget.bones[j].name = strRBName;
			
			// RB ID
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			int iSkelID = iData[0] >> 16;           // hi 16 bits = ID of bone's parent skeleton
			int iBoneID = iData[0] & 0xffff;       // lo 16 bits = ID of bone
			//Debug.Log("RBID:" + iBoneID + "  SKELID:"+iSkelID);
			strFrame += String.Format("RBID: {0}\n", iBoneID);
			skelTarget.bones[j].ID = iBoneID;
			
			// RB Parent
			Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
			strFrame += String.Format("RB Parent ID: {0}\n", iData[0]);
			skelTarget.bones[j].parentID = iData[0];
			
			// RB local position offset
			Vector3 localPos;
			Buffer.BlockCopy(b, offset, fData, 0, 4); offset += 4;
			strFrame += String.Format("X Offset: {0}\n", fData[0]);
			localPos.x = fData[0];
			
			Buffer.BlockCopy(b, offset, fData, 0, 4); offset += 4;
			strFrame += String.Format("Y Offset: {0}\n", fData[0]);
			localPos.y = fData[0];
			
			Buffer.BlockCopy(b, offset, fData, 0, 4); offset += 4;
			strFrame += String.Format("Z Offset: {0}\n", fData[0]);
			localPos.z = fData[0];
			skelTarget.bones[j].pos = localPos;
			
			//Debug.Log("[UDPClient] Added Bone: " + skelTarget.bones[j].name);
			
		}
		
		skelTarget.bHasHierarchyDescription = true;
		
	}
	
	public void Close() 
	{
		sockData.Close();
		Debug.Log("[UDPClient] Disconnected.");
	}
	
}


