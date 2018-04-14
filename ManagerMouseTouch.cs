using UnityEngine;
using System.Collections;

/// 鼠标控制
public class ManagerMouseTouch : MonoBehaviour
{
	/// 我截获的touch指令
	private Touch _myTouch;
	/// 速度
	private Vector2 _speed = Vector2.zero;
	/// 我的偏移位置
	private Vector2 _postionInit = Vector2.zero;
	
	/// 刷新函数
	void Update()
	{
		eventTouchFind();
		eventTouchMath();
	}
	/// 刷新鼠标
	private void eventTouchFind()
	{
		/// 如果我的touch不是空的返回
		if(_myTouch.phase != TouchPhase.Ended && _myTouch.phase != TouchPhase.Canceled)
			return;
		/// 没有触控
		if(Input.touchCount <= 0)
			return;
		
		/// 如果为1就获得touch
		if(Input.touchCount == 1)
		{
			_myTouch = Input.GetTouch(0);
			_postionInit = _myTouch.position;
		}
		
	}
	/// 计算触摸移动
	private void eventTouchMath()
	{
		/// 如果我的touch不是空的返回
		if(_myTouch.phase == TouchPhase.Ended || _myTouch.phase == TouchPhase.Canceled)
		{
			/// 减少计算量
			if(_speed.x != 0 || _speed.y != 0)
			{
				if(Vector2.Distance(Vector2.zero, _speed) > Time.deltaTime)
					_speed = _speed * 0.982f;
				else
					_speed = Vector2.zero;
			}
			/// 返回
			return;
		}
		/// 获得松手后的速度
		_speed = _myTouch.deltaPosition - _myTouch.position;
		
	}
	/// 是否触摸
	public bool isTouch()
	{
		if(_myTouch.phase != TouchPhase.Ended && _myTouch.phase != TouchPhase.Canceled)
			return true;
		return false;
	}
	/// 获得移动长度
	public Vector2 getDistance()
	{
		if(_myTouch.phase != TouchPhase.Ended && _myTouch.phase != TouchPhase.Canceled)
			return _myTouch.position - _postionInit;
		return Vector2.zero;
	}
	/// 获得移动速度
	public Vector2 getSpeed()
	{
		/// 移动速度
		if(_myTouch.phase != TouchPhase.Ended && _myTouch.phase != TouchPhase.Canceled)
			return _myTouch.deltaPosition - _myTouch.position;
		return _speed;
	}
}
