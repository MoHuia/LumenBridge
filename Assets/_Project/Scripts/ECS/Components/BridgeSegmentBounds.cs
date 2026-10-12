using Unity.Entities;
using Unity.Mathematics;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 一个桥段的矩形统计区域
    /// 只保存边界，数量统计由后续系统负责
    /// </summary>
    public struct BridgeSegmentBounds : IComponentData
    {
        // 矩形最小边界
        public float2 min;

        // 矩形最大边界
        public float2 max;
    }
}