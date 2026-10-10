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
        private Entity m_firefly;
        
        // 保存共同光源的实体标识，供初始化和清理使用
        private Entity m_lightTarget;
        
        // 在 Inspector 中指定负责显示这只实体的小球组件
        [SerializeField] private FireflyView m_view;
        
        // 将创建好的光源实体交给输入连接组件
        [SerializeField] private LightTargetInputBridge m_lightInputBridge;

        /// <summary>
        /// 进入运行后创建一只实体，并设置位置和速度
        /// </summary>
        private void Start()
        {
            // 未指定显示组件时提示接线问题，避免创建后无法显示
            if (m_view == null)
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
            
            // 创建光虫实体，添加位置、速度和到达距离组件。
            m_firefly = manager.CreateEntity(typeof(LocalTransform), typeof(FireflySpeed), typeof(FireflyArrivalDistance));
            
            // 将实体的初始位置设为原地
            // FromPosition 同时将旋转设为无旋转、缩放设为 1
            manager.SetComponentData(m_firefly,LocalTransform.FromPosition(0f,0f,0f));
            
            // 设置速度为每秒 4 个 Unity 单位
            // 这里只保存速度，尚未执行移动
            manager.SetComponentData(m_firefly,new FireflySpeed { value = 4f });

            // 设置这只光虫到达共同光源时的停止距离。
            // 距离光源不超过 0.2 个单位时，运动系统停止移动。
            manager.SetComponentData(m_firefly, new FireflyArrivalDistance
            {
                stoppingDistance = 0.2f
            });
            
            // 从实体上读回速度，验证数据是否设置成功
            var speed = manager.GetComponentData<FireflySpeed>(m_firefly);
            Debug.Log($"光虫实体已创建，速度：{speed.value}");
            
            // 实体创建并设置好数据后，把它交给显示组件
            m_view.Initialize(m_world,m_firefly);
            
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
                
                // 仅在实体仍然存在时销毁，避免重复清理
                if (manager.Exists(m_firefly))
                {
                    manager.DestroyEntity(m_firefly);
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
