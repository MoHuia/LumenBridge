using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 保存桥段矩形内光虫中心点的数量
    /// 它是统计结果；运行时由桥段统计系统负责更新
    /// </summary>
    public struct BridgeSegmentCount : IComponentData
    {
        // 单位为只；初始化为 0，后续每帧覆盖为实际数量
        public int value;
    }
}