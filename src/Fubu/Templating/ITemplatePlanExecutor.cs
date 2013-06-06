
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Fubu.Templating
{
    public interface ITemplatePlanExecutor
    {
        void Execute(NewCommandInput input, TemplatePlan plan, Action<TemplatePlanContext> continuation);
    }

    public class TemplatePlanExecutor : ITemplatePlanExecutor
    {
        private readonly IFileSystem _fileSystem;

        public TemplatePlanExecutor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Execute(NewCommandInput input, TemplatePlan plan, Action<TemplatePlanContext> continuation)
        {
            var context = BuildContext(input);

            plan.Preview(context);

            runSteps(plan, context);

            plan.Steps.OfType<IDisposable>().Each(x => x.SafeDispose());

            continuation(context);
            _fileSystem.DeleteDirectory(context.TempDir);
        }

        private static void runSteps(TemplatePlan plan, TemplatePlanContext context)
        {
            try
            {
                plan
                    .Steps
                    .Each(step => step.Execute(context));
            }
            catch (Exception exc)
            {
                context.RegisterError(exc.Message);
                context.RegisterError(exc.StackTrace);
            }
        }

        private TemplatePlanContext BuildContext(NewCommandInput input)
        {
            var context = new TemplatePlanContext
            {
                Input = input,
                TempDir = createTempDir()
            };

            _fileSystem.CreateDirectory(context.TempDir);

            var targetPath = input.ProjectName;
            if (input.OutputFlag.IsNotEmpty())
            {
                targetPath = input.OutputFlag;
            }
            else if (input.SolutionFlag.IsNotEmpty())
            {
                targetPath = _fileSystem.GetDirectory(input.SolutionFlag);
            }

            context.TargetPath = Path.Combine(Environment.CurrentDirectory, targetPath);
            return context;
        }

        // just a sanity check
        private string createTempDir()
        {
            var name = Guid.NewGuid().ToString();
            var tmpDir = Path.Combine(Environment.CurrentDirectory, name);
            if(_fileSystem.DirectoryExists(tmpDir))
            {
                return createTempDir();
            }

            return tmpDir;
        }
    }
}