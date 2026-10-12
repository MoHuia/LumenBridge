using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>桥段成立条件的配置，运行计时系统只读取它。</summary>
    public struct BridgeSegmentFormationSettings : IComponentData
    {
        // 数量阈值，单位为只；达到或超过该数量才开始计时。
        public int requiredCount;

        // 连续达标所需时间，单位为秒，初始化时设置为正数。
        public float duration;
    }
}