using BrunoGarcia.Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MITM
{
    class Program
    {
        static void Main()
        {
            var from = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30001);
            var destination = new IPEndPoint(IPAddress.Parse("125.39.187.84"), 30000);
            TcpForwarderSlim Listener = new TcpForwarderSlim();
            Console.WriteLine("Server listening on port 30001.  Press enter to exit.");
            Listener.Start(from, destination);
            Console.ReadLine();
        }
    }
}