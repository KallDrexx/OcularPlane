using System;
using System.ServiceModel;
using OcularPlane.Models;

namespace OcularPlane.Networking.WcfTcp.Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface INetworkInterface
    {
        [OperationContract]
        void Ping();

        [OperationContract]
        string[] GetContainerNames();

        [OperationContract]
        InstanceReference[] GetInstancesInContainer(string containerName);

        [OperationContract]
        InstanceDetails GetInstanceDetails(Guid instanceId);

        [OperationContract]
        void SetPropertyValue(Guid instanceId, string propertyName, string value);
    }
}
