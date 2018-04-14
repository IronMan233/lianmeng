using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 创建形象控制类
public class ManagerCreate
{
	/// 子弹的容器
	private static GameObject _contBullet;
	/// 特效容器
	private static GameObject _contEffect;
	
	/// 创建一个形象
	public static AIPlayerFB createPlayerFB(uint idServerPlayer, Vector3 postion)
	{
		GameObject view = new GameObject("_PlayerInFBControl");
		AIPlayerFB player = view.AddComponent<AIPlayerFB>();
		
		
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		/// 直接移动视图
		view.transform.position = postion;
		
		return player;
	}
	/// 创建一个形象
	public static AIPlayerWorldBoss createPlayerWorldBoss(uint idServerPlayer, Vector3 postion)
	{
		GameObject view = new GameObject("_PlayerInWorldBossControl");
		AIPlayerWorldBoss player = view.AddComponent<AIPlayerWorldBoss>();
		
		
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		/// 直接移动视图
		view.transform.position = postion;
		
		return player;
	}
	/// 创建村庄中的角色
	public static AIPlayerTown createPlayerTown(Vector3 postion)
	{
		GameObject view = new GameObject("_PlayerTownControl");
		AIPlayerTown player = view.AddComponent<AIPlayerTown>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		/// 直接移动视图
		view.transform.position = postion;
		
		return player;
	}
	/// 创建主角信息
	public static AICombat createCombatHero(int index, ulong idServerHero, bool isCanHandSkill, Vector3 postion, Vector3 rotation, List<InfoHero> team, InfoPlayer infoPlayer)
	{
		/// 英雄数据
		InfoHero infoHero = DataMode.getHero(idServerHero);
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(infoHero.idCsv);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		
		/// 绑定特效
		EffMngr3.getInstance().beginBindEff(view, csvHero.id, true, idServerHero);
		AudioBindMngr.getInstance().beginBindAudio(view, csvHero.id);
		
		/// 创建数据层
		AICombatData combatData = view.AddComponent<AICombatData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		
		/// 创建功能层
		AICombat combat = view.AddComponent<AICombat>();
		
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		/// 直接移动视图
		view.transform.position = postion;
		/// 角度
		superObjectData.postion.rotationInit = rotation;
		superObjectData.postion.rotationReal = rotation;
		/// 我的站位信息
		combatData.standIndex = index;
		combatData.myCsvHero = csvHero;
		combatData.myServerHero = infoHero;
		combatData.lv = infoHero.lv;
		/// 为了跨服准备。属性重新读取
		createCombatData(combatData, isCanHandSkill, team, infoPlayer, idServerHero);
		/// 给点烟好看
		ManagerCreate.createEffectByName("effect_common_monster_birth", postion, Quaternion.LookRotation(rotation));
		/// 返回
		return combat;
		
	}
	/// 创建敌人
	public static AICombatData createCombatEnemy(int index, int idCsvHero, int lv, Vector3 postion, Vector3 rotation)
	{
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(idCsvHero);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		
		/// 绑定特效
		EffMngr3.getInstance().beginBindEff(view, idCsvHero);
//		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);

//		return null;
		/// 创建数据层
		AICombatData combatData = view.AddComponent<AICombatData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		
		/// 创建功能层
		view.AddComponent<AICombat>();
		
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		/// 直接移动视图
		view.transform.position = postion;
		/// 角度
		superObjectData.postion.rotationInit = rotation;
		superObjectData.postion.rotationReal = rotation;
		/// 我的站位信息
		combatData.standIndex = index;
		combatData.myCsvHero = csvHero;
		combatData.lv = lv;
		createCombatData(combatData);
		/// 返回
		return combatData;
	}
	/// 设置主角技能
	private static void createCombatData(AICombatData combatData)
	{
		createCombatData(combatData, true);
	}
	private static void createCombatData(AICombatData combatData, bool isCanHandSkill)
	{
		createCombatData(combatData, isCanHandSkill, null);
	}
	private static void createCombatData(AICombatData combatData, bool isCanHandSkill, List<InfoHero> team)
	{
		createCombatData(combatData, isCanHandSkill, team, null);
	}
	private static void createCombatData(AICombatData combatData, bool isCanHandSkill, List<InfoHero> team, InfoPlayer infoPlayer, ulong idServerRealHero = 0)
	{
		/// 技能设定
		TypeCombatSkill combatSkill = null;
		/// 普通攻击设定
		combatSkill = new TypeCombatSkill();
		combatSkill.csvSkill = ManagerCsv.getHeroSkill(combatData.myCsvHero.idAttack);
		combatSkill.timeStampUnlock = Data.gameTime + combatSkill.csvSkill.cdInit;
		combatData.myAttack = combatSkill;
		/// 如果我的服务器对象不是空的,按照服务器对象来
		if(null != combatData.myServerHero)
		{
			if(idServerRealHero <= 0)
				combatData.myCombatCount = DataAttCount.getAttServer(combatData.myServerHero.idServer, team, infoPlayer);
			else
				combatData.myCombatCount = DataAttCount.getAttServer(idServerRealHero, team, infoPlayer);
			/// 血量设定
			combatData.hp = combatData.hpTotal = combatData.myCombatCount.hp;
			
			/// 释放技能设定
			if(null != combatData.myServerHero.infoSkill.getSkillRelease())
			{
				if(null != ManagerCsv.getHeroSkill(combatData.myServerHero.infoSkill.getSkillRelease().idCsv))
				{
					combatSkill = new TypeCombatSkill();
					combatSkill.csvSkill = ManagerCsv.getHeroSkill(combatData.myServerHero.infoSkill.getSkillRelease().idCsv);
					combatSkill.timeStampUnlock = Data.gameTime + combatSkill.csvSkill.cdInit;
					/// 基友自动放主动技能
					if(isCanHandSkill)
					{
						combatSkill.isCDInit = true;
						combatData.mySkillRelease.Add(combatSkill);
					} else {
						/// 手动技能上去就放吧
						combatData.mySkillAuto.Add(combatSkill);
					}
				}
			}
			/// 自动技能设定
			List<InfoSkill> skillAutos = combatData.myServerHero.infoSkill.getSkillAuto();
			if(null != skillAutos)
			{
				for(int index = 0; index < skillAutos.Count; index++)
				{
					if(null == ManagerCsv.getHeroSkill(skillAutos[index].idCsv))
						continue;
					combatSkill = new TypeCombatSkill();
					combatSkill.csvSkill = ManagerCsv.getHeroSkill(skillAutos[index].idCsv);
					combatSkill.timeStampUnlock = Data.gameTime + combatSkill.csvSkill.cdInit;
					combatData.mySkillAuto.Add(combatSkill);
				}
			}
		}
		/// 如果我的服务器对象时空的
		if(null == combatData.myServerHero)
		{
			combatData.myCsvHeroAIArr = ManagerCsv.getHeroAI(combatData.myCsvHero.id);
			combatData.myCombatCount = DataAttCount.getAttCsv(combatData.myCsvHero.id, combatData.lv);
			/// 血量设定
			combatData.hp = combatData.hpTotal = combatData.myCombatCount.hp;
			/// 自动技能设定
			if(null != combatData.myCsvHero.idSkill)
			{
				
				if(null != combatData.myCsvHeroAIArr && combatData.myCsvHeroAIArr.Count > 0)
				{
					/// 通过AI拾取技能
					combatData.switchAISkill();
				} else {
					/// 默认拾取技能
					for(int i = 0; i < combatData.myCsvHero.idSkill.Length; i++)
					{
						if(null == ManagerCsv.getHeroSkill(GMath.toInt(combatData.myCsvHero.idSkill[i])))
							continue;
						combatSkill = new TypeCombatSkill();
						combatSkill.csvSkill = ManagerCsv.getHeroSkill(GMath.toInt(combatData.myCsvHero.idSkill[i]));
						if(combatData.standIndex < 9 && i == 0)
						{
							combatSkill.isCDInit = true;
							combatData.mySkillRelease.Add(combatSkill);
						} else {
							combatSkill.timeStampUnlock = Data.gameTime + combatSkill.csvSkill.cdInit;
							combatData.mySkillAuto.Add(combatSkill);
						}
					}
				}
			}
		}
		/// AI 技能效果
		if (null != combatData.myCsvHero.idSkillAI && combatData.myCsvHero.idSkillAI.Length > 0)
		{
			/// 默认拾取技能
			for(int i = 0; i < combatData.myCsvHero.idSkillAI.Length; i++)
			{
				if(null == ManagerCsv.getHeroSkill(GMath.toInt(combatData.myCsvHero.idSkillAI[i])))
					continue;
				combatSkill = new TypeCombatSkill();
				combatSkill.csvSkill = ManagerCsv.getHeroSkill(GMath.toInt(combatData.myCsvHero.idSkillAI[i]));
				combatSkill.timeStampUnlock = Data.gameTime + combatSkill.csvSkill.cdInit;
				combatData.mySkillAI.Add(combatSkill);
			}
		}

	}
	public static AICombatData createFollower(
		int standIndex, 
		ulong idServerHero, 
		GameObject followerTarget, 
		Vector3 followerOffset, 
		Vector3 rotation, 
		bool isFriend, 
		List<InfoHero> team, 
		InfoPlayer infoPlayer)
	{
		/// 形象的地址
		int idCsvHero = DataMode.getHero(idServerHero).idCsv;
		int lv = DataMode.getHero(idServerHero).lv;
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(idCsvHero);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		view.transform.localRotation = Quaternion.LookRotation(rotation);
		
		EffMngr3.getInstance().beginBindEff(view, idCsvHero, true, idServerHero);
		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);
		/// 创建数据层
		AICombatData combatData = view.AddComponent<AICombatData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		AIFollowerData followerData = view.AddComponent<AIFollowerData>();
		
		/// 创建功能层
		view.AddComponent<AIFollower>();
		
		/// 设置初始化属性
		superObjectData.postion.postionInit = followerTarget.transform.position + followerOffset;
		superObjectData.postion.postionReal = followerTarget.transform.position + followerOffset;
		superObjectData.postion.postionTarget = followerTarget.transform.position + followerOffset;
		/// 角度
		superObjectData.postion.rotationInit = rotation;
		superObjectData.postion.rotationReal = rotation;
		
		view.transform.position = followerTarget.transform.position + followerOffset;
		/// 我的站位信息
		combatData.standIndex = standIndex;
		combatData.myCsvHero = csvHero;
		combatData.myServerHero = DataMode.getHero(idServerHero);
		combatData.isFriend = isFriend;
		combatData.lv = lv;
		
		createCombatData(combatData, true, team, infoPlayer);
		/// 我跟随的目标
		followerData.followerTarget = followerTarget;
		followerData.followerOffset = followerOffset;
		/// 返回
		return combatData;
	}
	/// 创建没有AICombatData的跟随选项
	public static AIFollowerTown createFollowerTown(int standIndex, ulong idServerHero, GameObject followerTarget, Vector3 followerOffset, Vector3 rotation)
	{
		AIFollowerTown result = createFollowerTownCsv(standIndex, DataMode.getHero(idServerHero).idCsv, followerTarget, followerOffset, rotation);
		return result;
	}
	public static AIFollowerTown createFollowerTownCsv(int standIndex, int idCsvHero, GameObject followerTarget, Vector3 followerOffset, Vector3 rotation)
	{
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(idCsvHero);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
		EffMngr3.getInstance().beginBindEff(view, idCsvHero);
		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);
		
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		
		/// 创建数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		AIFollowerData followerData = view.AddComponent<AIFollowerData>();
		
		/// 创建功能层
		AIFollowerTown follower = view.AddComponent<AIFollowerTown>();
		
		/// 设置初始化属性
		if(null != followerTarget)
		{
			superObjectData.postion.postionInit = followerTarget.transform.position + followerOffset;
			superObjectData.postion.postionReal = followerTarget.transform.position + followerOffset;
			superObjectData.postion.postionTarget = followerTarget.transform.position + followerOffset;
		}
		if(null == followerTarget)
		{
			superObjectData.postion.postionInit = followerOffset;
			superObjectData.postion.postionReal = followerOffset;
			superObjectData.postion.postionTarget = followerOffset;
		}
		/// 角度
		superObjectData.postion.rotationInit = rotation;
		superObjectData.postion.rotationReal = rotation;
		
		/// 我跟随的目标
		followerData.followerTarget = followerTarget;
		followerData.followerOffset = followerOffset;
		followerData.csvHero = csvHero;
		
		CapsuleCollider collider = view.AddComponent<CapsuleCollider>();
		collider.center = new Vector3(0f, 1f, 0f);
		collider.height = 2f;
		collider.radius = 0.8f;
		/// 返回
		return follower;
	}
	/// 生成ui形象
	public static AIUIHero createHeroUI(int idCsvHero, Vector3 followerOffset)
	{
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(idCsvHero);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
//		EffMngr.getInstance().beginBindEff(view, idCsvHero);
//		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);
		
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		
		/// 创建数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		
		/// 创建功能层
		AIUIHero control = view.AddComponent<AIUIHero>();
		
		control.csvHero = csvHero;
		
		superObjectData.postion.postionInit = followerOffset;
		superObjectData.postion.postionReal = followerOffset;
		superObjectData.postion.postionTarget = followerOffset;
		
		CapsuleCollider collider = view.AddComponent<CapsuleCollider>();
		collider.center = new Vector3(0f, 1f, 0f);
		collider.height = 2f;
		collider.radius = 0.8f;
		//add by yxh 
		Animation ani = view.GetComponentInChildren<Animation>();
		if(ani != null)
		{
			ani.cullingType = AnimationCullingType.AlwaysAnimate;
		}
		/// 返回
		return control;
	}

	public static AIUIBeast creaateBeastUI(int idCsv, int lv, Vector3 followerOffset)
	{	
		/// 获得形象
		TypeCsvBeast csvHero = ManagerCsv.getBeast(idCsv, lv);
		/// 创建形象

		GameObject view =  GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getHeroUrl(ManagerCsv.getView(csvHero.idView).url))) as GameObject;
		//		EffMngr.getInstance().beginBindEff(view, idCsvHero);
		//		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);
		
		//view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		view.transform.localScale = new Vector3(100, 100, 100);
		/// 创建数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		
		/// 创建功能层
		AIUIBeast control = view.AddComponent<AIUIBeast>();
		
		control.csvHero = csvHero;
		
		superObjectData.postion.postionInit = followerOffset;
		superObjectData.postion.postionReal = followerOffset;
		superObjectData.postion.postionTarget = followerOffset;
		
		CapsuleCollider collider = view.AddComponent<CapsuleCollider>();
		if(idCsv == 3 || idCsv == 4)
		{
			collider.center = new Vector3(0f, 6f, 0f);
			collider.height = 2f;
			collider.radius = 6f;
		}
		else
		{
			collider.center = new Vector3(0f, 1f, 0f);
			collider.height = 2f;
			collider.radius = 0.8f;
		}		
		//add by yxh 
		Animation ani = view.GetComponentInChildren<Animation>();
		if(ani != null)
		{
			ani.cullingType = AnimationCullingType.AlwaysAnimate;
		}
		/// 返回
		return control;
	}

	public static AIUIShopHero createShopHeroUI(int idCsvHero, Vector3 followerOffset)
	{
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(idCsvHero);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
//		EffMngr.getInstance().beginBindEff(view, idCsvHero);
//		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);
		
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		
		/// 创建数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		
		/// 创建功能层
		AIUIShopHero control = view.AddComponent<AIUIShopHero>();
		
		control.csvHero = csvHero;
		
		superObjectData.postion.postionInit = followerOffset;
		superObjectData.postion.postionReal = followerOffset;
		superObjectData.postion.postionTarget = followerOffset;
		
		CapsuleCollider collider = view.AddComponent<CapsuleCollider>();
		collider.center = new Vector3(0f, 1f, 0f);
		collider.height = 2f;
		collider.radius = 0.8f;
		/// 返回
		return control;
	}
	
	/// 创建敌人
	/// 因为看不见,所以不需要旋转方向
	public static AIEnemy createEnemy(int index, int idCsvEnemy, Vector3 postion)
	{
		GameObject view = new GameObject();
		/// 控制器
		AIEnemy enemy = view.AddComponent<AIEnemy>();
		/// 数据层
		AIEnemyData enemyData = view.AddComponent<AIEnemyData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置数据
		enemyData.index = index;
		enemyData.idCsv = idCsvEnemy;		
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		
		view.transform.position = superObjectData.postion.postionReal;
		/// 返回敌人信息
		return enemy;
	}
	/// 创建子弹 位置信息
	public static Bullet createBullet(int standShooter, int idCsv, Vector3 postion, GameObject targetObj, Vector3 target, float area, FunctionAttackMul callBack, bool isLight)
	{
		/// 子弹的视图
		TypeCsvBullet csvBullet = ManagerCsv.getBullet(idCsv);     
		/// 视图
		GameObject view = null;
		if(csvBullet.view == "#" || string.IsNullOrEmpty(csvBullet.view))
		{
			view = new GameObject("New Bullet " + standShooter + " " + idCsv);
		} else {
			view = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getBulletUrl(csvBullet.view))) as GameObject;
		}
		
		if(null == _contBullet)
		{
			_contBullet = new GameObject("_contBullet");
			_contBullet.tag = Config.UNDESTROY;
		}
		view.transform.parent = _contBullet.transform;
		
		
		view.transform.localScale = new Vector3(1f, 1f, 1f);
		/// 子弹 必须亮堂
		if(isLight)
			NGUITools.SetLayer(view, Config.LAYER_GAME_LIGHT);
		
		/// 控制器
		Bullet bullet = view.AddComponent<Bullet>();
		/// 数据层
		BulletData bulletData = view.AddComponent<BulletData>();
		/// 设置数据
		bulletData.csvBullet = csvBullet;
		bulletData.endAction = callBack;
		/// 半径范围内随机一个点
		Vector3 targetArea = Vector3.zero;
		Vector3 targetOffset = GMath.stringToVector3(csvBullet.offsetPostion);
		if(standShooter >= 9)
		{
			targetOffset.x = targetOffset.x * -1f;
		}
		
		if(area > 0f)
			targetArea = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * (area * Random.Range(0f, 1f));
		
		/// 设置我的坐标
		bulletData.target = targetObj;
		/// 设置子弹轨迹
		bulletData.bezier3 = new List<Vector3>();
		{
			/// 标准移动类型
			if(csvBullet.moveType == 3)
			{
				bulletData.bezier3.Add(new Vector3(target.x, -0.01f, target.z) + targetArea);
			} 
			else
			{
				/// 添加 初始点
				bulletData.bezier3.Add(postion);
				/// 添加 抛物线中间点
				if(csvBullet.moveType == 2)
				{
					Vector3 postionMid = (target + targetArea) - postion;
					bulletData.bezier3.Add(postion + new Vector3(postionMid.x, 10f, postionMid.z) * 0.5f);
					/// 添加 目标点
					bulletData.bezier3.Add(new Vector3(target.x, -0.01f, target.z) + targetArea);
				}
				/// 直线到达目标点
				if(csvBullet.moveType == 1)
				{
					/// 添加 目标点
					bulletData.bezier3.Add(new Vector3(target.x, target.y, target.z) + targetArea);
				}
				/// 乱轨迹 WILL DONE 魔法蛋效果
				if(csvBullet.moveType == 4)
				{
					Vector3 postionMid = (new Vector3(target.x, -0.01f, target.z) + targetArea) - postion;
					Vector3 offsetHeight = Quaternion.AngleAxis(-Random.Range(10f, 170f), postionMid) * Vector3.forward * 3f;
					bulletData.bezier3.Add(postion + postionMid * 0.5f + offsetHeight);
					/// 添加 目标点
					bulletData.bezier3.Add(new Vector3(target.x, -0.01f, target.z) + targetArea);
				}
				/// 直行下落
				if(csvBullet.moveType == 5)
				{
//					Vector3 postionMid = (target + targetArea) - postion;
//					bulletData.bezier3.Add(postion + new Vector3(postionMid.x, 0f, postionMid.z) * 0.5f);
					/// 添加 目标点
					bulletData.bezier3[0] += targetOffset;
					bulletData.bezier3.Add(new Vector3(target.x, -0.01f, target.z) + targetArea);
				}
			}
		}
		view.SetActive(false);
		view.transform.position = bulletData.bezier3[0] + targetOffset;
		view.transform.rotation = Quaternion.LookRotation(bulletData.bezier3[bulletData.bezier3.Count - 1] - (bulletData.bezier3[0]  + targetOffset));
		view.SetActive(true);
		
		bulletData.timeTotal = Vector3.Distance(view.transform.position, bulletData.bezier3[bulletData.bezier3.Count - 1]) / csvBullet.speed;
		/// 返回敌人信息
		return bullet;
	}
	/// 特效名字，坐标，时间，攻击次数，连接类型，连线个数
	public static BulletLine createBulletLine(string sEffectName, Vector3 sPostion, float sTimeDuration, int sAttackTimes, float sAtkMul, int sLineCnt)
	{
		GameObject view = new GameObject("_BullitLine_" + sEffectName);
		view.transform.localPosition = sPostion;
		BulletLine result = view.AddComponent<BulletLine>();
		result.attackTimes = sAttackTimes;
		result.timeDuration = sTimeDuration;
		result.lineCnt = sLineCnt;
		result.assetName = sEffectName;
		result.atkMul = sAtkMul;
		return result;
	}


	/// 创建普通特效
	public static void createEffectByName(string sEffectName, Vector3 sPostion, Quaternion sRotation)
	{
//		createEffectByName(sEffectName, sPostion, sRotation, Config.LAYER_GAME_LIGHT);
		createEffectByName(sEffectName, sPostion, sRotation, Config.LAYER_GAME_SENCE);
	}
	public static void createEffectByName(string sEffectName, Vector3 sPostion, Quaternion sRotation, int layer)
	{
		if(string.IsNullOrEmpty(sEffectName))
			return;
		if(sEffectName == "#")
			return;
		/// 获取特效
		EffMngr3.getInstance().getNoBindEff(sEffectName, createEffectByNameHD, sPostion, sRotation, layer);
	}
	private static void createEffectByNameHD(GameObject obj)
	{
		if(null == _contEffect)
		{
			_contEffect = new GameObject("_contEffect");
			_contEffect.transform.localPosition = Vector3.zero;
			_contEffect.transform.localRotation = Quaternion.identity;
			_contEffect.transform.localScale = Vector3.one;
			_contEffect.tag = Config.UNDESTROY;
		}
		obj.transform.parent = _contEffect.transform;
		obj.AddComponent<EffectParticleSystem>();
	}
	/// 创建各种道具玩意
	public static void createEffectDropProp(string assetName, Vector3 sPostion)
	{
		/// 生成图像直接指定坐标
		GameObject view = new GameObject("EffectDropProp_" + Data.IDSign);
		GameObject view2 = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl(assetName)), sPostion, Quaternion.Euler(0f,0f,0f)) as GameObject;
		view2.transform.parent = view.transform;
		view2.transform.localPosition = new Vector3(0f, 0.2f, 0f);

		NGUITools.SetLayer(view, Config.LAYER_GAME_SENCE);
		/// 控制器
		DropProp dropProp = view.AddComponent<DropProp>();
		/// 数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = sPostion;
		superObjectData.postion.postionReal = sPostion;
		superObjectData.postion.postionTarget = sPostion;
		superObjectData.postion.rotationReal = Vector3.zero;
		
		view.transform.position = superObjectData.postion.postionReal;
	}
	/// 创建宝箱
	public static DropBox createEffectDropBox(string assetName, Vector3 sPostion)
	{
		GameObject view = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl(assetName)), sPostion, Quaternion.Euler(0f,0f,0f)) as GameObject;
		NGUITools.SetLayer(view, Config.LAYER_GAME_SENCE);
		/// 控制器
		DropBox dropProp = view.AddComponent<DropBox>();
		/// 数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = sPostion;
		superObjectData.postion.postionReal = sPostion;
		superObjectData.postion.postionTarget = sPostion;
		superObjectData.postion.rotationReal = new Vector3(0f, 0f, -1f);
		view.transform.position = superObjectData.postion.postionReal;

		return dropProp;
	}
	/// 创建宝箱
	public static EffectPath createEffectPath(string assetName, float sRandomR, float sPeed, Vector3 sPost1, Vector3 sPost2, int sDropOverTimes)
	{
		GameObject view = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl(assetName)), sPost1, Quaternion.Euler(0f,0f,0f)) as GameObject;
		NGUITools.SetLayer(view, Config.LAYER_GAME_SENCE);
		/// 控制器
		EffectPath effect = view.AddComponent<EffectPath>();
		effect.randomPointMidR = sRandomR;
		effect.speed = sPeed;
		effect.setPoint(sPost1, sPost2);
		if(sDropOverTimes > 0)
		{
			effect.dispatchListener = "EventDropPropOver";
			effect.dispatchListenerTimes = sDropOverTimes;
		}
		return effect;
	}


	
	/// 创建轨迹的特效,在多少秒内移动完毕
	public static void createEffectByNameMove(string sEffectName, List<Vector3> sPostionPath, Quaternion sRotation, float sTime)
	{
		GameObject view = new GameObject(sEffectName);
		if(null == _contEffect)
		{
			_contEffect = new GameObject("_contEffect");
			_contEffect.tag = Config.UNDESTROY;
		}
		view.transform.parent = _contEffect.transform;
		if(null != sPostionPath && sPostionPath.Count > 0)
			view.transform.position = sPostionPath[0];
		view.transform.rotation = sRotation;
		EffectParticleSystemMove moveControl = view.AddComponent<EffectParticleSystemMove>();
		moveControl.postionPath = sPostionPath;
		moveControl.timeOver = sTime;
	}
	
	
	
	/// 创建鬼
	public static AIGhost createGhost(GameObject sTarget)
	{
		GameObject view = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl("ghost"))) as GameObject;
		/// 创建数据层
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 创建功能层
		AIGhost ghost = view.AddComponent<AIGhost>();
		ghost.followerTarget = sTarget;
		/// 设置初始化属性
		superObjectData.postion.postionInit = sTarget.transform.position;
		superObjectData.postion.postionReal = sTarget.transform.position;
		superObjectData.postion.postionTarget = sTarget.transform.position;
		/// 返回鬼
		return ghost;
	}
	/// 创建敌人
	public static AIWorldBoss createWorldBoss(int index, Vector3 postion)
	{
		GameObject view = new GameObject();
		/// 控制器
		AIWorldBoss enemy = view.AddComponent<AIWorldBoss>();
		/// 数据层
		AIEnemyData enemyData = view.AddComponent<AIEnemyData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置数据
		enemyData.index = index;
		/// 设置初始化属性
		superObjectData.postion.postionInit = postion;
		superObjectData.postion.postionReal = postion;
		superObjectData.postion.postionTarget = postion;
		
		view.transform.position = superObjectData.postion.postionReal;
		/// 返回敌人信息
		return enemy;
	}
	/// 创建跟随目标
	public static AICombatData createWorldBossFollower(
		int standIndex, 
		int idCsvHero, 
		int lv, 
		GameObject followerTarget, 
		Vector3 followerOffset, 
		Vector3 rotation)
	{
		/// 获得形象
		TypeCsvHero csvHero = ManagerCsv.getHero(idCsvHero);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		view.transform.localRotation = Quaternion.LookRotation(rotation);
		EffMngr3.getInstance().beginBindEff(view, idCsvHero);
		AudioBindMngr.getInstance().beginBindAudio(view, idCsvHero);
		
		/// 创建数据层
		AICombatData combatData = view.AddComponent<AICombatData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		AIFollowerData followerData = view.AddComponent<AIFollowerData>();
		
		/// 创建功能层
		AIFollower aiFollower = view.AddComponent<AIFollower>();

		/// 设置初始化属性
		superObjectData.postion.postionInit = followerTarget.transform.position + followerOffset;
		superObjectData.postion.postionReal = followerTarget.transform.position + followerOffset;
		superObjectData.postion.postionTarget = followerTarget.transform.position + followerOffset;
		/// 角度
		superObjectData.postion.rotationInit = rotation;
		superObjectData.postion.rotationReal = rotation;
		
		view.transform.position = followerTarget.transform.position + followerOffset;
		/// 我的站位信息
		combatData.standIndex = standIndex;
		combatData.myCsvHero = csvHero;
		combatData.lv = lv;
		
		createCombatData(combatData);
		/// 我跟随的目标
		followerData.followerTarget = followerTarget;
		followerData.followerOffset = followerOffset;
		/// 返回
		return combatData;
	}
	/// 创建召唤出来的对象
	public static AISummon createSummon(int sIdSummon, AICombat sCombat, Vector3 sPostion, Vector3 sRotation)
	{
		sPostion.y = Mathf.Max(0f, sPostion.y);
		TypeCsvSummon csvSummon = ManagerCsv.getSummon(sIdSummon);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvSummon.idHero);
		view.transform.localScale = new Vector3(csvSummon.scale, csvSummon.scale, csvSummon.scale);
		view.transform.localRotation = Quaternion.LookRotation(sRotation);
		EffMngr3.getInstance().beginBindEff(view, csvSummon.idHero);
		AudioBindMngr.getInstance().beginBindAudio(view, csvSummon.idHero);
		
		/// 创建数据层
		AISummonData summonData = view.AddComponent<AISummonData>();
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = sPostion + GMath.stringToVector3(csvSummon.postOffset);
		superObjectData.postion.postionReal = superObjectData.postion.postionInit;
		superObjectData.postion.postionTarget = superObjectData.postion.postionInit;
		/// 角度
		superObjectData.postion.rotationInit = sRotation;
		superObjectData.postion.rotationReal = sRotation;
		
		view.transform.position = superObjectData.postion.postionInit;
		/// 我的一些信息
		summonData.myMaster = sCombat;
		summonData.myCsvSummon = csvSummon;
		summonData.timeStamp = csvSummon.time + Data.gameTime;
		if(null != csvSummon.idSkill)
		{
			for(int index = 0; index < csvSummon.idSkill.Length; index++)
			{
				TypeCsvHeroSkill csvSkill = ManagerCsv.getHeroSkill(int.Parse(csvSummon.idSkill[index]));
				TypeCombatSkill skill = new TypeCombatSkill();
				skill.csvSkill = csvSkill;
				skill.timeStampUnlock = Data.gameTime + csvSkill.cd;
				summonData.mySkill.Add(skill);
			}
		}
		
		
		/// 添加控制器
		AISummon summon = view.AddComponent<AISummon>();
		summon.startAnim = "reast";
		/// 返回
		return summon;
		
	}
	/// 在哪里出光圈 啥时候消失
	public static AICombatSkillArea createSkillArea(Vector3 sPostion, float sArea, float sTime)
	{
		return createSkillArea(sPostion, sArea, sTime, null);
	}
	public static AICombatSkillArea createSkillArea(Vector3 sPostion, float sArea, float sTime, List<AICombat> sTarget)
	{
		GameObject view = new GameObject();
		view.transform.localPosition = sPostion;
		AICombatSkillArea result = view.AddComponent<AICombatSkillArea>();
		result.area = sArea;
		result.time = sTime;
		result.targets = sTarget;
		return result;
	}
	/// 绘制矩形范围
	public static AICombatSkillAreaRect createSkillAreaRect(Vector3 sPostion, Quaternion sRotation, float sWidth, float sHeignt)
	{
		GameObject view = new GameObject();
		view.transform.localPosition = sPostion;
		AICombatSkillAreaRect result = view.AddComponent<AICombatSkillAreaRect>();
		result.setPointData(sPostion, sRotation, sWidth, sHeignt);
		result.material = (Material)LoadMngr.getInstance().GetObject(ConfigUrl.getInfoNameUrl("redLine"));
		return result;
	}
	/// 绘制矩形范围
	public static AICombatSkillAreaRect createSkillAreaCircle(Vector3 sPostion, Quaternion sRotation, float sDis, float sAngle360)
	{
		GameObject view = new GameObject();
		view.transform.localPosition = sPostion;
		AICombatSkillAreaRect result = view.AddComponent<AICombatSkillAreaRect>();
		result.setDrawCircle(sPostion, sRotation, sDis, sAngle360);
		result.material = (Material)LoadMngr.getInstance().GetObject(ConfigUrl.getInfoNameUrl("redLine"));
		return result;
	}


	/// 创建buff变身玩意的控制器
	public static AIBuffViewChange createBuffViewChange(int sIDCsvView, Vector3 sPostion, Quaternion sRotation)
	{
		TypeCsvHero csvHero = ManagerCsv.getHero(sIDCsvView);
		/// 创建形象
		GameObject view = LoadHeroMngr.getHeroObj(csvHero.id);
		view.transform.localScale = new Vector3(csvHero.scale, csvHero.scale, csvHero.scale);
		view.transform.localRotation = sRotation;
		
		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = sPostion + new Vector3(0f, 0f, 0f);
		superObjectData.postion.postionReal = superObjectData.postion.postionInit;
		superObjectData.postion.postionTarget = superObjectData.postion.postionInit;
		/// 角度
		superObjectData.postion.rotationInit = sRotation * Vector3.forward;
		superObjectData.postion.rotationReal = sRotation * Vector3.forward;
		
		view.transform.position = superObjectData.postion.postionInit;

		/// 添加控制器
		AIBuffViewChange buffView = view.AddComponent<AIBuffViewChange>();
		buffView.startAnim = "reast";
		/// 返回
		return buffView;
		
	}
	/// 创建 魂兽
	public static AIBeastData createBeast(ulong sIDServer, int standIndex)
	{
		InfoBeast infoBeast = DataMode.getBeast(sIDServer);
		TypeCsvBeast csvBeast = ManagerCsv.getBeast(infoBeast.idCsv, infoBeast.lv);
		TypeCsvView csvView = ManagerCsv.getView(csvBeast.idView);
		GameObject view = (GameObject)GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getHeroUrl(csvView.url)));
		view.transform.localScale = Vector3.one * csvBeast.scale;

		EffMngr3.getInstance().beginBindEffBeast(view, sIDServer);
		AudioBindMngr.getInstance().beginBindAudioBeast(view,infoBeast.idCsv, sIDServer);

		SuperObjectData superObjectData = view.AddComponent<SuperObjectData>();
		/// 设置初始化属性
		superObjectData.postion.postionInit = new Vector3(0f, 0f, 0f);
		superObjectData.postion.postionReal = superObjectData.postion.postionInit;
		superObjectData.postion.postionTarget = superObjectData.postion.postionInit;
		/// 角度
		superObjectData.postion.rotationInit = Vector3.forward;
		superObjectData.postion.rotationReal = Vector3.forward;
		
		view.transform.position = superObjectData.postion.postionInit;

		AIBeastData result = view.AddComponent<AIBeastData>();
		result.dataAttCount = DataAttCountBeast.getAttServer(sIDServer);
		result.anger = result.dataAttCount.angerInit;
		result.infoBeast = infoBeast;
		result.standIndex = standIndex;
		view.SetActive(false);

		return result;
	}
}
