using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Project.Scripts.Presentation
{
    /// <summary>
    /// 光虫的显示组件
    /// 读取 ECS 实体位置，同步到当前小球，不参与运动计算
    /// </summary>
    public class FireflyView : MonoBehaviour
    {
        // 目标实体所在的 ECS 世界
        private World m_world;
        
        // 当前小球负责显示的实体
        private Entity m_entity;

        /// <summary>
        /// 由初始化入口明确指定显示哪个实体
        /// </summary>
        public void Initialize(World world, Entity entity)
        {
            m_world = world;
            m_entity = entity;
        }

        /// <summary>
        /// 在 LateUpdate 阶段读取模拟结构并更新显示位置
        /// </summary>
        private void LateUpdate()
        {
            // 尚未初始化，或者退出运行时世界已经释放，就停止同步
            if (m_world == null || !m_world.IsCreated)
                return;

            var manager = m_world.EntityManager;

            // 实体呗销毁后，不再读取它的数据
            if (!manager.Exists(m_entity))
                return;

            // 读取实体位置；这里只读，不修改 ECS 组件
            var position = manager.GetComponentData<LocalTransform>(m_entity).Position;
            
            // 将 ECS 使用的 float3 转为 GameObject 使用的 Vector3
            transform.position = new Vector3(position.x, position.y, position.z);
        }
    }
}