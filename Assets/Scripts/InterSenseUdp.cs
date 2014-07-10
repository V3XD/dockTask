using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;


   public class InterSenseUdp
    {
        public bool NewData = false;

        public class StationData
        {
            public bool NewData = false;
            public float Yaw = 0;
            public float Pitch = 0;
            public float Roll = 0;
            public float X = 0;
            public float Y = 0;
            public float Z = 0;
            public bool[] Button = new bool[8];
            public int[] AnalogData = new int[8];
        }

        public StationData[] Data = new StationData[8];

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct UdpStationPacket
        {
            public byte StartByte;
            public byte PacketType;
            public byte PacketSeqNum;
            public byte CheckSum;

            public byte Model;
            public byte StationNum;
            public byte TrackingStatus;
            public byte ButtonState;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] AnalogData;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] Euler;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] Position;

            public float TimeStamp;
        }

        private UdpStationPacket packet;

        private class UdpState
        {
            public IPEndPoint e;
            public UdpClient u;
        }
        private UdpState s;

        public InterSenseUdp(int port)
        {
            // Receive a message and write it to the console.
            IPEndPoint e = new IPEndPoint(IPAddress.Any, port);
            UdpClient u = new UdpClient(e);

            s = new UdpState();
            s.e = e;
            s.u = u;

            for (int i = 0; i < Data.Length; i++)
                Data[i] = new StationData();

            BeginReceive(s);
        }

        private static object RawDeserialize(byte[] rawData, int position, Type anyType)
        {
            int rawsize = Marshal.SizeOf(anyType);
            if (rawsize > rawData.Length)
                return null;
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            object retobj = Marshal.PtrToStructure(buffer, anyType);
            Marshal.FreeHGlobal(buffer);
            return retobj;
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

            Byte[] receiveBytes = u.EndReceive(ar, ref e);
            UdpStationPacket packet = (UdpStationPacket)RawDeserialize(receiveBytes, 0, typeof(UdpStationPacket));

            int snum = packet.StationNum - 1;
            if ( snum >= 0 && snum < Data.Length)
            {
                Data[snum].NewData = true;
                Data[snum].Yaw = packet.Euler[0];
                Data[snum].Pitch = packet.Euler[1];
                Data[snum].Roll = packet.Euler[2];
                Data[snum].X = packet.Position[0];
                Data[snum].Y = packet.Position[1];
                Data[snum].Z = packet.Position[2];

                for (int i = 0; i < Data[snum].AnalogData.Length; i++)
                {
                    Data[snum].AnalogData[i] = packet.AnalogData[i];
                }
                for (int i = 0; i < Data[snum].Button.Length; i++)
                {
                    if ((packet.ButtonState & (1 << i)) == 1 << i)
                        Data[snum].Button[i] = true;
                    else
                        Data[snum].Button[i] = false;
                }
            }
            BeginReceive(s);
        }

        private bool BeginReceive(UdpState s)
        {
            try
            {
                s.u.BeginReceive(new AsyncCallback(ReceiveCallback), s);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }