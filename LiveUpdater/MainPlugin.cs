using FlatRedBall.Glue.Parsing;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.Interfaces;
using LiveUpdater.CodeGeneration;
using LiveUpdater.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Controls;
using Glue;

namespace LiveUpdater
{
    [Export(typeof(PluginBase))]
    public class MainPlugin : PluginBase
    {
        ObjectRegistrationCodeGenerator generator;
        Views.PropertiesControl propertiesControl;
        RuntimeObjectListView runtimeObjectListView;

        PropertiesController controller;
        private PluginTab runtimeObjectsTab;

        Toolbar toolbar;

        public override string FriendlyName
        {
            get
            {
                return "Live Updater";
            }
        }

        public override Version Version
        {
            get
            {
                return new Version(0,1);
            }
        }

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            CodeWriter.CodeGenerators.Remove(generator);
            return true;
        }

        public override void StartUp()
        {
            AddUi();

            if (generator == null)
            {
                generator = new ObjectRegistrationCodeGenerator();
                CodeWriter.CodeGenerators.Add(generator);
            }

            base.ReactToLoadedGlux += HandleGluxLoaded;


        }

        private void HandleGluxLoaded()
        {
            EmbeddedCodeAdder.AddEmbeddedCode();

        }

        private void AddUi()
        {
            controller = new PropertiesController();

            controller.ConnectedChanged += HandleConnectedChanged;

            propertiesControl = new Views.PropertiesControl();
            runtimeObjectListView = new RuntimeObjectListView();

            controller.Grid = propertiesControl.DataGrid;
            controller.ListView = runtimeObjectListView;

            AddGridToMiddle();

            ShowRuntimeList();

            toolbar = new Toolbar();
        }

        private void HandleConnectedChanged(object sender, EventArgs e)
        {
            MainGlueWindow.Self.Invoke(() =>
            {
                if(controller.IsConnected)
                {
                    AddToToolBar(toolbar, "Runtime");
                }
                else
                {
                    RemoveFromToolbar(toolbar, "Runtime");
                }

            });
        }

        private void ShowRuntimeList()
        {
            if(runtimeObjectsTab == null)
            {
                runtimeObjectsTab = base.AddToTab(PluginManager.RightTab, runtimeObjectListView, "Runtime Objects");
            }
            else
            {
                base.ShowTab(runtimeObjectsTab);
            }
        }

        private void AddGridToMiddle()
        {

            base.AddToTab(PluginManager.CenterTab, propertiesControl, "Runtime Variables");


        }
    }
}
