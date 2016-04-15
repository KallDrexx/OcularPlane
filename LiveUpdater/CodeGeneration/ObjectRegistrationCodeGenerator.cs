using FlatRedBall.Glue.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Plugins.Interfaces;
using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.SaveClasses;

namespace LiveUpdater.CodeGeneration
{
    public class ObjectRegistrationCodeGenerator : ElementComponentCodeGenerator
    {
        public override CodeLocation CodeLocation
        {
            get
            {
                return CodeLocation.AfterStandardGenerated; 
            }
        }

        public override ICodeBlock GenerateFields(ICodeBlock codeBlock, IElement element)
        {
            if (element is ScreenSave)
            {
                codeBlock.Line("OcularPlane.Networking.WcfTcp.Host.OcularPlaneHost host;");
            }
            return codeBlock;
        }

        public override ICodeBlock GenerateAddToManagers(ICodeBlock codeBlock, IElement element)
        {
            if(element is ScreenSave)
            {
                codeBlock.Line(
                    "var containerManager = new OcularPlane.ContainerManager();");

                string screenName = element.ClassName;

                foreach (var namedObject in element.NamedObjects)
                {
                    bool shouldAddAsObject = namedObject.IsList == false;
                    if (shouldAddAsObject)
                    {
                        string line =
                            $"containerManager.AddObjectToContainer(nameof({screenName}), " +
                            $"{namedObject.InstanceName}, nameof({namedObject.InstanceName}));";

                        codeBlock.Line(line);
                    }
                }
                codeBlock.Line(
                    "host = new OcularPlane.Networking.WcfTcp.Host.OcularPlaneHost(containerManager, \"localhost\", 9999);");
            }


            return codeBlock;
        }

        public override void GenerateRemoveFromManagers(ICodeBlock codeBlock, IElement element)
        {
            if (element is ScreenSave)
            {
                codeBlock.Line("host.Dispose();");
            }
        }
    }
}
