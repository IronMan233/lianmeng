using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 掠夺 护送对
public class ManagerSenceEscortRob
{
	/// 基友信息
	private static ulong _idServerFriend;
	public static InfoPlayer infoFriend{get{return DataMode.getGUArmPlayer(_idServerFriend);}}
	public static InfoHero infoFriendHero{get{return DataMode.getGUArmHero(_idServerFriend);}}
	/// 攻打的队伍信息
	private static ulong _idServerSafeTeam;
	public static InfoEscortSafeTeam infoSafeTeam{get{return DataMode.getEscortSafeTeam(_idServerSafeTeam);}}

	/// 护送队伍信息
	private static ulong _idServerSafe;
	public static InfoEscortSafe infoSafe{get{return DataMode.getEscortSafe(_idServerSafe);}}

	private static int _idCsvMotMapRandom = 1;
	public static TypeCsvMotMap csvMotMap{get{return ManagerCsv.getMotMap(_idCsvMotMapRandom);}}

	/// 我要抢占 这个护送队
	public static void start(ulong sIDServerSafe, ulong sIDServerSafeTeam, ulong sIDServerFriend)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
		{
			WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
			return;
		}
		/// 随机给出一张地图
		if(sIDServerSafe != _idServerSafe)
		{
			_idCsvMotMapRandom = Random.Range(1, 100);
			while(null == csvMotMap)
			{
				_idCsvMotMapRandom = Random.Range(1, 100);
			}
		}


		DataMode.infoSetting.isPause = false;
		_idServerSafe = sIDServerSafe;
		_idServerFriend = sIDServerFriend;
		_idServerSafeTeam = sIDServerSafeTeam;


		if(DataMode.infoEscort.idServerEscortSearch != DataMode.infoEscort.idServerEscortRob)
			DataModeServer3.sendEscortRobInCheck(DataMode.infoEscort.idServerEscortSearch, recvRobInCheckHD);
		else
			recvRobInCheckHD(null);

	}
	/// 判断能不能进去
	private static void recvRobInCheckHD(UtilListenerEvent sRecv)
	{
		if(null == sRecv)
		{
			DataModeServer3.sendEscortDataRobTeam(null);
			DataModeServer3.sendEscortRobIn(_idServerSafeTeam, recvRobInHD);
			return;
		}

		switch(((net.unity3d.RM2C_ESCORT_BEAT_GROUP_ACT)sRecv.eventArgs).iResult)
		{
		case 1:
			DataModeServer3.sendEscortDataRobTeam(null);
			DataModeServer3.sendEscortRobIn(_idServerSafeTeam, recvRobInHD);
			break;
		case 1024:
			WindowsMngr.getInstance().showTextInfo(ConfigComment.ESCORT_COMBAT_IN_ROB);
			SuperUI.close("GusUISettingTeam");
			WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
			break;
		default :
			WindowsMngr.getInstance().showTextInfo("ERROR CODE " + ((net.unity3d.RM2C_ESCORT_BEAT_GROUP_ACT)sRecv.eventArgs).iResult);
			SuperUI.close("GusUISettingTeam");
			WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
			break;
		}
	}
	/// 进入副本
	private static void recvRobInHD(UtilListenerEvent sRecv)
	{
		createData();
		ManagerSYS.clear();
		ManagerSence.createSence(csvMotMap.urls, completeAssetCsv);
		/// 加载页面
//		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
//		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
	}
	/// 场景csv 表格加载完毕
	private static void completeAssetCsv()
	{
		List<string> urls = new List<string>();
		/// ui 预先加载
		urls.Add(ConfigUrl.getAtlasUrl("zhandou"));
		urls.Add(ConfigUrl.getAtlasUrl("hunshou"));
		urls.Add(ConfigUrl.getAtlasUrl("shuzi"));
		urls.Add(ConfigUrl.getAtlasUrl("skillicon"));
		urls.Add(ConfigUrl.getAtlasUrl("hero_touxiang"));
		urls.Add(ConfigUrl.getAtlasUrl("gezi"));
		
		/// 俩技能兰中的特效
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_01", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_02", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);

		/// 战斗中的特效
		EffMngr3.getInstance().getNoBindEff("effect_common_fire", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_monster_birth", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_shunyi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_rs_baoqi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);

		/// 下载角色资源
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.idTeamEscortRob));
		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamEscortRob));

		/// 下载敌人资源
		ConfigUrl.getAssetsServerHero(urls, infoSafeTeam.getTeam());
		ConfigUrl.getAssetsServerBeast(urls, infoSafeTeam.getTeamBeast());

		/// 如果有基友 下载基友函数
		if(null != infoFriendHero)
			ConfigUrl.getAssetsServerHero(urls, new List<InfoHero>(new InfoHero[]{infoFriendHero}));

		/// 战斗副本中预先加载的资源
		ConfigUrl.getAssetsInfoType(urls, "assetFB");

		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 胜利失败的玩意 ==============================================================================================

	/// 远征 胜利
	public static void combatWin()
	{
		SuperUI.close("GusUICombatOnlyButton");
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyBeast");
		SuperUI.close("GusUICombatOnlyTime");

		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		AICombat.showWin();
		ManagerAudio.playSound(ConfigUrl.UI_AUDIO_COMPLETE);		
	}
	/// 远征 失败
	public static void combatLost()
	{
		SuperUI.close("GusUICombatOnlyButton");
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyBeast");
		SuperUI.close("GusUICombatOnlyTime");

		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
	}
	/// 创建 人物 ui 战斗信息===========================================================================================

	/// 下载完成调用
	private static void complete(double sID)
	{
		createUI();
		createBeast();
		createHero();
		createEnemy();
		createCamera();
		createMusic();
		ManagerCombat.startEscortRobCombat();
		/// 加载页面
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}

	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		if(null != DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamEscortRob))
		{
			ManagerCombatBeast.beastSlef = ManagerCreate.createBeast(DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamEscortRob).idServer, 4);
			SuperUI.show<UICombatOnlyBeast>("GusUICombatOnlyBeast", null);
			if(null != DataMode.getEscortRobBeast(ManagerCombatBeast.beastSlef.infoBeast.idServer))
			{
				ManagerCombatBeast.beastSlef.anger = ManagerCombatBeast.beastSlef.dataAttCount.angerTotal * 
					DataMode.getEscortRobBeast(ManagerCombatBeast.beastSlef.infoBeast.idServer).angerMul;
			}
		}
		if(null != infoSafeTeam.getTeamBeast())
		{
			/// 跨服考虑
			ulong idServerBeast = infoSafeTeam.getTeamBeast().idServer;
			if(infoSafeTeam.isDummy)
				idServerBeast = DataMode.dummyEscortBeast(idServerBeast);
			/// 设置怒气值
			ManagerCombatBeast.beastEnemy = ManagerCreate.createBeast(idServerBeast, 13);
			if(null != DataMode.getEscortRobBeast(idServerBeast))
			{
				ManagerCombatBeast.beastEnemy.anger = ManagerCombatBeast.beastEnemy.dataAttCount.angerTotal * 
					DataMode.getEscortRobBeast(idServerBeast).angerMul;
			}
		}
	}
	/// 创建 数据
	private static void createData()
	{
		DataMode.myPlayer.isInEscortRob = true;
		ManagerAudio.soundSameLength = 3;
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
	}
	/// 创建 UI
	private static void createUI()
	{
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
		SuperUI.show<UICombatOnlyTime>("GusUICombatOnlyTime", null);
	}
	
	/// 创建 主角阵营数据
	private static void createHero()
	{
		Vector3 postCenter = Vector3.zero;
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(GMath.toInt(csvMotMap.idCombatStand[0]));
		List<AICombatData> listSkill = new List<AICombatData>();
		/// 我的出战团队
		List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.idTeamEscortRob);
		/// 添加我的好友进入队伍
		if(null != infoFriendHero)
			infoHeroList.Add(infoFriendHero);
		/// 主角家的站位信息
		/// 计算站位
		DataMode.myPlayer.infoHeroList.mathStandIndexArr(infoHeroList);
		
		postCenter = GMath.stringToVector3(csvMotMap.standInfo[0]);
		/// 生成角色
		foreach(InfoHero hero in infoHeroList)
		{
			InfoEscortRobHero infoEscortRobHero = DataMode.getEscortRobHero(hero.idServer);
			/// 事实证明这货死亡了
			if(null != infoEscortRobHero && infoEscortRobHero.hpMul <= 0)
				continue;
			Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
			post = post + postCenter;
			AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex, hero.idServer, true, post, new Vector3(1f, 0f, 0f), infoHeroList, DataMode.myPlayer);
			AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
			aicombatData.mySkillRelease[0].isCDInit = false;
			ManagerCombatBeast.addBuffBeast(aicombatData);
			/// 设置服务器转存属性
			if(infoEscortRobHero != null)
			{
				/// 血量 用服务器传来的重算
				aicombatData.hp *= infoEscortRobHero.hpMul;
				aicombatData.hp = Mathf.Max(1f, aicombatData.hp);
				/// 技能CD 用服务器传来的重算
				TypeCsvHeroSkill csvSkill = aicombatData.mySkillRelease[0].csvSkill;
				if(-1 != hero.infoSkill.skillSort.IndexOf(csvSkill.id))
					aicombatData.mySkillRelease[0].timeStampUnlock = Data.gameTime + csvSkill.cd - csvSkill.cd *
						infoEscortRobHero.skillCDMul[hero.infoSkill.skillSort.IndexOf(csvSkill.id)] - 0.05f;
				
				for(int index = 0; index < aicombatData.mySkillAuto.Count; index++)
				{
					csvSkill = aicombatData.mySkillAuto[index].csvSkill;
					if(-1 != hero.infoSkill.skillSort.IndexOf(csvSkill.id))
						aicombatData.mySkillAuto[index].timeStampUnlock = Data.gameTime + csvSkill.cd - csvSkill.cd *
							infoEscortRobHero.skillCDMul[hero.infoSkill.skillSort.IndexOf(csvSkill.id)] - 0.05f;
				}
			}
			/// 技能增加
			listSkill.Add(aicombatData);
		}
		/// 设置ui 手动技能
		SuperUI.getUI<UICombatOnlySkill>().setSkillData(listSkill);
		/// 创建ui 计时器
		SuperUI.getUI<UICombatOnlyTime>().setTime(Config.GAME_COMBAT_TIME);
	}
	
	/// 创建 敌人数据
	private static void createEnemy()
	{
		Vector3 postCenter = Vector3.zero;
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(GMath.toInt(csvMotMap.idCombatStand[1]));
		/// 主角家的站位信息
		/// 计算站位
		DataMode.myPlayer.infoHeroList.mathStandIndexArr(infoSafeTeam.getTeam());
		/// 我的出战团队
		List<InfoHero> infoHeroList = infoSafeTeam.getTeam();
		postCenter = GMath.stringToVector3(csvMotMap.standInfo[1]);
		/// 生成角色
		foreach(InfoHero hero in infoHeroList)
		{

			/// 跨服考虑
			ulong idServerHero = hero.idServer;
			if(infoSafeTeam.isDummy)
				idServerHero = DataMode.dummyEscortHero(idServerHero);

			InfoEscortRobHero infoEscortRobHero = DataMode.getEscortRobHero(idServerHero);
			/// 事实证明这货死亡了
			if(null != infoEscortRobHero && infoEscortRobHero.hpMul <= 0)
				continue;
			Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
			post = post + postCenter;
			AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex + 9, idServerHero, false, post, new Vector3(-1f, 0f, 0f), null, null);
			AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
			ManagerCombatBeast.addBuffBeast(aicombatData);
			/// 设置服务器转存属性
			if(infoEscortRobHero != null)
			{
				/// 血量 用服务器传来的重算
				aicombatData.hp *= infoEscortRobHero.hpMul;
				aicombatData.hp = Mathf.Max(1f, aicombatData.hp);
				/// 技能CD 用服务器传来的重算
				for(int index = 0; index < aicombatData.mySkillAuto.Count; index++)
				{
					TypeCsvHeroSkill csvSkill = aicombatData.mySkillAuto[index].csvSkill;
					aicombatData.mySkillAuto[index].timeStampUnlock = Data.gameTime + csvSkill.cd - csvSkill.cd *
						infoEscortRobHero.skillCDMul[hero.infoSkill.skillSort.IndexOf(csvSkill.id)] - 0.05f;
				}
			}
		}
	}
	/// 创建 音乐音效
	private static void createMusic()
	{
		if(csvMotMap.musicSence != "#" && !string.IsNullOrEmpty(csvMotMap.musicSence))
			ManagerAudio.playMusic(csvMotMap.musicSence);
	}
	/// 创建 相机数据
	private static void createCamera()
	{
		/// 设置摄像机属性
		CameraMoveData.valueData.changeMove(typeof(CameraMovePK));
		CameraMoveData.valueData.setTargetPostion((GMath.stringToVector3(csvMotMap.standInfo[0]) + GMath.stringToVector3(csvMotMap.standInfo[1])) * 0.5f, csvMotMap.initCamera);
		CameraMoveData.valueData.setPostionReal();
	}
}

