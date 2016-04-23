using OcularPlane;
using OcularPlane.Networking.WcfTcp.Host;

namespace $PROJECT_NAMESPACE$.OcularPlaneRuntime
{
    static class OcularPlaneManager
    {
        static OcularPlaneHost host;
        static ContainerManager containerManager;

        public static ContainerManager GetContainerManager()
        {
            if (containerManager == null)
            {
                containerManager = new ContainerManager();
                host = new OcularPlane.Networking.WcfTcp.Host.OcularPlaneHost(containerManager, "localhost", 9999);
            }

            return containerManager;
        }

    }
}
