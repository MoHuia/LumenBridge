using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 光虫的移动速度数据组件。
    /// 实现 IComponentData，使这份结构体数据能够添加到 ECS 实体上。
    /// 这里只保存速度；后续由运动系统读取速度并更新位置。
    /// </summary>
    public struct FireflySpeed : IComponentData
    {
        /// <summary>
        /// 移动速度，单位为 Unity 单位/秒。
        /// 例如 2 表示每秒移动 2 个单位；方向由运动逻辑另行决定。
        /// </summary>
        public float value;
    }
}
