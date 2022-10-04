using System.Collections.Generic;

namespace ColbyDoan
{
    public static class ArtifactFactory
    {
        public static ObjectFactory<Trigger> triggerFactory = new ObjectFactory<Trigger>();
        public static ObjectFactory<Effect> effectFactory = new ObjectFactory<Effect>();
        //public static ObjectFactory<PremadeArtifact> artifactFactory = new ObjectFactory<PremadeArtifact>();

        public static Dictionary<ItemType, IFactory> factoryFromItemType = new Dictionary<ItemType, IFactory>()
    {
        //{ ItemType.Artifact, artifactFactory },
        { ItemType.Effect, effectFactory },
        { ItemType.Trigger, triggerFactory }
    };
    }
}