using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSenceFB
{
	/// 我所在的副本信息
	private static int _idCsvFB;
	public static TypeCsvFB csvFB{get{return ManagerCsv.getFB(_idCsvFB);}}
	/// 召唤进来的基友
	private static InfoHero _infoHeroFriend = null;
	public static InfoHero infoHeroFriend{get{return _infoHeroFriend;}}
	public static AICombatData showBoss; 
	
	/// 是否有剧情模式
	private static bool _hasStory = false;
	public static bool hasStory
	{
		get
		{
			/// WILL CLOSE
			return _hasStory;
//			return true;
		}
	}
	
	/// 开始副本加载选项
	public static void start(int sIdCsvFB)
	{
		start(sIdCsvFB, 0);
	}
	public static void start(int sIdCsvFB, ulong sIdServerHeroFriend)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		DataMode.infoSetting.isPause = false;
		/// 我的好友
		_infoHeroFriend = DataMode.getHero(sIdServerHeroFriend);
		/// fb id引用
		_idCsvFB = sIdCsvFB;
		/// 我这职业返回
		DataModeServer.sendFBCombatReward(recvReward);
	}
	/// 协议返回
	private static void recvReward(UtilListenerEvent sEvent)
	{
		/// 清除索引
		ManagerCombatData.clearIndex();
		/// 清空角色
		ManagerSYS.clear();
		/// 副本数据
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		
		// add by ssy 
		// clean mem before load 
		if(!DataMode.myPlayer.isInFB)
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
		ManagerSence.createSence(csvFB.urls, completeSence);



		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;

		/// 创建天气效果
		ManagerSence.createWeather(csvFB.weather);
	}
	
	/// 副本 胜利
	public static void combatWin()
	{
		/// 波次胜利
		AIEnemy.setEnemyNext();
		/// 清除计时器
		SuperUI.close("GusUICombatOnlyTime");
		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		/// 开启角色控制
		AIPlayer.valueObject.lockControl = false;
		
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
			TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
			if(csvFB.musicCombatLast == "#" || string.IsNullOrEmpty(csvFB.musicCombatLast))
				return;
			ManagerAudio.playMusic(csvFB.musicCombatLast);
		}
		
	}
	/// 副本 失败
	public static void combatLost()
	{
		SuperUI.close("GusUICombatOnlyButton");
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyTime");
		SuperUI.close("GusUICombatOnlyBeast");
	
		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		/// 展示失败页面
		WindowsMngr.getInstance().showCombatResult(WindowsMngr.combatType.FB_LOSE);
	}
	/// 副本 通关
	public static void combatComplete()
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
	
	/// 功能函数====================================================================================================
	
	/// 完成场景加载 ---- 下载角色
	private static void completeSence()
	{
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		List<string> urls = new List<string>();
		/// url 敌人
		ConfigUrl.getAssetsFBEnemy(urls, csvFB);
		/// url 主角
		/// 我的出战团队
		List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam();
		/// 添加进去基友
		if(null != _infoHeroFriend)
			infoHeroList.Add(_infoHeroFriend);
		ConfigUrl.getAssetsServerHero(urls, infoHeroList);
		/// 加载魂兽资源
		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelect));

		/// url 副本必须要的玩意
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		/// url 副本必须要下载的技能ui啊,各种ui啊
		getAssetsUrlUI(urls);
		/// 剧情预加载管理
		{
			switch(csvFB.type)
			{
				/// 普通
			case 1:
				_hasStory = !DataMode.myPlayer.infoFBList.getFBInfoByCsv(csvFB.id).isComplete; 
				break;
				/// 精英
			case 2:
				_hasStory = !DataMode.myPlayer.infoFBListElit.getFBInfoByCsv(csvFB.id).isComplete; 
				break;
				/// 挑战
			case 3:
				_hasStory = !DataMode.myPlayer.infoFBListChallenge.getFBInfoByCsv(csvFB.id).isComplete; 
				break;
			}
			/// 加载资源
			if(hasStory)
			{
				
				if(csvFB.storyIn != -1)
					ConfigUrl.getAssetsStory(urls, csvFB.storyIn);
				if(csvFB.storyWin != -1)
					ConfigUrl.getAssetsStory(urls, csvFB.storyWin);
				if(null != csvFB.storyCombatBefor)
				{
					for(int index = 0; index < csvFB.storyCombatBefor.Length; index++)
					{
						if("#" != csvFB.storyCombatBefor[index])
							ConfigUrl.getAssetsStory(urls, GMath.toInt(csvFB.storyCombatBefor[index]));
					}
				}
			}
		}
		/// 落地音效
		urls.Add(ConfigUrl.getAudioUrl("SFX_Role10"));		
		/// 下载
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 获得ui url
	private static void getAssetsUrlUI(List<string> urls)
	{
		urls.Add(ConfigUrl.getAtlasUrl("zhandou"));
		urls.Add(ConfigUrl.getAtlasUrl("hunshou"));
		urls.Add(ConfigUrl.getAtlasUrl("shuzi"));
		urls.Add(ConfigUrl.getAtlasUrl("skillicon"));
		urls.Add(ConfigUrl.getAtlasUrl("hero_touxiang"));
		urls.Add(ConfigUrl.getAtlasUrl("gezi"));
		urls.Add(ConfigUrl.getAtlasUrl("guide"));
		DataMode.getFBRewardURL(urls);

		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		if(null != csvFB.musicSence && "#" != csvFB.musicSence)
			urls.Add(ConfigUrl.getAudioUrl(csvFB.musicSence));
		if(null != csvFB.musicCombatLast && "#" != csvFB.musicCombatLast)
			urls.Add(ConfigUrl.getAudioUrl(csvFB.musicCombatLast));
	}
	/// #############################################################################################################################
	/// 
	/// 加载完成后,进行初期启动
	
	/// 启动游戏
	private static void complete(double loader_id)
	{
		ConfigUrl.useAllAtlas();
		createData();
		
		ManagerSence.createLight();
		
		createBeast();

		createEnemy();
		createUI();
		createMusic();
		createPlayer();

		//add by yxh
		if(_leadId >= 0 && showLead != null)
		{
			showLead(null);
		}
	}
	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		if(null != DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelect))
		{
			ManagerCombatBeast.beastSlef = ManagerCreate.createBeast(DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelect).idServer, 4);
			SuperUI.show<UICombatOnlyBeast>("GusUICombatOnlyBeast", null);
		}
		ManagerCombatBeast.beastEnemy = null;
	}
	/// 创建敌人  在看不见的地方
	private static void createEnemy()
	{
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		AIEnemy.createEnemys(csvFB.idCombatEnemy.Length);
		showBoss = null;
		for(int index = 0; index < csvFB.idCombatEnemy.Length; index++)
		{
			ManagerCreate.createEnemy(index, GMath.toInt(csvFB.idCombatEnemy[index]), GMath.stringToVector3(csvFB.standInfo[index]));
			showBoss = ManagerCombat.createCombatEnemyHide(GMath.toInt(csvFB.idCombatEnemy[index]), GMath.stringToVector3(csvFB.standInfo[index]), index);
		}
	}
	/// 创建主角   在看不见的地方
	private static void createPlayer()
	{
		/// 副本信息
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvFB.idCombatStand);
		/// 创建主角
		AIPlayerFB player = ManagerCreate.createPlayerFB(1, GMath.stringToVector3(csvFB.initXYZ));
		AIPlayer.valueObject = player;
		/// 设置摄像机属性
		CameraMoveData.valueData.changeMove(typeof(CameraMoveLineTween));
		
		
		CameraMoveData.valueData.setTarget(player.transform, csvFB.initCamera);
		CameraMoveData.valueData.setRotationLerp(DataMode.infoSetting.cameraLerpFB);
		CameraMoveData.valueData.setPostionReal();
		
		/// 如果主角不是空的 创建有效的随从
		if(null != DataMode.myPlayer)
		{
			/// 计算站位
			DataMode.myPlayer.infoHeroList.mathStandIndex(DataMode.myPlayer.infoHeroList.idTeamSelect, _infoHeroFriend);
			/// 我的出战团队
			List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam();

			/// 添加进去基友
			if(null != _infoHeroFriend)
				infoHeroList.Add(_infoHeroFriend);
			
			float time = 0.5f;

			/// 生成角色
			foreach(InfoHero hero in infoHeroList)
			{
				ManagerSYS.clearListener.Add(UtilListener.calledTime(createPlayerDownHD, time - 0.5f, hero, infoHeroList));
				ManagerSYS.clearListener.Add(UtilListener.calledTime(createPlayerHD, time, hero, infoHeroList));
				time += 0.2f;
			}
//			time += 0.4f;
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
				/// 2015.11.02 gus
				ManagerCombatBeast.addBuffBeast(combatData);
			}
		}
		/// 显示队长技能
		if(null != SuperUI.getUI<UICombatOnlySkill>())
			SuperUI.getUI<UICombatOnlySkill>().setSkillData(listCombatData);
		
		/// 这个时候出剧情真的合适么？
		/// 是否显示剧情
		if(hasStory)
			/// 是否有进入副本剧情
			if(csvFB.storyIn > 0)
				SuperUI.showNew<UIStory>("GusUIStory").setStory(ManagerCsv.getStory(csvFB.storyIn));
	}
	/// 多长时间的事情
	private static void createPlayerDownHD(UtilListenerEvent sEvent)
	{
		/// 副本信息
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvFB.idCombatStand);
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
		/// 副本信息
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvFB.idCombatStand);
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
			!DataMode.myPlayer.infoHeroList.isInTeamNormal(hero.idServer),
			infoHeroList,
			DataMode.myPlayer);
		/// 落地特效
		ManagerCreate.createEffectByName("effect_common_role_born_smoke", standOffset + player.transform.position, Quaternion.identity);
		
		CameraMoveData.valueData.setShake(0.2f, 0.1f);
		/// 落地音效
		ManagerAudio.playSound("SFX_Role10");
		/// 2015.11.02 注释掉
//		ManagerCombatBeast.addBuffBeast(combatData);
		/// add by gus 2015.7.28
		if(infoHeroList.IndexOf(hero) >= infoHeroList.Count - 1)
			createSkillUI();
	}
	
	/// 创建UI
	private static void createUI()
	{
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
		
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
		
		SuperUI.show<UICombatOnlyFBInfo>("GusUICombatOnlyFBInfo", null);
		
		SuperUI.show<UICombatOnlyRewardBox>("GusUICombatOnlyRewardBox", null);
		
		/// 判断第一个板块 第四个副本中的引导
		if(DataMode.myPlayer.infoFBList.getFBInfoByCsv(5) != null)
			SuperUI.show<UICombatOnlyChangeCamera>("GusUICombatOnlyChangeCamera", null);
		
	}
	/// 创建音效
	private static void createMusic()
	{
		TypeCsvFB csvFB = ManagerCsv.getFB(_idCsvFB);
		/// 提前取出音乐
		if(null != csvFB.musicSence && "#" != csvFB.musicSence)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvFB.musicSence));
		if(null != csvFB.musicCombatLast && "#" != csvFB.musicCombatLast)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvFB.musicCombatLast));
		
		if(csvFB.musicSence == "#" || string.IsNullOrEmpty(csvFB.musicSence))
			return;
		ManagerAudio.playMusic(csvFB.musicSence);
	}
	/// 创建数据
	private static void createData()
	{
		ManagerAudio.soundSameLength = 3;
		/// 是否在副本中
		DataMode.myPlayer.isInFB = true;
//		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
	}
	
	/// 引导 add by yxh
	private static int _leadId = -1;
	private static UIEventListener.VoidDelegate showLead;
	public static void LeadSystem(int id,UIEventListener.VoidDelegate show)
	{
		_leadId = id;
		showLead = show;
	}
}

