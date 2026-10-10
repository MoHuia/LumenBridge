using System;
using _Project.Scripts.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Scripts.Input
{
    /// <summary>
    /// 连接输入与 ECS 光源数据
    /// 不创建实体，不计算光虫运动
    /// </summary>
    public class LightTargetInputBridge : MonoBehaviour
    {
        // Inspector 指定输入来源
        [SerializeField] private LightPointerInput m_input;
        
        // 由 Bootstrap 提供，不自行查找或创建世界、实体
        private World m_world;
        private Entity m_lightTarget;

        /// <summary> 启用时订阅输入通知 /// </summary>
        private void OnEnable()
        {
            if (m_input == null)
            {
                Debug.LogError("请指定光源输入组件。",this);
                return;
            }

            m_input.OnPositionChanged += HandlePositionChanged;
        }

        /// <summary> 禁用或销毁时取消订阅 /// </summary>
        private void OnDisable()
        {
            if (m_input != null)
            {
                m_input.OnPositionChanged -= HandlePositionChanged;
            }
        }

        /// <summary> 接受初始化入口创建的光源实体 /// </summary>
        public void Initialize(World world, Entity lightTarget)
        {
            m_world = world;
            m_lightTarget = lightTarget;
        }
        
        /// <summary> 将输入世界坐标写入已有的光源组件 </summary>
        private void HandlePositionChanged(Vector3 position)
        {
            // 初始化前或世界释放后，不访问 ECS
            if (m_world == null || !m_world.IsCreated)
                return;

            var manager = m_world.EntityManager;
            
            // 光源被销毁后，不再写入数据
            if (!manager.Exists(m_lightTarget))
                return;
            
            // Vector 转换为 ECS 数据使用的 float3
            // 光源移动在 XZ 平面上，Y 固定为 0
            manager.SetComponentData(m_lightTarget,
                new LightTargetPosition
                {
                    value = new float3(position.x,0f,position.z)
                });
        }
    }
}