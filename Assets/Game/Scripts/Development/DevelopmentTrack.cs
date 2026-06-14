using System.Collections.Generic;

namespace Mossmark.Development
{
    // Flat candidate list of stages; availability is recomputed fresh at attention time
    // via DevelopableEntity.GetAvailableStages().
    public class DevelopmentTrack
    {
        public IReadOnlyList<DevelopmentStage> Stages { get; }

        public DevelopmentTrack(params DevelopmentStage[] stages)
        {
            Stages = new List<DevelopmentStage>(stages);
        }
    }
}
