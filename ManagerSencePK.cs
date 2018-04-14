using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSencePK 
{
	
	/// 和谁打
	private static uint _idServerPlayer;
	/// 在哪个副本打
	private static int _idCsvPK = 1;
	
	/// 获得参战的敌人
	public static InfoPlayer infoPlayer
	{
		get{return DataMode.getPlayer(_idServerPlayer);}
	}
	/// 开始战斗
	public static void start(uint idServerPlayer)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		DataMode.infoSetting.isPause = false;
		ManagerSYS.clear();
		
		_idServerPlayer = idServerPlayer;
		/// 创建场景
		ManagerSence.createSence(ManagerCsv.getPK(_idCsvPK).urls, assetLoad);
		/// 加载页面
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
	}
	/// 副本 胜利
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
		
		UtilListener.calledTime(combatWinUI, 3f);
		
	}
	private static void combatWinUI()
	{
		List<string> urls = new List<string>();
		urls.Add(ConfigUrl.getAtlasUrl("jingjichang"));
		SuperUI.show<UIPKResult>("GusUIPKResult", urls.ToArray());
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
		UtilListener.calledTime(combatWinUI, 3f);
	}
	
	
	/// 获得所有的角色坐标
	private static void assetLoad()
	{
		List<string> urls = new List<string>();
		
		urls.Add(ConfigUrl.getAtlasUrl("zhandou"));
		urls.Add(ConfigUrl.getAtlasUrl("hunshou"));
		urls.Add(ConfigUrl.getAtlasUrl("shuzi"));
		urls.Add(ConfigUrl.getAtlasUrl("skillicon"));
		urls.Add(ConfigUrl.getAtlasUrl("hero_touxiang"));
		
		/// 俩技能兰中的特效
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_01", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_02", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		/// 战斗中的特效
		EffMngr3.getInstance().getNoBindEff("effect_common_fire", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_monster_birth", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_shunyi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_rs_baoqi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		/// 下载角色资源
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeamPK());
		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelectPK));
		ConfigUrl.getAssetsServerHero(urls, DataMode.getPlayer(_idServerPlayer).infoHeroList.getTeamPK());
		ConfigUrl.getAssetsServerBeast(urls, DataMode.getPlayer(_idServerPlayer).infoHeroList.getTeamBeast(DataMode.getPlayer(_idServerPlayer).infoHeroList.idTeamSelectPK));
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// ====================================================================================================
	/// 
	/// 好吧,我开始打了
	private static void complete(double loader_id)
	{
		ManagerSence.createLight();
		createUI();
		createData();
		createMusic();
		createBeast();
		createAICombat();
		ManagerCombat.startPKCombat();
	}

	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		if(null != DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelectPK))
		{
			ManagerCombatBeast.beastSlef = ManagerCreate.createBeast(DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelectPK).idServer, 4);
			SuperUI.show<UICombatOnlyBeast>("GusUICombatOnlyBeast", null);
		}
		if(null != DataMode.getPlayer(_idServerPlayer).infoHeroList.getTeamBeast(DataMode.getPlayer(_idServerPlayer).infoHeroList.idTeamSelectPK))
		{
			ManagerCombatBeast.beastEnemy = ManagerCreate.createBeast(DataMode.getPlayer(_idServerPlayer).infoHeroList.getTeamBeast(DataMode.getPlayer(_idServerPlayer).infoHeroList.idTeamSelectPK).idServer, 13);
		}
	}
	/// 创建敌人
	private static void createAICombat()
	{
		/// 副本信息
		TypeCsvPK csvPK = ManagerCsv.getPK(_idCsvPK);
		Vector3 postCenter1 = Vector3.zero;
		Vector3 postCenter2 = Vector3.zero;
		
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(GMath.toInt(csvPK.idCombatStand[0]));
		List<AICombatData> listSkill = new List<AICombatData>();
		/// 主角家的站位信息
		{
			/// 计算站位
			DataMode.myPlayer.infoHeroList.mathStandIndex(DataMode.myPlayer.infoHeroList.idTeamSelectPK);
			/// 我的出战团队
			List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeamPK();
			postCenter1 = GMath.stringToVector3(csvPK.standInfo[0]);
			/// 生成角色
			foreach(InfoHero hero in infoHeroList)
			{
				Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
				post = post + postCenter1;
				AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex, hero.idServer, true, post, new Vector3(1f, 0f, 0f), infoHeroList, DataMode.myPlayer);
				
				aiCombat.GetComponent<AICombatData>().hp = aiCombat.GetComponent<AICombatData>().hp * ManagerCsv.getAttribute().combatPKHpTotalMul;
				aiCombat.GetComponent<AICombatData>().hpTotal = aiCombat.GetComponent<AICombatData>().hpTotal * ManagerCsv.getAttribute().combatPKHpTotalMul;

				ManagerCombatBeast.addBuffBeast(aiCombat.GetComponent<AICombatData>());

				listSkill.Add(aiCombat.GetComponent<AICombatData>());
			}
			SuperUI.getUI<UICombatOnlySkill>().setSkillData(listSkill);
			SuperUI.getUI<UICombatOnlyTime>().setTime(Config.GAME_COMBAT_TIME);
		}
		/// 敌人家的站位信息
		{
			csvCombatStand = ManagerCsv.getCombatStand(GMath.toInt(csvPK.idCombatStand[1]));
			InfoPlayer player = DataMode.getPlayer(_idServerPlayer);
			/// 计算站位
			player.infoHeroList.mathStandIndex(player.infoHeroList.idTeamSelectPK);
			/// 我的出战团队
			List<InfoHero> infoHeroList = player.infoHeroList.getTeamPK();
			postCenter2 = GMath.stringToVector3(csvPK.standInfo[1]);
			/// 生成角色
			foreach(InfoHero hero in infoHeroList)
			{
				Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
				post = post + postCenter2;
				AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex + 9, hero.idServer, false, post, new Vector3(-1f, 0f, 0f), infoHeroList, player);
				
				aiCombat.GetComponent<AICombatData>().hp = aiCombat.GetComponent<AICombatData>().hp * ManagerCsv.getAttribute().combatPKHpTotalMul;
				aiCombat.GetComponent<AICombatData>().hpTotal = aiCombat.GetComponent<AICombatData>().hpTotal * ManagerCsv.getAttribute().combatPKHpTotalMul;
			
				ManagerCombatBeast.addBuffBeast(aiCombat.GetComponent<AICombatData>());
			}
		}
		/// 设置摄像机属性
		CameraMoveData.valueData.changeMove(typeof(CameraMovePK));
		CameraMoveData.valueData.setTargetPostion((postCenter2 + postCenter1) * 0.5f, csvPK.initCamera);
		CameraMoveData.valueData.setPostionReal();
	}
	/// 创建ui
	private static void createUI()
	{
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		/// WILL CLOSE
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
		
		SuperUI.show<UICombatOnlyTime>("GusUICombatOnlyTime", null);
		
		SuperUI.show<UICombatOnlyPKName>("GusUICombatOnlyPKName", null);
		
		
		/// 关闭加载条
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}
	/// 创建数据
	private static void createData()
	{
		DataMode.myPlayer.isInPK = true;
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		ManagerAudio.soundSameLength = 3;
	}
	/// 创建音乐
	private static void createMusic()
	{
		TypeCsvPK csvPK = ManagerCsv.getPK(_idCsvPK);
		if(csvPK.musicSence == "#" || string.IsNullOrEmpty(csvPK.musicSence))
			return;
		ManagerAudio.playMusic(csvPK.musicSence);
	}
	
}
