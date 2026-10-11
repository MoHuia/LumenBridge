using System;
using _Project.Scripts.ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace _Project.Scripts.Presentation
{
    /// <summary>
    /// 读取共同光源的位置，让范围圆环跟随
    /// 只更新显示，不创建实体，也不修改模拟数据
    /// </summary>
    public class LightRangeView : MonoBehaviour
    {
        // 圆环离地的显示高度，单位为世界单位
        // 保持略高于地面，避免重叠闪烁
        [SerializeField] private float m_displayHeight = 0.01f;
        
        // Bootstrap 明确提供光源所在的世界和实体
        private World m_world;
        private Entity m_lightTarget;
        
        // 对应 Shader Graph 外侧 Smoothstep 的 Edge2。
        // 若修改 Shader 外边界，此处也必须同步修改。
        private const float k_uvOuterRadius = 0.35f;

        /// <summary> 绑定需要显示的共同光源 /// </summary>
        public void Initialize(World world, Entity lightTarget)
        {
            m_world = world;
            m_lightTarget = lightTarget;
        }

        /// <summary> 在每帧较后的阶段，读取数据并同步显示 /// </summary>
        private void LateUpdate()
        {
            // 初始化前、世界是否后，都停止读取
            if (m_world == null || !m_world.IsCreated)
                return;

            var manager = m_world.EntityManager;
            
            // 光源实体被清理后，不再访问其组件
            if(!manager.Exists(m_lightTarget))
                return;

            // 读取光源的世界位置；这里没有写回 ECS
            var position = manager.GetComponentData<LightTargetPosition>(m_lightTarget).value;
            
            // X/Z 跟随光源，Y 使用圆环自己的显示高度
            transform.position = new Vector3(position.x, m_displayHeight, position.z);
            
            // 读取同一个光源的影响半径，单位为世界单位
            // 显示使用模拟数据，不另外保存一份稀有半径
            var radius = manager.GetComponentData<LightInfluenceRadius>(m_lightTarget).value;
            
            // Quad 原始边长为 1，因此缩放值就是需要的面片边长。
            var sideLength = radius / k_uvOuterRadius;

            // Quad 在自身 XY 平面上展开，旋转后才铺在世界 XZ 地面上。
            // 所以这里缩放本地 X/Y；本地 Z 保持 1。
            transform.localScale = new Vector3(sideLength, sideLength, 1f);
        }
    }
}