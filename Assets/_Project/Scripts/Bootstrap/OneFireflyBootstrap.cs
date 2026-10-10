using _Project.Scripts.ECS.Components;
using _Project.Scripts.Presentation;
using Unity.Entities;
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
        
        // 在 Inspector 中指定负责显示这只实体的小球组件
        [SerializeField] private FireflyView m_view;

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
            
            // 创建实体，并添加位置组件和自定义速度组件
            m_firefly = manager.CreateEntity(typeof(LocalTransform), typeof(FireflySpeed));
            
            // 将实体的初始位置设为原地
            // FromPosition 同时将旋转设为无旋转、缩放设为 1
            manager.SetComponentData(m_firefly,LocalTransform.FromPosition(0f,0f,0f));
            
            // 设置速度为每秒 4 个 Unity 单位
            // 这里只保存速度，尚未执行移动
            manager.SetComponentData(m_firefly,new FireflySpeed { value = 4f });

            // 从实体上读回速度，验证数据是否设置成功
            var speed = manager.GetComponentData<FireflySpeed>(m_firefly);
            Debug.Log($"光虫实体已创建，速度：{speed.value}");
            
            // 实体创建并设置好数据后，把它交给显示组件
            m_view.Initialize(m_world,m_firefly);
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
            }
        }
    }
}