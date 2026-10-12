using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 成立条件的实时计算结果，由计时系统更新。
    /// isReady 仅表示成立条件满足，不代表完整桥可通行。
    /// </summary>
    public struct BridgeSegmentFormationProgress : IComponentData
    {
        // 本轮连续达标时长，单位为秒；不足阈值就归零。
        public float elapsed;

        // 连续达标是否已经达到配置的 duration。
        public bool isReady;
    }
}