using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSenceAutoCombat : ManagerSence
{
	/// 我的指定ID信息
	private static int _idCsvCombatAuto;
	
	public static TypeCsvCombatAuto myCombatAuto{get{return ManagerCsv.getCombatAuto(_idCsvCombatAuto);}}
	
	/// 剧情
	public static int[] _STORY_SKILL = new int[]{2,3,4,5,6};
	public static int _STROY_STAR = 1;
	public static int _STROY_LOSE = 7;
	public static int _STROY_ENEMY_SKILL = 8001;
	
	/// 开始战斗
	public static void start(int idCsvCombatAuto)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		DataMode.infoSetting.isPause = false;
		if(null != DataMode.myPlayer)
			DataMode.myPlayer.isInAutoCombat = true;
		_idCsvCombatAuto = idCsvCombatAuto;
		/// 清除索引
		ManagerCombatData.clearIndex();
		/// 清空角色
		ManagerSYS.clear();
		/// 加载完csv文件信息,开始加载ui资源
		ManagerSence.createSence(myCombatAuto.urls, assetLoad);
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;

		/// add gus 2015.11.11 打点
		SDKBreakPoint.sendBreakPoint(SDKBreakPoint.PointType.GAME_AUTO_LOGIN, "");
	}	
	/// 副本 胜利
	public static void combatWin()
	{
		
		SuperUI.close("GusUICombatOnlyButton");
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyBeast");
		
		AICombat.showWin();
		
		ManagerAudio.playSound(ConfigUrl.UI_AUDIO_COMPLETE);
		
		UtilListener.calledTime(combatWinUI, 3f);
		
	}
	/// 出现胜利ui
	private static void combatWinUI()
	{
		DataMode.infoSetting.isPause = true;
		/// 爆卡吧
		UtilListener.dispatch("EventAnimationSpeed");
		/// 崩卡吧
		WindowsMngr.getInstance().showGetNewCard(null, 61, true);
		
	}
	/// 副本 失败
	public static void combatLost()
	{
		SuperUI.close("GusUICombatOnlyButton");
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyBeast");
		
		ManagerAudio.playSound(ConfigUrl.UI_AUDIO_COMPLETE);
		
		UtilListener.calledTime(combatWinUI, 3f);
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
		urls.Add(ConfigUrl.getAtlasUrl("guide"));
		urls.Add(ConfigUrl.getAtlasUrl("hero_touxiang"));
		urls.Add(ConfigUrl.getAudioUrl(ConfigUrl.UI_AUDOI_GET_NEW_CARD));
		urls.Add(ConfigUrl.getAudioUrl(ConfigUrl.UI_AUDOI_GET_NEW_CARD_F));
		WindowBase win = WindowsMngr.getInstance().viewMngr.root.uiWindows[WindowsID.SHOW_NEW_CARD];
		if(WindowsMngr.getInstance().widgetHolder.listWindowsUsingAtlas.ContainsKey(win.name))
		{
			List<string> load = (List<string>)WindowsMngr.getInstance().widgetHolder.listWindowsUsingAtlas[win.name];
			for(int i = 0; i < load.Count; i++)
			{
				urls.Add(ConfigUrl.getAtlasUrl(load[i]));
			}
		}

//		urls.Add(ConfigUrl.getOtherUrl("gnagrm_kcCamera_dh1"));
//		urls.Add(ConfigUrl.getOtherUrl("gnagrm_laoying_dh1"));
		
		/// 俩技能兰中的特效
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_01", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_02", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		
		/// 战斗中的特效
		EffMngr3.getInstance().getNoBindEff("effect_common_fire", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_monster_birth", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_shunyi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_rs_baoqi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		
		/// url 敌人
		ConfigUrl.getAssetsCombatEnemy(urls, int.Parse(myCombatAuto.idCombatEnemy[0]));
		ConfigUrl.getAssetsCombatEnemy(urls, int.Parse(myCombatAuto.idCombatEnemy[1]));
		
		/// 获得的卡牌玩意
		ConfigUrl.getAssetsCsvHero(urls, ManagerCsv.getHero(61));
		/// 副本中的各种玩意
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		
		
		/// TESTING 
		ConfigUrl.getAssetsStory(urls, _STROY_STAR);
		ConfigUrl.getAssetsStory(urls, _STROY_LOSE);
		ConfigUrl.getAssetsStory(urls, _STROY_ENEMY_SKILL);
		foreach(int idCsv in _STORY_SKILL)
		{
			ConfigUrl.getAssetsStory(urls, idCsv);
		}
		
		/// 加载guide中的资源
		urls.Add(ConfigUrl.getAtlasUrl("guide"));
		
		
		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 获得动画卡牌的资源
	private static void assetCartoon(List<string> urls, int idCsv)
	{
		while(null != ManagerCsv.getCartoon(idCsv))
		{
			TypeCsvCartoon csvCartoon = ManagerCsv.getCartoon(idCsv);
			TypeCsvView csvView = ManagerCsv.getView(csvCartoon.idView);
			
			// changed by ssy null exception
			if(csvView != null)
			{
				if(csvCartoon.type == 4)
				{
					EffMngr3.getInstance().getNoBindEff(csvView.url, null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
				} else {
					urls.Add(ConfigUrl.getAtlasUrl(csvView.iconAtlas));
				}
			}
			// changed end
			
			
			/// 下一个还是这个就是死循环
			if(idCsv == ManagerCsv.getCartoon(idCsv).idNext)
			{
				UtilLog.LogError("Cartoon.csv the idNext can't break;" + idCsv);
				break;
			}
			idCsv = ManagerCsv.getCartoon(idCsv).idNext;
		}
	}
	/// 加载成功
	private static void complete(double loader_id)
	{
		///数据挖掘 yxh 14.9.15
		if(ManagerServer.isLoadStatue && ManagerServer.isLoadStatueNew)
		{
			DataModeServer.sendLoadStatues(1,2);
		}
		/// 显示黑屏
		SuperUI.showNew<UIScreenBlack>("GusUIScreenBlack");
		
		/// 关闭loading条
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);

		playStoryMP4();
		/// 创建元素
		createHD();
	}
	/// 播放视频
	public static void playStoryMP4()
	{
#if UNITY_IPHONE || UNITY_ANDROID
//		Handheld.PlayFullScreenMovie("story.mp4", Color.black, FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit);
		Handheld.PlayFullScreenMovie("story.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);

		/// add gus 2015.11.11 打点
		SDKBreakPoint.sendBreakPoint(SDKBreakPoint.PointType.GAME_AUTO_CG, "");
#endif
	}

	/// 创建场景
	private static void createHD()
	{
		/// 创建相机动画
//		GameObject sAnim = (GameObject)GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getOtherUrl("gnagrm_kcCamera_dh1")), Vector3.zero, Quaternion.identity);
//		GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getOtherUrl("gnagrm_laoying_dh1")), Vector3.zero, Quaternion.identity);
//		CameraMoveData.valueData.camera.enabled = true;
//		CameraMoveData.valueData.transform.SetParent(sAnim.transform);
//		CameraMoveData.valueData.transform.localPosition = Vector3.zero;
//		CameraMoveData.valueData.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);


//		UtilListener.calledTime(waitAnimCreateHD, 17f);
		waitAnimCreateHD();
	}
	private static void waitAnimCreateHD()
	{
		CameraMoveData.valueData.transform.SetParent(null);
		ManagerSence.createLight();
		/// 可以提到上面去
		createData();
		createAICombat();


		createUI();
		createMusic();
		ManagerCombat.startAutoCombat();

		
		/// 设置剧情
		SuperUI.showNew<UIStory>("GusUIStory").setStory(ManagerCsv.getStory(_STROY_STAR));
	}
	
	/// 创建敌人
	private static void createAICombat()
	{
		/// 副本信息
		Vector3 postCenter1 = Vector3.zero;
		Vector3 postCenter2 = Vector3.zero;
		
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = null;

		/// 主角家的站位信息
		{
			TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(int.Parse(myCombatAuto.idCombatEnemy[0]));
			csvCombatStand = ManagerCsv.getCombatStand(csvCombatEnemy.idCombatStand);
			postCenter1 = GMath.stringToVector3(myCombatAuto.standInfo[0]);
			/// 生成角色
			for(int index = 0; index < 9; index++)
			{
				int idCsvHero = (int)csvCombatEnemy.GetType().GetField("index" + index).GetValue(csvCombatEnemy);
				if(idCsvHero <= 0)
					continue;
				Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + index).GetValue(csvCombatStand));
				post = post + postCenter1;
				AICombatData aiCombatData = ManagerCreate.createCombatEnemy(index, idCsvHero, csvCombatEnemy.lv, post, new Vector3(1f, 0f, 0f));
				/// 这个自动战斗，必须放技能初始cd
				aiCombatData.mySkillRelease[0].timeStampUnlock = Data.gameTime + aiCombatData.mySkillRelease[0].csvSkill.cdInit + 0.5f;
				_skill.Add(aiCombatData);
			}

		}
		/// 敌人的站位信息
		{
			TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(int.Parse(myCombatAuto.idCombatEnemy[1]));
			csvCombatStand = ManagerCsv.getCombatStand(csvCombatEnemy.idCombatStand);
			postCenter2 = GMath.stringToVector3(myCombatAuto.standInfo[1]);
			/// 生成角色
			for(int index = 0; index < 9; index++)
			{
				int idCsvHero = (int)csvCombatEnemy.GetType().GetField("index" + index).GetValue(csvCombatEnemy);
				if(idCsvHero <= 0)
					continue;
				Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + index).GetValue(csvCombatStand));
				post = post + postCenter2;
				ManagerCreate.createCombatEnemy(index + 9, idCsvHero, csvCombatEnemy.lv, post, new Vector3(-1f, 0f, 0f));
			}
		}
		/// 设置摄像机属性
		CameraMoveData.valueData.changeMove(typeof(CameraMovePK));
		CameraMoveData.valueData.setTargetPostion((postCenter2 + postCenter1) * 0.5f, myCombatAuto.initCamera);
		CameraMoveData.valueData.setPostionReal();

	}
	/// 暂停技能
	private static List<AICombatData> _skill = new List<AICombatData>();
	/// 创建UI
	private static void createUI()
	{		
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);		
		_skill.Sort(new IComparerAICombatByDistance());
		SuperUI.getUI<UICombatOnlySkill>().setSkillData(_skill);

		SuperUI.show<UICombatOnlyFBInfo>("GusUICombatOnlyFBInfo", null);
		
		SuperUI.show<UICombatOnlyRewardBox>("GusUICombatOnlyRewardBox", null);
		
		/// 显示副本的的标准ui
		WindowsMngr.getInstance().showDefaultFB();
		
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}
	/// 创建音效
	private static void createMusic()
	{
		/// 提前取出音乐
		if(null != myCombatAuto.musicSence && "#" != myCombatAuto.musicSence)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(myCombatAuto.musicSence));
		
		if(myCombatAuto.musicSence == "#" || string.IsNullOrEmpty(myCombatAuto.musicSence))
			return;
		ManagerAudio.playMusic(myCombatAuto.musicSence);
	}
	/// 创建数据
	private static void createData()
	{
		AICombat.combatType = 7;
		
		ManagerAudio.soundSameLength = 3;
		/// 清除卡牌信息
		DataMode.infoFBRewardList.clear();
		
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		
	}
}
