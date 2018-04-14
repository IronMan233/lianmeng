using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 海山 奖励关卡
public class ManagerSenceTBCReward
{
	/// 获得 索引
	private static int _index;
	/// 获得 敌人 角色
	public static InfoHero infoHeroEnemy{get{return DataMode.getHero(DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(_index - 1).idServer);}}
	/// 获得 场景 地图
	public static TypeCsvMotMap csvMotMap{get{return ManagerCsv.getMotMap(DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(_index - 1).idSence);}}

	/// 基友信息
	private static ulong _idServerFriend;
	public static InfoPlayer infoPlayerFriend{get{return DataMode.getGUArmPlayer(_idServerFriend);}}
	public static InfoHero infoHeroFriend{get{return DataMode.getGUArmHero(_idServerFriend);}}
	/// 打第几个奖励关卡
	public static void start(int sIndex, ulong sIDServerFriend)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		_idServerFriend = sIDServerFriend;
		_index = sIndex;

		/// 进入海山守护关卡
		DataModeServer3.sendTBCRewardIn(sIndex, recvTBCRewardInHD);
	}
	/// 返回进去tbc战场
	private static void recvTBCRewardInHD(UtilListenerEvent sEvent)
	{
		createData();
		ManagerSYS.clear();
		ManagerSence.createSence(csvMotMap.urls, completeAssetCsv);
		/// 加载页面
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
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
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.idTeamTBCReward));
		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamTBCReward));
		/// 防守的人物魂兽
		ConfigUrl.getAssetsServerHero(urls, new List<InfoHero>(new InfoHero[]{infoHeroEnemy}));
		/// 下载基友信息
		if(null != infoHeroFriend)
			ConfigUrl.getAssetsServerHero(urls, new List<InfoHero>(new InfoHero[]{infoHeroFriend}));
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
	
	/// 创建数据以及一些其他===========================================================================================
	/// 下载完所有资源
	private static void complete(double id)
	{
		createUI();
		createBeast();
		createHero();
		createEnemy();
		createCamera();
		createMusic();
		ManagerCombat.startTBCRewardCombat();
		/// 加载页面
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}
	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		if(null != DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamTBCReward))
		{
			ManagerCombatBeast.beastSlef = ManagerCreate.createBeast(DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamTBCReward).idServer, 4);
			SuperUI.show<UICombatOnlyBeast>("GusUICombatOnlyBeast", null);
		}
	}
	/// 创建 数据
	private static void createData()
	{
		DataMode.myPlayer.isInTBCReward = true;
		ManagerAudio.soundSameLength = 3;
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		
	}
	/// 创建 UI
	private static void createUI()
	{
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
//		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
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
		List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.idTeamTBCReward);
		/// 添加我的好友进入队伍
		if(null != infoHeroFriend)
			infoHeroList.Add(infoHeroFriend);
		/// 主角家的站位信息
		/// 计算站位
		DataMode.myPlayer.infoHeroList.mathStandIndexArr(infoHeroList);
		
		postCenter = GMath.stringToVector3(csvMotMap.standInfo[0]);
		/// 生成角色
		foreach(InfoHero hero in infoHeroList)
		{
			Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
			post = post + postCenter;
			AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex, hero.idServer, true, post, new Vector3(1f, 0f, 0f), infoHeroList, DataMode.myPlayer);
			AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
			ManagerCombatBeast.addBuffBeast(aicombatData);
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
		/// 主角家的站位
		infoHeroEnemy.standIndex = 4;
		postCenter = GMath.stringToVector3(csvMotMap.standInfo[1]);
		/// 生成角色
		Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + infoHeroEnemy.standIndex).GetValue(csvCombatStand));
		post = post + postCenter;
		AICombat aiCombat = ManagerCreate.createCombatHero(infoHeroEnemy.standIndex + 9, infoHeroEnemy.idServer, false, post, new Vector3(-1f, 0f, 0f), null, null);
		aiCombat.transform.localScale *= ManagerCsv.getAttribute().motPkHeroScale;

		AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
		ManagerCombatBeast.addBuffBeast(aicombatData);
		mathTBCEnemyAttri(aicombatData);
		mathTBCPlayerBuff(aicombatData);
	}


	/// 远征敌人系数
	private static void mathTBCEnemyAttri(AICombatData sCombatData)
	{

		TypeCsvMotEnemyAttri csvMotAttri = ManagerCsv.getMotEnemyAttri(100 + _index);
		if(null != csvMotAttri)
		{
			DataAttCount addCount = new DataAttCount();
			addCount.agi = sCombatData.myCombatCount.agi * (csvMotAttri.agiMul - 1);
			addCount.str = sCombatData.myCombatCount.str * (csvMotAttri.strMul - 1);
			addCount.intell = sCombatData.myCombatCount.intell * (csvMotAttri.intellMul - 1);
			addCount.vit = sCombatData.myCombatCount.vit * (csvMotAttri.vitMul - 1);
			
			addCount.hp = sCombatData.myCombatCount.hp * (csvMotAttri.hpMul - 1);
			addCount.atk = sCombatData.myCombatCount.atk * (csvMotAttri.atkMul - 1);
			addCount.atkDef = sCombatData.myCombatCount.atkDef * (csvMotAttri.atkDefMul - 1);
			addCount.magic = sCombatData.myCombatCount.magic * (csvMotAttri.magicMul - 1);
			addCount.magicDef = sCombatData.myCombatCount.magicDef * (csvMotAttri.magicDefMul - 1);
			
			/// 计算属性换算
			DataAttCount.mathHeroAttributeMath(addCount);
			/// 追加到属性当中
			DataAttCount.addData(sCombatData.myCombatCount, addCount);
			/// 生命总值重新获得
			sCombatData.hp = sCombatData.hpTotal = sCombatData.myCombatCount.hp;
		}
	}
	/// 获得远征buff
	private static void mathTBCPlayerBuff(AICombatData sCombatData)
	{
		TypeCsvMotBuff csvMotBuff = ManagerCsv.getMotBuff(DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(_index - 1).selectBuffer);
		if(null != csvMotBuff)
		{
			if(UtilLog.isBulidLog)UtilLog.Log("Use MotBuff For DEFEnemy ID = " + csvMotBuff.id);
			/// 创建buff属性的追加值
			DataAttCount addCount = new DataAttCount();
			addCount.agi = sCombatData.myCombatCount.agi * (csvMotBuff.agiMul - 1);
			addCount.str = sCombatData.myCombatCount.str * (csvMotBuff.strMul - 1);
			addCount.intell = sCombatData.myCombatCount.intell * (csvMotBuff.intellMul - 1);
			addCount.vit = sCombatData.myCombatCount.vit * (csvMotBuff.vitMul - 1);
			
			addCount.hp = sCombatData.myCombatCount.hp * (csvMotBuff.hpMul - 1);
			addCount.atk = sCombatData.myCombatCount.atk * (csvMotBuff.atkMul - 1);
			addCount.atkDef = sCombatData.myCombatCount.atkDef * (csvMotBuff.atkDefMul - 1);
			addCount.magic = sCombatData.myCombatCount.magic * (csvMotBuff.magicMul - 1);
			addCount.magicDef = sCombatData.myCombatCount.magicDef * (csvMotBuff.magicDefMul - 1);
			/// 计算属性换算
			DataAttCount.mathHeroAttributeMath(addCount);
			/// 追加到属性当中
			DataAttCount.addData(sCombatData.myCombatCount, addCount);
			/// 生命总值重新获得
			sCombatData.hp += (sCombatData.myCombatCount.hp - sCombatData.hpTotal);
			sCombatData.hpTotal = sCombatData.myCombatCount.hp;
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
