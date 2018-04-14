using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 切换其他
public class ManagerSenceTown
{
	
	/// 我所在的副本信息
	private static int _idCsvTown;
	private static bool _isCreateUI;
	public static void start(int sIDCsvTown, bool sIsCreateUI)
	{
		/// 场景锁定的时候，什么都不要干
		if(ManagerSence.isLockSence)
			return;
		if(!DataMode.myPlayer.isInTown)
			DataModeServer3.sendActiveMissionUpdate(null);
		DataMode.infoSetting.isPause = false;
		_isCreateUI = sIsCreateUI;
		ManagerSYS.clear();
		/// fb id引用
		_idCsvTown = sIDCsvTown;
		/// 副本数据
		TypeCsvTown csvTown = ManagerCsv.getTown(_idCsvTown);
		
		// add by ssy 
		// clean mem before load 
	//	if(sIsCreateUI)
	//	{
			EffMngr3.getInstance().clearAllEff();
			LoadMngr.getInstance().cleanMemsAll();
			
	//	}
		// add end
		
		/// 创建场景
		ManagerSence.createSence(csvTown.urls, completeSence);
		/// 只有场景锁定了,才出现加载页面
		WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
		
		
		
	}
	/// 切换村庄
	public static void start(int sIDCsvTown){start(sIDCsvTown, true);}
	
	// changed by ssy
	// 优化低版本ios上的表现
	/// 完成场景加载 ---- 下载角色
	private static void completeSence()
	{
		if(LoadMngr.getInstance().listFbWord.Count == 0)
		{
			// only once
			WindowsMngr.getInstance().viewMngr.root.fbGlobalData.initCacheBaseRes();
			LoadMngr.getInstance().load(LoadMngr.getInstance().listFbWord.ToArray(), comLoadFbGlobal);
		}
		else
		{
			completeSenceTrue();
		}
	}

	

	private static void comLoadFbGlobal(double loader_id)
	{
		foreach(string str in LoadMngr.getInstance().listFbWord)
		{
			if(str.Contains(FBGlobalWindow.RES_EFF_NEXT))
			{
				AstPool.initPool(str, 1, 1);
			}
			else if(str.Contains(FBGlobalWindow.RES_SENCE))
			{
				AstPool.initPool(str, 1, 1);
			}
			else if(str.Contains(FBMusterWindow.RES_ITEM_FLAG_NORMAL))
			{
				AstPool.initPool(str, 6, 10);
			}
			else if(str.Contains(FBMusterWindow.RES_ITEM_FLAG_NORMAL_BOSS) || 
			        str.Contains(FBMusterWindow.RES_ITEM_FLAG_ELIT) ||
			        str.Contains(FBMusterWindow.RES_ITEM_FLAG_CHALLENGE) || 
			        str.Contains(FBMusterWindow.RES_BOSS_QUAN) ||
			        str.Contains(FBMusterWindow.RES_BOSS_QUAN_ELIT) ||
			        str.Contains(FBMusterWindow.RES_BOSS_QUAN_CHALLANGE))
			{
				AstPool.initPool(str, 3, 5);
			}
			else if(str.Contains(UIFBFlagPath.RES_DIAN))
			{
				AstPool.initPool(str, 30, 60);
			}
			else if(str.Contains("ddt1_xiaodian_m3g"))
			{
				AstPool.initPool(str, 15, 80);
			}
			else if(str.Contains("ddt1") && 
			   (str.Contains("tx1") || str.Contains("m1g")))
			{
				AstPool.initPool(str, 1, 1);
			}


		}

		// create camera
	//	WindowsMngr.getInstance().viewMngr.root.fbGlobalData.createCamera(0);
	//	WindowsMngr.getInstance().viewMngr.root.fbGlobalData.uiCamera3D.gameObject.SetActive(false);


		ManagerCsv.initCsv();

		completeSenceTrue();


	}


	


	private static void completeSenceTrue()
	{	
		///数据挖掘 yxh 14.9.15
		if(ManagerServer.isLoadStatue && !ManagerServer.isLoadStatueNew)
		{
			DataModeServer.sendLoadStatues(1,3);
		}
		
		List<string> urls = new List<string>();
		/// 下载主角
		ConfigUrl.getAssetsServerHero(urls, DataMode.myPlayer.infoHeroList.getTeam());
//		/// 下载魂兽资源 虽然没用 就是下
//		ConfigUrl.getAssetsServerBeast(urls, DataMode.myPlayer.infoHeroList.getTeamBeast(DataMode.myPlayer.infoHeroList.idTeamSelect));

		/// TESTING ASSET WILL CLOSE 下载村庄中的资源
		ConfigUrl.getAssetsInfoType(urls, "assetTown");

		urls.Add(ConfigUrl.getAtlasUrl("zhandou_bengzi"));
		
		for(int index = 0; index < ConfigUrl.EFFECT_HERO_GRADE.Length; index++)
		{
			string[] assetEffectUrls = EffMngr3.getInstance().getEffReList(ConfigUrl.EFFECT_HERO_GRADE[index]);
			GUtil.addRange<string>(urls, assetEffectUrls);
		}
		/// 下载
		if(urls.Count <= 0)
			complete();
		else
			LoadMngr.getInstance().load(urls.ToArray(), complete);
		
	}
	// changed end
	
	/// #############################################################################################################################
	/// 
	/// 加载完成后,进行初期启动
	
	/// 所有加载完毕,开始游戏
	private static void complete(double loader_id = 0)
	{
		///数据挖掘 yxh 14.9.15
		if(ManagerServer.isLoadStatue)
		{
			DataModeServer.sendLoadStatues(1,4);
			ManagerServer.isLoadStatue = false;
		}
		
		/// 创建数据
		createData();
		/// 播放音乐
		createMusic();
		/// 创建主角
		createPlayer();
		createCamera();
		/// 显示UI
		createUI();
		
		
		
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
		ManagerDeviceAuto.isBegin = true;
		

	}	
	/// 创建主角
	private static void createPlayer()
	{
		/// 获得村庄
		TypeCsvTown csvTown = ManagerCsv.getTown(_idCsvTown);
		/// 创建村庄的角色
		ManagerCreate.createPlayerTown(GMath.stringToVector3(csvTown.initXYZ));
	}
	/// 设置摄像机数据
	private static void createCamera()
	{
		/// 设置相机
		TypeCsvTown csvTown = ManagerCsv.getTown(_idCsvTown);
		CameraMoveData.valueData.changeMove(typeof(CameraMoveTown));
		CameraMoveData.valueData.setTargetPostion(GMath.stringToVector3(csvTown.initXYZ), csvTown.initCamera);
	}
	/// 播放音乐
	public static void createMusic()
	{
		TypeCsvTown csvTown = ManagerCsv.getTown(_idCsvTown);
		if(null == csvTown)
			return;
		if(null != csvTown.musicSence && "#" != csvTown.musicSence)
			ManagerAudio.playMusic(csvTown.musicSence);
	}
	/// 播放音乐
	private static void createUI()
	{
		/// 不创建ui就返回
		if(!_isCreateUI) return;
		// changed by ssy temp for good look
//		UtilListener.dispatchTime(WindowsMngr.EVT_SHOW_DEFAULT_TOWN_LATE, 1f);
		UtilListener.dispatch(WindowsMngr.EVT_SHOW_DEFAULT_TOWN_LATE);
		/// 显示副本的的标准ui
		//WindowsMngr.getInstance().showDefaultTown();
		// changed end		
	}
	/// 创建数据
	private static void createData()
	{
		DataMode.myPlayer.isInTown = true;
		ManagerAudio.soundSameLength = 1;
	}
	
	
}

