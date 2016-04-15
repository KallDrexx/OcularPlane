using FlatRedBall.Glue.Parsing;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.Interfaces;
using LiveUpdater.CodeGeneration;
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
        ObjectRegistrationCodeGenerator generator;
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
            CodeWriter.CodeGenerators.Remove(generator);
            return true;
        }

        public override void StartUp()
        {
            AddGridToMiddle();

            if (generator == null)
            {
                generator = new ObjectRegistrationCodeGenerator();
            }
            CodeWriter.CodeGenerators.Add(generator);
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
