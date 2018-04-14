using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 燃烧的远征。。。。。
public class ManagerSenceTBC : MonoBehaviour
{
	public static int idTBC;
	public static TypeCsvMot csvMot{get{return ManagerCsv.getMot(idTBC);}}
	public static TypeCsvMotMap csvMotMap{get{return ManagerCsv.getMotMap(DataMode.infoTBC.GetInfoEnemyTeam().idSence);}}
	public static ulong idServerHero;
	public static uint idServerPlayer
	{
		get
		{
			if(null != DataMode.getGUArm(idServerHero))
				return DataMode.getGUArm(idServerHero).idServerPlayer;
			return 0;
		}
	}
	/// 远征挑战 开始
	public static void start(ulong sIDServerHero)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
		{
			WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
			return;
		}
		DataMode.infoSetting.isPause = false;
		idServerHero = sIDServerHero;
		idTBC = DataMode.infoTBC.id;
		DataModeServer2.sendTBCIn(recvTBCInHD);
	}
	/// 返回进去tbc战场
	private static void recvTBCInHD(UtilListenerEvent sEvent)
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
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.idTeamTBC));
		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamTBC));

		ConfigUrl.getAssetsServerHero(urls, new List<InfoHero>(DataMode.infoTBC.infoTBCTeam[idTBC].teams));
		ConfigUrl.getAssetsServerBeast(urls, DataMode.infoTBC.infoTBCTeam[idTBC].infoBeast);

		if(idServerHero > 0)
			ConfigUrl.getAssetsServerHero(urls, new List<InfoHero>(new InfoHero[]{DataMode.getHero(idServerHero)}));
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
//		CameraMoveData.valueData.changeMove(typeof(CameraMoveCombatWin));
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
		ManagerCombat.startTBCCombat();
		/// 加载页面
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
	}
	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		if(null != DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamTBC))
		{
			ManagerCombatBeast.beastSlef = ManagerCreate.createBeast(DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamTBC).idServer, 4);
			SuperUI.show<UICombatOnlyBeast>("GusUICombatOnlyBeast", null);
			if(null != DataMode.getTBCBeast(ManagerCombatBeast.beastSlef.infoBeast.idServer))
			{
				ManagerCombatBeast.beastSlef.anger = ManagerCombatBeast.beastSlef.dataAttCount.angerTotal * 
					DataMode.getTBCBeast(ManagerCombatBeast.beastSlef.infoBeast.idServer).angerMul;
			}
		}
		if(null != DataMode.infoTBC.infoTBCTeam[idTBC].getTeamBeast())
		{
			ManagerCombatBeast.beastEnemy = ManagerCreate.createBeast(DataMode.infoTBC.infoTBCTeam[idTBC].getTeamBeast().idServer, 13);
			if(null != DataMode.getTBCBeast(ManagerCombatBeast.beastEnemy.infoBeast.idServer))
			{
				ManagerCombatBeast.beastEnemy.anger = ManagerCombatBeast.beastEnemy.dataAttCount.angerTotal * 
					DataMode.getTBCBeast(ManagerCombatBeast.beastEnemy.infoBeast.idServer).angerMul;
			}
		}
	}
	/// 创建 数据
	private static void createData()
	{
		DataMode.myPlayer.isInTBC = true;
		ManagerAudio.soundSameLength = 3;
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		
	}
	/// 创建 UI
	private static void createUI()
	{
		/// 显示技能条
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);
//		SuperUI.show<UICombatOnlyRewardBox>("GusUICombatOnlyRewardBox", null);
		SuperUI.show<UICombatOnlyTime>("GusUICombatOnlyTime", null);
		
		
		TypeCsvMotBuff csvMotBuff = ManagerCsv.getMotBuff(DataMode.infoTBC.infoTBCTeam[idTBC].GetBuffer().selectBuffer);
		if(null != csvMotBuff)
			SuperUI.showNew<UITBCBuff>("GusCreateUITBCBuff").setText(ConfigLabel.EXPEDITION_BUFF_INFO + csvMotBuff.info, 2f);
	}
	
	/// 创建 主角阵营数据
	private static void createHero()
	{
		Vector3 postCenter = Vector3.zero;
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(GMath.toInt(csvMotMap.idCombatStand[0]));
		List<AICombatData> listSkill = new List<AICombatData>();
		/// 我的出战团队
		List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.idTeamTBC);
		/// 添加我的好友进入队伍
		if(null != DataMode.getHero(idServerHero))
			infoHeroList.Add(DataMode.getHero(idServerHero));
		/// 主角家的站位信息
		/// 计算站位
		DataMode.myPlayer.infoHeroList.mathStandIndexArr(infoHeroList);
		
		postCenter = GMath.stringToVector3(csvMotMap.standInfo[0]);
		/// 生成角色
		foreach(InfoHero hero in infoHeroList)
		{
			InfoTBCHero infoTBCHero = DataMode.getTBCHero(hero.idServer);
			/// 事实证明这货死亡了
			if(null != infoTBCHero && infoTBCHero.hpMul <= 0)
				continue;
			Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
			post = post + postCenter;
			AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex, hero.idServer, true, post, new Vector3(1f, 0f, 0f), infoHeroList, DataMode.myPlayer);
			AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
			aicombatData.mySkillRelease[0].isCDInit = false;
			ManagerCombatBeast.addBuffBeast(aicombatData);
			/// 设置服务器转存属性
			if(infoTBCHero != null)
			{
				/// 血量 用服务器传来的重算
				aicombatData.hp *= infoTBCHero.hpMul;
				aicombatData.hp = Mathf.Max(1f, aicombatData.hp);
				/// 技能CD 用服务器传来的重算
				TypeCsvHeroSkill csvSkill = aicombatData.mySkillRelease[0].csvSkill;
				aicombatData.mySkillRelease[0].timeStampUnlock = Data.gameTime + csvSkill.cd - csvSkill.cd *
						infoTBCHero.skillCDMul[hero.infoSkill.skillSort.IndexOf(csvSkill.id)] - 0.05f;

				for(int index = 0; index < aicombatData.mySkillAuto.Count; index++)
				{
					csvSkill = aicombatData.mySkillAuto[index].csvSkill;
					aicombatData.mySkillAuto[index].timeStampUnlock = Data.gameTime + csvSkill.cd - csvSkill.cd *
						infoTBCHero.skillCDMul[hero.infoSkill.skillSort.IndexOf(csvSkill.id)] - 0.05f;
				}
			}
			/// 计算个buff属性
			mathTBCPlayerBuff(aicombatData);
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
		DataMode.myPlayer.infoHeroList.mathStandIndexArr(DataMode.infoTBC.infoTBCTeam[idTBC].getTeam());
		/// 我的出战团队
		List<InfoHero> infoHeroList = DataMode.infoTBC.infoTBCTeam[idTBC].getTeam();
		postCenter = GMath.stringToVector3(csvMotMap.standInfo[1]);
		/// 生成角色
		foreach(InfoHero hero in infoHeroList)
		{
			InfoTBCHero infoTBCHero = DataMode.getTBCHero(hero.idServer);
			/// 事实证明这货死亡了
			if(null != infoTBCHero && infoTBCHero.hpMul <= 0)
				continue;
			Vector3 post = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
			post = post + postCenter;
			AICombat aiCombat = ManagerCreate.createCombatHero(hero.standIndex + 9, hero.idServer, false, post, new Vector3(-1f, 0f, 0f), null, null);
			AICombatData aicombatData = aiCombat.GetComponent<AICombatData>();
			ManagerCombatBeast.addBuffBeast(aicombatData);
			mathTBCEnemyAttri(aicombatData);
			/// 设置服务器转存属性
			if(infoTBCHero != null)
			{
				/// 血量 用服务器传来的重算
				aicombatData.hp *= infoTBCHero.hpMul;
				aicombatData.hp = Mathf.Max(1f, aicombatData.hp);
				/// 技能CD 用服务器传来的重算
				for(int index = 0; index < aicombatData.mySkillAuto.Count; index++)
				{
					TypeCsvHeroSkill csvSkill = aicombatData.mySkillAuto[index].csvSkill;
					aicombatData.mySkillAuto[index].timeStampUnlock = Data.gameTime + csvSkill.cd - csvSkill.cd *
						infoTBCHero.skillCDMul[hero.infoSkill.skillSort.IndexOf(csvSkill.id)] - 0.05f;
				}
			}
		}
	}
	
	/// 获得远征buff
	private static void mathTBCPlayerBuff(AICombatData sCombatData)
	{
		TypeCsvMotBuff csvMotBuff = ManagerCsv.getMotBuff(DataMode.infoTBC.infoTBCTeam[idTBC].GetBuffer().selectBuffer);
		if(null != csvMotBuff)
		{
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
	/// 远征敌人系数
	private static void mathTBCEnemyAttri(AICombatData sCombatData)
	{
		TypeCsvMotEnemyAttri csvMotAttri = ManagerCsv.getMotEnemyAttri(idTBC);
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
