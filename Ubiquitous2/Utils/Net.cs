using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UB.Utils
{
    public static class Net
    {
        public static void DemandTCPPermission()
        {
            SocketPermission permitSocket = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, String.Empty, SocketPermission.AllPorts);
            permitSocket.Demand();
        }
        public static void TestTCPPort( String hostName, int portNumber, Action<IPHostEntry,Exception> callback, int timeoutMs = 2000 )
        {

            ThreadPool.QueueUserWorkItem(f => {
                IPHostEntry hosts = null;
                try
                {
                    hosts = Dns.GetHostEntry(hostName);
                }
                catch {
                    callback(null, new Exception("Host resolution error " + hostName));
                    return;
                }


                Task[] portTestTasks = new Task[hosts.AddressList.Count()];

                for (int i = 0; i < portTestTasks.Length; i++)
                {
                    var index = i;
                    portTestTasks[i] = Task.Factory.StartNew(() =>
                    {
                        var host = hosts.AddressList[index];
                        hosts.AddressList[index] = null;
                        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            sock.Connect(host, portNumber);
                            if (sock.Connected == true)
                            {
                                hosts.AddressList[index] = host;
                                sock.Close();
                                return;
                            }

                        }
                        catch {
                        }
                    });
                    Thread.Sleep(16);
                }
                Task.WaitAll(portTestTasks, timeoutMs);

                hosts.AddressList = hosts.AddressList.Where(entry => entry != null).ToArray();
                callback(hosts, null);
            });

        }
    }
}
