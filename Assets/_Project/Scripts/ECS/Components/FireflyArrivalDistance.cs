using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 光虫接近共同光源时使用的停止距离数据。
    /// 目的地由 LightTargetPosition 统一提供，这里只保存到达判定参数。
    /// </summary>
    public struct FireflyArrivalDistance : IComponentData
    {
        // 距离目标小于等于此值时，视为到达
        // 单位为 Unity 世界单位
        public float stoppingDistance;
    }
}
