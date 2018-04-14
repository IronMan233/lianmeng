using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSenceSunWell
{
	
	/// 我的指定ID信息
	private static int _idCsv;
	/// 召唤进来的基友
	public static InfoHero infoHeroFriend = null;
	
	public static InfoPlayer infoPlayerFriend = null;
	
	public static InfoActive infoActive{get{return DataMode.getActiv(_idCsv);}}
	public static TypeCsvActivEvent csvActive{get{return ManagerCsv.getActivEvent(_idCsv);}}
	/// 开始战斗
	public static void start(int idCsv)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		DataMode.infoSetting.isPause = false;
		/// 我这职业返回
		_idCsv = idCsv;
		/// 奖励先回来
		if(null == infoHeroFriend || null == infoPlayerFriend)
			DataModeServer.sendActiveUpdateEnemy(idCsv, 0, 0, recvActiveEvent);	
		else
			DataModeServer.sendActiveUpdateEnemy(idCsv, infoPlayerFriend.idServer, infoHeroFriend.idServer, recvActiveEvent);	
	}
	private static void recvActiveEvent(UtilListenerEvent sEvent)
	{
		/// 清除索引
		ManagerCombatData.clearIndex();
		/// 清空角色
		ManagerSYS.clear();
		
		// add by ssy 
		// clean mem before load 
		if(!DataMode.myPlayer.isInSunWell)
		{
			EffMngr3.getInstance().clearAllEff();
			
			LoadMngr.getInstance().cleanMemsAll();
		}
		// add end
		
		
		
		/// 俩技能兰中的特效
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_01", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_02", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		/// 战斗中的特效
		EffMngr3.getInstance().getNoBindEff("effect_common_fire", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_monster_birth", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_shunyi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_rs_baoqi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_role_born_line_head", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_role_born_smoke", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		
		/// 加载完csv文件信息,开始加载ui资源
		ManagerSence.createSence(csvActive.urls, assetLoad);
		
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
		
		/// 创建天气效果
		ManagerSence.createWeather(csvActive.weather);
	}
	
	
	/// 副本 胜利
	public static void combatWin()
	{
		/// 波次胜利
		AIEnemy.setEnemyNext();
		/// 开启角色控制
		AIPlayer.valueObject.lockControl = false;
		
		/// 清除计时器
		SuperUI.close("GusUICombatOnlyTime");
		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		/// 看看是不是通关
		if(null == AIEnemy.getEnemyNext())
		{
			SuperUI.close("GusUICombatOnlyButton");
			SuperUI.close("GusUICombatOnlySkill");
//			SuperUI.close("GusUICombatOnlyBeast");
			SuperUI.close("GusUICombatHPWorldBoss");

			AICombat.showWinRotation();
			UtilListener.calledTime(AICombat.showWinAnimation, 0.7f);
			UtilListener.calledTime(combatComplete, 3f);
			
			ManagerAudio.playSound(ConfigUrl.UI_AUDIO_COMPLETE);
			
			CameraMoveData.valueData.changeMove(typeof(CameraMoveCombatWin));
			
			return;
		}
		/// 变成跟随者
		changeFollower();
		/// 相机滚回去
		CameraMoveData.valueData.setTarget(AIPlayerFB.valueObject.transform, null);
		
		/// 如果是最后的敌人
		if(AIEnemy.isLastEnemy())
		{
			if(csvActive.musicCombatLast == "#" || string.IsNullOrEmpty(csvActive.musicCombatLast))
				return;
			ManagerAudio.playMusic(csvActive.musicCombatLast);
		}
	}
	/// 副本 失败
	public static void combatLost()
	{
		SuperUI.close("GusUICombatOnlyButton");
		SuperUI.close("GusUICombatOnlySkill");
//		SuperUI.close("GusUICombatOnlyBeast");
		SuperUI.close("GusUICombatOnlyTime");
		SuperUI.close("GusUICombatHPWorldBoss");

		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		/// 展示失败页面
		WindowsMngr.getInstance().openWindow(WindowsID.SUNWELLRESULT, 0);
	}
	/// 副本 通关
	private static void combatComplete()
	{
		/// 展示通关页面
		WindowsMngr.getInstance().openWindow(WindowsID.SUNWELLRESULT, 1);
	}
	
	
	/// 改变成跟随模式
	private static void changeFollower()
	{
		UnityEngine.Object[] combats = GameObject.FindObjectsOfType(typeof(AICombat));
		/// 转换控制器
		foreach(UnityEngine.Object obj in combats)
		{
			AICombat combat = (AICombat)obj;
			/// 没有跟随效果,结束
			AIFollowerData followerData = combat.gameObject.GetComponent<AIFollowerData>();
			SuperObjectData superData = combat.gameObject.GetComponent<SuperObjectData>();
			if(null == followerData || null == superData)
				continue;
			/// 删除控制器
			combat.destroyControl();
			/// 添加控制器
			combat.gameObject.AddComponent<AIFollower>();
			/// 添加特效
		}
	}
	
	/// ===================================================================================================
	
	
	
	/// 资源加载
	private static void assetLoad()
	{
		List<string> urls = new List<string>();
		
		urls.Add(ConfigUrl.getAtlasUrl("zhandou"));
		urls.Add(ConfigUrl.getAtlasUrl("hunshou"));
		urls.Add(ConfigUrl.getAtlasUrl("shuzi"));
		urls.Add(ConfigUrl.getAtlasUrl("skillicon"));
		urls.Add(ConfigUrl.getAtlasUrl("uibase"));
		urls.Add(ConfigUrl.getAtlasUrl("hero_touxiang"));
		urls.Add(ConfigUrl.getAtlasUrl("gezi"));
		
		
		DataMode.getFBRewardURL(urls);

		/// url 敌人
		foreach(int idCsvCombatEnemy in infoActive.idCsvCombatEnemy)
		{
			ConfigUrl.getAssetsCombatEnemy(urls, idCsvCombatEnemy);
		}
		
		/// 获得主角队伍
		List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeamByTeamType(ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType);
		if(null != infoHeroFriend)
			infoHeroList.Add(infoHeroFriend);
		ConfigUrl.getAssetsServerHero(urls, infoHeroList);
		/// 加载魂兽资源
//		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeastByType(ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType));
		
		/// 副本中的各种玩意
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		
		
		urls.Add(ConfigUrl.getAudioUrl("SFX_Role10"));

		if(null != csvActive.musicSence && "#" != csvActive.musicSence)
			urls.Add(ConfigUrl.getAudioUrl(csvActive.musicSence));
		if(null != csvActive.musicCombatLast && "#" != csvActive.musicCombatLast)
			urls.Add(ConfigUrl.getAudioUrl(csvActive.musicCombatLast));

		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 加载成功
	private static void complete(double loader_id)
	{
		createData();
		
		createUI();
//		createBeast();
		
		createEnemy();
		createMusic();
		createPlayer();
		ManagerSence.createLight();
		ManagerCombat.startSunWellCombat();

	}
	
	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		uint idServerTeam = DataMode.myPlayer.infoHeroList.teamList[ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType];
		if(null != DataMode.myPlayer.infoHeroList.getTeamBeast(idServerTeam))
		{
			ManagerCombatBeast.beastSlef = ManagerCreate.createBeast(DataMode.myPlayer.infoHeroList.getTeamBeast(idServerTeam).idServer, 4);
			SuperUI.show<UICombatOnlyBeast>("GusUICombatOnlyBeast", null);
		}
		ManagerCombatBeast.beastEnemy = null;
	}
	/// 创建敌人  在看不见的地方
	private static void createEnemy()
	{
		AIEnemy.createEnemys(infoActive.idCsvCombatEnemy.Count);
		for(int index = 0; index < infoActive.idCsvCombatEnemy.Count; index++)
		{
			TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[index]);
			TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvCombatEnemy.idCombatStand);
			Vector3 postCenter = GMath.stringToVector3(csvActive.standInfo[index]);
			/// 生成角色
			for(int i = 0; i < 9; i++)
			{
				int idCsvHero = (int)csvCombatEnemy.GetType().GetField("index" + i).GetValue(csvCombatEnemy);
				if(idCsvHero <= 0)
					continue;
				Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + i).GetValue(csvCombatStand));
				post = post + postCenter;
				AICombatData combatData = ManagerCreate.createCombatEnemy(i + 9, idCsvHero, csvCombatEnemy.lv, post, new Vector3(-1f, 0f, 0f));

				/// 给世界boss点面子吧data
				SuperUI.showJust<UICombatHPSunWell>("GusUICombatHPWorldBoss", null);
				SuperUI.getUIList<UICombatHPSunWell>()[SuperUI.getUIList<UICombatHPSunWell>().Count - 1].setCombatData(combatData);
			}
		}
	}
	/// 创建主角   在看不见的地方
	private static void createPlayer()
	{
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(GMath.toInt(csvActive.idCombatStand));
		List<AICombatData> listSkill = new List<AICombatData>();
		/// 我的出战团队
		List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeamByTeamType(ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType);
		/// 添加基友
		if(null != infoHeroFriend)
			infoHeroList.Add(infoHeroFriend);
		/// 计算站位
		DataMode.myPlayer.infoHeroList.mathStandIndexArr(infoHeroList);
		/// 主角家的站位信息
		Vector3 postCenter = GMath.stringToVector3(csvActive.initXYZ);
		/// 生成角色
		foreach(InfoHero hero in infoHeroList)
		{
			Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
			post = post + postCenter;
			AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex, hero.idServer, true, post, new Vector3(1f, 0f, 0f), infoHeroList, DataMode.myPlayer);
			AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
			aicombatData.isFriend = (hero == infoHeroFriend);
//			aicombatData.mySkillRelease[0].isCDInit = false;
//			ManagerCombatBeast.addBuffBeast(aicombatData);
			/// 技能增加
			listSkill.Add(aicombatData);
		}
		/// 设置ui 手动技能
		SuperUI.getUI<UICombatOnlySkill>().setSkillData(listSkill);
		/// 创建ui 计时器
		SuperUI.getUI<UICombatOnlyTime>().setTime(Config.GAME_COMBAT_TIME);

		/// 设置摄像机属性
		CameraMoveData.valueData.changeMove(typeof(CameraMoveLineTween));
		CameraMoveData.valueData.setTargetPostion((GMath.stringToVector3(csvActive.initXYZ) + GMath.stringToVector3(csvActive.standInfo[0])) * 0.5f, csvActive.initCamera);
		CameraMoveData.valueData.setPostionReal();
	}
	/// 创建UI
	private static void createUI()
	{
		/// 左边显示
		SuperUI.show<UICombatOnlyTime>("GusUICombatOnlyTime", null);
		SuperUI.getUI<UICombatOnlyTime>().pivot = UIWidget.Pivot.TopLeft;

		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
		
//		SuperUI.show<UICombatOnlyFBInfo>("GusUICombatOnlyFBInfo", null);
//		
//		SuperUI.show<UICombatOnlyRewardBox>("GusUICombatOnlyRewardBox", null);
//		
//		SuperUI.show<UICombatOnlyChangeCamera>("GusUICombatOnlyChangeCamera", null);
		
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}
	/// 创建音效
	private static void createMusic()
	{
		if(csvActive.musicSence == "#" || string.IsNullOrEmpty(csvActive.musicSence))
			return;
		ManagerAudio.playMusic(csvActive.musicSence);
	}
	/// 创建数据
	private static void createData()
	{
		//		/// 清除卡牌信息
		//		DataMode.infoFBRewardList.clear();
		
		ManagerAudio.soundSameLength = 3;
		/// 是否在副本中
		DataMode.myPlayer.isInSunWell = true;
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		
		/// 提前取出音乐
		if(null != csvActive.musicSence && "#" != csvActive.musicSence)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvActive.musicSence));
		if(null != csvActive.musicCombatLast && "#" != csvActive.musicCombatLast)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvActive.musicCombatLast));
	}
}
