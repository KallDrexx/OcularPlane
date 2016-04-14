using System;
using System.ServiceModel;
using OcularPlane.Networking.WcfTcp.Common;

namespace OcularPlane.Networking.WcfTcp.Host
{
    public class OcularPlaneHost : IDisposable
    {
        private readonly ServiceHost _serviceHost;

        public OcularPlaneHost(ContainerManager containerManager, string serverName, int port)
        {
            var networkInterface = new HostInterface(containerManager);

            var uri = new Uri($"net.tcp://{serverName}:{port}");
            _serviceHost = new ServiceHost(networkInterface, uri);
            _serviceHost.AddServiceEndpoint(typeof (INetworkInterface), new NetTcpBinding(), uri);
            _serviceHost.Open();
        }

        public void Dispose()
        {
            _serviceHost.Close();
        }
    }
}
