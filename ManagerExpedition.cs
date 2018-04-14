using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using net;
using net.unity3d;

public class ManagerExpedition{
	public delegate void OnResetFinish();
	private static OnResetFinish onfishReset;
	
	#region 常量
	/// 云 2关通过开0云 5开1 8开2 12开3
	private static readonly string _str_root_yun = "yun1_haijiaershan1_m1g(Clone)" ;
	private static readonly string _str_yun0 = "yun1_zhucheng2_m1";
	private static readonly string _str_yun1 = "yun1_zhucheng2_m2";
	private static readonly string _str_yun2 = "yun1_zhucheng2_m3";
	private static readonly string _str_yun3 = "yun1_zhucheng2_m4";
	/// 路径obj的父级
	private static readonly string _root_position = "gn_haijiaershan_zuobiao(Clone)";
	/// 路径前缀
	private static readonly string _str_position = "gn_haijiaershan_zuobiao";
	/// 宝箱坐标的根
	private static readonly string _root_box = "gn_baoxiang_zuobiao(Clone)";
	/// 宝箱的坐标的前缀
	private static readonly string _str_box = "gn_haijiaershan_zuobiao";

	private static readonly string _def_position = "gn_baoxiang_zuobiao2(Clone)";

	/// 玩法说明
	//public static readonly string urls_info = "TBCInfo.txt";
	
	/// 箱子 资源
	private static readonly string _url_box_normal_full = ConfigUrl.ROOT + "others/gn_haijiaershan_baoxiang1.assetbundle";
	/// 高级宝箱
	private static readonly string _url_box_high_full = ConfigUrl.ROOT + "others/gn_haijiaershan_baoxiang2.assetbundle";
	/// 最后的宝箱
	private static readonly string _url_box_final_full = ConfigUrl.ROOT + "others/gn_haijiaershan_baoxiang3.assetbundle";
	
	/// 宝箱的动作 
	private static string _box_ani_jump = "jump";
	private static string _box_ani_open = "open";
	private static string _box_ani_opened = "opened";
	
	///坟
	private static string _url_tomb = ConfigUrl.ROOT + "others/gn_haijiaershan_flog03.assetbundle";
	/// 怪
	private static string _url_enemy = ConfigUrl.ROOT + "others/gn_haijiaershan_flog01.assetbundle";
	/// boss 怪
	private static string _url_enemy_boss = ConfigUrl.ROOT + "others/gn_haijiaershan_flog02.assetbundle";
	
	/// 场景
	private static readonly string _url_sence = "csv/gns/gn_haijiaershan/csv/";
	/// 箭头
	private static readonly string _url_eff = "effect_ui_map_jiantou";
	
	/// 相机
	private static readonly string _camera_animatin_name = "jingtou_haijiaershan_d1";
	private static readonly string _url_animation = ConfigUrl.ROOT + "others/"+_camera_animatin_name+ConfigUrl.EXTENSION;
	
	#endregion
	
	#region 变量
	/// 箭头特效引用
	private static GameObject _eff;
	private static GameObject _root_yun;
	private static GameObject _root_yun0;
	private static GameObject _root_yun1;
	private static GameObject _root_yun2;
	private static GameObject _root_yun3;
	/// key 为 每段云的根
	private static Dictionary<GameObject,List<GameObject>> _cache_yun = new Dictionary<GameObject, List<GameObject>>();
	/// 保存的每朵云的缩放
	private static Dictionary<string,Vector3> _cache_yun_scale = new Dictionary<string, Vector3>();

	/// 模型移动的速度
	private const float moveSpeed = 0.5f;
	private static bool _reset;
	/// 模型
	public static GameObject Mode;
	/// 模型是否初始化
	private static bool _iniMode = false;
	/// 场景是否初始化
	private static bool _iniSence = false;
	
	/// 路径的点 开始的关卡为 1 (取决于场景中的坐标是否从1开始)
	private static Dictionary<string,List<Vector3>> _position = new Dictionary<string, List<Vector3>>();
	/// key 开始为0 
	private static Dictionary<int,GameObject> _levelObj = new Dictionary<int, GameObject>();
	/// key 开始为0 宝箱
	private static Dictionary<int,GameObject> _boxObj = new Dictionary<int, GameObject>();
	/// 防守队伍
	private static Dictionary<int,GameObject> _defObj = new Dictionary<int, GameObject>();
	/// 防守队伍位置放的宝箱
	private static Dictionary<int,GameObject> _defBoxObj = new Dictionary<int, GameObject>();

	private static Dictionary<int,GameObject> _defBoxEff = new Dictionary<int, GameObject>();

	/// 开始的点
	private static List<Vector3> _beginPoint = new List<Vector3>();
	
	/// 自定义3D 相机
	private static UIExpeditionCamera _camera;
	/// 返回场景相机
	public static UIExpeditionCamera CameraEx
	{
		get
		{
			return _camera;
		}
	}
	
	/// 显示用的 
	public static int idShow = -1;

	public static bool moveCamera = true;
	#endregion
	
	private static readonly string _url_eff_box_active = "effect_ui_yuanzheng_box_light";
	private static readonly string _url_eff_box_open = "effect_UI_yuanzheng_box_open";
	
	private static GameObject _eff_box_active;
	private static GameObject _eff_box_open;
	
	private static bool _showWin = true;
	
	private static GameObject _view;

	public static WindowsMSG.ReopenActiveMission m_reopenMsgActive = null;

	public static bool isOpen
	{
		get
		{
			return _view != null;
		}
	}
	/// 开始
	public static void Start(object obj = null)
	{
		if(obj!=null && (obj is WindowsMSG.ReopenActiveMission))
		{
			m_reopenMsgActive = ( WindowsMSG.ReopenActiveMission)obj;
		}

		if(_view != null)
		{
			return;
		}
//		DataModeServer2.sendMotPKEnemyInfo(Ini);
		ManagerSYS.clear();
		_showWin = true;
		_iniMode = false;
		_iniSence = false;
		_reset = false;
		_boxObj.Clear();
		_levelObj.Clear();
		_beginPoint.Clear();
		_position.Clear();
		_defObj.Clear();
		_defBoxObj.Clear();
		_defBoxEff.Clear();
		_view = new GameObject("Expedition View");
		BeginLoad();
	}

//	private static void Ini(UtilListenerEvent sData)
//	{
//		RM2C_MOT_PK_ENEMY_INFO recv = (RM2C_MOT_PK_ENEMY_INFO)sData.eventArgs;
//		if(recv.iResult == 1)
//		{
//
//		}
//	}
	
	private static void BeginLoad()
	{
		if(!_reset)
		{
			//地址
			List<string> urls = new List<string>();

			string[] temp = LoadHeroMngr.getAllHeroRes(DataMode.myPlayer.infoHeroList.getTeamLeader().idCsv);
			foreach(string str in temp)
			{
				if(!string.IsNullOrEmpty(str))
				{
					urls.Add(str);
				}
				else
				{
					if(UtilLog.isBulidLog)UtilLog.Log("enemy csvid error!");
				}
			}
			InfoTBCCupDefHero defHero = null;
			string[] defUrls = null;
			///初始化 防守阵营
			for(int i = 0; i < 5; i++)
			{
				defHero = DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(i);
				if(defHero != null)
				{
					if(!defHero.pass)
					{
						if(defHero.idCsv > 0)
						{
							defUrls = LoadHeroMngr.getAllHeroRes(defHero.idCsv);
							for(int index = 0; index < defUrls.Length; index++)
							{
								urls.Add(defUrls[index]);
							}
						}
					}
				}
			}
			
			urls.Add(_url_tomb);
			urls.Add(_url_enemy);
			urls.Add(_url_enemy_boss);
			urls.Add(_url_box_high_full);
			urls.Add(_url_box_normal_full);
			urls.Add(_url_box_final_full);
		//	urls.Add(ConfigUrl.ROOT+urls_info);
			urls.Add(ConfigUrl.getInfoNameUrl("shadowGreen"));
			urls.Add(ConfigUrl.getInfoNameUrl("shadowRed"));
			urls.Add(ConfigUrl.getInfoNameUrl("shadowBoss"));
			urls.Add(_url_animation);
			
			EffMngr3.getInstance().getNoBindEff(_url_eff_box_active,LoadEffBoxActiveCallBack,Vector3.zero,Quaternion.identity);
			EffMngr3.getInstance().getNoBindEff(_url_eff_box_open,LoadEffBoxOpenCallBack,Vector3.zero,Quaternion.identity);
			EffMngr3.getInstance().getNoBindEff(_url_eff,LoadEffCallBack,Vector3.zero,Quaternion.identity);
			EffMngr3.getInstance().getNoBindEff(ExpeditionWinWindow.url_eff,null,Vector3.zero,Quaternion.identity);
			WindowsMngr.getInstance().viewMngr.root.loadData.CloseSelf = false;
			LoadMngr.getInstance().load(urls.ToArray(),Creat3DMode);
			ManagerSence.createSence(_url_sence,CreatSence);
			WindowsMngr.getInstance().openWindow(WindowsID.LOADING);
		}
		else
		{
			Creat3DMode(1);
		}
		EffMngr3.getInstance().getNoBindEff(TBCCupWindow.eff, null, Vector3.zero, Quaternion.identity);
	}
	
	private static void LoadEffBoxActiveCallBack(GameObject obj)
	{
		if(_eff_box_active != null)
		{
			GameObject.Destroy(_eff_box_active);
		}
		_eff_box_active = obj;
		_eff_box_active.SetActive(false);
		SetBoxEff(true);
		SetDefBoxEff();
	}
	
	private static void LoadEffBoxOpenCallBack(GameObject obj)
	{
		if(_eff_box_open != null)
		{
			GameObject.Destroy(_eff_box_open);
		}
		_eff_box_open = obj;
		_eff_box_open.SetActive(false);
	}
	
	private static void LoadEffCallBack(GameObject obj)
	{
		if(_eff != null)
		{
			GameObject.Destroy(_eff);
		}
		_eff = obj;
		SetEff();
	}
	
	/// 初始化模型
	private static void Creat3DMode(double loadid)
	{
		if(!_reset)
		{
			///创建角色
			CreatHero();
		}
		//创建关卡
		CreatLevel();
		///创建宝箱
		CreatBoxReward();
		///创建防守阵营
		CreatObjDef();
		_iniMode = true;
		IniSence();
	}
	
	/// 初始化场景 路径 相机
	private static void CreatSence()
	{
		_position.Clear();
		_beginPoint.Clear();
		
		string key = "";
		string[] path;
		List<Vector3> tempList;
		Transform child;
		
		GameObject temp = GameObject.Find(_root_position);
		for(int i = 0; i < temp.transform.childCount; i++)
		{
			child = temp.transform.GetChild(i);
			key = child.name.Replace(_str_position,"");
			if(key.Replace("_","") != key)
			{
				path = key.Split('_');
				key = path[0];
			}
			else
			{
				path = null;
			}
			
			if(key == "0")
			{
				if(path == null)
				{
					_beginPoint.Insert(0,child.position);
				}
				else
				{
					if(GMath.toInt(path[1]) > _beginPoint.Count)
					{
						_beginPoint.Add(child.position);
					}
					else
					{
						_beginPoint.Insert(GMath.toInt(path[1]),child.position);
					}
				}
			}
			else
			{
				if(!_position.ContainsKey(key))
				{
					_position.Add(key,new List<Vector3>());
				}
				tempList = _position[key];
				
				if(path != null)
				{
					if(GMath.toInt(path[1]) > tempList.Count)
					{
						tempList.Add(child.position);
					}
					else
					{
						tempList.Insert(GMath.toInt(path[1]),child.position);
					}
				}
				else
				{
					tempList.Insert(0,child.position);
				}
			}
		}
		
		_iniSence = true;
		IniSence();
	}
	
	/// 游戏开始 初始化 模型位置
	/// 初始化 相机
	/// 初始化 奖励等等
	private static void IniSence()
	{
		if(_reset)
		{
			SetSence();
		}
		else if(_iniMode && _iniSence)
		{
			CreatCamera(Mode);
			SetSence();
			WindowsMngr.getInstance().openWindow(WindowsID.EXPEDITION,0);
		}
	}
	
	/// 创建角色
	private static void CreatHero()
	{
		Mode = LoadHeroMngr.getHeroObj(DataMode.myPlayer.infoHeroList.getTeamLeader().idCsv);
		TypeCsvHero csvhero = ManagerCsv.getHero(DataMode.myPlayer.infoHeroList.getTeamLeader().idCsv);
		Mode.transform.localScale = Mode.transform.localScale * csvhero.scale * 0.7f;
		UIPlayerAutoMove auto = Mode.AddComponent<UIPlayerAutoMove>();
		auto.moveSpeed = moveSpeed;
	}
	/// 创建宝箱
	private static void CreatBoxReward()
	{
		_boxObj.Clear();
		for(int i = 0; i < 15; i++)
		{
			_boxObj.Add(i+1 ,GetRewardBoxAss(i+1));
		}
	}
	/// 获得这个位置上宝箱的资源
	private static GameObject GetRewardBoxAss(int index)
	{
		GameObject tempObj = null;
		
		if(index == 15)
		{
			tempObj = LoadMngr.getInstance().getObjectGame(_url_box_final_full);
		}
		//高级宝箱
		else if(index % 3 == 0)
		{
			tempObj = LoadMngr.getInstance().getObjectGame(_url_box_high_full);
		}
		else
		{
			tempObj = LoadMngr.getInstance().getObjectGame(_url_box_normal_full);
		}
		
		if(tempObj != null)
		{
			tempObj = GameObject.Instantiate(tempObj) as GameObject;
			if(DataMode.infoTBC.idBox == index)
			{
				if(DataMode.infoTBC.id > DataMode.infoTBC.idBox)
				{
					//通关未领取
					tempObj.animation.wrapMode = WrapMode.Loop;
					tempObj.animation.CrossFade(_box_ani_jump);
				}
				else
				{
					//未通关
					tempObj.animation.Stop();
				}
			}
			else if(DataMode.infoTBC.idBox < index)
			{
				//未通关
				tempObj.animation.Stop();
			}
			else
			{
				//通关领完奖励
//				tempObj.animation.wrapMode = WrapMode.Once;
//				tempObj.animation.CrossFade(_box_ani_opened);
				tempObj.gameObject.SetActive(false);
			}
		}
		return tempObj;
	}
	/// 创建关卡
	private static void CreatLevel()
	{
		//应该是缺少collider  Ty
		List<int> temp = new List<int>();
		_levelObj.Clear();
		
		GameObject obj = null;
		for(int i = 0; i < DataMode.infoTBC.infoTBCTeam.Count; i++)
		{
			if(DataMode.infoTBC.id > i+1)
			{
				//坟
				obj = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(_url_tomb)) as GameObject;
			}
			else
			{
				//boss 
				if((i+1) % 3 == 0)
				{
					obj = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(_url_enemy_boss)) as GameObject;
				}
				else
				{
					obj = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(_url_enemy)) as GameObject;
				}
//				
//				CapsuleCollider collider = obj.GetComponentInChildren<CapsuleCollider>();
//				if(collider == null)
//				{
//					collider = obj.AddComponent<CapsuleCollider>();
//				}
//				collider.center = new Vector3(0f, 1f, 0f);
//				collider.height = 2f;
//				collider.radius = 0.8f;
			}
			if(obj != null)
			{
				obj.transform.localScale = new Vector3(7,7,7);
				_levelObj.Add(i+1,obj);
			}
			else
			{
				UtilLog.LogError("Level enemy team leader error! index of team = " + i.ToString());
			}
		}
	}
	/// 设置场景所有的东西 
	private static void SetSence()
	{
		Transform child;
		string key = "";
		string[] id;

		//宝箱位置
		GameObject temp = GameObject.Find(_root_box);
		for(int i = 0; i < temp.transform.childCount; i++)
		{
			child = temp.transform.GetChild(i);
			key = child.name.Replace(_str_box,"");
			id = key.Split('_');
			_boxObj[GMath.toInt(id[0])].transform.position = child.transform.position;
		}
		
		List<Vector3> tempList = null;
		//处理关卡
		//注意坟和人的分别处理
		for(int i = 0; i < _levelObj.Count; i++)
		{
			tempList = _position[(i+1).ToString()];
			_levelObj[i+1].transform.position = tempList[0]-new Vector3(0,0.5f,0);
		}
		
		
		//角色位置
		List<Vector3> last = new List<Vector3>();
		tempList = new List<Vector3>();
		last.Add(_beginPoint[0]);
		last.Add(_beginPoint[_beginPoint.Count-1]);
		for(int i = 0; i < _position.Count; i++)
		{
			tempList = _position[(i+1).ToString()];
			if(i + 1 >= _position.Count && tempList.Count <= 1)
			{
				//第1修正法 如果这个点是最后一个点 但是路径只有一个位置 那就不添加了
				continue;
			}
			last.Add(tempList[tempList.Count-1]);
		}
		
		if(idShow == 0 && DataMode.infoTBC.idBox == 1)
		{
			Mode.transform.position = last[0];
		}
		else
		{
			if(DataMode.infoTBC.idBox < last.Count)
			{
				Mode.transform.position = last[DataMode.infoTBC.idBox];
			}
			else
			{
				//第3修正法 如果位置超出人物可能存在的任何点的集合 则取最后一个点
				Mode.transform.position = last[last.Count-1];
			}
			if(idShow < DataMode.infoTBC.idBox-1)
			{
				//第2修正法 如果idshow 小于 idbox - 1 说明有什么原因造成 
				//人物角色的位置距离关卡过远 修正一下 (这种情况,只有在极其特殊的情况下才会发生)
				//实际上idshow 应该是等于 idbox的 但是isshow有的时候需要通过一次显示的移动(即玩家点击宝箱的时候) idbox会+1
				//所以在移动以前 idbox与idshou 的差值是1
				idShow = DataMode.infoTBC.idBox - 1;
				UtilLog.LogError("距离过远");
			}
		}
		UIPlayerAutoMove auto = Mode.GetComponent<UIPlayerAutoMove>();
		auto.LookAt(GetNextLevelPosition());
		
		///相机的处理
		if(moveCamera)
		{
			moveCamera = false;
			_camera.Ini(Mode);
		}
		else
		{
			_camera.LookTarget();
		}
		if(!_reset)
		{
			IniYun();
			_camera.gameObject.AddComponent<UICamera>();
		}
		SetYun();
		SetEff();
		SetLevel();
		SetDefObj();
		SetBoxEff(true);
		
		WindowsMngr.getInstance().closeWindow(WindowsID.LOADING);
		if(idShow < 1)
		{
			Animation camera_animation = _camera.gameObject.GetComponent<Animation>();
			if(camera_animation == null)
			{
				AnimationClip clip = LoadMngr.getInstance().GetObject(_url_animation) as AnimationClip;
				camera_animation = _camera.gameObject.AddComponent<Animation>();
				camera_animation.AddClip(clip,_camera_animatin_name);
			}
			camera_animation.wrapMode = WrapMode.Once;
			camera_animation.Play(_camera_animatin_name);
		}
		
		if(_reset)
		{
			_reset = false;
			ResetFinish();
		}
		else
		{
			PlayerMusic();
		}
	}
	
	private static void PlayerMusic()
	{
		TypeCsvTown csvTown = ManagerCsv.getTown(1);
		if(null == csvTown)
			return;
		if(null != csvTown.musicSence && "#" != csvTown.musicSence)
			ManagerAudio.playMusic(csvTown.musicSence);
	}
	
	public static Vector3 GetNextLevelPosition()
	{
		if(_levelObj.ContainsKey(DataMode.infoTBC.id))
		{
			return _levelObj[DataMode.infoTBC.id].transform.position;
		}
		return Vector3.zero;
	}
	
	#region 云
	/// 初始化 云的数据
	private static void IniYun()
	{
		_cache_yun.Clear();
		_cache_yun_scale.Clear();
		_root_yun = GameObject.Find(_str_root_yun);
		_root_yun0 = _root_yun.transform.Find(_str_yun0).gameObject;
		_root_yun1 = _root_yun.transform.Find(_str_yun1).gameObject;
		_root_yun2 = _root_yun.transform.Find(_str_yun2).gameObject;
		_root_yun3 = _root_yun.transform.Find(_str_yun3).gameObject;
		
		List<GameObject> temp = new List<GameObject>();
		temp.Add(_root_yun0);
		temp.Add(_root_yun1);
		temp.Add(_root_yun2);
		temp.Add(_root_yun3);
		
		List<GameObject> childs = null;
		foreach(GameObject obj in temp)
		{
			for(int i = 0; i < obj.transform.childCount; i++)
			{
				if(!_cache_yun.ContainsKey(obj))
				{
					_cache_yun.Add(obj, new List<GameObject>());
				}
				childs = _cache_yun[obj];
				childs.Add(obj.transform.GetChild(i).gameObject);
				_cache_yun_scale.Add(obj.name + "_" + obj.transform.GetChild(i).name,obj.transform.GetChild(i).localScale);
			}
		}
	}
	
	/// 初始化 云
	private static void SetYun()
	{
		_root_yun0.SetActive(DataMode.infoTBC.idBox < 3);
		_root_yun1.SetActive(DataMode.infoTBC.idBox < 6);
		_root_yun2.SetActive(DataMode.infoTBC.idBox < 9);
		_root_yun3.SetActive(DataMode.infoTBC.idBox < 13);
		SetAllItem(_root_yun0,true,null);
		SetAllItem(_root_yun1,true,null);
		SetAllItem(_root_yun2,true,null);
		SetAllItem(_root_yun3,true,null);
	}
	
	/// 隐藏和显示云 设置云的状态
	private static void SetAllItem(GameObject target, bool show, EventDelegate.Callback onFinish)
	{
		TweenScale tween = null;
		List<GameObject> temp = _cache_yun[target];
		for(int i = 0; i < temp.Count; i++)
		{
			if(show)
			{
				temp[i].transform.localScale = _cache_yun_scale[target.name + "_" + temp[i].name];
			}
			else
			{
				tween = TweenScale.Begin<TweenScale>(temp[i],2f);
				tween.from = temp[i].transform.localScale;
				tween.to = Vector3.zero;
				tween.style = UITweener.Style.Once;
			}
		}
		if(tween != null)
		{
			tween.onFinished.Clear();
			tween.AddOnFinished(onFinish);
		}
		else if(onFinish != null)
		{
			onFinish();
		}
	}
	
	/// 隐藏不需要的云
	public static void DisableYun()
	{
		if(DataMode.infoTBC.idBox >= 3 &&  _root_yun0.activeSelf)
		{
			SetAllItem(_root_yun0,false,SetYun);
		}
		if(DataMode.infoTBC.idBox >= 6 && _root_yun1.activeSelf)
		{
			SetAllItem(_root_yun1,false,SetYun);
		}
		if(DataMode.infoTBC.idBox >= 9 && _root_yun2.activeSelf)
		{
			SetAllItem(_root_yun2,false,SetYun);
		}
		if(DataMode.infoTBC.idBox >= 13 && _root_yun3.activeSelf)
		{
			SetAllItem(_root_yun3,false,SetYun);
		}
	}
	
	#endregion
	
	
	/// 创建相机
	private static void CreatCamera(GameObject obj)
	{
		GameObject temp = new GameObject();
		temp.name = "CameraExpedition";
		temp.transform.position = obj.transform.position;
		temp.transform.position += new Vector3(0f,10.36f,13.26f);
		temp.transform.eulerAngles = new Vector3(47.7f,180f,0f);
		Camera camera = temp.AddComponent<Camera>();
		camera.gameObject.AddComponent("FlareLayer");
		camera.cullingMask = 1 ;
		camera.clearFlags = CameraClearFlags.Depth;
		camera.fieldOfView = 60f;
		
		//却很多东西 例如 UICamera脚本  Ty
		_camera = temp.gameObject.AddComponent<UIExpeditionCamera>();
	}
	
	/// 移动到下一个点
	public static void MoveToLevel()
	{
		SetEff();
		
		bool canMove = false;
		List<Vector3> paths = new List<Vector3>();
		if(idShow == 0)
		{
			idShow = 1;
			canMove = true;
			for(int i = 0; i < _beginPoint.Count; i++)
			{
				paths.Add(_beginPoint[i]);
			}
		}
		else if(DataMode.infoTBC.id == DataMode.infoTBC.idBox) 
		{
			if(idShow < DataMode.infoTBC.id)
			{
				canMove = true;
				List<Vector3> temp;
				
				if(idShow == 1)
				{
					paths.Add(_beginPoint[_beginPoint.Count-1]);
				}
				else if(idShow-1 < _position.Count)
				{
					temp = _position[(idShow-1).ToString()];
					paths.Add(temp[temp.Count-1]);
				}
				else
				{
					return;
				}
				
				temp = _position[(idShow).ToString()];
				for(int i = 0; i < temp.Count; i++)
				{
					paths.Add(temp[i]);
				}
				idShow = DataMode.infoTBC.id;
			}
			else
			{
				canMove = false;
			}
		}
		else
		{
			canMove = false;
		}
		
		if(Mode != null && canMove)
		{
			Mode.GetComponent<UIPlayerAutoMove>().MoveNext(paths);
		}
	}
	/// 获得所有关卡
	public static List<GameObject> GetLevelObj()
	{
		List<GameObject> temp = new List<GameObject>();
		for(int i = 0; i < _levelObj.Count; i++)
		{
			temp.Add(_levelObj[i+1]);
		}
		return temp;
	}
	/// 获得所有的宝箱
	public static List<GameObject> GetBoxRewardObj()
	{
		List<GameObject> temp = new List<GameObject>();
		for(int i = 0; i < _boxObj.Count; i++)
		{
			temp.Add(_boxObj[i+1]);
		}
		return temp;
	}
	
	/// 设置模型的collider
	public static void SetLevel()
	{
		if(_levelObj != null && _levelObj.Count > 0)
		{
			Collider co;
			Transform eff;
			SkinnedMeshRenderer render;
			foreach(int key in _levelObj.Keys)
			{
				if(_levelObj[key] != null)
				{
					co = _levelObj[key].GetComponentInChildren<Collider>();
					if(co == null)
					{
						continue;
					}
					
					eff = co.transform.Find("eff_expedition_0");
					if(eff == null)
					{
						eff = co.transform.Find("eff_expedition_1");
					}
					if(eff == null)
					{
						eff = co.transform.Find("eff_expedition_2");
					}
					
					render = co.GetComponentInChildren<SkinnedMeshRenderer>();
					
					if(key < DataMode.infoTBC.id)
					{
						if(eff != null && eff.name != "eff_expedition_0")
						{
							GameObject.Destroy(eff.gameObject);
							eff = null;
						}
						if(eff == null)
						{
							eff = (GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl("shadowGreen"))) as GameObject).transform;
							eff.name = "eff_expedition_0";
						}
						if(render.materials.Length > 1)
						{
							render.materials = new Material[]{render.materials[0]};
						}
					}
					else if(key == DataMode.infoTBC.id)
					{
						if(eff != null && eff.name != "eff_expedition_1")
						{
							GameObject.Destroy(eff.gameObject);
							eff = null;
						}
						if(eff == null)
						{
							eff = (GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl("shadowBoss"))) as GameObject).transform;
							eff.name = "eff_expedition_1";
						}
//						Material mater = new Material(Shader.Find("Character/Ghost Shader"));
//						mater.SetColor("_RimColor",new Color(0f,139f/255f,1f,0f));
//						mater.SetFloat("_RimPower",0.4f);
//						mater.SetFloat("_Brightness",2f);
//						render.materials = new Material[]{render.material,mater};
					}
					else
					{
						if(eff != null && eff.name != "eff_expedition_2")
						{
							GameObject.Destroy(eff.gameObject);
							eff = null;
						}
						if(eff == null)
						{
							eff = (GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getInfoNameUrl("shadowRed"))) as GameObject).transform;
							eff.name = "eff_expedition_2";
						}
						
						if(render.materials.Length > 1)
						{
							render.materials = new Material[]{render.materials[0]};
						}
					}
					
					if(eff != null)
					{
						eff.parent = co.gameObject.transform;
						NGUITools.SetChildLayer(eff.transform,co.gameObject.layer);
//						eff.transform.localPosition = Vector3.zero;
						eff.transform.localPosition = new Vector3(0,0.065f,0);
						eff.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
						TweenRotation tween = TweenRotation.Begin<TweenRotation>(eff.gameObject,5f);
						tween.from = Vector3.zero;
						tween.to = new Vector3(0,720,0);
						tween.style = UITweener.Style.Loop;
					}
						
					if(key > DataMode.infoTBC.id)
					{
						co.enabled = false;
					}
					else
					{
						co.enabled = true;
					}
				}
			}
		}
	}
	
	/// 清理
	public static void Clear()
	{
		if(_levelObj != null)
		{
			foreach(int key in _levelObj.Keys)
			{
				GameObject.Destroy(_levelObj[key]);
			}
			_levelObj.Clear();
		}
		
		if(_boxObj != null)
		{
			foreach(int key in _boxObj.Keys)
			{
				GameObject.Destroy(_boxObj[key]);
			}
			_boxObj.Clear();
		}
		_beginPoint.Clear();
		_position.Clear();
		
		if(_eff != null)
		{
			GameObject.Destroy(_eff.gameObject);
			_eff = null;
		}
		
		_root_yun = null;
		_root_yun0 = null;
		_root_yun1 = null;
		_root_yun2 = null;
		_root_yun3 = null;
		_cache_yun.Clear();
		_cache_yun_scale.Clear();
		_reset = false;
		if(Mode != null)
		{
			GameObject.Destroy(Mode);
			Mode = null;
		}

		_iniMode = false;
	    _iniSence = false;
		
		if(_camera != null)
		{
			GameObject.Destroy(_camera.gameObject);
			_camera = null;
		}
		if(_eff_box_active != null)
		{
			GameObject.Destroy(_eff_box_active);
			_eff_box_active = null;
		}
		if(_eff_box_open != null)
		{
			GameObject.Destroy(_eff_box_open);
			_eff_box_open = null;
		}
	
		if(_view != null)
		{
			GameObject.Destroy(_view);
			_view = null;
		}

//		m_reopenMsgActive = null;

		if(_defBoxObj != null)
		{
			foreach(int key in _defBoxObj.Keys)
			{
				GameObject.Destroy(_defBoxObj[key]);
			}
			_defBoxObj.Clear();
		}

		if(_defObj != null)
		{
			foreach(int key in _defObj.Keys)
			{
				GameObject.Destroy(_defObj[key]);
			}
			_defObj.Clear();
		}

		if(_defBoxEff != null)
		{
			_defBoxEff.Clear();
		}
	}
	
	/// 重置
	public static void Reset(OnResetFinish onfinish)
	{
		if(_levelObj != null)
		{
			foreach(int key in _levelObj.Keys)
			{
				GameObject.Destroy(_levelObj[key]);
			}
			_levelObj.Clear();
		}
		
		if(_boxObj != null)
		{
			foreach(int key in _boxObj.Keys)
			{
				GameObject.Destroy(_boxObj[key]);
			}
			_boxObj.Clear();
		}

		if(_defBoxObj != null)
		{
			foreach(int key in _defBoxObj.Keys)
			{
				GameObject.Destroy(_defBoxObj[key]);
			}
			_defBoxObj.Clear();
		}
		
		if(_defObj != null)
		{
			foreach(int key in _defObj.Keys)
			{
				GameObject.Destroy(_defObj[key]);
			}
			_defObj.Clear();
		}
		
		if(_defBoxEff != null)
		{
			_defBoxEff.Clear();
		}

		if(Mode != null)
		{
			UIPlayerAutoMove auto = Mode.GetComponent<UIPlayerAutoMove>();
			auto.Stop();
		}

		ExpeditionWinWindow.needShow = false;
		_reset = true;
		onfishReset = onfinish;
		idShow = 0;
		BeginLoad();
	}
	/// 重置回调
	private static void ResetFinish()
	{
		if(onfishReset != null)
		{
			onfishReset();
		}
	}
	
	private static void SetEff()
	{
		if(_eff != null && _levelObj != null && _boxObj != null)
		{
			Transform target = null;
			if(DataMode.infoTBC.id == DataMode.infoTBC.idBox && _levelObj.ContainsKey(DataMode.infoTBC.id) &&
				_levelObj[DataMode.infoTBC.id] != null)	
			{
				_eff.SetActive(true);
				target = _levelObj[DataMode.infoTBC.id].transform;
				_eff.transform.parent = target;
				_eff.transform.localPosition = new Vector3(0,0.55f,0);
			}
			else if(_boxObj.ContainsKey(DataMode.infoTBC.idBox) &&
				_boxObj[DataMode.infoTBC.idBox] != null)	
			{
				_eff.SetActive(true);
				target = _boxObj[DataMode.infoTBC.idBox].transform;
				_eff.transform.parent = target;
				_eff.transform.localPosition = new Vector3(0,1.6f,0);
			}
			else
			{
				_eff.SetActive(false);
			}
		}
	}
	
	private static void SetBoxEff(bool show)
	{
		if(_boxObj != null && _boxObj.Count > 0 && _eff_box_active != null)
		{
			if(show)
			{
				if(DataMode.infoTBC.id > DataMode.infoTBC.idBox)
				{
					GameObject obj = _boxObj[DataMode.infoTBC.idBox];
					_eff_box_active.transform.parent = obj.transform;
					_eff_box_active.transform.localPosition = new Vector3(0,0.35f,0);
					_eff_box_active.transform.localScale = Vector3.one;
					Transform[] tran = obj.transform.GetComponentsInChildren<Transform>();
					foreach(Transform t in tran)
					{
						if(t.name.Length > 5 && t.name.Substring(0,5) == "Dummy")
						{
							_eff_box_active.transform.parent = t.transform;
							_eff_box_active.SetActive(true);
							break;
						}
					}
				}
				else
				{
					_eff_box_active.SetActive(false);
				}
			}
			else
			{
				_eff_box_active.SetActive(false);
			}
		}
	}
	
	public static void DisableBox()
	{
		for(int i = 1; i < DataMode.infoTBC.idBox; i++)
		{
			_boxObj[i].SetActive(false);
		}
		UITipMngr.getInstance().HideAll();
	}
	
	/// 打开宝箱
	public static Animation OpenBox()
	{
		Animation animation = null;
		if(DataMode.infoTBC.id > DataMode.infoTBC.idBox && _boxObj != null && _boxObj.Count > 0)
		{
			animation = _boxObj[DataMode.infoTBC.idBox].GetComponentInChildren<Animation>();
			animation.wrapMode = WrapMode.Once;
			animation.CrossFade(_box_ani_open);
			if(_eff_box_open != null)
			{
				_eff_box_open.transform.parent = _boxObj[DataMode.infoTBC.idBox].transform;
				_eff_box_open.transform.localPosition = new Vector3(0,0.5f,0);
				_eff_box_open.transform.localScale = Vector3.one;
				_eff_box_open.SetActive(false);
				_eff_box_open.SetActive(true);
			}
			SetBoxEff(false);
		}
		return animation;
	}	
	
	public static void ShowWin()
	{
		if(_showWin)
		{
			_showWin = false;
			SuperUI.closeAll();
			WindowsMngr.getInstance().getWindow(WindowsID.EXPEDITION_WIN).onMessage(null);
		}
	}

	////初始换海山防守队伍
	private static void CreatObjDef()
	{
		InfoTBCCupDefHero hero = null;
		GameObject obj = null;
		TypeCsvHero csvhero = null;

		for(int i = 0; i < 5; i++)
		{
			hero = DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(i);
			if(hero != null)
			{
				if(!hero.pass)
				{
					obj = LoadHeroMngr.getHeroObj(hero.idCsv);
					csvhero = ManagerCsv.getHero(hero.idCsv);
					CapsuleCollider collider = obj.AddComponent<CapsuleCollider>();
					collider.center = new Vector3(0f, 1f, 0f);
					collider.height = 2f;
					collider.radius = 0.8f;
					obj.transform.localScale = obj.transform.localScale * csvhero.scale * 0.7f;
					_defObj.Add(i,obj);
				}
				else if(!hero.reward)
				{
					obj = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(_url_box_high_full)) as GameObject;
					Animation animation = obj.GetComponentInChildren<Animation>();
					animation.wrapMode = WrapMode.Loop;
					animation.CrossFade(_box_ani_jump);
					_defBoxObj.Add(i,obj);
				}
				else if(hero.showbox)
				{
					obj = GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(_url_box_high_full)) as GameObject;
					Animation animation = obj.GetComponentInChildren<Animation>();
					animation.wrapMode = WrapMode.Loop;
					animation.CrossFade(_box_ani_opened);
					_defBoxObj.Add(i,obj);
				}
			}
		}
		SetDefBoxEff();
	}

	private static void SetDefObj()
	{
		GameObject obj = GameObject.Find(_def_position);
		GameObject[] temp = new GameObject[5];
		for(int i = 0; i < obj.transform.childCount; i++)
		{
			int index = GMath.toInt(obj.transform.GetChild(i).gameObject.name) - 1;
			if(index >= 0 && index <= 4)
			{
				temp[index] = obj.transform.GetChild(i).gameObject;
			}
		}

		InfoTBCCupDefHero hero = null;

		for(int i = 0; i < 5; i++)
		{
			hero = DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(i);
			if(hero != null)
			{
				if(!hero.pass)
				{
					if(_defObj.ContainsKey(i))
					{
						obj = _defObj[i];
						obj.transform.position = temp[i].transform.position;
					}
				}
				else
				{
					if(_defBoxObj.ContainsKey(i))
					{
						obj = _defBoxObj[i];
						obj.transform.position = temp[i].transform.position;
					}
				}
			}
		}
	}

	/// 打开打败防守队伍之后打开宝箱
	public static Animation OpenDefBox(int local)
	{
		Animation animation = null;
		if(_defBoxObj.ContainsKey(local))
		{
			GameObject obj = _defBoxObj[local];
			animation = obj.GetComponentInChildren<Animation>();
			animation.wrapMode = WrapMode.Once;
			animation.CrossFade(_box_ani_open);
			if(_eff_box_open != null)
			{
				_eff_box_open.transform.parent = obj.transform;
				_eff_box_open.transform.localPosition = new Vector3(0,0.5f,0);
				_eff_box_open.transform.localScale = Vector3.one;
				_eff_box_open.SetActive(false);
				_eff_box_open.SetActive(true);
			}
			return animation;
		}
		return animation;
	}

	/// 获得所有的宝箱(防守)
	public static List<GameObject> GetDefBoxObj()
	{
		List<GameObject> temp = new List<GameObject>();
		for(int i = 0; i < 5; i++)
		{
			if(_defBoxObj.ContainsKey(i))
			{
				temp.Add(_defBoxObj[i]);
			}
		}
		return temp;
	}

	/// 获得所有的防守英雄
	public static List<GameObject> GetDefHeroObj()
	{
		List<GameObject> temp = new List<GameObject>();
		for(int i = 0; i < 5; i++)
		{
			if(_defObj.ContainsKey(i))
			{
				temp.Add(_defObj[i]);
			}
		}
		return temp;
	}

	public static int getIndexDefLevel(GameObject obj)
	{
		if(obj != null)
		{
			foreach(int key in _defObj.Keys)
			{
				if(_defObj[key] == obj)
				{
					return key;
				}
			}
		}
		return -1;
	}

	public static int getIndexDefBox(GameObject obj)
	{
		if(obj != null)
		{
			foreach(int key in _defBoxObj.Keys)
			{
				if(_defBoxObj[key] == obj)
				{
					return key;
				}
			}
		}
		return -1;
	}

	private static void SetDefBoxEff()
	{
		if(_eff_box_active == null || _defBoxObj == null || _defBoxObj.Count == 0)
		{
			return;
		}
		InfoTBCCupDefHero hero = null;
		GameObject obj = null;
		foreach(int index in _defBoxObj.Keys)
		{
			hero = DataMode.infoTBC.infoTBCCup.GetDefHeroInfo(index);
			if(hero != null)
			{
				if(hero.pass && !hero.reward)
				{
					GameObject eff = null;
					if(!_defBoxEff.ContainsKey(index))
					{
						eff = GameObject.Instantiate(_eff_box_active) as GameObject;
						_defBoxEff.Add(index, eff);
					}
					eff = _defBoxEff[index];
					eff.SetActive(true);
					obj = _defBoxObj[index];
					eff.transform.parent = obj.transform;
					eff.transform.localPosition = new Vector3(0,0.35f,0);
					eff.transform.localScale = Vector3.one;
					Transform[] tran = obj.transform.GetComponentsInChildren<Transform>();
					foreach(Transform t in tran)
					{
						if(t.name.Length > 5 && t.name.Substring(0,5) == "Dummy")
						{
							eff.transform.parent = t.transform;
							eff.SetActive(true);
							break;
						}
					}
				}
			}
		}
	}

	public static void SetDefBoxEffClose(int local)
	{
		if(_defBoxEff.ContainsKey(local))
		{
			if(_defBoxEff[local] != null)
			{
				_defBoxEff[local].SetActive(false);
			}
		}
	}
}
