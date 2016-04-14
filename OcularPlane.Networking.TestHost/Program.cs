﻿using System.Threading;
using OcularPlane.Networking.WcfTcp.Host;

namespace OcularPlane.Networking.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var containerManager = new ContainerManager();
            containerManager.AddObjectToContainer("container1", new TestObject(), "testobject");
            containerManager.AddObjectToContainer("container2", new TestObject(), "testobject2");

            using (var host = new OcularPlaneHost(containerManager, "localhost", 9999))
            {
                while (true)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}