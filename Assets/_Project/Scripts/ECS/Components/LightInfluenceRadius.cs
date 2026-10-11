using Unity.Entities;

namespace _Project.Scripts.ECS.Components
{
    /// <summary>
    /// 共同光源的吸引半径，只添加到光源实体。
    /// 与位置分开保存，拖光更新位置时不会覆盖半径。
    /// </summary>
    public struct LightInfluenceRadius : IComponentData
    {
        // 光虫距离光源不超过此值时参与追光，单位为世界单位。
        public float value;
    }
}