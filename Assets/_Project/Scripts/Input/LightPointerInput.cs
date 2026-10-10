using System;
using UnityEngine;

namespace _Project.Scripts.Input
{
    /// <summary>
    /// 采集玩家点击，转换为地面上的世界坐标
    /// 当前只验证输入，不操作 ECS 或光虫
    /// </summary>
    public class LightPointerInput : MonoBehaviour
    {
        // 明确指定游戏摄像机，避免每帧查找
        [SerializeField] private Camera m_camera;

        // 通知订阅者：玩家选择了一个新的地面世界坐标
        // 输入只提供坐标，不负责修改 ECS
        public event Action<Vector3> OnPositionChanged; 
        
        // 法线朝上、经过原点的数学平面，即 y=0
        // 它不是场景物体，不需要地面 Collider
        private Plane m_groundPlane = new Plane(Vector3.up, Vector3.zero);

        /// <summary> 初始化时检查摄像机引用 /// </summary>
        private void Awake()
        {
            if (m_camera == null)
            {
                Debug.LogError("请指定游戏摄像机。",this);
                enabled = false;
            }
        }

        /// <summary> 每次按下鼠标左键时，输出地面交点 /// </summary>
        private void Update()
        {
            // 使用完整名称，避免与当前 Input 命名空间混淆
            if (!UnityEngine.Input.GetMouseButton(0))
                return;
            
            // 从屏幕点击位置生成一条世界空间射线
            var ray = m_camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            
            // distance 是沿射线到交点的距离，不是交点坐标
            if (!m_groundPlane.Raycast(ray,out var distance))
                return;

            // 根据射线距离取得地面上的世界坐标
            var position = ray.GetPoint(distance);
            
            // 有订阅者时，将计算好的世界坐标交给它
            // ？.表示没有订阅者时不调用，避免空引用
            OnPositionChanged?.Invoke(position);
        }
    }
}