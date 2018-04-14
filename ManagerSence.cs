using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 场景管理
public class ManagerSence
{

	private static List<Animation> _animationArr = new List<Animation>();
	public static void clearAnimation()
	{
		_animationArr.Clear();
	}
	public static void playAnimation()
	{
		foreach(Animation anim in _animationArr)
		{
			if(null == anim)
				continue;
//			anim.Play();
			try
			{
				anim[anim.clip.name].speed = 1f;
			}catch(System.Exception sE){}
		}
	}
	public static void stopAnimation()
	{
		foreach(Animation anim in _animationArr)
		{
			if(null == anim)
				continue;
//			anim.Stop();
			try
			{
				anim[anim.clip.name].speed = 0f;
			}catch(System.Exception sE){}
		}
	}
	/// 场景锁定
	private static bool _isLockSence = false;
	public static bool isLockSence{get {return _isLockSence;} 
		set {
			_isLockSence = value;
//			_completeSence = null;
//			_completeSenceCsv = null;
		
		}
	}
	/// 场景资源缓存
	private static Dictionary<string, Object> _senceMemory = new Dictionary<string, Object>();
	/// 完成场景后调用函数
	private static Function _completeSence = null;
	private static Function _completeSenceCsv = null;
	/// 开始函数 
	/// url为相对对峙
	public static void createSence(string url, Function sComplete){createSence(url, sComplete, null);}
	public static void createSence(string url, Function sComplete, Function sCompleteCsv)
	{
		if(_isLockSence)
		{
			UtilLog.LogError("Sence is lock");
			return;
		}
		/// 场景加锁
		_isLockSence = true;
		/// 加载场景
		_completeSence = sComplete;
		_completeSenceCsv = sCompleteCsv;
		/// 加载csv表格
		ManagerCsv.loadCsvSence(url, completeCsv);
	}	
	/// =========================================================================================================================
	/// 场景加载
	
	/// 完成csv加载
	private static void completeCsv()
	{
		List<string> urls = new List<string>();
		UtilCsvReader csvReader = ManagerCsv.getCsvSence();
		int index = 1;
		/// 加载 遍历表格,获得路径
		while(true)
		{
			Dictionary<string, string> search = csvReader.search("id", index);
			index ++;
			if(null == search)
				break;
			
			/// 低配置不下载的资源
			if(!ManagerDevice.isOpenGoodTown)
				if(search["url_view"].IndexOf("dipeiyouhua") != -1)
					continue;
				
				
			/// 装载场景模型
			urls.Add(ConfigUrl.ROOT_SENCE + search["url_view"] + ConfigUrl.EXTENSION);
			/// 加载材质球一些玩意
			string[] renders = search["renders"].Split("|".ToCharArray());
			for(int i = 3; i < renders.Length; i += 5 + 4)
			{				
				//贴图
				if("" != renders[i + 1] && "#" != renders[i + 1])
				{
					urls.Add(ConfigUrl.ROOT_SENCE + renders[i + 1] + ConfigUrl.EXTENSION);
				}
			}
		}
		/// 加载 环境贴图
		csvReader = ManagerCsv.getCsvSenceLightMaps();
		index = 1;
		while(true)
		{
			Dictionary<string, string> search = csvReader.search("id", index);
			index ++;
			if(null == search)
				break;
			/// 装载场景模型
			if(null != search["texture_far"] && "#" != search["texture_far"])
				urls.Add(ConfigUrl.ROOT_SENCE + search["texture_far"] + ConfigUrl.EXTENSION);
			if(null != search["texture_near"] && "#" != search["texture_near"])
				urls.Add(ConfigUrl.ROOT_SENCE + search["texture_near"] + ConfigUrl.EXTENSION);
		}
		/// 确定场景是否加载
		bool isLoad = false;
		foreach(string myUrl in urls)
		{
			if(!LoadMngr.getInstance().isHasAsset(myUrl))
			{
				isLoad = true;
				break;
			}
		}
		/// 加载 场景
		if(isLoad)
		{
			LoadMngr.getInstance().load(urls.ToArray(), completeSence);
			/// csv地址加载完毕
			if(null != _completeSenceCsv)
				_completeSenceCsv();
		} else {
			
			/// csv地址加载完毕
			if(null != _completeSenceCsv)
				_completeSenceCsv();
			completeSence();
		}
		
	}
	/// 全部加载完成 创建场景
	private static void completeSence(double loader_id = 0)
	{
		///数据挖掘 yxh 14.9.15
		if(ManagerServer.isLoadStatue)
		{
			if(ManagerServer.isLoadStatueNew)
			{
				if(ManagerServer.isLoadStatueBefor == false)
				{
					DataModeServer.sendLoadStatues(1,1);
					ManagerServer.isLoadStatueBefor = true;
				}
				else
				{
					DataModeServer.sendLoadStatues(1,3);
				}
			}
			else
			{
				DataModeServer.sendLoadStatues(1,2);
			}
		}
		
		/// 场景解锁
		_isLockSence = false;
		/// 场景创建
		createSence();
		/// 设置 light map
		createLightmap();
		/// 设置 环境雾气
		createRenderseting();
		/// 停止场景中动画
		stopAnimation();
		/// 场景回调
		if(null != _completeSence)
			_completeSence();
		/// 侦听滞空
		_completeSence = null;
	}
	/// 场景创建
	private static void createSence()
	{
		
		/// 场景解锁
		UtilCsvReader csvReader = ManagerCsv.getCsvSence();
		/// 物件索引
		int indexObject = 1;
		/// 遍历表格,获得路径
		while(true)
		{
			/// 列表搜索
			Dictionary<string, string> search = csvReader.search("id", indexObject);
			/// 指向下一个物件
			indexObject ++;
			/// 如果没有搜索到 返回
			if(null == search)
				break;
			/// 创建物件
			try
			{
				if(!ManagerDevice.isOpenGoodTown)
					if(search["url_view"].IndexOf("dipeiyouhua") != -1)
						continue;
				
				/// 创建白膜
				GameObject obj = (GameObject)GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.ROOT_SENCE + search["url_view"] + ConfigUrl.EXTENSION));
				obj.transform.localPosition = new Vector3(GMath.toFloat(search["x"]), GMath.toFloat(search["y"]), GMath.toFloat(search["z"]));
				obj.transform.localRotation = Quaternion.Euler(GMath.toFloat(search["rotationX"]), GMath.toFloat(search["rotationY"]), GMath.toFloat(search["rotationZ"]));
				obj.transform.localScale = new Vector3(GMath.toFloat(search["scaleX"]), GMath.toFloat(search["scaleY"]), GMath.toFloat(search["scaleZ"]));
				Animation[] animArr = obj.GetComponentsInChildren<Animation>();
				if(null != animArr || animArr.Length > 0)
					GUtil.addRange<Animation>(_animationArr, animArr);
				/// 创建renderer数据
				string[] rendInfo = search["renders"].ToString().Split("|".ToCharArray());
				Renderer[] renders = obj.GetComponentsInChildren<Renderer>(true);
				//遍历材质信息
				for(int index = 0; index < rendInfo.Length - 1; index += 5 + 4)
				{
					//跳过渲染
					if(rendInfo[index + 3] == "" || rendInfo[index + 3] == "#")
						continue;
					//遍历renders
					foreach(Renderer rend in renders)
					{
						rend.lightmapIndex = GMath.toInt(rendInfo[index + 2]);
						rend.receiveShadows = true;
						obj.isStatic = true;
						//如果材质球是这个渲染器的
						if(rend.name.Replace("(Clone)", "") == rendInfo[index])
						{
							int i = GMath.toInt(rendInfo[index + 1]);
							if(null == rend.sharedMaterials[i])
								continue;
#if UNITY_IPHONE || UNITY_WEBPLAYER || UNITY_ANDROID
							/// 切换用新的shader
							if(null != rend.sharedMaterials[i].shader)
							{
								bool isChange = false;
								/// 有些乱起八糟的shader
								if(rend.sharedMaterials[i].shader.name == "T4MShaders/ShaderModel2/MobileLM/T4M 4 Textures for Mobile")
									isChange = true;
								if(rend.sharedMaterials[i].shader.name == "T4MShaders/ShaderModel2/MobileLM/T4M 3 Textures for Mobile")
									isChange = true;
								if(rend.sharedMaterials[i].shader.name == "T4MShaders/ShaderModel2/MobileLM/T4M 2 Textures for Mobile")
									isChange = true;
								
								/// 确定修改shader
								if(isChange)
								{
									foreach(Shader shader in ShaderImport.valueObj.shaders)
									{
										if(null == shader)
											continue;
										if(shader.name != rend.sharedMaterials[i].shader.name)
											continue;
										rend.sharedMaterials[i].shader = shader;
										break;
									}
								}
							}
#endif
							
							if(rendInfo[index + 4] != "" && rendInfo[index + 4] != "#")
							{
								Material[] materials = rend.sharedMaterials;
								materials[i] = materials[i];
								// changed by ssy no cache
								/// 缓存图片
								string urlTexture = ConfigUrl.ROOT_SENCE + rendInfo[index + 4] + ConfigUrl.EXTENSION;
								/// 指定到图片上
								try
								{
									if(null == (Texture)LoadMngr.getInstance().GetObject(urlTexture))
									{
										UtilLog.LogError("url texture is Null url = " + urlTexture);
									}
								}catch(System.Exception e){UtilLog.LogError("url texture is Error url = " + urlTexture);}
								
								materials[i].mainTexture = (Texture)LoadMngr.getInstance().GetObject(urlTexture);
								// changed end
								rend.sharedMaterials = materials;
							}
							//设置灯光题图偏移
							Vector4 lightmapoffset = new Vector4();
							lightmapoffset.x = GMath.toFloat(rendInfo[index + 5]);
							lightmapoffset.y = GMath.toFloat(rendInfo[index + 6]);
							lightmapoffset.z = GMath.toFloat(rendInfo[index + 7]);
							lightmapoffset.w = GMath.toFloat(rendInfo[index + 8]);
							rend.lightmapTilingOffset = lightmapoffset;
						}		
					}
				}
			}catch(System.Exception e){UtilLog.LogError(e);}
		}
	}
	/// lightmap 设置
	private static void createLightmap()
	{
		List<LightmapData> lightMaps = new List<LightmapData>();
		UtilCsvReader csvReader = ManagerCsv.getCsvSenceLightMaps();
		int index = 1;
		while(true)
		{
			Dictionary<string, string> search = csvReader.search("id", index);
			index ++;
			if(null == search)
				break;
			LightmapData lightMapData = new LightmapData();
			/// 装载环境光贴图
			if(null != search["texture_far"] && "#" != search["texture_far"])
				lightMapData.lightmapFar = (Texture2D)LoadMngr.getInstance().GetObject(ConfigUrl.ROOT_SENCE + search["texture_far"] + ConfigUrl.EXTENSION);
			if(null != search["texture_near"] && "#" != search["texture_near"])
				lightMapData.lightmapFar = (Texture2D)LoadMngr.getInstance().GetObject(ConfigUrl.ROOT_SENCE + search["texture_near"] + ConfigUrl.EXTENSION);
			lightMaps.Add(lightMapData);
		}
		LightmapSettings.lightmaps = lightMaps.ToArray();
	}
	/// 环境光属性设置
	private static void createRenderseting()
	{
		/// 设置环境观
		UtilCsvReader csvReader = ManagerCsv.getCsvSenceRenderSetting();
		/// 检索
		Dictionary<string, string> row = csvReader.search("id", "0");
		//id,fog,col_r,col_g,col_b,col_a,mode,density,linear_start,liner_end
		//雾化
		RenderSettings.fog = true;
		RenderSettings.fogColor = new Color(
			GMath.toFloat(row["col_r"]),
			GMath.toFloat(row["col_g"]),
			GMath.toFloat(row["col_b"]),
			GMath.toFloat(row["col_a"]));
		/// 2014.10.29 相机也要改成环境颜色
		if(null != GameObject.Find("_MainCamera"))
		{
			Camera[] cameraArr = GameObject.Find("_MainCamera").GetComponents<Camera>();
//			Camera.main.backgroundColor = RenderSettings.fogColor;
			if(null != cameraArr)
			{
				foreach(Camera camera in cameraArr)
				{
					camera.backgroundColor = RenderSettings.fogColor;
				}
			}
		}
		//模式
		switch(row["mode"].ToString())
		{
			case "Exponential":
				RenderSettings.fogMode = FogMode.Exponential;
				break;
			case "Linear":
				RenderSettings.fogMode = FogMode.Linear;
				break;
			case "ExponentialSquared":
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				break;
		}
		//
		RenderSettings.fogDensity = GMath.toFloat(row["density"]);
		RenderSettings.fogStartDistance = GMath.toFloat(row["linear_start"]);
		RenderSettings.fogEndDistance = GMath.toFloat(row["liner_end"]);
		
		if(row.ContainsKey("light_r"))
		{
//			Color color = new Color(
//				GMath.toFloat(row["light_r"]),
//				GMath.toFloat(row["light_g"]),
//				GMath.toFloat(row["light_b"]),
//				GMath.toFloat(row["light_a"])
//			);
//			RenderSettings.ambientLight = color;
		}
		if(row.ContainsKey("halo_strength"))
		{
			RenderSettings.haloStrength = GMath.toFloat("halo_strength");
		}
		if(row.ContainsKey("flare_strength"))
		{
			RenderSettings.flareStrength = GMath.toFloat("flare_strength");
		}
	}
	
	
	private static GameObject _lightObject; 
	/// 创建灯光
	public static void createLight()
	{

//		return;
		if(DataMode.infoSetting.isValue(InfoSettingEnum.isCloseLight))
			return;
		
		if(null != _lightObject)
			return;
		_lightObject = new GameObject("_____Light");
		Light light = _lightObject.AddComponent<Light>();
		light.type = LightType.Directional;
		light.shadows = LightShadows.Hard;
		light.shadowStrength = 0.4f;
		light.intensity = 0.01f;
		light.shadowBias = 2f;
		light.cullingMask = 1 << Config.LAYER_GAME_SENCE;
		
		_lightObject.transform.LookAt(new Vector3(-1f, -1f, 1f));
	}
	/// 灯开关
	public static void setLight(bool isTurn)
	{
		if(null == _lightObject)
			return;
		_lightObject.SetActive(isTurn);
	}
	/// 加入天气效果
	private static double _watherID;
	private static string _wather;
	public static void createWeather(string[] sWatherArr)
	{
		if(null == sWatherArr)
			return;
		
		int index = Random.Range(0, sWatherArr.Length);
		if(string.IsNullOrEmpty(sWatherArr[index]) || sWatherArr[index] == "#")
			return;
		_wather = sWatherArr[index];
		_watherID = LoadMngr.getInstance().load(ConfigUrl.getWeatherUrl(_wather), createWeatherHD, LoadMngr.ELoadPriority.EFront);
	}
	private static void createWeatherHD(double sID)
	{
		if(_watherID != sID)
			return;
		GameObject.Instantiate(LoadMngr.getInstance().getObjectGame(ConfigUrl.getWeatherUrl(_wather)), Vector3.zero, Quaternion.identity);
	}
}
