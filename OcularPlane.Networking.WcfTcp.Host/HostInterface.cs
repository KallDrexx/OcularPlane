using System;
using System.Collections.Generic;
using System.ServiceModel;
using OcularPlane.Models;
using OcularPlane.Networking.WcfTcp.Common;

namespace OcularPlane.Networking.WcfTcp.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class HostInterface : INetworkInterface
    {
        private readonly ContainerManager _containerManager;

        public HostInterface(ContainerManager containerManager)
        {
            _containerManager = containerManager;
        }

        public void Ping()
        {
            // Just used to make sure the connection is still active
        }

        public string[] GetContainerNames()
        {
            return _containerManager.GetContainerNames();
        }

        public InstanceReference[] GetInstancesInContainer(string containerName)
        {
            return _containerManager.GetInstancesInContainer(containerName);
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            return _containerManager.GetInstanceDetails(instanceId);
        }

        public void SetPropertyValue(Guid instanceId, string propertyName, string value)
        {
            _containerManager.SetPropertyValue(instanceId, propertyName, value);
        }

        public MethodReference[] GetMethodsInContainer(string containerName)
        {
            return _containerManager.GetMethodsInContainer(containerName);
        }

        public void ExecuteMethod(Guid methodId, Dictionary<string, string> parameters)
        {
            _containerManager.ExecuteMethod(methodId, parameters);
        }
    }
}
