using _Project.Scripts.ECS.Components;
using _Project.Scripts.Input;
using _Project.Scripts.Presentation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Project.Scripts.Bootstrap
{
    /// <summary>
    /// 一只光虫的初始化入口
    /// 只负责创建实体、设置初始数据和清理实体
    /// 运动由后续的 ECS 系统处理，显示同步由表现脚本处理
    /// </summary>
    public class OneFireflyBootstrap : MonoBehaviour
    {
        // 保存创建实体时使用的 ECS 世界，清理时操作同一个世界
        private World m_world;

        // 光虫的实体标识，用于读写它的组件数据
        // 它不是画面中小球的 GameObject
        // 本次先验证 10 只；至少创建一只，供现有显示组件使用
        [SerializeField, Min(1)] private int m_fireflyCount = 10;

        // 普通 C# 数组，只保存本入口创建的实体标识
        // 位置、速度等组件数据仍保存在 ECS 世界里
        // 保留这些标识，是为了销毁时只清理自己创建的实体
        private Entity[] m_fireflies;
        
        // 保存共同光源的实体标识，供初始化和清理使用
        private Entity m_lightTarget;
        
        /// Inspector 指定 Project 中的光虫预制体。
        // 它是创建显示对象的模板，每只实体使用一个独立实例。
        [SerializeField] private FireflyView m_viewPrefab;
        
        // 将创建好的光源实体交给输入连接组件
        [SerializeField] private LightTargetInputBridge m_lightInputBridge;

        /// <summary>
        /// 进入运行后创建一只实体，并设置位置和速度
        /// </summary>
        private void Start()
        {
            // 未指定显示组件时提示接线问题，避免创建后无法显示
            if (m_viewPrefab == null)
            {
                Debug.LogError("请在 Inspector 中指定光虫显示组件",this);
                return;
            }
            
            // 取得 Unity 默认创建的 ECS 世界
            m_world = World.DefaultGameObjectInjectionWorld;
            
            // EntityManager 是创建实体、读写组件和销毁实体的入口
            var manager = m_world.EntityManager;
            
            // 单独创建光源数据实体
            // 此组件只添加到这一个实体上，不添加到每只光虫身上
            m_lightTarget = manager.CreateEntity(typeof(LightTargetPosition));
            
            // 创建标识数组：这一步还没有创建任何 ECS 实体
            // Math.Max 同时保证运行时数量至少为 1
            m_fireflies = new Entity[Mathf.Max(1, m_fireflyCount)];

            // 每行放 10 只光虫；列沿 X，行沿 Z
            var columns = 10;
            
            // 整数除法会舍去小数，因此先加 columns - 1，实现向上取证
            // 例如 100 只需要 10 行，11 只需要 2 行
            var rows = (m_fireflies.Length + columns - 1) / columns;

            for (var i = 0; i < m_fireflies.Length; i++)
            {
                // 每次循环创建一个独立实体，拥有自己的位置和速度
                // 不添加 LightTargetPosition: 让所有光虫读取同一个共同光源
                var firefly = manager.CreateEntity(
                    typeof(LocalTransform),
                    typeof(FireflySpeed),
                    typeof(FireflyArrivalDistance));
                
                // 立即记录归本入口所有的实体，供后续绑定和清理使用
                m_fireflies[i] = firefly;
                
                // 取余得到列号：0～9，超过一行后从 0 重新开始。
                // 整数除法得到行号：每创建 10 只，行号增加 1。
                var column = i % columns;
                var row = i / columns;

                // 减去半宽/半高，使网格以世界原点为中心。
                // X 间距为 1，Z 间距为 0.5，单位均为世界单位。
                var spawnX = (column - (columns - 1) * 0.5f) * 1f;
                var spawnZ = (row - (rows - 1) * 0.5f) * 0.5f;

                // ECS 实体从网格位置出生，Y 保持在地面平面。
                manager.SetComponentData(
                    firefly,
                    LocalTransform.FromPosition(spawnX, 0f, spawnZ));
                
                // 设置每只光虫的移动速度，单位为世界单位/秒。
                // 添加组件后默认值为 0，必须赋值才能产生位移。
                manager.SetComponentData(
                    firefly, new FireflySpeed { value = 4f });

                // 设置接近共同光源时的停止距离，单位为世界单位。
                manager.SetComponentData(
                    firefly, new FireflyArrivalDistance { stoppingDistance = 0.2f });
                
                // 复制整只光虫模型，并取得新对象上的 FireflyView。
                // 使用与实体一致的世界出生位置，避免显示先出现在原点。
                // transform 是当前初始化对象，将显示对象放在它的层级下面。
                var view = Instantiate(
                    m_viewPrefab,
                    new Vector3(spawnX, 0f, spawnZ),
                    Quaternion.identity,
                    transform);

                // 名字只用于在 Hierarchy 中检查数量和区分显示对象。
                view.gameObject.name = $"FireflyView_{i}";

                // 当前循环创建的显示对象，绑定当前循环创建的实体。
                // 每只显示对象因此读取不同的实体位置。
                view.Initialize(m_world, firefly);
            }

            Debug.Log($"已创建 {m_fireflies.Length} 只光虫，共用一个光源。");
            
            // 原实验场景可以不接输入；接入后绑定同一个世界与光源实体
            if (m_lightInputBridge != null)
            {
                m_lightInputBridge.Initialize(m_world, m_lightTarget);
            }
        }

        /// <summary>
        /// 初始化对象销毁时，清理它创建的实体
        /// </summary>
        private void OnDestroy()
        {
            // 退出运行时，世界可能已经被释放，需要先检查
            if (m_world != null && m_world.IsCreated)
            {
                var manager = m_world.EntityManager;
                
                // Start 未完成时，数组可能尚未创建。
                if (m_fireflies != null)
                {
                    // 逐个清理本入口创建的光虫，不影响世界里的其他实体。
                    foreach (var firefly in m_fireflies)
                    {
                        // 已经销毁的实体不重复清理。
                        if (manager.Exists(firefly))
                        {
                            manager.DestroyEntity(firefly);
                        }
                    }
                }
                
                // 初始化入口负责清理自己创建的光源实体
                if (manager.Exists(m_lightTarget))
                {
                    manager.DestroyEntity(m_lightTarget);
                }
            }
        }
    }
}
