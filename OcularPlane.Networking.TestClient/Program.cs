using System;
using System.Collections.Generic;
using OcularPlane.Networking.WcfTcp.Client;

namespace OcularPlane.Networking.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new OcularPlaneClient("localhost", 9999);
            client.Ping();

            Console.WriteLine("Connected!");
            Console.WriteLine();

            Console.WriteLine("Containers:");
            foreach (var containerName in client.GetContainerNames())
            {
                Console.WriteLine($"--> {containerName}");
            }
            Console.WriteLine();

            Console.WriteLine("Instances in container 1:");
            var instances = client.GetInstancesInContainer("container1");
            foreach (var instance in instances)
            {
                Console.WriteLine($"--> {instance.Name} ({instance.TypeName}) with id {instance.InstanceId}");
            }
            Console.WriteLine();

            Console.WriteLine($"Properties of {instances[0].Name}:");
            var details = client.GetInstanceDetails(instances[0].InstanceId);
            foreach (var property in details.Properties)
            {
                Console.WriteLine($"--> {property.Name} ({property.TypeName}): {property.ValueAsString}");
            }
            Console.WriteLine();

            Console.WriteLine("Changing property values");
            client.SetPropertyValue(details.InstanceId, details.Properties[0].Name, "122");
            client.SetPropertyValue(details.InstanceId, details.Properties[1].Name, "STRING");
            client.SetPropertyValue(details.InstanceId, details.Properties[2].Name, "Value2");
            Console.WriteLine("Done!");
            Console.WriteLine();

            Console.WriteLine($"Properties of {instances[0].Name}:");
            details = client.GetInstanceDetails(instances[0].InstanceId);
            foreach (var property in details.Properties)
            {
                Console.WriteLine($"--> {property.Name} ({property.TypeName}): {property.ValueAsString}");
            }
            Console.WriteLine();

            Console.WriteLine($"Methods in container:");
            var methods = client.GetMethodsInContainer("container1");
            foreach (var methodReference in methods)
            {
                Console.Write($"--> {methodReference.Name} ({methodReference.MethodId}) with parameters:");
                foreach (var parameterReference in methodReference.Parameters)
                {
                    Console.WriteLine($"----> {parameterReference.ParameterName} of type {parameterReference.TypeName}");
                }
            }
            Console.WriteLine();

            Console.WriteLine("Setting values:");
            var parameters = new Dictionary<string, string>
            {
                {"intVal", "100"},
                {"stringVal", "weeeeee"},
                {"enumVal", "Value2" }
            };

            client.ExecuteMethod(methods[0].MethodId, parameters);

            Console.WriteLine($"Properties of {instances[0].Name}:");
            details = client.GetInstanceDetails(instances[0].InstanceId);
            foreach (var property in details.Properties)
            {
                Console.WriteLine($"--> {property.Name} ({property.TypeName}): {property.ValueAsString}");
            }
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}
