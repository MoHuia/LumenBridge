using _Project.Scripts.ECS.Components;
using Unity.Entities;

namespace _Project.Scripts.ECS.Systems
{
    /// <summary>
    /// 消费虫数和成立条件，更新单段桥的支撑阶段与不足计时
    /// 不重算数量，不修改成立进度，不驱动材质或旅人
    /// </summary>
    // 读取本帧已经更新过的成立条件
    [UpdateAfter(typeof(BridgeSegmentFormationSystem))]
    public partial class BridgeSegmentSupportSystem : SystemBase
    {
        /// <summary> 存在支撑状态后才开始更新 /// </summary>
        protected override void OnCreate()
        {
            RequireForUpdate<BridgeSegmentSupportState>();
        }

        /// <summary> 根据当前阶段采用成立或维持规则 /// </summary>
        protected override void OnUpdate()
        {
            // 不足计时使用 ECS 帧时间，单位为秒
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            // 三份输入只读；只有支撑状态允许修改
            foreach (var (count,formation,settings,support) in 
                     SystemAPI.Query<RefRO<BridgeSegmentCount>,
                     RefRO<BridgeSegmentFormationProgress>,
                     RefRO<BridgeSegmentSupportSettings>,
                     RefRW<BridgeSegmentSupportState>>())
            {
                if (support.ValueRO.phase == BridgeSegmentPhase.Missing)
                {
                    // 未成立/已失效时，不累计支撑不足时间
                    support.ValueRW.shortageElapsed = 0f;
                    
                    // 只有成立计时已完成，才能进入稳定状态
                    if (formation.ValueRO.isReady)
                    {
                        support.ValueRW.phase = BridgeSegmentPhase.Stable;
                    }
                    
                    // 本桥段本帧的成立判断结束，处理下一个阶段
                    continue;
                }
                
                // 走到这说明桥已经成立：Stable 或 Unstable
                // 按保持阈值判断，不再要求 isReady 一直为 true
                if (count.ValueRO.value >= settings.ValueRO.maintainCount)
                {
                    // 宽限期内一旦恢复到 12，只需立即恢复稳定
                    support.ValueRW.phase = BridgeSegmentPhase.Stable;
                    support.ValueRW.shortageElapsed = 0f;
                }
                else
                {
                    // 从首次不足这一帧开始累计连续不足的秒数
                    support.ValueRW.shortageElapsed += deltaTime;

                    if (support.ValueRO.shortageElapsed >= settings.ValueRO.graceDuration)
                    {
                        // 超时后失效；下帧走 Missing 的成立分支
                        // 不写成立进度：数量低于 12 时，成立系统
                        // 已按低于 18 的规则把进度清零
                        support.ValueRW.phase = BridgeSegmentPhase.Missing;
                        support.ValueRW.shortageElapsed = 0f;
                    }
                    else
                    {
                        // 尚未超时，桥段进入或保持不稳定状态
                        support.ValueRW.phase = BridgeSegmentPhase.Unstable;
                    }
                }
            }
        }
    }
}