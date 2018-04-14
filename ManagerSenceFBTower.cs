using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSenceFBTower : ManagerSence
{
	/// 我的指定ID信息
	private static int _idCsvTower;
	private static int _idCsvFloor;
	
	public static TypeCsvFBTowerFloor myFloor{get{return ManagerCsv.getFBTowerFloor(_idCsvTower, _idCsvFloor);}}
	public static TypeCsvFBTower myTower{get{return ManagerCsv.getFBTower(_idCsvTower);}}
	
	/// 开始战斗
	public static void start(int idCsvTower, int idCsvFloor)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		DataMode.infoSetting.isPause = false;
		/// 清空角色
		ManagerSYS.clear();
		
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		_idCsvTower = idCsvTower;
		_idCsvFloor = idCsvFloor;
		/// 清除索引
		ManagerCombatData.clearIndex();
		
		// add by ssy 
		// clean mem before load 
		if(!DataMode.myPlayer.isInTower)
		{
			EffMngr3.getInstance().clearAllEff();
			LoadMngr.getInstance().cleanMemsAll();
			
		}
		// add end		
		
		/// 副本数据
		TypeCsvFBTowerFloor csvTowerFloor = ManagerCsv.getFBTowerFloor(_idCsvTower, _idCsvFloor);
		/// 加载完csv文件信息,开始加载ui资源
		ManagerSence.createSence(csvTowerFloor.urls, assetLoad);
		
		createData();
		
		
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
	private static void combatWinUI()
	{
		ManagerSYS.clearUI();
		WindowsMngr.getInstance().openWindow(WindowsID.WINALL, CombatWinAllWindow.Type.TOWER_WIN);	
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
		WindowsMngr.getInstance().openWindow(WindowsID.WINALL, CombatWinAllWindow.Type.TOWER_LOSE);	
	}
	/// 返回城镇按钮
	private static void backTownHD(GameObject sObj)
	{
		ManagerSenceTown.start(1, false);
		SuperUI.show<UITopHead>("GusUITopHead", null);
		SuperUI.show<UITopHeadIcon>("GusUITopHeadIcon", null);
//		WindowsMngr.getInstance().showTower();
		
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
		
		/// 俩技能兰中的特效
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_01", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_02", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		/// 战斗中的特效
		EffMngr3.getInstance().getNoBindEff("effect_common_fire", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_monster_birth", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_shunyi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_rs_baoqi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		
		/// 获得所有的敌人
		TypeCsvFBTowerFloor csvTowerFloor = ManagerCsv.getFBTowerFloor(_idCsvTower, _idCsvFloor);
		/// url 敌人
		ConfigUrl.getAssetsCombatEnemy(urls, csvTowerFloor.idCombatEnemy);
		/// 下载角色资源
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeam());
		/// 副本中的各种玩意
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 加载成功
	private static void complete(double loader_id)
	{
		ManagerSence.createLight();
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
//		createData();
		createUI();
		createMusic();
		createAICombat();
		ManagerCombat.startTowerCombat();
	}
	
	/// 创建敌人
	private static void createAICombat()
	{
		/// 副本信息
		TypeCsvFBTowerFloor csvTowerFloor = ManagerCsv.getFBTowerFloor(_idCsvTower, _idCsvFloor);
		Vector3 postCenter1 = Vector3.zero;
		Vector3 postCenter2 = Vector3.zero;
		
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvTowerFloor.idCombatStand);
		List<AICombatData> listSkill = new List<AICombatData>();
		/// 主角家的站位信息
		{
			/// 计算站位
			DataMode.myPlayer.infoHeroList.mathStandIndex(DataMode.myPlayer.infoHeroList.idTeamSelect);
			/// 我的出战团队
			List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam();
			postCenter1 = GMath.stringToVector3(csvTowerFloor.standInfo[0]);
			/// 生成角色
			foreach(InfoHero hero in infoHeroList)
			{
				Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
				post = post + postCenter1;
				AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex, hero.idServer, true, post, new Vector3(1f, 0f, 0f), infoHeroList, DataMode.myPlayer);
				listSkill.Add(aiCombat.GetComponent<AICombatData>());
			}
			SuperUI.getUI<UICombatOnlySkill>().setSkillData(listSkill);
			SuperUI.getUI<UICombatOnlyTime>().setTime(Config.GAME_COMBAT_TIME);
		}
		/// 敌人的站位信息
		{
			TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(csvTowerFloor.idCombatEnemy);
			csvCombatStand = ManagerCsv.getCombatStand(csvCombatEnemy.idCombatStand);
			postCenter2 = GMath.stringToVector3(csvTowerFloor.standInfo[1]);
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
		CameraMoveData.valueData.setTargetPostion((postCenter2 + postCenter1) * 0.5f, csvTowerFloor.initCamera);
		CameraMoveData.valueData.setPostionReal();
	}
	
	/// 创建UI
	private static void createUI()
	{
		
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
		
		SuperUI.show<UICombatOnlyTime>("GusUICombatOnlyTime", null);
		
		SuperUI.show<UICombatOnlyFBInfo>("GusUICombatOnlyFBInfo", null);
		
		SuperUI.show<UICombatOnlyRewardBox>("GusUICombatOnlyRewardBox", null);
		
		/// 显示副本的的标准ui
		WindowsMngr.getInstance().showDefaultFB();
		
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}
	/// 创建音效
	private static void createMusic()
	{
		
		/// 我这职业返回
		TypeCsvFBTowerFloor csvTowerFloor = ManagerCsv.getFBTowerFloor(_idCsvTower, _idCsvFloor);
		/// 提前取出音乐
		if(null != csvTowerFloor.musicSence && "#" != csvTowerFloor.musicSence)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvTowerFloor.musicSence));
		
		if(csvTowerFloor.musicSence == "#" || string.IsNullOrEmpty(csvTowerFloor.musicSence))
			return;
		ManagerAudio.playMusic(csvTowerFloor.musicSence);
	}
	/// 创建数据
	private static void createData()
	{
		ManagerAudio.soundSameLength = 3;
		/// 是否在副本中
		DataMode.myPlayer.isInTower = true;
		/// 清除卡牌信息
		DataMode.infoFBRewardList.clear();
		
	}
}
