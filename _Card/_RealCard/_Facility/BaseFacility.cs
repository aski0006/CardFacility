using _Card._RealCard._Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Card._RealCard._Facility {
    public enum RegionType {
        Containment, // 收容区
        Processing, // 加工区
        Purification // 净化区
    }

    [Serializable]
    public class FacilityResourceCost {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int cost;

        public ResourceType ResourceType => resourceType;
        public int Cost => cost;
    }

    public abstract class BaseFacility : BaseCard {
        [SerializeField] private Vector2Int facilitySize;
        [SerializeField] private Vector2Int facilityCenter;
        [SerializeField] private List<FacilityResourceCost> buildCost = new List<FacilityResourceCost>();
        [SerializeField] private bool isActive = true;

        #region 属性

        public Vector2Int FacilitySize {
            get => facilitySize;
            set => facilitySize = value;
        }
        public Vector2Int FacilityCenter {
            get => facilityCenter;
            set => facilityCenter = value;
        }
        public IReadOnlyCollection<FacilityResourceCost> BuildCost {
            get => buildCost.AsReadOnly();
            set => buildCost = value.ToList();
        }
        public bool IsActive {
            get => isActive;
            set => isActive = value;
        }

        #endregion

        public void ResetFacilityZoneSize(int deltaX, int deltaY) {
            this.facilitySize.x += deltaX;
            this.facilitySize.y += deltaY;
        }
        public override void ApplyCardEffect() { }
        public override void RemoveCardEffect() { }
        public override void OnCardPlace() { }
        public override void OnCardRemove() { }
    }
}
