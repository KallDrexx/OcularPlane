using FlatRedBall.Glue.Plugins.ExportedImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfDataUi.DataTypes;
using System.Timers;
using OcularPlane.Networking.WcfTcp.Client;
using OcularPlane.Models;
using FlatRedBall.Glue.Parsing;
using Glue;

namespace LiveUpdater
{
    public class PropertiesController
    {
        InstanceReference[] instances = new InstanceReference[0];
        System.Timers.Timer timer;


        OcularPlaneClient client;

        Guid lastInstanceGuid = Guid.Empty;

        WpfDataUi.DataUiGrid grid;
        public WpfDataUi.DataUiGrid Grid
        {
            get { return grid; }
            set { grid = value; }
        }

        public bool IsConnected { get; private set; }

        public PropertiesController()
        {
            CreateTimer();

        }

        private void CreateTimer()
        {
            const int millisecondFrequency = 200;

            timer = new System.Timers.Timer(millisecondFrequency);
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;
        }

        bool runningTimedEvent = false;
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (!runningTimedEvent)
            {
                runningTimedEvent = true;
                UpdateIsConnected();

                if (IsConnected)
                {
                    PullFromHost();

                    UpdateGridCategories();
                }
                else
                {
                    MainGlueWindow.Self.Invoke(() =>
                    {
                        grid.Categories.Clear();
                    });
                }
                runningTimedEvent = false;
            }
        }

        private void HideGrid()
        {
            throw new NotImplementedException();
        }

        private void PullFromHost()
        {
            var currentScreen = GlueState.Self.CurrentScreenSave;
            if (currentScreen != null)
            {
                instances = client.GetInstancesInContainer(currentScreen.ClassName);
            }
        }

        private void UpdateIsConnected()
        {
            bool succeeded = true;
            try
            {
                if (!IsConnected)
                {
                    client = new OcularPlaneClient("localhost", 9999);
                }
                client.Ping();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect because " + e.ToString());
                succeeded = false;
            }

            this.IsConnected = succeeded;
        }

        private void UpdateGridCategories()
        {
            MainGlueWindow.Self.Invoke(() =>
            {

                var glueNamedObject = GlueState.Self.CurrentNamedObjectSave;
                var instanceName = glueNamedObject?.InstanceName;

                var foundInstance = instances.FirstOrDefault(item => item.Name == instanceName);

                if (foundInstance != null)
                {

                    bool shouldReconstructGrid = lastInstanceGuid != foundInstance.InstanceId;

                    if (shouldReconstructGrid)
                    {
                        grid.Categories.Clear();

                        var mainCategory = new MemberCategory("Main Variables");
                        var debuggingCategory = new MemberCategory("Debugging Variables");

                        var details = client.GetInstanceDetails(foundInstance.InstanceId);
                        foreach (var property in details.Properties)
                        {
                            var member = new ConnectedInstanceMember();
                            member.Client = client;
                            member.InstanceReference = foundInstance;
                            member.PropertyReference = property;
                            member.Type = TypeManager.GetTypeFromString(property.TypeName);

                            member.Name = property.Name;
                            member.DisplayName = FlatRedBall.Utilities.StringFunctions.InsertSpacesInCamelCaseString(member.Name);


                            bool shouldAppearInMainList = glueNamedObject.HasCustomVariable(member.Name);

                            if (shouldAppearInMainList)
                            {
                                mainCategory.Members.Add(member);
                            }
                            else
                            {
                                debuggingCategory.Members.Add(member);
                            }
                        }

                        if(mainCategory.Members.Count != 0)
                        {
                            grid.Categories.Add(mainCategory);
                        }
                        if(debuggingCategory.Members.Count != 0)
                        {
                            grid.Categories.Add(debuggingCategory);
                        }

                        lastInstanceGuid = foundInstance.InstanceId;
                    }
                    else
                    {
                        var details = client.GetInstanceDetails(foundInstance.InstanceId);

                        foreach (var category in grid.Categories)
                        {
                            foreach (var uncastedMember in category.Members)
                            {
                                var member = uncastedMember as ConnectedInstanceMember;

                                member.PropertyReference = details.Properties.FirstOrDefault(item => item.Name == member.Name);
                            }
                        }

                        grid.Refresh();
                    }
                }
                else
                {
                    lastInstanceGuid = Guid.Empty;
                    grid.Categories.Clear();
                }
            });
            //var instanceMember = new InstanceMember("Some value", this);
            //instanceMember.CustomSetEvent += (owner, value) =>
            //{
            //    System.Console.WriteLine($"Setting the value of {owner} to {value}");
            //};

            //instanceMember.CustomGetEvent += (owner) =>
            //{
            //    System.Console.WriteLine($"Returning the value for {owner}");
            //    return 10;
            //};

            //instanceMember.CustomGetTypeEvent += (owner) =>
            //{
            //    System.Console.WriteLine($"Returning the type for {owner}");

            //    return typeof(int);
            //};

            //category.Members.Add(instanceMember);

            //Grid.Categories.Add(category);
        }

        private List<PropertyReference> GetObjectProperties(object host, string v, object p)
        {
            throw new NotImplementedException();
        }
    }
}
