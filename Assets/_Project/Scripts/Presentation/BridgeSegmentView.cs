using System;
using _Project.Scripts.ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace _Project.Scripts.Presentation
{
    /// <summary>
    /// 桥段的表现入口，后续读取模拟状态并更新桥板显示
    /// 不创建实体，不判断桥是否成立，也不修改模拟数据
    /// </summary>
    public class BridgeSegmentView : MonoBehaviour
    {
        // 保存 Bootstrap 提供的世界和桥段身份
        // 场景中的桥模型与 ECS 实体，通过这两个引用建立对应关系
        private World m_world;
        private Entity m_bridgeSegment;
        
        // 区分首次读取，保证初始 Missing 状态也输出一次
        private bool m_hasReadPhase;
        
        // 保存上一次显示的状态，用来避免每帧重复输出
        private BridgeSegmentPhase m_lastPhase;

        // Inspector 明确指定六块桥板，不包含底梁与柳钉
        [SerializeField] private Renderer[] m_deckRenderers;
        
        // 与 Shader Graph 属性的 Reference 完全一致
        private const string k_supportAmountProperty = "_SupportAmount";
        
        // 与 Shader Graph 的 Reference 一致，用于控制失稳脉动
        private const string k_instabilityProperty = "_Instability";
        
        // 重复使用同一个属性块，避免每次更新都创建对象
        private MaterialPropertyBlock m_propertyBlock;
        
        /// <summary>
        /// 由初始化入口明确提供依赖，不自行搜索世界里的桥段
        /// </summary>
        public void Initialize(World world,Entity bridgeSegment)
        {
            m_world = world;
            m_bridgeSegment = bridgeSegment;
            m_hasReadPhase = false;
            
            // 本步只验证绑定调用，每次初始化输出一次
            Debug.Log("桥段显示绑定完成",this);
        }

        /// <summary> 组件加载时创建材质参数的临时容器 /// </summary>
        private void Awake()
        {
            m_propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// 将模拟状态映射为颜色比例，只更新指定桥板的显示
        /// </summary>
        private void ApplyPhase(BridgeSegmentPhase phase)
        {
            // 未成立：暗色，不脉动
            var amount = 0f;
            var instability = 0f;

            if (phase == BridgeSegmentPhase.Stable)
            {
                amount = 1f;
            }
            else if (phase == BridgeSegmentPhase.Unstable)
            {
                // 失稳：保留金色，开启 Shader 中的明暗脉动
                amount = 1f;
                instability = 1f;
            }
            
            // 没有接入桥板时，停止显示更新
            if(m_deckRenderers == null)
                return;

            foreach (var deckRenderer in m_deckRenderers)
            {
                // 当前槽位为空时，只跳过这块桥板，继续处理后面的桥板
                if(deckRenderer == null)
                    continue;
                
                // 先取得该桥板已有的覆盖参数，保留其他属性
                deckRenderer.GetPropertyBlock(m_propertyBlock);
                
                // 颜色与脉动开关一起交给当前桥板
                m_propertyBlock.SetFloat(k_supportAmountProperty,amount);
                m_propertyBlock.SetFloat(k_instabilityProperty, instability);
                deckRenderer.SetPropertyBlock(m_propertyBlock);
            }
        }

        /// <summary>
        /// 每帧较后的阶段读取模拟结果，只观察，不修改桥段状态
        /// </summary>
        private void LateUpdate()
        {
            // 初始化之前、世界释放之后，都不能读取实体数据
            if (m_world == null || !m_world.IsCreated)
                return;

            var manager = m_world.EntityManager;
            
            // 桥段被清理后，不再访问它的组件
            if (!manager.Exists(m_bridgeSegment))
                return;
            
            // 读取状态系统已经算好的结果，不在这里重复判断虫数
            var phase = manager.GetComponentData<BridgeSegmentSupportState>(m_bridgeSegment).phase;
            
            // 第一次读取，或者状态发生变化，才输出一次
            if (!m_hasReadPhase || phase != m_lastPhase)
            {
                Debug.Log($"桥段显示读取状态：{phase}",this);
                
                // 首次读取或状态变化时，同步六块桥板的颜色。
                ApplyPhase(phase);
                
                // 记录本次状态，供下一帧比较
                m_lastPhase = phase;
                m_hasReadPhase = true;
            }
        }
    }
}