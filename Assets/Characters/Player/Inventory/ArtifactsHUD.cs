// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ColbyDoan
{
    public class ArtifactsHUD : MonoBehaviour
    {
        public ArtifactManager artifactManager;
        public ArtifactIndicator indicatorProp;

        // [SerializeField] ArtifactIndicator[] indicators;
        Dictionary<string, ArtifactIndicator> indicators = new Dictionary<string, ArtifactIndicator>();

        void Start()
        {
            artifactManager.OnArtifactAdded += AddNewArtifact;
        }

        void AddNewArtifact(NewArtifactInfo info)
        {
            ArtifactIndicator indicator;
            if (!indicators.TryGetValue(info.triggerItem.idName, out indicator))
            {
                // trigger not present, create new indicator, add effect
                indicator = Instantiate<ArtifactIndicator>(indicatorProp, transform);
                indicator.SetTrigger(info);

                indicators.Add(info.triggerItem.idName, indicator);
            }
            // add/update effect only
            indicator.AddEffect(info);
        }
    }
}
