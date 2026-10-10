using Unity.Entities;
using Unity.Mathematics;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 玩家控制的光源位置数据
    /// 后续放在一个独立实体上，供光虫运动系统共同读取
    /// </summary>
    public struct LightTargetPosition : IComponentData
    {
        // 光源的世界坐标
        // 游戏在 XZ 平面上进行，因此 y 保持为 0
        public float3 value;
    }
}