using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary> 单段桥的支撑阶段，不代表整座桥已经可通行 /// </summary>
    public enum BridgeSegmentPhase
    {
        // 0 为默认值：新建组件时桥尚未成立
        Missing = 0,
        // 已成立，且当前数量足以维持支撑
        Stable,
        // 已成立但数量不足，处于恢复宽限期
        Unstable
    }

    /// <summary> 单段桥的实时支撑状态，由状态系统负责写入 /// </summary>
    public struct BridgeSegmentSupportState : IComponentData
    {
        // 当前阶段；初始化默认为 Missing
        public BridgeSegmentPhase phase;

        // 连续低于保持阈值的时长，单位为秒
        // 恢复支撑、失效或重新成立时清零
        public float shortageElapsed;
    }
}