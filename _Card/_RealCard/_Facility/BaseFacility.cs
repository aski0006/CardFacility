using _Enums;
using _ScriptableObject;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Card._RealCard._Facility {

    [Serializable]
    public class FacilityResourceCost {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int cost;

        public ResourceType ResourceType => resourceType;
        public int Cost => cost;
    }

    public abstract class BaseFacility : BaseCard {
        public Vector2Int FacilitySize => ((FacilityCardData)CardData).FacilitySize;
        public IReadOnlyCollection<FacilityResourceCost> BuildCost => ((FacilityCardData)CardData).BuildCost;

        public Vector2Int CurrentFacilityZoneSize { get; set; }
        public Vector2Int CurrentFacilityZoneCenter { get; set; }

        protected override void Awake() {
            base.Awake();
            CurrentFacilityZoneSize = FacilitySize;
        }
        public virtual void ResetFacilityZoneSize(int deltaX, int deltaY) { }
        public override void ApplyCardEffect() { }
        public override void RemoveCardEffect() { }
    }
}
