using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSenceActive : ManagerSence
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
		if(!DataMode.myPlayer.isInActive)
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
			SuperUI.close("GusUICombatOnlyBeast");




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
		SuperUI.close("GusUICombatOnlyBeast");
		SuperUI.close("GusUICombatOnlyTime");

		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		/// 展示失败页面
		WindowsMngr.getInstance().showCombatResult(WindowsMngr.combatType.FB_LOSE);
	}
	/// 副本 通关
	private static void combatComplete()
	{
		/// 展示通关页面
		WindowsMngr.getInstance().showCombatResult(WindowsMngr.combatType.FB_WIN);
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
		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeastByType(ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType));
		
		/// 副本中的各种玩意
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		
		
		urls.Add(ConfigUrl.getAudioUrl("SFX_Role10"));
		
		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 加载成功
	private static void complete(double loader_id)
	{
		createData();
		
		createUI();
		createBeast();

		createEnemy();
		createMusic();
		createPlayer();
		ManagerSence.createLight();
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
			ManagerCreate.createEnemy(index, infoActive.idCsvCombatEnemy[index], GMath.stringToVector3(csvActive.standInfo[index]));
			ManagerCombat.createCombatEnemyHide(infoActive.idCsvCombatEnemy[index], GMath.stringToVector3(csvActive.standInfo[index]), index);
		}
	}
	/// 创建主角   在看不见的地方
	private static void createPlayer()
	{
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvActive.idCombatStand);
		/// 创建主角
		AIPlayerFB player = ManagerCreate.createPlayerFB(1, GMath.stringToVector3(csvActive.initXYZ));
		AIPlayer.valueObject = player;
		/// 设置摄像机属性
		CameraMoveData.valueData.changeMove(typeof(CameraMoveLineTween));
		
		CameraMoveData.valueData.setTarget(player.transform, csvActive.initCamera);
//		CameraMoveData.valueData.setRotationLerp(0);
		CameraMoveData.valueData.setRotationLerp(DataMode.infoSetting.cameraLerpFB);
		CameraMoveData.valueData.setPostionReal();
		
		/// 如果主角不是空的 创建有效的随从
		if(null != DataMode.myPlayer)
		{
			/// 计算站位
			DataMode.myPlayer.infoHeroList.mathStandIndex(DataMode.myPlayer.infoHeroList.teamList[ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType], infoHeroFriend);
			/// 我的出战团队
			List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeamByTeamType(ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType);
			/// 添加进去基友
			if(null != infoHeroFriend)
				infoHeroList.Add(infoHeroFriend);
			
			float time = 0.5f;
			/// 生成角色
			foreach(InfoHero hero in infoHeroList)
			{
				ManagerSYS.clearListener.Add(UtilListener.calledTime(createPlayerDownHD, time - 0.5f, hero, infoHeroList));
				ManagerSYS.clearListener.Add(UtilListener.calledTime(createPlayerHD, time, hero, infoHeroList));
				time += 0.2f;
			}
//			UtilListener.calledTime(createSkillUI, time);
		}
	}
	/// 创建技能
	private static void createSkillUI()
	{
		UnityEngine.Object[] followers = GameObject.FindObjectsOfType(typeof(AIFollower));
		List<AICombatData> listCombatData = new List<AICombatData>();
		/// 转换控制器
		foreach(UnityEngine.Object obj in followers)
		{
			AIFollower follower = (AIFollower)obj;
			AICombatData combatData = follower.GetComponent<AICombatData>();
			if(combatData.standIndex < 9)
			{
				listCombatData.Add(combatData);
				/// 2015.11.02 gus 添加
				ManagerCombatBeast.addBuffBeast(combatData);
			}
		}
		/// 显示队长技能
		if(null != SuperUI.getUI<UICombatOnlySkill>())
			SuperUI.getUI<UICombatOnlySkill>().setSkillData(listCombatData);
	}
	/// 多长时间的事情
	private static void createPlayerDownHD(UtilListenerEvent sEvent)
	{
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvActive.idCombatStand);
		/// 创建主角
		AIPlayer player = AIPlayer.valueObject;
		/// 角色
		InfoHero hero = (InfoHero)sEvent.eventTarget;
		/// 偏移
		Vector3 standOffset = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
		List<Vector3> postionPath = new List<Vector3>();
		postionPath.Add(standOffset + player.transform.position + new Vector3(-3f, 8f, 0f));
		postionPath.Add(standOffset + player.transform.position);
		/// 下落特效
		ManagerCreate.createEffectByNameMove("effect_common_role_born_line_head", postionPath, Quaternion.identity, 0.5f);
	}
	/// 创建角色
	private static void createPlayerHD(UtilListenerEvent sEvent)
	{
		if(null == AIPlayer.valueObject || !(AIPlayer.valueObject is AIPlayerFB))
			return;
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvActive.idCombatStand);
		/// 创建主角
		AIPlayer player = AIPlayer.valueObject;
		/// 角色
		InfoHero hero = (InfoHero)sEvent.eventTarget;
		List<InfoHero> infoHeroList = (List<InfoHero>)sEvent.eventArgs;
		/// 创建角色
		Vector3 standOffset = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
		AICombatData combatData = ManagerCreate.createFollower(hero.standIndex,
			hero.idServer,
			player.gameObject,
			standOffset,
			new Vector3(1f, 0f, 0f),
			!DataMode.myPlayer.infoHeroList.isInTeam((int)DataMode.myPlayer.infoHeroList.teamList[ManagerCsv.getCombatEnemy(infoActive.idCsvCombatEnemy[0]).teamType], hero.idServer),
			infoHeroList,
			DataMode.myPlayer);

		/// 2015.11.02 gus 注释掉
//		ManagerCombatBeast.addBuffBeast(combatData);

		/// 落地特效
		ManagerCreate.createEffectByName("effect_common_role_born_smoke", standOffset + player.transform.position, Quaternion.identity);
		
		CameraMoveData.valueData.setShake(0.2f, 0.1f);
		/// 落地音效
		ManagerAudio.playSound("SFX_Role10");

		/// add by gus 2015.7.28
		if(infoHeroList.IndexOf(hero) >= infoHeroList.Count - 1)
			createSkillUI();
	}
	/// 创建UI
	private static void createUI()
	{
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
		
		SuperUI.show<UICombatOnlyFBInfo>("GusUICombatOnlyFBInfo", null);
		
		SuperUI.show<UICombatOnlyRewardBox>("GusUICombatOnlyRewardBox", null);
		
		SuperUI.show<UICombatOnlyChangeCamera>("GusUICombatOnlyChangeCamera", null);
		
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
		DataMode.myPlayer.isInActive = true;
//		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		
		/// 提前取出音乐
		if(null != csvActive.musicSence && "#" != csvActive.musicSence)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvActive.musicSence));
		if(null != csvActive.musicCombatLast && "#" != csvActive.musicCombatLast)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvActive.musicCombatLast));
	}
}
