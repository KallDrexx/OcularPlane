OcularPlane is a library that allows users to remotely track data from a live running application, as well as remotely update that data at any time.  

The ultimate goal of the project is to allow developers to gain live insights into what's going on with their application, and see how modifications to that data affect the application without rebuilding or redeploying, even if the application is currently an iOS application running on an iPhone.

## Concepts

The library works based on the idea of containers.  The server application (application with the data to debug) creates containers (identified via strings) and places objects into those containers.  The client (via a remote interface of some sort) then queries those containers for objects, and then can perform options on those objects.  Clients are also able to store, list and execute methods stored in containers.

## Networking Implementations

The library comes with an optional networking implementation supporting WcF TCP bindings

## Simple Example

Let's assume you are making a game and you want to gain visibility the player's ship, and change their fire rate while the game is running.

In the game code we will instantiate a new container manager, start the wcf host, and add the player to a container

```c#
  var containerManager = new ContainerManager(); // Create an overall manager to manage all the containers
  var host = new OcularPlaneHost(containerManager, "localhost", 9999) // Starts the wcf host
  
  // ....
  // Later in the game, add the player object to the "CurrentLevel" container
  
  containerManager.AddObjectToContainer("CurrentLevel", player, "player");
```

Now on another application (even on another computer) we can connect via the wcf client and query for the current properties of the player:

```c#
var client = new OcularPlaneClient("localhost", 9999);
client.Ping(); // Make sure the connection is live

var container = client.GetContainerNames().Single(x => x == "CurrentLevel");
var playerInstance = client.GetInstancesInContainer(container).Single(x => x.Name == "player');
var playerInstanceDetails = client.GetInstanceDetails(playerInstance.InstanceId);

// Let's change the Player.FireRate property to 5.5
var fireRateProperty = playerInstanceDetails.Properties.Single(x => x.Name == "FireRate");
client.SetPropertyValue(playerInstanceDetails.InstanceId, details.Properties[0].Name, "3.3");

// Verify it changed
var newPlayerInstanceDetails = client.GetInstanceDetails(playerInstance.InstanceId);
var property = newPlayerInstanceDetails = Properties.Single(x => x.Name == "FireRate");
// property.ValueAsString == 3.3
```

Now let's say the player has a method to spawn enemies, and they would like the ability for their remote tools to execute this method on demand to test scenarios without resetting the level (or building a debug specific UI inside the game).  This can be done via the following:

```c#
//....................
// On the host
//....................

public enum ScenarioType { FiveEnemyFlank, TenEnemiesFromTop }
public void SpawnEnemyScenario(ScenarioType scenario) { ... }

containerManager.AddMethodToContainer("CurrentLevel", () => game.SpawnEnemyScenario(0), "spawn enemies");

//....................
// On the client
//....................

var method = client.GetMethodsInContainer("CurrentLevel").Single(x => x.Name == "spawn enemies");
var parameters = new Dictionary<string, string> {{"scenario", "TenEnemiesFromTop"}};
client.ExecuteMethod(method.MethodId, parameters); // method is now invoked on the host
```
