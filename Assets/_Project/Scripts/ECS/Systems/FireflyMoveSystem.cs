using _Project.Scripts.ECS.Components;
using Unity.Entities;
using Unity.Transforms;

namespace _Project.Scripts.ECS.Systems
{
    /// <summary>
    /// 光虫运动系统
    /// 读取速度，更新实体的位置，不直接操作显示用的 GameObject
    /// </summary>
    public partial class FireflyMoveSystem : SystemBase
    {
        /// <summary>
        /// 系统每次更新时，让符合条件的实体沿 X 正方向移动
        /// </summary>
        protected override void OnUpdate()
        {
            // ECS 世界这一帧经过的时间，单位为秒
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            // 查询同时拥有位置组件和速度组件的实体
            // RefRW：允许读取并修改位置
            // RefRO：只读取速度，不修改速度
            foreach (var (localTransform,speed) in 
                     SystemAPI.Query<RefRW<LocalTransform>,RefRO<FireflySpeed>>())
            {
                // 将每秒移动距离换算成这一帧移动距离
                // ValueRW 访问可修改的数据，ValueRO 访问只读数据
                localTransform.ValueRW.Position.x += speed.ValueRO.value * deltaTime;
            }
        }
    }
}