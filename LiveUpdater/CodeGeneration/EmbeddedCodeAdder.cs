using FlatRedBall.Glue.VSHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveUpdater.CodeGeneration
{
    public class EmbeddedCodeAdder
    {
        public static void AddEmbeddedCode()
        {
            var adder = new CodeBuildItemAdder();
            adder.OutputFolderInProject = "OcularPlaneRuntime";
            adder.AddFileBehavior = AddFileBehavior.IfOutOfDate;
            adder.Add("LiveUpdater.CodeGeneration.EntireFiles.OcularPlaneManager.cs");

            var assembly = typeof(EmbeddedCodeAdder).Assembly;

            adder.PerformAddAndSave(assembly);
        }
    }
}
