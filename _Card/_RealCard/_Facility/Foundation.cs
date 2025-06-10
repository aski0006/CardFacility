// 重写 Foundation 类

using _Card._RealCard._Facility;
using _Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class Foundation : BaseFacility {
    [SerializeField] private Material foundationMaterial;
    private GameObject _foundationZone;

    [Button]
    public void test() => OnCardPlace();
    public override void OnCardPlace() {
        // 创建地基区域对象
        _foundationZone = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _foundationZone.name = "FoundationZone";
        _foundationZone.transform.SetParent(transform, false);

        // 配置材质和尺寸
        var foundationMeshRenderer = _foundationZone.GetComponent<Renderer>();
        foundationMeshRenderer.material = foundationMaterial;
        foundationMeshRenderer.material.color = new Color(0.2f, 0.8f, 0.3f, 0.5f);

        // 设置区域尺寸（根据卡牌配置）
        _foundationZone.transform.localScale = new Vector3(
            CurrentFacilityZoneSize.x,
            CurrentFacilityZoneSize.y,
            1
        );

        // 激活地基状态
        _foundationZone.SetActive(true);

        // 注册到网格系统
        GridSystem.Instance.PlaceFoundation(transform.position, this);
    }

    public override void OnCardRemove() {
        // 回收地基
        GridSystem.Instance.RecycleFoundation(this);
        Destroy(_foundationZone);
    }
}
