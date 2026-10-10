using _Project.Scripts.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
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
        /// 系统创建时，设置运行所需的数据条件
        /// </summary>
        protected override void OnCreate()
        {
            // 世界中存在光源位置组件后，才执行 OnUpdate
            // 这里只检查是否存在，不保证只有一份
            RequireForUpdate<LightTargetPosition>();
        }

        /// <summary>
        /// 系统每次更新时，让光虫朝目标移动，到达停止范围后停止
        /// </summary>
        protected override void OnUpdate()
        {
            // ECS 世界这一帧经过的时间，单位为秒
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            // 读取世界中唯一的一份光源位置数据
            // 放在循环外，让所有光虫使用本帧读取的共同位置
            // GetSingleton 要求匹配实体恰好只有一个
            var lightTarget = SystemAPI.GetSingleton<LightTargetPosition>();
            
            // 查询同时拥有位置、速度和到达距离组件的光虫实体。
            // RefRW：允许读取并修改位置
            // RefRO：只读取速度，不修改速度
            foreach (var (localTransform, speed, arrivalDistance) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<FireflySpeed>, RefRO<FireflyArrivalDistance>>())
            {
                // 从当前位置指向共同光源的向量，包含方向和距离信息。
                var offset = lightTarget.value - localTransform.ValueRO.Position;
                
                // 向量长度就是当前位置与目标之间的距离
                var distance = math.length(offset);
                
                // 已进入停止范围，跳过这只虫本帧的移动
                // 同时避免后面除以零
                if (distance <= arrivalDistance.ValueRO.stoppingDistance || distance <= 0f)
                    continue;
                
                // 除以长度，得到长度为 1 的方向向量
                // 让移动速度不随目标远近改变
                var direction = offset / distance;
                
                // 这一帧最多走 “速度 x 时间” 这么远
                // 限制步长，避免一步越过目标
                var step = math.min(speed.ValueRO.value * deltaTime, distance);
                
                // 将每秒移动距离换算成这一帧移动距离
                // ValueRW 访问可修改的数据，ValueRO 访问只读数据
                localTransform.ValueRW.Position += direction * step;
            }
        }
    }
}
