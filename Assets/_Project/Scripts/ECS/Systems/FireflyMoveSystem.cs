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
            
            // 半径数据存在后，系统才能读取并执行范围判断
            RequireForUpdate<LightInfluenceRadius>();
            
            // 减速参数存在后，系统才能读取它
            RequireForUpdate<LightSlowdownRadius>();
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
            
            // 所有光虫共用同一份减速参数。
            var slowdownRadius = SystemAPI.GetSingleton<LightSlowdownRadius>();
            
            // 所有光虫共用这份半径，本帧只需在循环外读取一次
            var influenceRadius = SystemAPI.GetSingleton<LightInfluenceRadius>();
            
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
                
                // 范围外跳过这只虫本帧的移动
                // 当前暂时保持原位，后续再加入游荡行为
                if (distance > influenceRadius.value)
                    continue;
                
                // 已进入停止范围，跳过这只虫本帧的移动
                // 同时避免后面除以零
                if (distance <= arrivalDistance.ValueRO.stoppingDistance || distance <= 0f)
                    continue;
                
                // 除以长度，得到长度为 1 的方向向量
                // 让移动速度不随目标远近改变
                var direction = offset / distance;
                
                // saturate 将比例限制在 0～1。
                // 距离至少为 0.8 时比例为 1，保持全速；越靠近中心比例越小。
                // Max 防止误将减速半径设为 0 时发生除零。
                var speedScale = math.saturate(
                    distance / math.max(slowdownRadius.value, 0.0001f));

                // 按比例降低速度，再乘时间步得到本帧位移。
                // 仍限制步长，避免越过目标。
                var step = math.min(
                    speed.ValueRO.value * speedScale * deltaTime,
                    distance);
                
                // 将每秒移动距离换算成这一帧移动距离
                // ValueRW 访问可修改的数据，ValueRO 访问只读数据
                localTransform.ValueRW.Position += direction * step;
            }
        }
    }
}
