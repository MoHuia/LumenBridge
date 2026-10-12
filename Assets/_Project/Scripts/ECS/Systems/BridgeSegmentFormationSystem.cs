using _Project.Scripts.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project.Scripts.ECS.Systems
{
    /// <summary>
    /// 读取区域虫数和成立配置，更新连续达标时间与准备结果
    /// 不修改虫数、不驱动显示，也不作旅人通行判断
    /// </summary>
    // 与计数系统同处默认模拟组，先统计，再读取本帧数量
    [UpdateAfter(typeof(BridgeSegmentCountSystem))]
    public partial class BridgeSegmentFormationSystem : SystemBase
    {
        /// <summary> 桥段计时数据存在后才更新 </summary>
        protected override void OnCreate()
        {
            RequireForUpdate<BridgeSegmentFormationProgress>();
        }

        /// <summary> 连续达标就累加，不足就重新开始 </summary>
        protected override void OnUpdate()
        {
            // 与光虫运动使用同一 ECS 时间步，单位为秒
            var deltaTime = SystemAPI.Time.DeltaTime;

            // 数量和配置只读；只有计时结果允许写入
            foreach (var (count, settings, progress) in
                     SystemAPI.Query<RefRO<BridgeSegmentCount>,
                         RefRO<BridgeSegmentFormationSettings>,
                         RefRW<BridgeSegmentFormationProgress>>())
            {
                if (count.ValueRO.value >= settings.ValueRO.requiredCount)
                {
                    // 在上次连续时间上增加这一帧经过的秒数
                    // 达到所需时间后封顶，避免无限增长
                    progress.ValueRW.elapsed = math.min(
                        progress.ValueRO.elapsed + deltaTime,
                        settings.ValueRO.duration);
                }
                else
                {
                    // 一旦不足，打断本轮连续计时
                    progress.ValueRW.elapsed = 0f;
                }

                // 根据本帧更新后的时间，覆盖准备结果
                // 计时封顶后采用 >=，达到要求的那一帧即为 true
                progress.ValueRW.isReady =
                    progress.ValueRO.elapsed >= settings.ValueRO.duration;
            }
        }
    }
}