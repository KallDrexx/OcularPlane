using FlatRedBall.Glue.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Plugins.Interfaces;
using FlatRedBall.Glue.CodeGeneration.CodeBuilder;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.Glue.Plugins.ExportedImplementations;

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
                codeBlock.Line(
                    "OcularPlane.ContainerManager containerManager;");

            }
            return codeBlock;
        }

        public override ICodeBlock GenerateAddToManagers(ICodeBlock codeBlock, IElement element)
        {
            if(element is ScreenSave)
            {
                codeBlock.Line("#if DEBUG");
                string projectNamespace = GlueState.Self.ProjectNamespace;
                codeBlock.Line(
                    $"containerManager = {projectNamespace}.OcularPlaneRuntime.OcularPlaneManager.GetContainerManager();");
                string screenName = element.ClassName;

                foreach (var namedObject in element.NamedObjects)
                {
                    if (namedObject.IsList)
                    {
                        GenerateList(codeBlock, namedObject);
                    }
                    else
                    {
                        string line = GetAddToContainerStringFor(namedObject);

                        codeBlock.Line(line);
                    }
                }
                codeBlock.Line("#endif");
            }


            return codeBlock;
        }

        private static string GetAddToContainerStringFor(NamedObjectSave namedObject)
        {
            return $"containerManager.AddObjectToContainer(\"CurrentScreen\", " +
                $"{namedObject.InstanceName}, nameof({namedObject.InstanceName}));";
        }

        private static void GenerateList(ICodeBlock codeBlock, NamedObjectSave namedObject)
        {
            codeBlock = codeBlock.Block();
            {
                codeBlock.Line($"var listId = {GetAddToContainerStringFor(namedObject)};");

                var instanceName = namedObject.InstanceName;
                codeBlock.Line($"this.{instanceName}.CollectionChanged += (sender, args) =>");
                var block1 = codeBlock.Block();
                {
                    var block2 = block1.If("args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add");
                    {
                        var foreachBlock = block2.ForEach("var item in args.NewItems");
                        {
                            foreachBlock.Line($"containerManager.AddObjectToContainer(\"CurrentScreen\", item, \"{instanceName}.\" + item.GetType().Name + \"Instance\", listId);");
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
        }

        public override void GenerateRemoveFromManagers(ICodeBlock codeBlock, IElement element)
        {
            GenerateClearContainerManager(codeBlock, element);
        }

        private static void GenerateClearContainerManager(ICodeBlock codeBlock, IElement element)
        {
            if (element is ScreenSave)
            {
                codeBlock.Line("#if DEBUG");

                string projectNamespace = GlueState.Self.ProjectNamespace;
                string line =
                    $"{projectNamespace}.OcularPlaneRuntime.OcularPlaneManager.GetContainerManager().ClearObjects(\"CurrentScreen\");";
                codeBlock.Line(line);
                codeBlock.Line("#endif");

            }
        }

        public override ICodeBlock GenerateDestroy(ICodeBlock codeBlock, IElement element)
        {
            GenerateClearContainerManager(codeBlock, element);
            return codeBlock;
        }
    }
}
