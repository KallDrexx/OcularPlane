﻿using System.Threading;
using OcularPlane.Networking.WcfTcp.Host;

namespace OcularPlane.Networking.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var containerManager = new ContainerManager();
            var testObject = new TestObject();
            containerManager.AddObjectToContainer("container1", testObject, "testobject");
            containerManager.AddMethodToContainer("container1", () => testObject.SetProperties(0, null, 0), "setTestValues");


            containerManager.AddObjectToContainer("container2", new TestObject(), "testobject2");
            containerManager.AddObjectToContainer("GameScreen", 
                new TestObject
                {
                    IntValue = 1234,
                    StringValue = "This is background"
                }
                , "BackgroundInstance");
            containerManager.AddObjectToContainer("GameScreen", 
                new TestObject
                {
                    IntValue = 5678,
                    StringValue = "This is explosion"
                }
                , "ExplosionInstance");




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
