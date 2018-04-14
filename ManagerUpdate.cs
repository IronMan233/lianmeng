using UnityEngine;
using System.Collections;

public class ManagerUpdate
{
	/// 静态刷新的函数
	public static void Update()
	{
		ManagerMouse.valueData.Update();
		
		eventServerTime();
		eventTest();
		
		UtilListener.Update();
		ControlTouch.update();
		/// 移动相机去两个怪物的中间
		eventCamera();
		/// 生成体力值
		eventPower();
		/// 减速
		eventCameraSlow();
		///生成技能点
		eventSkillPoint();
		/// 刷新时间
		eventPlayerUpdate();
		///面包刷新
		updateBread();
		/// vip体验卡
		eventVIPTimeOver();

	}
	/// 刷新时间
	public static void FixedUpdate()
	{
		/// 怪物cd点数
		eventCDHide();
	}
	
	/// 缓动动画设置
	private static void eventCameraSlow()
	{
		/// 如果 时间戳 < 减速时间戳
		if((Data.gameTime - DataMode.infoSetting.relSkillTimeTeamp) < 0f)
			return;
		if((Data.gameTime - DataMode.infoSetting.relSkillTimeTeamp) > Time.deltaTime)
			return;
		UtilListener.dispatch("EventAnimationSpeed");
			
	}
	/// 战斗中 技能cd隐藏设定
	private static void eventCDHide()
	{
		/// 没有角色 不搞
		if(DataMode.myPlayer == null)
			return;
		/// 不在副本中 不搞
		if(!DataMode.myPlayer.isInFB)
			return;
		/// 设置 主角 技能 cd
		foreach(AIFollower follower in ManagerCombat.getHideFollower())
		{
			if(null == follower)
				continue;
			AICombatData combatData = follower.GetComponent<AICombatData>();
			if(null == combatData)
				continue;
			
			combatData.myAttack.timeStampUnlock += Time.deltaTime * DataMode.infoSetting.speedReal;
			if(null != combatData.mySkillRelease)
			{
				foreach(TypeCombatSkill skill in combatData.mySkillRelease)
				{
					skill.timeStampUnlock += Time.deltaTime * DataMode.infoSetting.speedReal;
				}
			}
			if(null != combatData.mySkillAuto)
			{
				foreach(TypeCombatSkill skill in combatData.mySkillAuto)
				{
					skill.timeStampUnlock += Time.deltaTime * DataMode.infoSetting.speedReal;
				}
			}
		}
	}
	
	
	
	
	/// 相机移动
	private static void eventCamera()
	{
		if(!AICombat.isStartCombat)
			return;
		/// 失败的时候也不移动相机
		if(AICombat.isCombatOver == 2)
			return;
		/// 世界boss也不需要管相机
		if(AICombat.combatType == 3)
			return;
		/// 竞技的时候也不移动
		if(AICombat.combatType == 2)
			return;
		/// 好友切磋的时候不移动
		if(AICombat.combatType == 5 || AICombat.combatType == 6)
			return;
		/// 爬塔的时候不移动
		if(AICombat.combatType == 4)
			return;
		/// 燃烧远征不允许
		if(AICombat.combatType == 9)
			return;
		
		float midX = AICombat.getMidX();
//		if(midX <= CameraMoveData.valueData.targetPostion.x)
//			return;
		CameraMoveData.valueData.targetPostion.x = midX;
	}
	/// 时间增加
	private static void eventServerTime()
	{
		if(Data.serverTime >= 0f)
			Data.serverTime += Time.deltaTime;
		
		float speedReal = DataMode.infoSetting.speedReal;
		Data.gameTime += Time.deltaTime * DataMode.infoSetting.speedRealUnObject;
		if(speedReal != DataMode.infoSetting.speedReal)
			UtilListener.dispatch("EventAnimationSpeed");
	}
	/// 绘制体力值
	private static void eventPower()
	{
		/// 锁定几秒
		if(_lockPowerTime >= 0f)
		{
			_lockPowerTime -= Time.deltaTime;
			return;
		}
		/// 不需要loading界面
		if(LoginWindow.view != null)
		{
			return;
		}
		/// 掉线判断
		if(!Document.valueObj.sendping)
		{
			return;
		}
//		/// 如果锁定了 不	请求
//		if(_isLockPowerMath)
//			return;
		/// 如果没有主角 不请求
		if(null == DataMode.myPlayer)
			return;
		/// 如果体力超上限 不请求
		if(DataMode.myPlayer.power >= DataMode.myPlayer.powerMax)
			return;
		/// 如果是cd 不请求
		if(DataMode.myPlayer.powerCD.timeTeamp >= Data.serverTime)
			return;
		/// 请求并锁定
		_lockPowerTime = 3f;
		_isLockPowerMath = true;
		DataModeServer.sendPowerAdd(handEventPower);
	}
	/// 是否锁定的体力长
	private static bool _isLockPowerMath = false;
	private static float _lockPowerTime = 0f;
	private static void handEventPower(UtilListenerEvent sEvent){_isLockPowerMath = false;_lockPowerTime = 3f;}


	//面包刷新
	private static void updateBread()
	{
		/// 锁定几秒
		if(_lockBreadTime >= 0f)
		{
			_lockBreadTime -= Time.deltaTime;
			return;
		}
		/// 不需要loading界面
		if(LoginWindow.view != null)
		{
			return;
		}
		/// 掉线判断
		if(!Document.valueObj.sendping)
		{
			return;
		}

		/// 如果没有主角 不请求
		if(null == DataMode.myPlayer)
			return;
		/// 如果面包数量大于上限
		TypeCsvAttribute _att = ManagerCsv.getAttribute();
		if(DataMode.infoEscort.bread >= _att.escortMaxBread)
			return;
		/// 如果是cd 不请求
		if(Data.serverTime - DataMode.infoEscort.breadTimeStamp <= _att.escortCDBread)
			return;
		/// 请求并锁定
		_lockBreadTime = 3f;
		DataModeServer3.sendEscortBreadTime(handEventBread);
	}
	///锁定几秒 面包
	private static float _lockBreadTime = 0;
	private static void handEventBread(UtilListenerEvent sEvent){_lockBreadTime = 3f;}

	private static void eventSkillPoint()
	{
		/// 锁定几秒
		if(lockSkillTime >= 0f)
		{
			lockSkillTime -= Time.deltaTime;
			return;
		}
		/// 不需要loading界面
		if(LoginWindow.view != null)
		{
			return;
		}
		/// 掉线判断
		if(!Document.valueObj.sendping)
		{
			return;
		}
//		if(isLockSkillMath)
//			return;
		if(null == DataMode.myPlayer)
			return;
		/// 如果技能点超上限 
		if(DataMode.myPlayer.skillPoint >= DataMode.myPlayer.maxSkillPoint)
			return;
		/// cd中
		if(DataMode.myPlayer.skillCD.timeTeamp >= Data.serverTime)
			return;
		///请求开锁
		isLockSkillMath = true;
		DataModeServer.sendSkillPointAdd(handEventSkill);		
	}
	/// 是否锁定技能点增长
	private static bool isLockSkillMath = false;
	private static float lockSkillTime = 0f;
	private static void handEventSkill(UtilListenerEvent sEvent){isLockSkillMath = false;lockSkillTime = 3f;}
		
	
	
	public static bool lockPlayUpdate = false;
	/// 半夜12点系统刷新
	/// WILL DONE
	private static void eventPlayerUpdate()
	{
		if(lockPlayUpdate)
			return;
		/// 不需要loading界面
		if(LoginWindow.view != null)
			return;
		/// 掉线判断
		if(!Document.valueObj.sendping)
			return;
		/// 如果没有主角 不请求
		if(null == DataMode.myPlayer)
			return;
		/// 如果没有在场景中
		if(!(DataMode.myPlayer.isInTown || DataMode.myPlayer.isInFB))
			return;

		/// 如果是同一天干的事。不发送
//		if(Mathf.FloorToInt((float)((Data.serverTime + 3600 * 8) / (3600 * 24))) <= Mathf.FloorToInt((float)((DataMode.myPlayer.timeTeampUpdate + 3600 * 8) / (3600 * 24))))
//			return;

		/// 如果是同一天干的事。不发送 早上5点 也就是 东8区 + 5
//		if(Mathf.FloorToInt((float)((Data.serverTime + 3600 * 13) / (3600 * 24))) <= Mathf.FloorToInt((float)((DataMode.myPlayer.timeTeampUpdate + 3600 * 13) / (3600 * 24))))
//			return;
//		if(Mathf.FloorToInt((float)((Data.serverTime + 3600 * 8) / (3600 * 24))) <= Mathf.FloorToInt((float)((DataMode.myPlayer.timeTeampUpdate + 3600 * 13) / (3600 * 24))))
//			return;
		if(Data.serverTime <= DataMode.myPlayer.timeTeampUpdate + 3600f * 24f)
			return;

		lockPlayUpdate = true;
		DataModeServer2.sendPlayerUpdate(null);
	}

	/// VIP体验卡过期
	private static bool _isHasVIP = false;
	private static void eventVIPTimeOver()
	{
		/// 不需要loading界面
		if(LoginWindow.view != null)
			return;
		/// 掉线判断
		if(!Document.valueObj.sendping)
			return;
		/// 如果没有主角 不请求
		if(null == DataMode.myPlayer)
			return;
		/// 如果没有在场景中
		if(!DataMode.myPlayer.isInTown)
			return;
		bool isHasVIPNow = DataMode.myPlayer.vipLvTempTimeSteamp > Data.serverTime;
		if(_isHasVIP && !isHasVIPNow)
		{
			GusUIAlert.showAlert(
				ConfigLabel.POPUP_TITLE_TISHI,
				ConfigComment.parseText(ConfigComment.VIP_TEMP_TIME_OVER),
				GusUIAlert.createButton(ConfigLabel.POPUP_BUT_NAME_OK, null, null)
				);
		}
		_isHasVIP = isHasVIPNow;
	}
	
	/// 从鼠标位置创建56个特效
	public static bool _touch;
	private static void eventTest()
	{
		
//		if(Input.GetKeyDown(KeyCode.A))
//			ManagerServer.getInstance().contactAccount();
		
		
		if(!Config.TESTING)
			return;

//		if(Input.GetKeyDown(KeyCode.Space))
//		{
//			GameObject view = new GameObject();
//			AICombatSkillAreaRect line360 = view.AddComponent<AICombatSkillAreaRect>();
//			line360.setDrawCircle(ManagerMouse.valueData.getMouseOnSence(), Quaternion.identity, 1f, 360f);
//			line360.material = (Material)LoadMngr.getInstance().GetObject(ConfigUrl.getInfoNameUrl("redLine"));
//		}


		if(Input.GetKeyDown(KeyCode.U))
			UtilLog.isLog = !UtilLog.isLog;
		if(Input.GetKeyDown(KeyCode.I))
			UtilLog.isLogStackTrace = !UtilLog.isLogStackTrace;
		if(Input.GetKeyDown(KeyCode.O))
			UtilLog.isDrawBack = !UtilLog.isDrawBack;
		if(Input.GetKeyDown(KeyCode.P))
			UtilLog.clear();

		/// 测试结果
		if(Input.GetKeyDown(KeyCode.T))
			Config.TESTING_COMBAT = Config.TESTING_COMBAT >= 3 ? 0 : Config.TESTING_COMBAT + 1;

		
		if(Input.GetKeyDown(KeyCode.S))
		{
			switch((int)DataMode.infoSetting.speed)
			{
			case 1:
				DataMode.infoSetting.speed = 3f;
				break;
			case 3:
				DataMode.infoSetting.speed = 1f;
				break;
			default:
				DataMode.infoSetting.speed = 1f;
				break;
			}
			UtilListener.dispatch("EventAnimationSpeed");
		}
		
		



		/// 3个触摸点的时候显示和隐藏
        //if (Input.touchCount >= 1)
        //{
        //    UtilLog.isLog = !UtilLog.isLog;
        //}
        if (Input.touchCount >= 3 && !_touch)
        {
            UtilLog.isLog = !UtilLog.isLog;
        }
        _touch = Input.touchCount >= 3;
		
//		DataModeServer.sendTowerCombatStart();
//		DataModeServer.sendPKCombatStart();
//		if(Input.GetMouseButtonDown(1))
//		{
//			for(int i = 0; i < 5; i++)
//			{
//				ManagerCreate.createEffectDropProp(Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 10f)), new Vector3(Screen.width - 170f, Screen.height - 38f, Camera.main.nearClipPlane + 10f));
//			}
//		}
	}
}

