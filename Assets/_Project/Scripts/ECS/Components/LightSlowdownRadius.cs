using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 共同光源附近的减速区域，只添加到光源实体
    /// </summary>
    public struct LightSlowdownRadius : IComponentData
    {
        // 单位为世界单位，必须大于0
        // 当前设置为 0.8，大于光虫的停止距离0.2
        public float value;
    }
}