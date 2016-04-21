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
using FlatRedBall.Glue.Plugins;
using LiveUpdater.ViewModels;
using LiveUpdater.Views;
using System.ComponentModel;

namespace LiveUpdater
{
    public class PropertiesController
    {
        public List<InstanceReference> instances = new List<InstanceReference>();
        System.Timers.Timer timer;

        RuntimeObjectListViewModel objectListViewModel = new RuntimeObjectListViewModel();

        OcularPlaneClient client;

        Guid lastInstanceGuid = Guid.Empty;

        WpfDataUi.DataUiGrid grid;
        public WpfDataUi.DataUiGrid Grid
        {
            get { return grid; }
            set { grid = value; }
        }

        RuntimeObjectListView listView;
        public RuntimeObjectListView ListView
        {
            get
            {
                return listView;
            }
            set
            {
                listView = value;

                if(listView != null)
                {
                    listView.DataContext = objectListViewModel;
                }
            }
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
        private void OnTimedEvent(object sender, ElapsedEventArgs ev)
        {
            if (!runningTimedEvent)
            {
                runningTimedEvent = true;
                try
                {

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
                            this.objectListViewModel.MenuItems.Clear();
                        });
                    }
                }
                catch(Exception ex)
                {
                    // do nothing, just keep going...
                    //PluginManager.ReceiveError("Unexpected error in Live Updater plugin:\n" + ex.ToString());
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
            instances.Clear();

            instances.AddRange(
                client.GetInstancesInContainer("CurrentScreen"));


            UpdateListView();
        }

        private void UpdateListView()
        {
            MainGlueWindow.Self.Invoke(() =>
            {
                var existingUiItems = objectListViewModel.MenuItems;
                //ListView.Items.Cast<CustomMenuItem<InstanceReference>>().ToList();

                HashSet<CustomMenuItem<InstanceReference>> visitedUiItems = new HashSet<CustomMenuItem<InstanceReference>>();

                foreach (var item in instances)
                {
                    var foundItem = existingUiItems
                        .FirstOrDefault(uiItem => (uiItem.Tag as InstanceReference).InstanceId == item.InstanceId);

                    var alreadyPartOfList = foundItem != null;

                    if (!alreadyPartOfList)
                    {

                        var menuItem = new CustomMenuItem<InstanceReference>();
                        menuItem.Title = item.Name;
                        menuItem.Tag = item;
                        objectListViewModel.MenuItems.Add(menuItem);

                        visitedUiItems.Add(menuItem);
                    }
                    else
                    {
                        visitedUiItems.Add(foundItem);
                    }
                }

                // Remove any items which haven't been visited:
                for(int i = objectListViewModel.MenuItems.Count -1; i > -1; i--)
                {
                    var removalCandidate = objectListViewModel.MenuItems[i];

                    if(!visitedUiItems.Contains(removalCandidate))
                    {
                        objectListViewModel.MenuItems.RemoveAt(i);
                    }
                }
            });
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
                var selectedInstance = objectListViewModel?.SelectedItem?.Tag;
                if (selectedInstance != null)
                {

                    bool shouldReconstructGrid = lastInstanceGuid != selectedInstance.InstanceId;

                    if (shouldReconstructGrid)
                    {
                        grid.Categories.Clear();

                        var mainCategory = new MemberCategory("Main Variables");
                        var debuggingCategory = new MemberCategory("Debugging Variables");

                        var details = client.GetInstanceDetails(selectedInstance.InstanceId);
                        foreach (var property in details.Properties)
                        {
                            var member = new ConnectedInstanceMember();
                            member.Client = client;
                            member.InstanceReference = selectedInstance;
                            member.PropertyReference = property;
                            member.Type = TypeManager.GetTypeFromString(property.TypeName);

                            member.Name = property.Name;
                            member.DisplayName = FlatRedBall.Utilities.StringFunctions.InsertSpacesInCamelCaseString(member.Name);

                            // eventually find the matching NOS and update it.
                            bool shouldAppearInMainList = false;// glueNamedObject.HasCustomVariable(member.Name);

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

                        lastInstanceGuid = selectedInstance.InstanceId;
                    }
                    else
                    {
                        var details = client.GetInstanceDetails(selectedInstance.InstanceId);

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
