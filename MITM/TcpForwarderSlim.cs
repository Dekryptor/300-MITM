using MITM;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static MITM.Packets;

namespace BrunoGarcia.Net
{
    public class TcpForwarderSlim
    {
        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TcpForwarderSlim();
                var state = new State(source, destination._mainSocket);
                destination.Connect(remote, source);
                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceiveSource, state);
            }
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceiveSource(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    Console.WriteLine("[Recv] " + ByteArrayToString(TrimEnd(state.Buffer)));
                    handlePacket(state);
                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private static bool handlePacket(State state)
        {
            var header = new Packets.PacketHeader(state.Buffer);
            Console.WriteLine("Magic: " + header.cmd);
            return true;
        }

        public static bool login = true;

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    // E1 2C
                    var first       = "DB 16 00 00 00 00 00 00 22 6C 4E 05 05 00 00 01 01 08 09 11 E5 B4";
                    var forut       = "DB 16 00 00 0C 0C 24 24 22 6C 4E 05 05 00 00 01 01 08 09 11 E5 B4";

                    // E7 21
                    var first1      = "DB 16 00 00 0C 0C 35 35 22 6C 4E 05 05 00 00 01 01 08 09 11 E5 B4";
                    var secon1      = "DB 16 00 00 0A 0A 56 56 22 6C 4E 05 05 00 00 01 01 08 09 11 E5 B4";

                    // E2 2A
                    var first2      = "DB 16 00 00 00 00 00 00 22 6C 4E 05 05 00 00 01 01 08 09 11 E5 B4";
                    var second2     = "DB 16 00 00 00 00 00 00 22 6C 4E 05 05 00 00 01 01 08 09 11 E5 B4";

                    //Console.WriteLine("[Send] " + ByteArrayToString(TrimEnd(state.Buffer)));
                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }
        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ");
        }
        public static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }
        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[8192];
            }
        }
    }
}