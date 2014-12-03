using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace RimDev.Automation.WebHosting
{
    internal class NetworkUtilities
    {
        private const int MinHttpPort = 49152;
        private const int MaxHttpPort = ushort.MaxValue;

        private const int MinHttpsPort = 44300;
        private const int MaxHttpsPort = 44399;

        public static int GetHttpPort()
        {
            return FindAvailablePort(MinHttpPort);
        }

        public static int GetHttpsPort()
        {
            return FindAvailablePort(MinHttpsPort, MaxHttpsPort);
        }

        private static int FindAvailablePort(int minPort, int maxPort = MaxHttpPort)
        {
            var globalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var connections = globalProperties.GetActiveTcpConnections().Select(x => x.LocalEndPoint.Port).ToArray();

            var random = new Random();
            return Enumerable.Range(minPort, (maxPort - minPort) + 1)
                .OrderBy(x => random.Next())
                .First(x => !connections.Contains(x));
        }
    }
}
