using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveUpdater
{
    [Export(typeof(PluginBase))]
    public class MainPlugin : PluginBase
    {

        Views.PropertiesControl control;
        PropertiesController controller;

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
                return new Version();
            }
        }

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            return true;
        }

        public override void StartUp()
        {
            AddGridToMiddle();


        }

        private void AddGridToMiddle()
        {
            control = new Views.PropertiesControl();

            base.AddToTab(PluginManager.CenterTab, control, "Runtime Variables");

            controller = new PropertiesController();
            controller.Grid = control.DataGrid;

        }
    }
}
