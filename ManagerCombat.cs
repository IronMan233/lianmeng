using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerCombat
{
	/// 开始战斗
	public static void startFBCombat(int idCsvCombatEnemy, Vector3 postionEnemy, int sSaveID)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startFBCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 1;
		/// 战斗模式开启
		changeFollower();
		/// 创建敌人
		hashCombatShow(sSaveID);
		/// 开始战斗		
		AICombat.startCombat();
		
		/// 设置相机,哇哈哈哈
		CameraMoveData.valueData.changeMove(typeof(CameraMoveLineTween));
		
		/// 设置位置
		Vector3 postionTarget = (AIPlayerFB.valueObject.transform.position - postionEnemy) * 0.5f + postionEnemy;
		CameraMoveData.valueData.setTargetPostion(postionTarget, null);
		
		/// 看看显示剧情么
		if(ManagerSenceFB.hasStory)
		{
			if(ManagerSenceFB.csvFB.storyCombatBefor != null && ManagerSenceFB.csvFB.storyCombatBefor.Length > sSaveID)
				if("#" != ManagerSenceFB.csvFB.storyCombatBefor[sSaveID])
					SuperUI.showNew<UIStory>("GusUIStory").setStory(ManagerCsv.getStory(int.Parse(ManagerSenceFB.csvFB.storyCombatBefor[sSaveID])));
		}
	}
	/// 开始活动战斗
	public static void startActiveCombat(int idCsvCombatEnemy, Vector3 postionEnemy, int sSaveID)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startActiveCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 8;
		/// 战斗模式开启
		changeFollower();
		/// 创建敌人
//		createCombatEnemy(idCsvCombatEnemy, postionEnemy);
		hashCombatShow(sSaveID);
		
		
		
		AICombat.startCombat();
		/// 设置相机,哇哈哈哈
		CameraMoveData.valueData.changeMove(typeof(CameraMoveLineTween));
//		CameraMoveData.valueData.changeMove(typeof(CameraMoveWalkCombat));
		
		Vector3 postionTarget = (AIPlayerFB.valueObject.transform.position - postionEnemy) * 0.5f + postionEnemy;
		CameraMoveData.valueData.setTargetPostion(postionTarget, null);
	}
	
	/// 开始pk的战斗
	public static void startPKCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startPKCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 2;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	/// 开始世界boss的pk
	public static void startWorldBossCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startWorldBossCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 3;
		/// 战斗模式开启
		changeFollower();
		
		AICombat.startCombat();
		/// 设置相机,哇哈哈哈
	}
	/// 开始pk的战斗
	public static void startTowerCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startTowerCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 4;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	/// 开始羞辱好友
	public static void startPKFriendCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startPKFriendCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 5;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	/// 被好友羞辱后反击复仇
	public static void startPKFriendRevengeCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startPKFriendRevengeCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 6;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	
	/// 开始pk的战斗
	public static void startAutoCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startAutoCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 7;
		AICombat.startCombat();
		/// add gus 2015.11.11 打点
		SDKBreakPoint.sendBreakPoint(SDKBreakPoint.PointType.GAME_AUTO_COMBAT, "");
	}
	/// 燃烧的远征
	public static void startTBCCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startTBCCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 9;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	/// 太阳井的战斗
	public static void startSunWellCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startSunWellCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 10;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	/// 护送掠夺
	public static void startEscortRobCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startEscortRobCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 11;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}
	/// 燃烧的远征 奖励关
	public static void startTBCRewardCombat()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("战斗委托 >> startTBCRewardCombat()");
		/// 让战斗开始吧
		AICombat.combatType = 12;
		UtilListener.calledTime(showCombatStartAnim, _startAnimTime);
		AICombat.startCombat();
	}

	/// 战斗开始动画
	private static float _startAnimTime = 0.3f;
	/// 显示开始动画
	private static void showCombatStartAnim()
	{
		SuperUI.showNew<UICombatStartAnim>("GusNew_UICombatStartAnim");
	}
	
	/// 功能函数===============================================================================
	
	
	/// 将所有follower改成combat结构
	public static void changeFollower()
	{
		UnityEngine.Object[] followers = GameObject.FindObjectsOfType(typeof(AIFollower));
		List<AICombatData> listCombatData = new List<AICombatData>();
		/// 转换控制器
		foreach(UnityEngine.Object obj in followers)
		{
			AIFollower follower = (AIFollower)obj;
			follower.destroyControl();
			follower.gameObject.AddComponent<AICombat>();
			
			AICombatData combatData = follower.GetComponent<AICombatData>();
			if(combatData.standIndex < 9)
				listCombatData.Add(combatData);
		}
		SuperUI.show<UICombatOnlyTime>("GusUICombatOnlyTime", null);
		/// 世界boss时间显示在左边
		if(AICombat.combatType == 3)
			SuperUI.getUI<UICombatOnlyTime>().pivot = UIWidget.Pivot.TopLeft;
	}
	/// 隐藏显示跟随的对象
	private static List<AIFollower> _hideFollower = new List<AIFollower>();
	public static void hideFollower()
	{
		UnityEngine.Object[] followers = GameObject.FindObjectsOfType(typeof(AIFollower));
		/// 转换控制器
		foreach(UnityEngine.Object obj in followers)
		{
			AIFollower follower = (AIFollower)obj;
			if(follower.gameObject.activeSelf)
			{
				follower.gameObject.SetActive(false);
				_hideFollower.Add(follower);
			}
		}
	}
	public static void showFollower()
	{
		foreach(AIFollower follower in _hideFollower)
		{
			follower.gameObject.SetActive(true);
		}
		_hideFollower.Clear();
	}
	/// 获得隐藏的角色
	public static List<AIFollower> getHideFollower()
	{
		return _hideFollower;
	}
	/// 创建战斗形象
	private static void createCombatEnemy(int idCsvCombatEnemy, Vector3 postionEnemy)
	{
		createCombatEnemy(idCsvCombatEnemy, postionEnemy, -1, -1);
	}
	private static void createCombatEnemy(int idCsvCombatEnemy, Vector3 postionEnemy, int sLv, float sHp)
	{
		/// 获得敌人数据
		TypeCsvCombatEnemy csvEnemy = ManagerCsv.getCombatEnemy(idCsvCombatEnemy);
		/// 获得敌人站位置信息
		TypeCsvCombatStand csvStand = ManagerCsv.getCombatStand(csvEnemy.idCombatStand);

		/// 获得等级
		int lv = (sLv == -1 ? csvEnemy.lv : sLv);
		
		AICombatData boss = null;
		
		/// 旋转方向
		Vector3 rotation = new Vector3(-1f,0f,0f);
		float time = 0f;
		
		/// 创建战斗形象
		if(-1 != csvEnemy.index0)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(09, csvEnemy.index0, lv, GMath.stringToVector3(csvStand.index0) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index1)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(10, csvEnemy.index1, lv, GMath.stringToVector3(csvStand.index1) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index2)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(11, csvEnemy.index2, lv, GMath.stringToVector3(csvStand.index2) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index3)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(12, csvEnemy.index3, lv, GMath.stringToVector3(csvStand.index3) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index4)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(13, csvEnemy.index4, lv, GMath.stringToVector3(csvStand.index4) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index5)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(14, csvEnemy.index5, lv, GMath.stringToVector3(csvStand.index5) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index6)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(15, csvEnemy.index6, lv, GMath.stringToVector3(csvStand.index6) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index7)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(16, csvEnemy.index7, lv, GMath.stringToVector3(csvStand.index7) + postionEnemy, rotation, sHp));
		if(-1 != csvEnemy.index8)
			UtilListener.calledTime(createCombatEnemyHD, time++ * 0.04f, null, 
				new ManagerCombat.TypeCreateCombatEnemy(17, csvEnemy.index8, lv, GMath.stringToVector3(csvStand.index8) + postionEnemy, rotation, sHp));
		
	}
	/// 毫秒级出现的人物
	private static void createCombatEnemyHD(UtilListenerEvent sEvent)
	{
		if(null != DataMode.myPlayer && DataMode.myPlayer.isInTown)
			return;
		
		TypeCreateCombatEnemy args = (TypeCreateCombatEnemy)sEvent.eventArgs;
		
		try
		{
			AICombatData boss = ManagerCreate.createCombatEnemy(args.standIndex, args.idCsv, args.lv, args.postion, args.rotation);
			if(args.hp >= 1)
				boss.hp = args.hp;
		} catch (System.Exception sE) {UtilLog.LogError(sE);}
	}
	/// 创建敌人类型信息
	public class TypeCreateCombatEnemy
	{
		public TypeCreateCombatEnemy
			(
				int sstandIndex,
				int sidCsv,
				int slv,
				Vector3 spostion,
				Vector3 srotation,
				float shp
			)
		{
			standIndex = sstandIndex;
			idCsv = sidCsv;
			lv = slv;
			postion = spostion;
			rotation = srotation;
			hp = shp;
		}
		public int standIndex;
		public int idCsv;
		public int lv;
		public Vector3 postion;
		public Vector3 rotation;
		public float hp;
	}
	
	/// 缓存
	private static Dictionary<int, List<AICombatData>> _hashCombat = new Dictionary<int, List<AICombatData>>();
	/// 创建敌人,然后隐藏掉 返回值为当中的BOSS对象
	public static AICombatData createCombatEnemyHide(int idCsvCombatEnemy, Vector3 postionEnemy, int sSaveID)
	{
		/// 获得敌人数据
		TypeCsvCombatEnemy csvEnemy = ManagerCsv.getCombatEnemy(idCsvCombatEnemy);
		/// 获得敌人站位置信息
		TypeCsvCombatStand csvStand = ManagerCsv.getCombatStand(csvEnemy.idCombatStand);

		/// 获得等级
		int lv = csvEnemy.lv;		
		/// 旋转方向
		Vector3 rotation = new Vector3(-1f,0f,0f);
		/// 临时变量
		AICombatData combat = null;
		
		AICombatData result = null;
		
		try
		{
			/// 创建战斗形象
			if(-1 != csvEnemy.index0)
			{
				combat = ManagerCreate.createCombatEnemy(09, csvEnemy.index0, lv, GMath.stringToVector3(csvStand.index0) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index1)
			{
				combat = ManagerCreate.createCombatEnemy(10, csvEnemy.index1, lv, GMath.stringToVector3(csvStand.index1) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index2)
			{
				combat = ManagerCreate.createCombatEnemy(11, csvEnemy.index2, lv, GMath.stringToVector3(csvStand.index2) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index3)
			{
				combat = ManagerCreate.createCombatEnemy(12, csvEnemy.index3, lv, GMath.stringToVector3(csvStand.index3) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index4)
			{
				combat = ManagerCreate.createCombatEnemy(13, csvEnemy.index4, lv, GMath.stringToVector3(csvStand.index4) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index5)
			{
				combat = ManagerCreate.createCombatEnemy(14, csvEnemy.index5, lv, GMath.stringToVector3(csvStand.index5) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index6)
			{
				combat = ManagerCreate.createCombatEnemy(15, csvEnemy.index6, lv, GMath.stringToVector3(csvStand.index6) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index7)
			{
				combat = ManagerCreate.createCombatEnemy(16, csvEnemy.index7, lv, GMath.stringToVector3(csvStand.index7) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
			if(-1 != csvEnemy.index8)
			{
				combat = ManagerCreate.createCombatEnemy(17, csvEnemy.index8, lv, GMath.stringToVector3(csvStand.index8) + postionEnemy, rotation);
				if(combat.myCsvHero.type == 2) result = combat;
				hashCombatHideSave(combat, sSaveID);
			}
		} catch (System.Exception sE) {UtilLog.LogError(sE);}
		/// 返回boss
		return result;
	}
	/// 进行数据保存
	private static void hashCombatHideSave(AICombatData sCombat, int sSaveID)
	{
		if(!_hashCombat.ContainsKey(sSaveID))
		{
			_hashCombat.Add(sSaveID, new List<AICombatData>());
		}
		_hashCombat[sSaveID].Add(sCombat);
		sCombat.gameObject.SetActive(false);
	}
	/// 显示数据
	private static void hashCombatShow(int sSaveID)
	{
		if(!_hashCombat.ContainsKey(sSaveID))
		{
			UtilLog.LogError("数据尚未保存 >>> sSaveID = " + sSaveID);
			return;
		}
		foreach(AICombatData combat in _hashCombat[sSaveID])
		{
			/// 给点烟好看
			if(!combat.gameObject.activeSelf)
				ManagerCreate.createEffectByName("effect_common_monster_birth", combat.transform.localPosition, combat.transform.localRotation);
			ManagerCombatBeast.addBuffBeast(combat);
			combat.gameObject.SetActive(true);
			
		}
	}
	private static void hashCombatShowBoss(int sSaveID)
	{
		if(!_hashCombat.ContainsKey(sSaveID))
		{
			UtilLog.LogError("数据尚未保存 >>> sSaveID = " + sSaveID);
			return;
		}
		foreach(AICombatData combat in _hashCombat[sSaveID])
		{
			if(combat.myCsvHero.type == 2)
			{
				combat.gameObject.SetActive(true);
				/// 给点烟好看
				ManagerCreate.createEffectByName("effect_common_monster_birth", combat.transform.localPosition, combat.transform.localRotation);
			}
		}
	}
	/// 销毁所有数据
	public static void hashCombatClear()
	{
		foreach(int key in _hashCombat.Keys)
		{
			foreach(AICombatData combat in _hashCombat[key])
			{
				if(null != combat)
				{
					if(null != combat.GetComponent<SuperObject>())
						combat.GetComponent<SuperObject>().destroy();
					else
						GameObject.Destroy(combat.gameObject);
				}
			}
		}
		_hashCombat.Clear();
		foreach(AIFollower follower in _hideFollower)
		{
			follower.destroy();
		}
		_hideFollower.Clear();
	}
}

