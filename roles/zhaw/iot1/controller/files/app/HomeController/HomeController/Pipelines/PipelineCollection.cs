using System.Collections.Generic;

namespace HomeController.Pipelines
{
    public class PipelineCollection : List<IPipeline>
    {
        public static PipelineCollection Create() => new();

        public new PipelineCollection Add(IPipeline pipeline)
        {
            base.Add(pipeline);
            return this;
        }
    }
}
