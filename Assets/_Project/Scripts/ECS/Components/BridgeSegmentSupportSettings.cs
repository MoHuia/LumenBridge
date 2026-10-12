using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary> 已经成立的桥段如何维持支撑，状态系统只读配置 </summary>
    public struct BridgeSegmentSupportSettings : IComponentData
    {
        // 保持阈值，单位为只；本步为 12，低于成立阈值 18
        public int maintainCount;

        // 连续支撑不足的宽限时长，单位为秒；本步为 0.8
        public float graceDuration;
    }
}