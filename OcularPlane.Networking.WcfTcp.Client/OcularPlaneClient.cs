using System;
using System.ServiceModel;
using OcularPlane.Models;
using OcularPlane.Networking.WcfTcp.Common;

namespace OcularPlane.Networking.WcfTcp.Client
{
    public class OcularPlaneClient
    {
        private readonly INetworkInterface _proxy;

        public OcularPlaneClient(string host, int port)
        {
            var uri = new Uri($"net.tcp://{host}:{port}");
            var endpoint = new EndpointAddress(uri);
            var factory = new ChannelFactory<INetworkInterface>(new NetTcpBinding(), endpoint);
            _proxy = factory.CreateChannel();
        }

        public void Ping()
        {
            _proxy.Ping();
        }

        public string[] GetContainerNames()
        {
            return  _proxy.GetContainerNames();
        }

        public InstanceReference[] GetInstancesInContainer(string containerName)
        {
            return _proxy.GetInstancesInContainer(containerName);
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            return _proxy.GetInstanceDetails(instanceId);
        }

        public void SetPropertyValue(Guid instanceId, string propertyName, string value)
        {
            _proxy.SetPropertyValue(instanceId, propertyName, value);
        }
    }
}
