using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerSenceWorldBoss : ManagerSence
{
	/// 我的指定ID信息
	private static int _idCsv;
	public static int idCsv{get{return _idCsv;}}
	private static bool _isDie;
	
	/// 开始战斗
	public static void start(int idCsv){start(idCsv, false);}
	public static void start(int idCsv, bool isDie)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		DataMode.infoSetting.isPause = false;
		_idCsv = idCsv;
		_isDie = isDie;
		/// 清除索引
		ManagerCombatData.clearIndex();
		/// 清空角色
		ManagerSYS.clear();
		/// 副本数据
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		/// 加载完csv文件信息,开始加载ui资源
		ManagerSence.createSence(csvWorldEvent.urls, assetLoad);
		
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
	}
	/// 副本 胜利
	public static void combatWin()
	{
//		UtilListener.calledTime(combatComplete, 3f);
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyBeast");
		SuperUI.close("GusUICombatOnlyTime");
		SuperUI.close("GusUICombatHPWorldBoss");

		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
		AICombat.showWin();
		
		ManagerAudio.playSound(ConfigUrl.UI_AUDIO_COMPLETE);
	}
	/// 副本 失败
	public static void combatLost()
	{
		SuperUI.close("GusUICombatOnlySkill");
		SuperUI.close("GusUICombatOnlyBeast");
		SuperUI.close("GusUICombatOnlyTime");
		SuperUI.close("GusUICombatHPWorldBoss");

		SuperUI.close("GusUICombatSkillChange");
		SuperUI.close("GusUICombatSkillChangeWorldBoss");
		SuperUI.close("GusUICombatSkillChangeSunWell");
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
	/// 将玩家各种的复活,怪物谁去管它呢
	public static void changeRelife()
	{
		
		ManagerCombatData.clearData();
		UnityEngine.Object[] combats = GameObject.FindObjectsOfType(typeof(AICombat));
		/// 转换控制器
		foreach(UnityEngine.Object obj in combats)
		{
			AICombat combat = (AICombat)obj;
			
			/// buff还原
			combat.buffClear();
			/// 技能复原
			combat.getCombatData().myAttack.timeStampUnlock = Data.gameTime + combat.getCombatData().myAttack.csvSkill.cdInit;
			foreach(TypeCombatSkill skill in combat.getCombatData().mySkillAuto)
			{
				skill.timeStampUnlock = Data.gameTime + skill.csvSkill.cdInit;
				skill.isDoneOrder = 0;
			}
			foreach(TypeCombatSkill skill in combat.getCombatData().mySkillRelease)
			{
				skill.timeStampUnlock = 0f;
				skill.isDoneOrder = 0;
			}
			/// 编号比九小的
			if(combat.getCombatData().standIndex >= 9)
				continue;			
			/// 不是主角的
			if(combat.getCombatData().myServerHero == null)
				continue;
			/// 没死的
			if(!combat.isDie())
			{
				/// 复活吧我的勇士
				combat.getCombatData().hp = combat.getCombatData().hpTotal;
			}
			/// 死的
			if(combat.isDie())
			{
				/// 复活吧我的勇士
				combat.relife(combat.getCombatData().hpTotal);
			}
		}
		AICombat.startCombatRelife();
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

		/// 俩技能兰中的特效
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_01", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_ui_skill_02", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		/// 战斗中的特效
		EffMngr3.getInstance().getNoBindEff("effect_common_fire", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_monster_birth", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_common_shunyi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		EffMngr3.getInstance().getNoBindEff("effect_rs_baoqi", null, LoadMngr.ELoadPriority.EFront, Vector3.zero, Quaternion.identity, -1);
		
		/// 获得所有的敌人
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		/// url 敌人
		ConfigUrl.getAssetsWorldEvent(urls, csvWorldEvent);
		/// 下载角色资源
		TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(int.Parse(csvWorldEvent.idCombatEnemy[0]));
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.teamList[csvCombatEnemy.teamType]));
//		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeastByType(csvCombatEnemy.teamType));

		
		
		/// 副本中的各种玩意
		ConfigUrl.getAssetsInfoType(urls, "assetFB");
		
		/// 预加载声音
		urls.Add(ConfigUrl.getAudioUrl("SFX_BossCG0"));
		urls.Add(ConfigUrl.getAudioUrl("SFX_BossCG1"));
		urls.Add(ConfigUrl.getAudioUrl("SFX_BossCG12"));
		urls.Add(ConfigUrl.getAudioUrl("SFX_BossCG2"));

		urls.Add(ConfigUrl.getOtherUrl("AnimCamera_shijieboss_1_2"));
		
		/// 进行pk吧
		LoadMngr.getInstance().load(urls.ToArray(), complete);
	}
	/// 加载成功
	private static void complete(double loader_id)
	{
		createData();

//		createBeast();

		createUI();
		
		createEnemy();
		
		createPlayer();
		
		createCamera();
		
		
		
		/// 播放世界boss音效
		if(_hasCameraAnim)
		{
			TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
			switch(csvWorldEventTime.idCamera)
			{
			case 1:
				UtilListener.calledTime(createMusic, 8f);
				break;
			case 2:
				UtilListener.calledTime(createMusic, 16f);
				break;
			case 3:
				UtilListener.calledTime(createMusic, 8f);
				break;
			default:
				createMusic();
				break;
			}
		} else {
			createMusic();
		}
	}
	/// 创建摄像机
	private static void createCamera()
	{
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		/// 初始化摄像机属性
		if(null == csvWorldEvent.initCameraTargetPost || csvWorldEvent.initCameraTargetPost.Length < 3)
			CameraMoveData.valueData.setTargetPostion(GMath.stringToVector3(csvWorldEvent.standInfo[0]) - new Vector3(13f, 0f, 0f), csvWorldEvent.initCamera);
		else
			CameraMoveData.valueData.setTargetPostion(GMath.stringToVector3(csvWorldEvent.initCameraTargetPost), csvWorldEvent.initCamera);
		CameraMoveData.valueData.setPostionReal();

		/// 世界boss非要特殊做
		if(csvWorldEventTime.idCamera == 2)
		{
			/// 如果有动画
			if(_hasCameraAnim)
			{
				GameObject animObj = (GameObject)GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getOtherUrl("AnimCamera_shijieboss_1_2")));
				animObj.AddComponent<CameraAnimationControl>();
				/// 相机滞空
				CameraMoveData.valueData.changeMove(null);
				CameraMoveData.valueData.transform.localPosition = Vector3.one;
				CameraMoveData.valueData.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
			} else {
				CameraMoveData.valueData.changeMove(typeof(CameraMovePK));
				CameraMoveData.valueData.setTargetPostion(GMath.stringToVector3(csvWorldEvent.initCameraTargetPost), csvWorldEvent.initCamera);
				CameraMoveData.valueData.setPostionReal();
			}
			return;
		}


		/// 摄像机动画属性
		CameraMoveWorldBoss cameraAnim = (CameraMoveWorldBoss)CameraMoveData.valueData.changeMove(typeof(CameraMoveWorldBoss));
		
		string cameraAnimName = "";
		/// 打印出来的东西 =======================================================================================================
		switch(cameraAnimName)
		{
		case "ding dian":
			cameraAnim.addFrameData(0f, new Vector3(-170.3676f, -51.34411f, -191.6f), new Vector3(-170.3676f, -51.34411f, -178.5768f), 30f);
			cameraAnim.addFrameData(2f, new Vector3(-170.3676f, -51.34411f, -188.5768f), new Vector3(-170.3676f, -51.34411f, -178.5768f), 30f);
			cameraAnim.addFrameData(21f, new Vector3(-170.3676f, -44.66819f, -182.2228f), new Vector3(-170.3676f, -44.66819f, -172.2228f), 30f);
			cameraAnim.addFrameData(60f, new Vector3(-177.9582f, -49.46324f, -188.41f), new Vector3(-177.9582f, -49.46324f, -178.41f), 30f);
			cameraAnim.addFrameData(65f, new Vector3(-170.3676f, -45.64095f, -183.3172f), new Vector3(-170.3676f, -45.64095f, -173.3172f), 30f);
			cameraAnim.addFrameData(69f, new Vector3(-167.9929f, -49.21099f, -182.4499f), new Vector3(-167.9929f, -49.21099f, -172.4499f), 81f);
			cameraAnim.addFrameData(70f, new Vector3(-166.3876f, -49.21099f, -186.7111f), new Vector3(-166.3876f, -49.21099f, -176.7111f), 81f);
			cameraAnim.addFrameData(71f, new Vector3(-163.5593f, -49.21099f, -184.1376f), new Vector3(-163.5593f, -49.21099f, -174.1376f), 81f);
			cameraAnim.addFrameDataFinished();
			break;
		case "1":
			cameraAnim.addFrameData(0f, new Vector3(1.526695f, 8.243542f, -10.67301f), new Vector3(3.639647f, 8.261156f, -0.8987989f), 60f);
			cameraAnim.addFrameData(30f, new Vector3(1.15784f, 8.820758f, -11.3264f), new Vector3(3.183735f, 8.784163f, -1.533831f), 60f);
			cameraAnim.addFrameData(40f, new Vector3(-0.1688176f, 9.977085f, -14.98617f), new Vector3(1.182402f, 9.738729f, -5.080748f), 60f);
			cameraAnim.addFrameData(90f, new Vector3(-1.63949f, 8.701749f, -22.68471f), new Vector3(-2.079076f, 8.221531f, -12.70593f), 75f);
			cameraAnim.addFrameData(130f, new Vector3(-0.05814075f, 9.651039f, -34.67445f), new Vector3(-0.05814075f, 9.651039f, -24.67445f), 75f);
			cameraAnim.addFrameData(170f, new Vector3(0.6964318f, 6.472289f, -39.43467f), new Vector3(0.6964623f, 9.225628f, -29.82119f), 75f);
			cameraAnim.addFrameData(180f, new Vector3(0.6978436f, 3.625948f, -41.26873f), new Vector3(0.6978969f, 7.946691f, -32.25034f), 75f);
			cameraAnim.addFrameData(230f, new Vector3(0.7092091f, 2.347545f, -43.83208f), new Vector3(0.7092639f, 6.741891f, -34.84933f), 70f);
			cameraAnim.addFrameData(250f, new Vector3(0.1110169f, 1.6f, -30.58f), new Vector3(0.1110169f, 3.542377f, -20.77045f), 75f);
			cameraAnim.addFrameDataFinished();
			break;
		case "2":
			cameraAnim.addFrameData(0f, new Vector3(-5.298487f, 4.24517f, -23.19368f), new Vector3(-5.298487f, 4.640868f, -13.20151f), 65f);
			cameraAnim.addFrameData(50f, new Vector3(-3.456373f, 1.611422f, -25.88148f), new Vector3(-3.456373f, 3.553755f, -16.07192f), 65f);
			cameraAnim.addFrameData(70f, new Vector3(-2.566992f, 2.088171f, -28.1146f), new Vector3(-2.566992f, 8.402582f, -20.36036f), 90f);
			cameraAnim.addFrameData(150f, new Vector3(-1.791018f, 0.9503766f, -29.04699f), new Vector3(-1.791018f, 2.89271f, -19.23744f), 65f);
			cameraAnim.addFrameData(180f, new Vector3(0.9565537f, 1.74f, -47.01432f), new Vector3(-0.1577564f, 3.680154f, -37.26782f), 55f);
			cameraAnim.addFrameDataFinished();
			break;
		case "3":
			cameraAnim.addFrameData(0f, new Vector3(-7.082432f, 7.583794f, -7.930702f), new Vector3(-7.082432f, 7.583794f, 2.069298f), 90f);
			cameraAnim.addFrameData(30f, new Vector3(-7.082432f, 7.583794f, -21.54647f), new Vector3(-7.082432f, 7.583794f, -11.54647f), 60f);
			cameraAnim.addFrameData(100f, new Vector3(-7.082432f, 7.583794f, -34.48768f), new Vector3(-7.082432f, 7.583794f, -24.48768f), 60f);
			cameraAnim.addFrameData(180f, new Vector3(-6.007987f, 1.892802f, -37.9527f), new Vector3(-6.195565f, 3.460878f, -28.07819f), 60f);
			cameraAnim.addFrameDataFinished();
			break;
		}
		/// 保存数据......
		CameraMoveDataAnim cameraAnimData = cameraAnim.gameObject.GetComponent<CameraMoveDataAnim>();
		{
			List<CameraMoveDataAnim.AnimData> animDataArr = new List<CameraMoveDataAnim.AnimData>();
			/// 0关键点
			CameraMoveDataAnim.AnimData frame0 = new CameraMoveDataAnim.AnimData();
			frame0.frame = 0f;
			frame0.postion = new Vector3(-170.3676f, -51.34411f, -191.6f);
			frame0.rotation = new Vector3(-170.3676f, -51.34411f, -178.5768f);
			frame0.field = 30f;
			animDataArr.Add(frame0);
			/// 1关键点
			CameraMoveDataAnim.AnimData frame1 = new CameraMoveDataAnim.AnimData();
			frame1.frame = 2f;
			frame1.postion = new Vector3(-170.3676f, -51.34411f, -188.5768f);
			frame1.rotation = new Vector3(-170.3676f, -51.34411f, -178.5768f);
			frame1.field = 30f;
			animDataArr.Add(frame1);
			/// 2关键点
			CameraMoveDataAnim.AnimData frame2 = new CameraMoveDataAnim.AnimData();
			frame2.frame = 21f;
			frame2.postion = new Vector3(-170.3676f, -44.66819f, -182.2228f);
			frame2.rotation = new Vector3(-170.3676f, -44.66819f, -172.2228f);
			frame2.field = 30f;
			animDataArr.Add(frame2);
			/// 3关键点
			CameraMoveDataAnim.AnimData frame3 = new CameraMoveDataAnim.AnimData();
			frame3.frame = 60f;
			frame3.postion = new Vector3(-177.9582f, -49.46324f, -188.41f);
			frame3.rotation = new Vector3(-177.9582f, -49.46324f, -178.41f);
			frame3.field = 30f;
			animDataArr.Add(frame3);
			/// 4关键点
			CameraMoveDataAnim.AnimData frame4 = new CameraMoveDataAnim.AnimData();
			frame4.frame = 65f;
			frame4.postion = new Vector3(-170.3676f, -45.64095f, -183.3172f);
			frame4.rotation = new Vector3(-170.3676f, -45.64095f, -173.3172f);
			frame4.field = 30f;
			animDataArr.Add(frame4);
			/// 5关键点
			CameraMoveDataAnim.AnimData frame5 = new CameraMoveDataAnim.AnimData();
			frame5.frame = 69f;
			frame5.postion = new Vector3(-167.9929f, -49.21099f, -182.4499f);
			frame5.rotation = new Vector3(-167.9929f, -49.21099f, -172.4499f);
			frame5.field = 81f;
			animDataArr.Add(frame5);
			/// 6关键点
			CameraMoveDataAnim.AnimData frame6 = new CameraMoveDataAnim.AnimData();
			frame6.frame = 70f;
			frame6.postion = new Vector3(-166.3876f, -49.21099f, -186.7111f);
			frame6.rotation = new Vector3(-166.3876f, -49.21099f, -176.7111f);
			frame6.field = 81f;
			animDataArr.Add(frame6);
			/// 7关键点
			CameraMoveDataAnim.AnimData frame7 = new CameraMoveDataAnim.AnimData();
			frame7.frame = 71f;
			frame7.postion = new Vector3(-163.5593f, -49.21099f, -184.1376f);
			frame7.rotation = new Vector3(-163.5593f, -49.21099f, -174.1376f);
			frame7.field = 81f;
			animDataArr.Add(frame7);
			cameraAnimData.Remove("ding dian");
			cameraAnimData.Add("ding dian", animDataArr);
		}
		{
			List<CameraMoveDataAnim.AnimData> animDataArr = new List<CameraMoveDataAnim.AnimData>();
			/// 0关键点
			CameraMoveDataAnim.AnimData frame0 = new CameraMoveDataAnim.AnimData();
			frame0.frame = 0f;
			frame0.postion = new Vector3(1.526695f, 8.243542f, -10.67301f);
			frame0.rotation = new Vector3(3.639647f, 8.261156f, -0.8987989f);
			frame0.field = 60f;
			animDataArr.Add(frame0);
			/// 1关键点
			CameraMoveDataAnim.AnimData frame1 = new CameraMoveDataAnim.AnimData();
			frame1.frame = 30f;
			frame1.postion = new Vector3(1.15784f, 8.820758f, -11.3264f);
			frame1.rotation = new Vector3(3.183735f, 8.784163f, -1.533831f);
			frame1.field = 60f;
			animDataArr.Add(frame1);
			/// 2关键点
			CameraMoveDataAnim.AnimData frame2 = new CameraMoveDataAnim.AnimData();
			frame2.frame = 40f;
			frame2.postion = new Vector3(-0.1688176f, 9.977085f, -14.98617f);
			frame2.rotation = new Vector3(1.182402f, 9.738729f, -5.080748f);
			frame2.field = 60f;
			animDataArr.Add(frame2);
			/// 3关键点
			CameraMoveDataAnim.AnimData frame3 = new CameraMoveDataAnim.AnimData();
			frame3.frame = 90f;
			frame3.postion = new Vector3(-1.63949f, 8.701749f, -22.68471f);
			frame3.rotation = new Vector3(-2.079076f, 8.221531f, -12.70593f);
			frame3.field = 75f;
			animDataArr.Add(frame3);
			/// 4关键点
			CameraMoveDataAnim.AnimData frame4 = new CameraMoveDataAnim.AnimData();
			frame4.frame = 130f;
			frame4.postion = new Vector3(-0.05814075f, 9.651039f, -34.67445f);
			frame4.rotation = new Vector3(-0.05814075f, 9.651039f, -24.67445f);
			frame4.field = 75f;
			animDataArr.Add(frame4);
			/// 5关键点
			CameraMoveDataAnim.AnimData frame5 = new CameraMoveDataAnim.AnimData();
			frame5.frame = 170f;
			frame5.postion = new Vector3(0.6964318f, 6.472289f, -39.43467f);
			frame5.rotation = new Vector3(0.6964623f, 9.225628f, -29.82119f);
			frame5.field = 75f;
			animDataArr.Add(frame5);
			/// 6关键点
			CameraMoveDataAnim.AnimData frame6 = new CameraMoveDataAnim.AnimData();
			frame6.frame = 180f;
			frame6.postion = new Vector3(0.6978436f, 3.625948f, -41.26873f);
			frame6.rotation = new Vector3(0.6978969f, 7.946691f, -32.25034f);
			frame6.field = 75f;
			animDataArr.Add(frame6);
			/// 7关键点
			CameraMoveDataAnim.AnimData frame7 = new CameraMoveDataAnim.AnimData();
			frame7.frame = 230f;
			frame7.postion = new Vector3(0.7092091f, 2.347545f, -43.83208f);
			frame7.rotation = new Vector3(0.7092639f, 6.741891f, -34.84933f);
			frame7.field = 70f;
			animDataArr.Add(frame7);
			/// 8关键点
			CameraMoveDataAnim.AnimData frame8 = new CameraMoveDataAnim.AnimData();
			frame8.frame = 250f;
			frame8.postion = new Vector3(0.1110169f, 1.6f, -30.58f);
			frame8.rotation = new Vector3(0.1110169f, 3.542377f, -20.77045f);
			frame8.field = 75f;
			animDataArr.Add(frame8);
			cameraAnimData.Remove("1");
			cameraAnimData.Add("1", animDataArr);
		}
		{
			List<CameraMoveDataAnim.AnimData> animDataArr = new List<CameraMoveDataAnim.AnimData>();
			/// 0关键点
			CameraMoveDataAnim.AnimData frame0 = new CameraMoveDataAnim.AnimData();
			frame0.frame = 0f;
			frame0.postion = new Vector3(-5.298487f, 4.24517f, -23.19368f);
			frame0.rotation = new Vector3(-5.298487f, 4.640868f, -13.20151f);
			frame0.field = 65f;
			animDataArr.Add(frame0);
			/// 1关键点
			CameraMoveDataAnim.AnimData frame1 = new CameraMoveDataAnim.AnimData();
			frame1.frame = 50f;
			frame1.postion = new Vector3(-3.456373f, 1.611422f, -25.88148f);
			frame1.rotation = new Vector3(-3.456373f, 3.553755f, -16.07192f);
			frame1.field = 65f;
			animDataArr.Add(frame1);
			/// 2关键点
			CameraMoveDataAnim.AnimData frame2 = new CameraMoveDataAnim.AnimData();
			frame2.frame = 70f;
			frame2.postion = new Vector3(-2.566992f, 2.088171f, -28.1146f);
			frame2.rotation = new Vector3(-2.566992f, 8.402582f, -20.36036f);
			frame2.field = 90f;
			animDataArr.Add(frame2);
			/// 3关键点
			CameraMoveDataAnim.AnimData frame3 = new CameraMoveDataAnim.AnimData();
			frame3.frame = 150f;
			frame3.postion = new Vector3(-1.791018f, 0.9503766f, -29.04699f);
			frame3.rotation = new Vector3(-1.791018f, 2.89271f, -19.23744f);
			frame3.field = 65f;
			animDataArr.Add(frame3);
			/// 4关键点
			CameraMoveDataAnim.AnimData frame4 = new CameraMoveDataAnim.AnimData();
			frame4.frame = 180f;
			frame4.postion = new Vector3(0.9565537f, 1.74f, -47.01432f);
			frame4.rotation = new Vector3(-0.1577564f, 3.680154f, -37.26782f);
			frame4.field = 55f;
			animDataArr.Add(frame4);
			cameraAnimData.Remove("2");
			cameraAnimData.Add("2", animDataArr);
		}
		{
			List<CameraMoveDataAnim.AnimData> animDataArr = new List<CameraMoveDataAnim.AnimData>();
			/// 0关键点
			CameraMoveDataAnim.AnimData frame0 = new CameraMoveDataAnim.AnimData();
			frame0.frame = 0f;
			frame0.postion = new Vector3(-7.082432f, 7.583794f, -7.930702f);
			frame0.rotation = new Vector3(-7.082432f, 7.583794f, 2.069298f);
			frame0.field = 90f;
			animDataArr.Add(frame0);
			/// 1关键点
			CameraMoveDataAnim.AnimData frame1 = new CameraMoveDataAnim.AnimData();
			frame1.frame = 30f;
			frame1.postion = new Vector3(-7.082432f, 7.583794f, -21.54647f);
			frame1.rotation = new Vector3(-7.082432f, 7.583794f, -11.54647f);
			frame1.field = 60f;
			animDataArr.Add(frame1);
			/// 2关键点
			CameraMoveDataAnim.AnimData frame2 = new CameraMoveDataAnim.AnimData();
			frame2.frame = 100f;
			frame2.postion = new Vector3(-7.082432f, 7.583794f, -34.48768f);
			frame2.rotation = new Vector3(-7.082432f, 7.583794f, -24.48768f);
			frame2.field = 60f;
			animDataArr.Add(frame2);
			/// 3关键点
			CameraMoveDataAnim.AnimData frame3 = new CameraMoveDataAnim.AnimData();
			frame3.frame = 180f;
			frame3.postion = new Vector3(-6.007987f, 1.892802f, -37.9527f);
			frame3.rotation = new Vector3(-6.195565f, 3.460878f, -28.07819f);
			frame3.field = 60f;
			animDataArr.Add(frame3);
			cameraAnimData.Remove("3");
			cameraAnimData.Add("3", animDataArr);
		}
		/// 打印出来的东西 =======================================================================================================
		cameraAnimData.editName = "" + csvWorldEventTime.idCamera;
		cameraAnim.setAnimData("" + csvWorldEventTime.idCamera);
		if(!_hasCameraAnim)
			cameraAnim.changeOver();
	}
	private static bool _hasCameraAnim;


	/// 创建魂兽 在看不见的地方
	private static void createBeast()
	{
		/// 副本信息
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));		
		TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(int.Parse(csvWorldEvent.idCombatEnemy[0]));
		uint idServerTeam = DataMode.myPlayer.infoHeroList.teamList[csvCombatEnemy.teamType];
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
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		AIEnemy.createEnemys(csvWorldEvent.idCombatEnemy.Length);
		_hasCameraAnim = false;
		/// WILL CLOSE
//		_hasCameraAnim = true;
		for(int index = 0; index < csvWorldEvent.idCombatEnemy.Length; index++)
		{
			AIWorldBoss aiWorldBoss = ManagerCreate.createWorldBoss(index, GMath.stringToVector3(csvWorldEvent.standInfo[index]));
			aiWorldBoss.timeWait = 1f;
			TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(GMath.toInt(csvWorldEvent.idCombatEnemy[index]));
			TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvCombatEnemy.idCombatStand);
			for(int i = 0; i < 9; i++)
			{
				int idCsvEnemy = (int)csvCombatEnemy.GetType().GetField("index" + i).GetValue(csvCombatEnemy);
				if(idCsvEnemy != -1)
				{
					Vector3 standOffset = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + i).GetValue(csvCombatStand));
					AICombatData combatData = ManagerCreate.createWorldBossFollower(i + 9, idCsvEnemy, DataMode.getWorldBoss(_idCsv).lv, aiWorldBoss.gameObject, standOffset, new Vector3(0f,0f,-1f));
					combatData.hp = DataMode.getWorldBoss(_idCsv).hp;
					_hasCameraAnim = _hasCameraAnim || Mathf.Abs(combatData.hp - combatData.hpTotal) <= 1f;
					if(_hasCameraAnim)
					{
						combatData.GetComponent<AIFollower>().startAnim = "reast";
					}
					/// 通过AI拾取技能
					combatData.switchAISkill();
					ManagerCombatBeast.addBuffBeast(combatData);
					/// 给世界boss点面子吧
					SuperUI.getUI<UICombatHPWorldBoss>().setCombatData(combatData);
				}
			}
			/// 如果是剧情等待的时间
			if(_hasCameraAnim)
			{
				aiWorldBoss.timeWait = cameraAnimTime;
			}
		}
	}
	/// 获得动画时间
	private static float cameraAnimTime
	{
		get
		{
			float result = 0f;
			TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
			switch(csvWorldEventTime.idCamera)
			{
			case 1:
				result = 8f;
				break;
			case 2:
				result = 9.5f;
				break;
			case 3:
				result = 6f;
				break;
			}
			return result;
		}
	}
	/// 创建主角   在看不见的地方
	private static void createPlayer()
	{
		/// 副本信息
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		/// 主角站位
		TypeCsvCombatStand csvCombatStand = ManagerCsv.getCombatStand(csvWorldEvent.idCombatStand);
		/// 创建主角
		AIPlayerWorldBoss player = ManagerCreate.createPlayerWorldBoss(1, GMath.stringToVector3(csvWorldEvent.initXYZ));
		
		AIPlayer.valueObject = player;
		
		List<AICombatData> listCombatData = new List<AICombatData>();
		
		TypeCsvCombatEnemy csvCombatEnemy = ManagerCsv.getCombatEnemy(int.Parse(csvWorldEvent.idCombatEnemy[0]));
		
		/// 如果主角不是空的 创建有效的随从
		if(null != DataMode.myPlayer)
		{
			
			/// 我的出战团队
			List<InfoHero> infoHeroList = DataMode.myPlayer.infoHeroList.getTeam(DataMode.myPlayer.infoHeroList.teamList[csvCombatEnemy.teamType]);
			/// 计算站位
			DataMode.myPlayer.infoHeroList.mathStandIndexArr(infoHeroList);
			
			/// 生成角色
			foreach(InfoHero hero in infoHeroList)
			{
				Vector3 standOffset = GMath.stringToVector3((string[])csvCombatStand.GetType().GetField("index" + hero.standIndex).GetValue(csvCombatStand));
				AICombatData combatData = ManagerCreate.createFollower(hero.standIndex, hero.idServer, player.gameObject, standOffset, new Vector3(0f, 0f, 1f), false, infoHeroList, DataMode.myPlayer);
				listCombatData.Add(combatData);
				ManagerCombatBeast.addBuffBeast(combatData);
				if(_isDie)
				{
					combatData.hp = 0;
					/// 隐藏我自己
					NGUITools.SetLayer(combatData.gameObject, Config.LAYER_HIDE);
				}
			}
			/// 如果是剧情等待的时间
			if(_hasCameraAnim)
			{
				UtilListener.calledTime(createUICombat, cameraAnimTime, null, listCombatData);
			} else {
				if(null != SuperUI.getUI<UICombatOnlySkill>())
					SuperUI.getUI<UICombatOnlySkill>().setSkillData(listCombatData);
			}
		}
	}
	/// 创建ui 技能的
	private static void createUICombat(UtilListenerEvent sEvent)
	{
		if(null != SuperUI.getUI<UICombatOnlySkill>())
			SuperUI.getUI<UICombatOnlySkill>().setSkillData((List<AICombatData>)sEvent.eventArgs);
	}

	/// 创建UI
	private static void createUI()
	{
		/// 显示ui	
		SuperUI.show<UICombatOnlySkill>("GusUICombatOnlySkill", null);
		
		SuperUI.show<UICombatHPWorldBoss>("GusUICombatHPWorldBoss", null);

		/// WILL CLOSE
//		SuperUI.show<UICombatOnlyButton>("GusUICombatOnlyButton", null);

		/// 显示黑屏
		SuperUI.showNew<UIScreenBlack>("GusUIScreenBlack");
		SuperUI.getUI<UIScreenBlack>().animTime = 1f;
		/// 显示副本的的标准ui
		WindowsMngr.getInstance().showDefaultFB();
		
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
		/// 如果死亡显示死亡ui
		if(_isDie)
			combatLost();
	}
	/// 创建音效
	private static void createMusic()
	{
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		if(csvWorldEvent.musicSence == "#" || string.IsNullOrEmpty(csvWorldEvent.musicSence))
			return;
		ManagerAudio.playMusic(csvWorldEvent.musicSence);
	}
	/// 创建数据
	private static void createData()
	{
		ManagerAudio.soundSameLength = 3;
		/// 是否在副本中
		DataMode.myPlayer.isInFBWorldBoss = true;
		/// 停止自动战斗
		DataMode.infoSetting.setValue(InfoSettingEnum.isAuto, false);
		/// 清除卡牌信息
		DataMode.infoFBRewardList.clear();
		/// 我这职业返回
		TypeCsvWorldEventTime csvWorldEventTime = ManagerCsv.getWorldEventTime(_idCsv);
		TypeCsvWorldEvent csvWorldEvent = ManagerCsv.getWorldEvent(int.Parse(csvWorldEventTime.id_event[DataMode.getWorldBoss(_idCsv).bossIndex]));
		/// 提前取出音乐
		if(null != csvWorldEvent.musicSence && "#" != csvWorldEvent.musicSence)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvWorldEvent.musicSence));
		if(null != csvWorldEvent.musicCombatLast && "#" != csvWorldEvent.musicCombatLast)
			LoadMngr.getInstance().GetAudio(ConfigUrl.getAudioUrl(csvWorldEvent.musicCombatLast));
	}
}
