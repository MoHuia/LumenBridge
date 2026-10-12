using _Project.Scripts.ECS.Components;
using Unity.Entities;
using Unity.Transforms;

namespace _Project.Scripts.ECS.Systems
{
    /// <summary>
    /// 读取光虫位置，统计各桥段区域内的光虫数量
    /// 只写计数，不修改位置，不操作 GameObject 或 UI
    /// </summary>
    // 两个系统默认都在 SimulationSystemGroup 内
    // UpdateAfter 指定每帧先移动，再统计更新后的位置
    [UpdateAfter(typeof(FireflyMoveSystem))]
    public partial class BridgeSegmentCountSystem : SystemBase
    {
        /// <summary> 桥段数据存在时才开始更新 </summary>
        protected override void OnCreate()
        {
            RequireForUpdate<BridgeSegmentBounds>();
            RequireForUpdate<BridgeSegmentCount>();
            // 不要求必须有光虫；零只光虫时仍需把数量更新为 0。
        }

        /// <summary> 每帧重新统计，并将结果写回各桥段 </summary>
        protected override void OnUpdate()
        {
            // 外层每次处理一个同时拥有边界与计数组件的桥段
            // RefRO 只读边界；RefRW 允许写入统计结果
            foreach (var (bounds, result) in
                     SystemAPI.Query<RefRO<BridgeSegmentBounds>,
                         RefRW<BridgeSegmentCount>>())
            {
                // 每个桥段、每一帧都重新从 0 计数
                var count = 0;
                
                // 与现有运动系统使用相同的光虫识别组件组合
                // 速度和到达距离用于筛选光虫，本次不读取它们的值
                // 三项均为只读，统计不能改变运动数据
                foreach (var (localTransform,speed,arrivalDistance) in
                         SystemAPI.Query<RefRO<LocalTransform>,RefRO<FireflySpeed>,RefRO<FireflyArrivalDistance>>())
                {
                    // 当前实体无父级，所以 Position 可按世界位置使用
                    // 以后若加入 Parent，需重新核对位置空间
                    var position = localTransform.ValueRO.Position;
                    
                    // 边界的第二分量保存世界 Z，不是世界高度 Y
                    // 四项必须满足
                    var isInside =
                        position.x >= bounds.ValueRO.min.x &&
                        position.x < bounds.ValueRO.max.x &&
                        position.z >= bounds.ValueRO.min.y &&
                        position.z < bounds.ValueRO.max.y;

                    if (isInside)
                    {
                        count++;
                    }
                }
                
                // 覆盖实际结果：离开区域的虫下一帧就不再计入
                result.ValueRW.value = count;
            }
        }
    }
}