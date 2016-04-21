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
                codeBlock.Line(
                    "OcularPlane.ContainerManager containerManager = new OcularPlane.ContainerManager();");

            }
            return codeBlock;
        }

        public override ICodeBlock GenerateAddToManagers(ICodeBlock codeBlock, IElement element)
        {
            if(element is ScreenSave)
            {
                string screenName = element.ClassName;

                foreach (var namedObject in element.NamedObjects)
                {
                    if (namedObject.IsList)
                    {
                        var instanceName = namedObject.InstanceName;
                        codeBlock.Line($"this.{instanceName}.CollectionChanged += (sender, args) =>");
                        var block1 = codeBlock.Block();
                        {
                            var block2 = block1.If("args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add");
                            {
                                var foreachBlock = block2.ForEach("var item in args.NewItems");
                                {
                                    foreachBlock.Line($"containerManager.AddObjectToContainer(\"CurrentScreen\", item, \"{instanceName}.\" + item.GetType().Name + \"Instance\");");
                                }
                            }
                            block2 = block1.ElseIf("args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove");
                            {
                                var foreachBlock = block2.ForEach("var item in args.OldItems");
                                {
                                    foreachBlock.Line("containerManager.RemoveInstanceByObject(item);");
                                }
                            }
                        };
                        codeBlock.Line(";");
                    }
                    else
                    {

                        string line =
                            $"containerManager.AddObjectToContainer(\"CurrentScreen\", " +
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
