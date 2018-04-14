using UnityEngine;
using System.Collections;
/// 鼠标下载
public class ManagerMouse
{
	/// 静态引用
	public static ManagerMouse valueData = new ManagerMouse();
	
	/// 参数2代表的是默认位置信息 也就是地图高度
	private Plane _plane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
	/// 是否下载了
	private bool _isDown;
	/// 下落时间
	private float _downTimeTeamp;
	/// 鼠标摁下去的默认点
	private Vector3 _downPostion = Vector3.zero;
	
	/// 剔除ui
	public bool isWithOut = true;
	/// 鼠标当前位置
	public void Update ()
	{
		eventMouse();
	}
	/// 判断鼠标是否摁下
	private void eventMouse()
	{
		/// 鼠标是否摁下
		if(Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width)
		{
			if(Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
			{
				/// 如果鼠标摁下
				if(Input.GetMouseButtonDown(0) && Input.touchCount <= 1)
				{
					/// 如果不在ui上
					if(!isMouseOnUI())
					{
						_isDown = true;
						_downPostion = Input.mousePosition;
						_downTimeTeamp = Time.time;
					}
				}

			}
		}
		/// 鼠标抬起
		if(Input.GetMouseButtonUp(0) || Input.touchCount > 1)
		{
			_isDown = false;
		}
	}
	/// 鼠标是否在ui上
	private bool isMouseOnUI()
	{
		/// 如果不需要剔除ui
		if(!isWithOut)
			return false;
		
		
		if(UICamera.inputHasFocus)
			return true;
		if(null != UICamera.hoveredObject)
			return true;
		return false;
	}
	/// 获得场景上的坐标
	public Vector3 getMouseOnSence()
	{
		Vector3 post = Input.mousePosition;
		post.z = SuperUI.senceCamera.nearClipPlane;
		Vector3 offset = screenPointToWorldPointOnPlane(post, _plane, SuperUI.senceCamera); 
		return offset;
	}
	/// 鼠标是否按下的
	public bool isMouseDown()
	{
		return _isDown;
	}
	/// 鼠标位置 刚摁下
	public Vector3 getMousePostionStart()
	{
		return _downPostion;
	}
	/// 鼠标位置 当前
	public Vector3 getMousePostion()
	{
		return Input.mousePosition;
	}
	/// 鼠标按下的时候移动距离
	public Vector3 getMouseDownMove()
	{
		if(_isDown)
		{
			Vector3 result = Input.mousePosition - _downPostion;
			if(float.IsNaN(result.x))
				result.x = 0;
			if(float.IsNaN(result.y))
				result.y = 0;
			if(float.IsNaN(result.z))
				result.z = 0;
			return result;
			
		}	
		/// 鼠标没有摁下时候,不旋转
		return Vector3.zero;		
	}


	
	public void setMouseDownX(float sx)
	{
		_downPostion.x = sx;
	}
	public void setMouseDownY(float sy)
	{
		_downPostion.y = sy;
	}
	
	/// 直接返回时间点
	public float getMouseDownTime()
	{
		/// 如果没摁下
		if(!_isDown) 
			return 0;
		return Mathf.Max(Time.time - _downTimeTeamp, 0f);
	}
	/// 通过plane由屏幕坐标向世界坐标转换
	public static Vector3 screenPointToWorldPointOnPlane(Vector3 screenPoint, Plane plane, Camera camera)
	{  
	    //將滑鼠的螢幕位置轉換成空間中的射線 ray=射線 
	    Ray ray = camera.ScreenPointToRay(screenPoint);  
	    //找出射線與平面相交 
	    return planeRayIntersection(plane, ray); 
	}
	/// 获得交点
	public static Vector3 planeRayIntersection(Plane plane, Ray ray) 
	{
	    float dist = 0;  
	    //光线投射 一条射线和平面相交。 
	    plane.Raycast(ray, out dist);  
	    //获取点 返回沿着射线在distance距离单位的点。 
	    return ray.GetPoint (dist);  
	}  
}
