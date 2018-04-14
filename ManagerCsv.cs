using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text;

/// 我的数据表格存放地址
public class ManagerCsv
{	
	
	public static bool isCsvFile = true;
	/// 获得csv全部文件加载
	protected static UtilCsvReader _info;
	/// csv文件列表信息
	protected static Dictionary<string, UtilCsvReader> _csvTables = new Dictionary<string, UtilCsvReader>();
	/// 加载锁定
	protected static bool _isLoad = false;
	/// 数据缓存
	protected static Dictionary<string, object> _csvItemMemory = new Dictionary<string, object>();
	/// 完成回调
	private static Function _complete;
	
	
	/// 读取csv文件路径
	public static void load(Function complete)
	{
		if(_isLoad)
		{
			UtilLog.LogError("这个函数不应该调用那么多次");
			return;
		}
		_isLoad = true;
		_complete = complete;
		LoadMngr.getInstance().load(ConfigUrl.URL_INFO, completeInfo);
	}
	/// 完成的调用
	private static void completeInfo(double loader_id)
	{
		_info = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.URL_INFO));
		_info.isClearData = false;
		/// 地址表
		List<Dictionary<string, string>> urlTable = null;
		/// 用来读取 csv源文件还是assetbound的判断
		#region csv selcet
		if(isCsvFile)
			urlTable = _info.searchs("type", "csv");
		else
			urlTable = _info.searchs("type", "csvAsset");
		#endregion
		/// 地址列表
		List<string> urls = new List<string>();
		/// 遍历地址
		foreach(Dictionary<string, string> item in urlTable)
		{
#if UNITY_EDITOR && UNITY_WEBPLAYER
			if(item["value"].Substring(0, 4) == "csv/")
				urls.Add(ConfigUrl.ROOT + item["value"].Replace("csv/", "csvUnityEditor/"));
			else
				urls.Add(ConfigUrl.ROOT + item["value"]);
#else
			if(item["value"].Substring(0, 4) == "csv/" && Document.valueObj.isCsvTest)
			{
				urls.Add(ConfigUrl.ROOT + item["value"].Replace("csv/", "csvTest/"));
			}
			else
			{
				urls.Add(ConfigUrl.ROOT + item["value"]);
			}
			
#endif
		}

		//add by yxh
		urls.Add(ConfigUrl.UI_ILLEGALWORD);
		/// 加载
		LoadMngr.getInstance().load(urls.ToArray(), completeCsv);
	}
	/// csv 文件调用
	private static void completeCsv(double loader_id)
	{
		/// 地址表
		List<Dictionary<string, string>> urlTable = _info.searchs("type", "csv");
		
		List<Dictionary<string, string>> urlTableAsset = _info.searchs("type", "csvAsset");
		CsvAsset csvAsset = null;
		if(!isCsvFile)
		{
#if UNITY_EDITOR && UNITY_WEBPLAYER
			csvAsset = LoadMngr.getInstance().getObjectGame(ConfigUrl.ROOT + urlTableAsset[0]["value"].Replace("csv/", "csvUnityEditor/")).GetComponent<CsvAsset>();
#else
			if(Document.valueObj.isCsvTest)
			{
				csvAsset = LoadMngr.getInstance().getObjectGame(ConfigUrl.ROOT + urlTableAsset[0]["value"].Replace("csv/", "csvTest/")).GetComponent<CsvAsset>();
			}
			else
			{
				csvAsset = LoadMngr.getInstance().getObjectGame(ConfigUrl.ROOT + urlTableAsset[0]["value"]).GetComponent<CsvAsset>();

			}
#endif
		}
		/// 遍历地址
		foreach(Dictionary<string, string> item in urlTable)
		{
			try
			{
				
				UtilCsvReader csvReader = null;
				if(isCsvFile)
				{
#if UNITY_EDITOR && UNITY_WEBPLAYER
					if(item["value"].Substring(0, 4) == "csv/")
						csvReader = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + item["value"].Replace("csv/", "csvUnityEditor/")));
					else
						csvReader = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + item["value"]));
#else
					if(item["value"].Substring(0, 4) == "csv/" && Document.valueObj.isCsvTest)
					{
						csvReader = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + item["value"].Replace("csv/", "csvTest/")));
					}
					else
					{
						csvReader = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + item["value"]));
						
					}
#endif
				} else {
					csvReader = new UtilCsvReader(csvAsset.getText(item["value"].Replace("eff/", "").Replace("audio/bind/", "").Replace("eff2/", "")));
				}
				
				
				string key = item["value"].Substring(item["value"].LastIndexOf("/") + 1);
				key = key.Replace(".csv", "");
				
				/// 一些表格不清楚数据
				if(key == "ViewFI")
					csvReader.isClearData = false;
				if(key == "Hero")
					csvReader.isClearData = false;
				if(key == "HeroAI")
					csvReader.isClearData = false;
				if(key == "Prop")
					csvReader.isClearData = false;
				if(key == "PropEquip")
					csvReader.isClearData = false;
				if(key == "PropExp")
					csvReader.isClearData = false;
				
				_csvTables.Add(key, csvReader);
			} catch(Exception e) {Debug.LogError(item["value"] + " 表格错误\n" + e.ToString());}
		}
		SensitiveWordMngr.Read();
		/// 完成回调
		if(null != _complete)
			_complete();
	}
	
	public static void loadCsv(string csvName,string csvUrl)
	{
		try
		{
			UtilCsvReader csvReader = new UtilCsvReader(LoadMngr.getInstance().GetText(csvUrl));
			_csvTables.Add(csvName, csvReader);
		} 
		catch(Exception e) 
		{
			Debug.LogError(e);
			Debug.LogError(csvName + ".csv 表格错误");
		}
	}
	
	/// ###############################################################################################################
	/// 
	/// 加载关卡csv表格
	/// 
	
	/// 加载场景表格
	private static Function _completeCsvSence;
	/// 地址信息
	private static string _urlCsvSence;
	/// 下载索引
	private static int _loadCsvSenceIndex;
	/// 完成索引
	private static int _loadCsvSenceComplete;

	public static void clearSenceCsv()
	{
		_loadCsvSenceIndex = _loadCsvSenceComplete = 0;
	}

	/// 加载地址url
	public static void loadCsvSence(string url, Function complete)
	{
		
		
		_urlCsvSence = url;
		_completeCsvSence = complete;
		_loadCsvSenceIndex++;
		
		if(isCsvFile)
		{
			/// 下载地址表格
			List<string> urls = new List<string>();
#if UNITY_EDITOR && UNITY_WEBPLAYER
			urls.Add(ConfigUrl.ROOT + "csvUnityEditor" + url.Substring(3) + "info_lightmaps.csv");
			urls.Add(ConfigUrl.ROOT + "csvUnityEditor" + url.Substring(3) + "info_renderseting.csv");
			urls.Add(ConfigUrl.ROOT + "csvUnityEditor" + url.Substring(3) + "info_sence.csv");
#else
			if(Document.valueObj.isCsvTest)
			{
				urls.Add(ConfigUrl.ROOT + "csvTest" + url.Substring(3) + "info_lightmaps.csv");
				urls.Add(ConfigUrl.ROOT + "csvTest" + url.Substring(3) + "info_renderseting.csv");
				urls.Add(ConfigUrl.ROOT + "csvTest" + url.Substring(3) + "info_sence.csv");

			}
			else
			{
				urls.Add(ConfigUrl.ROOT + url + "info_lightmaps.csv");
				urls.Add(ConfigUrl.ROOT + url + "info_renderseting.csv");
				urls.Add(ConfigUrl.ROOT + url + "info_sence.csv");
			}


#endif

			bool isLoad = false;
			foreach(string myUrl in urls)
			{
				if(!LoadMngr.getInstance().isHasAsset(myUrl))
				{
					isLoad = true;
					break;
				}
			}
			
			/// 加载
			if(isLoad)
				LoadMngr.getInstance().load(urls.ToArray(), completeCsvSence);
			else
				completeCsvSence();
		} else {
			completeCsvSence();
		}
	}
	/// 完成csv下载
	private static void completeCsvSence(double loader_id = 0)
	{
		///数据挖掘 yxh 14.9.15
		if(ManagerServer.isLoadStatue && !ManagerServer.isLoadStatueNew)
		{
			DataModeServer.sendLoadStatues(1,1);
		}
		
		_loadCsvSenceComplete ++;
		if(_loadCsvSenceComplete != _loadCsvSenceIndex)
			return;
		if(isCsvFile)
		{
#if UNITY_EDITOR && UNITY_WEBPLAYER
			/// 地址表
			if(!_csvTables.ContainsKey("info_lightmaps"))
				_csvTables.Add("info_lightmaps", null);
			_csvTables["info_lightmaps"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + "csvUnityEditor" + _urlCsvSence.Substring(3) + "info_lightmaps.csv"));
			
			if(!_csvTables.ContainsKey("info_renderseting"))
				_csvTables.Add("info_renderseting", null);
			_csvTables["info_renderseting"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + "csvUnityEditor" + _urlCsvSence.Substring(3) + "info_renderseting.csv"));
			
			if(!_csvTables.ContainsKey("info_sence"))
				_csvTables.Add("info_sence", null);
			_csvTables["info_sence"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + "csvUnityEditor" + _urlCsvSence.Substring(3) + "info_sence.csv"));
#else
			if(Document.valueObj.isCsvTest)
			{
				if(!_csvTables.ContainsKey("info_lightmaps"))
					_csvTables.Add("info_lightmaps", null);
				_csvTables["info_lightmaps"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + "csvTest" + _urlCsvSence.Substring(3) + "info_lightmaps.csv"));
				
				if(!_csvTables.ContainsKey("info_renderseting"))
					_csvTables.Add("info_renderseting", null);
				_csvTables["info_renderseting"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + "csvTest" + _urlCsvSence.Substring(3) + "info_renderseting.csv"));
				
				if(!_csvTables.ContainsKey("info_sence"))
					_csvTables.Add("info_sence", null);
				_csvTables["info_sence"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + "csvTest" + _urlCsvSence.Substring(3) + "info_sence.csv"));

			}
			else
			{
				/// 地址表
				if(!_csvTables.ContainsKey("info_lightmaps"))
					_csvTables.Add("info_lightmaps", null);
				_csvTables["info_lightmaps"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + _urlCsvSence + "info_lightmaps.csv"));
				
				if(!_csvTables.ContainsKey("info_renderseting"))
					_csvTables.Add("info_renderseting", null);
				_csvTables["info_renderseting"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + _urlCsvSence + "info_renderseting.csv"));
				
				if(!_csvTables.ContainsKey("info_sence"))
					_csvTables.Add("info_sence", null);
				_csvTables["info_sence"] = new UtilCsvReader(LoadMngr.getInstance().GetText(ConfigUrl.ROOT + _urlCsvSence + "info_sence.csv"));
			}
			
#endif
		} else {
			List<Dictionary<string, string>> urlTableAsset = _info.searchs("type", "csvAsset");
			CsvAsset csvAsset = LoadMngr.getInstance().getObjectGame(ConfigUrl.ROOT + urlTableAsset[0]["value"]).GetComponent<CsvAsset>();

			/// 地址表
			if(!_csvTables.ContainsKey("info_lightmaps"))
				_csvTables.Add("info_lightmaps", null);
			_csvTables["info_lightmaps"] = new UtilCsvReader(csvAsset.getText(_urlCsvSence + "info_lightmaps.csv"));
			
			if(!_csvTables.ContainsKey("info_renderseting"))
				_csvTables.Add("info_renderseting", null);
			_csvTables["info_renderseting"] = new UtilCsvReader(csvAsset.getText(_urlCsvSence + "info_renderseting.csv"));
			
			if(!_csvTables.ContainsKey("info_sence"))
				_csvTables.Add("info_sence", null);
			_csvTables["info_sence"] = new UtilCsvReader(csvAsset.getText(_urlCsvSence + "info_sence.csv"));
			
		}
		/// 完成回调
		if(null != _completeCsvSence)
			_completeCsvSence();
	}
	
	/// 获得 场景物件表格
	public static UtilCsvReader getCsvSence()
	{

		return _csvTables["info_sence"];
	}
	/// 获得 环境渲染属性
	public static UtilCsvReader getCsvSenceRenderSetting()
	{
		return _csvTables["info_renderseting"];
	}
	/// 获得 lightmap数据
	public static UtilCsvReader getCsvSenceLightMaps()
	{
		return _csvTables["info_lightmaps"];
	}
	/// 获得配置文件信息
	public static UtilCsvReader getCsvInfo()
	{
		return _info;
	}
	/// ###############################################################################################################
	/// 
	/// 以下为数据表格读取数据
	/// 需要的表格自行添加
	
	
	/// 获得主角 的数据
	public static TypeCsvHero getHero(int idCsv)
	{
		/// 数据缓存部分
		string key = string.Intern(new StringBuilder("public static TypeCsvHero getHero(int idCsv) idCsv = ").Append(idCsv).ToString());
//		string key = string.Intern("public static TypeCsvHero getHero(int idCsv) idCsv = " + idCsv);
//		string key = "public static TypeCsvHero getHero(int idCsv) idCsv = " + idCsv;
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHero)_csvItemMemory[key];
		/// 返回值
		TypeCsvHero result = _csvTables["Hero"].searchAndNew<TypeCsvHero>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得主角 的数据 图集的其他信息
	public static TypeCsvHeroInfo getHeroInfo(int idHeroSame)
	{
		/// 数据缓存部分
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroInfo getHeroInfo(int idHeroSame) idHeroSame = ").Append(idHeroSame).ToString());
//		string key = "public static TypeCsvHeroInfo getHeroInfo(int idHeroSame) idHeroSame = " + idHeroSame;
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroInfo)_csvItemMemory[key];
		/// 返回值
		TypeCsvHeroInfo result = _csvTables["HeroInfo"].searchAndNew<TypeCsvHeroInfo>("idHeroSame", idHeroSame);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得主角 相同组中的数据
	public static List<TypeCsvHero> getHeroSameItem(int idSame)
	{
		/// 数据缓存部分
		string key = string.Intern(new StringBuilder("public static List<TypeCsvHero> getHeroSameItem(int idSame)idSame = ").Append(idSame).ToString());
//		string key = "public static List<TypeCsvHero> getHeroSameItem(int idSame)idSame = " + idSame;
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvHero>)_csvItemMemory[key];
		/// 返回值
		List<TypeCsvHero> result = _csvTables["Hero"].searchsT<TypeCsvHero>("idSame", idSame);
		/// 值存储
		_csvItemMemory.Add(key, result);
		
		/// 替换掉角色中的数据(缩减内存占用过量)
		string key2 = string.Intern("public static TypeCsvHero getHero(int idCsv) idCsv = ");
//		string key2 = "public static TypeCsvHero getHero(int idCsv) idCsv = ";
		for(int index = 0; index < result.Count; index++)
		{
			if(_csvItemMemory.ContainsKey(string.Intern(key2 + result[index].id)))
			   _csvItemMemory[string.Intern(key2 + result[index].id)] = result[index];
			else
				_csvItemMemory.Add(string.Intern(key2 + result[index].id), result[index]);
//			if(_csvItemMemory.ContainsKey(key2 + result[index].id))
//				_csvItemMemory[key2 + result[index].id] = result[index];
//			else
//				_csvItemMemory.Add(key2 + result[index].id, result[index]);
		}
		
		/// 返回
		return result;
	}
	
	/// 检索vip数据
	public static TypeCsvCntVIP getVIP(int vipLv)
	{
		/// 数据缓存部分
		string key = string.Intern(new StringBuilder("public static TypeCsvVIP getVIP(int vipLv) vipLv = ").Append(vipLv).ToString());
//		string key = "public static TypeCsvVIP getVIP(int vipLv) vipLv = " + vipLv;
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvCntVIP)_csvItemMemory[key];
		/// 返回值
		TypeCsvCntVIP result = _csvTables["CntVIP"].searchAndNew<TypeCsvCntVIP>("level", vipLv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得vip列表
	public static List<TypeCsvCntVIP> getVIPList()
	{
		string key = "public static List<TypeCsvCntVIP> getVIPList()";
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvCntVIP>)_csvItemMemory[key];
		int lv = 0;
		List<TypeCsvCntVIP> result = new List<TypeCsvCntVIP>();
		while(true)
		{
			TypeCsvCntVIP csv = ManagerCsv.getVIP(lv);
			if(csv == null)
			{
				break;
			}
			result.Add(csv);
			lv++;
		}
		_csvItemMemory.Add(key, result);
		return result;
	}

	/// 获得角色等级的*量数据
	public static TypeCsvHeroLv getHeroLv(int lv)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroLv getHeroLv(int lv) lv = ").Append(lv).ToString());
//		string key = "public static TypeCsvHeroLv getHeroLv(int lv) lv = " + lv;
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroLv)_csvItemMemory[key];
		/// 返回值
		TypeCsvHeroLv result = _csvTables["HeroLv"].searchAndNew<TypeCsvHeroLv>("lv", lv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得角色等级的*量数据
	public static TypeCsvHeroStar getHeroStar(int idCsvHeroSame)
	{
//		string key = "public static TypeCsvHeroStar getHeroStar(int idCsvHeroSame) idCsvHeroSame = " + idCsvHeroSame;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroStar getHeroStar(int idCsvHeroSame) idCsvHeroSame = ").Append(idCsvHeroSame).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroStar)_csvItemMemory[key];
		/// 返回值
		TypeCsvHeroStar result = _csvTables["HeroStar"].searchAndNew<TypeCsvHeroStar>("id", idCsvHeroSame);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得角色 身上进化石的属性
	public static TypeCsvHeroUp getHeroUp(int idCsvHero)
	{
//		string key = "public static TypeCsvHeroUp getHeroUp(int idCsvHero) = " + idCsvHero;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroUp getHeroUp(int idCsvHero) = ").Append(idCsvHero).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroUp)_csvItemMemory[key];
		/// 返回值
		TypeCsvHeroUp result = _csvTables["HeroUp"].searchAndNew<TypeCsvHeroUp>("id", idCsvHero);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得角色 属性的换算
	public static TypeCsvHeroAttributeMath getHeroAttributeMath(int sType)
	{
//		string key = "public static TypeCsvHeroAttributeMath getHeroAttributeMath(int sType) = ";
//		string key2 = "public static TypeCsvHeroAttributeMath getHeroAttributeMath(string sNameSprite) = ";

		string key = string.Intern("public static TypeCsvHeroAttributeMath getHeroAttributeMath(int sType) = ");
		string key2 = string.Intern("public static TypeCsvHeroAttributeMath getHeroAttributeMath(string sNameSprite) = ");
		
		if(_csvItemMemory.ContainsKey(key + sType))
			return (TypeCsvHeroAttributeMath)_csvItemMemory[key + sType];
		/// 返回值
		TypeCsvHeroAttributeMath result = _csvTables["HeroAttributeMath"].searchAndNew<TypeCsvHeroAttributeMath>("type", sType);
		/// 值存储
		_csvItemMemory.Add(key + sType, result);
		if(null != result)
			_csvItemMemory.Add(key2 + result.nameSprite, result);
		/// 返回
		return result;
	}
	/// 获得角色 属性的换算
	public static TypeCsvHeroAttributeMath getHeroAttributeMath(string sNameSprite)
	{
//		string key = "public static TypeCsvHeroAttributeMath getHeroAttributeMath(string sNameSprite) = ";
//		string key2 = "public static TypeCsvHeroAttributeMath getHeroAttributeMath(int sType) = ";

		string key = string.Intern("public static TypeCsvHeroAttributeMath getHeroAttributeMath(string sNameSprite) = ");
		string key2 = string.Intern("public static TypeCsvHeroAttributeMath getHeroAttributeMath(int sType) = ");

		if(_csvItemMemory.ContainsKey(key + sNameSprite))
			return (TypeCsvHeroAttributeMath)_csvItemMemory[key + sNameSprite];
		/// 返回值
		TypeCsvHeroAttributeMath result = _csvTables["HeroAttributeMath"].searchAndNew<TypeCsvHeroAttributeMath>("nameSprite", sNameSprite);
		/// 值存储
		_csvItemMemory.Add(key + sNameSprite, result);
		if(null != result)
			_csvItemMemory.Add(key2 + result.type, result);
		/// 返回
		return result;
	}
	
	
	/// 获得标准技能
	public static TypeCsvHeroSkillBase getHeroSkillBase(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvHeroSkill getHeroSkill(int idCsv) = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroSkill getHeroSkill(int idCsv) = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key) && null != _csvItemMemory[key])
		{
			return (TypeCsvHeroSkillBase)_csvItemMemory[key];
		}
//		key = "public static TypeCsvHeroSkillAttribute getHeroSkillAttribute(int idCsv) = " + idCsv;
		key = string.Intern(new StringBuilder("public static TypeCsvHeroSkillAttribute getHeroSkillAttribute(int idCsv) = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key) && null != _csvItemMemory[key])
		{
			return (TypeCsvHeroSkillBase)_csvItemMemory[key];
		}
		/// 主动没有去被动技能中招
		TypeCsvHeroSkillBase result = getHeroSkill(idCsv);
		if(null == result)
			result = getHeroSkillAttribute(idCsv);
		
		/// 返回
		return result;
	}
	/// 获得主角 技能数据
	public static TypeCsvHeroSkill getHeroSkill(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvHeroSkill getHeroSkill(int idCsv) = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroSkill getHeroSkill(int idCsv) = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvHeroSkill)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvHeroSkill result = _csvTables["HeroSkill"].searchAndNew<TypeCsvHeroSkill>("id", idCsv);
		if(null != result)
			_csvTables["HeroSkillBase"].searchAndSet<TypeCsvHeroSkill>(result, "id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得主角 被动技能数据
	public static TypeCsvHeroSkillAttribute getHeroSkillAttribute(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvHeroSkillAttribute getHeroSkillAttribute(int idCsv) = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroSkillAttribute getHeroSkillAttribute(int idCsv) = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvHeroSkillAttribute)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvHeroSkillAttribute result = _csvTables["HeroSkillAttribute"].searchAndNew<TypeCsvHeroSkillAttribute>("id", idCsv);
		if(null != result)
			_csvTables["HeroSkillBase"].searchAndSet<TypeCsvHeroSkillAttribute>(result, "id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 返回队长技能的品级加成
	public static TypeCsvHeroSkillLeaderLv getHeroSkillLeaderLv(int grade, int lv)
	{
		/// 数据缓存部分
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroSkillLeaderLv getHeroSkillLeaderLv(int grade, int lv) grade = ").Append(grade).Append(" lv = ").Append(lv).ToString());
//		string key = "public static TypeCsvHeroSkillLeaderLv getHeroSkillLeaderLv(int grade, int lv) grade = " + grade + " lv = " + lv;
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvHeroSkillLeaderLv)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvHeroSkillLeaderLv result = _csvTables["HeroSkillLeaderLv"].searchAndNew<TypeCsvHeroSkillLeaderLv>("grade", grade, "lv", lv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;		
	}
	/// 检索副本的数据
	public static TypeCsvFB getFB(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvFB getFB(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvFB getFB(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvFB)_csvItemMemory[key];
		/// 返回值
		TypeCsvFB result = _csvTables["FB"].searchAndNew<TypeCsvFB>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	///检索邮件的数据 - wen
	public static TypeCsvEmail getEmail(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvEmail getEmail(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvEmail getEmail(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvEmail)_csvItemMemory[key];
		/// 返回值
		
		TypeCsvEmail result = _csvTables["Email"].searchAndNew<TypeCsvEmail>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;	
	}
	
	public static TypeCsvWebEmail getWebEmail(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvWebEmail getWebEmail(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvWebEmail getWebEmail(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvWebEmail)_csvItemMemory[key];
		/// 返回值
		
		TypeCsvWebEmail result = _csvTables["WebEmail"].searchAndNew<TypeCsvWebEmail>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;	
	}
	
	/// 检索副本的数据
	public static TypeCsvPK getPK(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvPK getPK(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvPK getPK(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvPK)_csvItemMemory[key];
		/// 返回值
		TypeCsvPK result = _csvTables["PK"].searchAndNew<TypeCsvPK>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}


	// add by ssy
	/// 检索副本集合表的数据
	public static TypeCsvFBMuster getFBMuster(int idCsv)
	{
//		string key = "public static TypeCsvFBMuster getFBMuster(int idCsv) idcsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvFBMuster getFBMuster(int idCsv) idcsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvFBMuster)_csvItemMemory[key];
		}
		
		TypeCsvFBMuster ret = _csvTables["FBMuster"].searchAndNew<TypeCsvFBMuster>("id", idCsv);
		_csvItemMemory.Add(key, ret);
		return ret;
		
	}
	// add end

	/// 检索副本中敌人的信息
	public static TypeCsvCombatEnemy getCombatEnemy(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCombatEnemy getCombatEnemy(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvCombatEnemy getCombatEnemy(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvCombatEnemy)_csvItemMemory[key];
		/// 返回值
		TypeCsvCombatEnemy result = _csvTables["CombatEnemy"].searchAndNew<TypeCsvCombatEnemy>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	
	
	/// 检索副本中敌人的信息
	public static TypeCsvCombatStand getCombatStand(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCombatStand getCombatStand(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvCombatStand getCombatStand(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvCombatStand)_csvItemMemory[key];
		/// 返回值
		TypeCsvCombatStand result = _csvTables["CombatStand"].searchAndNew<TypeCsvCombatStand>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得csv的视图
	public static TypeCsvView getView(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvView getView(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvView getView(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvView)_csvItemMemory[key];
		/// 返回值
		TypeCsvView result = _csvTables["View"].searchAndNew<TypeCsvView>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得第一次使用ui的id数据
	public static TypeCsvSystemFirstUse getSystemFirstUse(int idSystem)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvSystemFirstUse getSystemFirstUse(int idSystem) idSystem = " + idSystem;
		string key = string.Intern(new StringBuilder("public static TypeCsvSystemFirstUse getSystemFirstUse(int idSystem) idSystem = ").Append(idSystem).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvSystemFirstUse)_csvItemMemory[key];
		/// 返回值
		TypeCsvSystemFirstUse result = _csvTables["SystemFirstUse"].searchAndNew<TypeCsvSystemFirstUse>("idSystem", idSystem);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得csv的视图
	public static TypeCsvSystemFirstUsePlay getSystemFirstUsePlay(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvSystemFirstUsePlay getSystemFirstUsePlay(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvSystemFirstUsePlay getSystemFirstUsePlay(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvSystemFirstUsePlay)_csvItemMemory[key];
		/// 返回值
		TypeCsvSystemFirstUsePlay result = _csvTables["SystemFirstUsePlay"].searchAndNew<TypeCsvSystemFirstUsePlay>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	
	
	/// 获得动作标签
	public static List<TypeCsvViewFI> getViewFI(string name, string anim)
	{
		/// 数据缓存部分
//		string key = "public static List<TypeCsvViewFI> getViewFI(string name, string anim) name" + name + "/ anim = " + anim;
		string key = string.Intern(new StringBuilder("public static List<TypeCsvViewFI> getViewFI(string name, string anim) name").Append(name).Append("/ anim = ").Append(anim).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvViewFI>)_csvItemMemory[key];
		/// 返回值
		List<TypeCsvViewFI> result = _csvTables["ViewFI"].searchsT<TypeCsvViewFI>("name", name, "anim", anim);
		
		/// 进行一次浅递归,找通用的数据
		if((null == result || result.Count == 0) && name != "general")
			result = getViewFI("general", anim);
		
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	/// <summary>
	/// Inits the view F.
	/// </summary>
	public static void initViewFI()
	{
		UtilCsvReader csv_reader = _csvTables["ViewFI"];
		List<string> cache = new List<string> ();
		if(csv_reader == null)
		{
			return;
		}

//		List<TypeCsvViewFI> view_fis =  csv_reader.getTableAndNew<TypeCsvViewFI>();
		List<TypeCsvViewFI> view_fis =  csv_reader.getTableAndNew2<TypeCsvViewFI>("name", "anim");
		foreach(TypeCsvViewFI fi in view_fis)
		{
			if(cache.Contains(fi.name + fi.anim))
			{
				continue;
			}

			getViewFI(fi.name, fi.anim);
			if(!cache.Contains(fi.name + fi.anim))
			{
				cache.Add(fi.name + fi.anim);
			}

		}

		cache.Clear();

	}

	public static void initView()
	{
		UtilCsvReader csv_reader = _csvTables["View"];
		if(csv_reader == null)
		{
			return;
		}
//		List<TypeCsvView> views = csv_reader.getTableAndNew<TypeCsvView>();
		List<TypeCsvView> views = csv_reader.getTableAndNew2<TypeCsvView>("id");
//		foreach(TypeCsvView view in views)
		for(int i = 0; i < views.Count; i++)
		{
			TypeCsvView view = views[i];
			string key = string.Intern(new StringBuilder("public static TypeCsvView getView(int idCsv) idCsv = ").Append(view.id).ToString());
			if(view.id <= 0)
				view = null;
			if(!_csvItemMemory.ContainsKey(key))
			{
				_csvItemMemory.Add(key, view);
			}

		}
	}

	public static void initPropEquip()
	{
		UtilCsvReader csv_reader = _csvTables["PropEquip"];
		if(csv_reader == null)
		{
			return;
		}
//		List<TypeCsvPropEquip> props = csv_reader.getTableAndNew<TypeCsvPropEquip>();
		List<TypeCsvPropEquip> props = csv_reader.getTableAndNew2<TypeCsvPropEquip>("id");
		foreach(TypeCsvPropEquip prop in props)
		{
			getPropEquip(prop.id);
		}
	}

	public static void initProp()
	{
		UtilCsvReader csv_reader = _csvTables["Prop"];
		if(csv_reader == null)
		{
			return;
		}
		List<TypeCsvProp> props = csv_reader.getTableAndNew2<TypeCsvProp>("id");
		for(int index = 0; index < props.Count; index++)
		{
			TypeCsvProp prop = props[index];
			string key = string.Intern(new StringBuilder("public static TypeCsvProp getProp(int idCsvProp)").Append(prop.id).ToString());
			if(prop.id <= 0)
				prop = null;
			if(!_csvItemMemory.ContainsKey(key))
			{
				_csvItemMemory.Add(key, prop);
			}

		}
	}

	public static void initCsv()
	{
		ManagerCsv.initViewFI();
		ManagerCsv.initView();
		ManagerCsv.initProp();
//		ManagerCsv.initPropEquip();
	}

	/// 获得动作标签
	public static TypeCsvTown getTown(int idCsvTown)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvTown getTown(int idCsvTown) idCsvTown" + idCsvTown;
		string key = string.Intern(new StringBuilder("public static TypeCsvTown getTown(int idCsvTown) idCsvTown").Append(idCsvTown).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvTown)_csvItemMemory[key];
		/// 返回值
		TypeCsvTown result = _csvTables["Town"].searchAndNew<TypeCsvTown>("id", idCsvTown);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得动作标签
	public static TypeCsvBullet getBullet(int idCsvBullet)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvBullet getBullet(int idCsvBullet)" + idCsvBullet;
		string key = string.Intern(new StringBuilder("public static TypeCsvBullet getBullet(int idCsvBullet)").Append(idCsvBullet).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvBullet)_csvItemMemory[key];
		/// 返回值
		TypeCsvBullet result = _csvTables["Bullet"].searchAndNew<TypeCsvBullet>("id", idCsvBullet);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得战斗特效
	public static TypeCsvCombatEffect getCombatEffect(int idCsvEffect)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCombatEffect getCombatEffect(int idCsvEffect)" + idCsvEffect;
		string key = string.Intern(new StringBuilder("public static TypeCsvCombatEffect getCombatEffect(int idCsvEffect)").Append(idCsvEffect).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvCombatEffect)_csvItemMemory[key];
		/// 返回值
		TypeCsvCombatEffect result = _csvTables["CombatEffect"].searchAndNew<TypeCsvCombatEffect>("id", idCsvEffect);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 变量数据
	public static TypeCsvAttribute getAttribute()
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvAttribute getAttribute()";
		string key = string.Intern("public static TypeCsvAttribute getAttribute()");
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvAttribute)_csvItemMemory[key];
		/// 返回值
		TypeCsvAttribute result = _csvTables["Attribute"].getAttribue<TypeCsvAttribute>("name", "val");
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得buff数据
	public static TypeCsvBuff getBuff(int idCsvBuff)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvBuff getBuff(int idCsvBuff)" + idCsvBuff;
		string key = string.Intern(new StringBuilder("public static TypeCsvBuff getBuff(int idCsvBuff)").Append(idCsvBuff).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvBuff)_csvItemMemory[key];
		/// 返回值
		TypeCsvBuff result = _csvTables["Buff"].searchAndNew<TypeCsvBuff>("id", idCsvBuff);
//		if(null != result)
//			result.idViewChange = 2027;
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得消耗数据
	public static TypeCsvConsume getConsume(int idCsvConsume)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvConsume getConsume(int idCsvConsume)" + idCsvConsume;
		string key = string.Intern(new StringBuilder("public static TypeCsvConsume getConsume(int idCsvConsume)").Append(idCsvConsume).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvConsume)_csvItemMemory[key];
		/// 返回值
		TypeCsvConsume result = _csvTables["Consume"].searchAndNew<TypeCsvConsume>("id", idCsvConsume);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得 道具表
	public static TypeCsvProp getProp(int idCsvProp)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvProp getProp(int idCsvProp)" + idCsvProp;
		string key = string.Intern(new StringBuilder("public static TypeCsvProp getProp(int idCsvProp)").Append(idCsvProp).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvProp)_csvItemMemory[key];
		/// 返回值
		TypeCsvProp result = _csvTables["Prop"].searchAndNew<TypeCsvProp>("id", idCsvProp);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// <summary>
	/// 获得  道具经验表
	/// </summary>
	public static TypeCsvPropExp getPropExp(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvPropExp getPropExp(int idCsv)" + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvPropExp getPropExp(int idCsv)").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvPropExp)_csvItemMemory[key];
		/// 返回值
		TypeCsvPropExp result = _csvTables["PropExp"].searchAndNew<TypeCsvPropExp>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
		/// 获得 卡片碎片表
	public static TypeCsvSearchRoad getSearchRoad(int swarchId)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvHeroReward getSearchRoad(int swarchId)" + swarchId;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroReward getSearchRoad(int swarchId)").Append(swarchId).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvSearchRoad)_csvItemMemory[key];
		/// 返回值
		TypeCsvSearchRoad result = _csvTables["SearchRoad"].searchAndNew<TypeCsvSearchRoad>("id", swarchId);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得 世界boss 活动信息
	public static TypeCsvWorldEvent getWorldEvent(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvWorldEvent getWorldEvent(int idCsv)" + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvWorldEvent getWorldEvent(int idCsv)").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvWorldEvent)_csvItemMemory[key];
		/// 返回值
		TypeCsvWorldEvent result = _csvTables["WorldEvent"].searchAndNew<TypeCsvWorldEvent>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 世界boss 活动时间
	public static TypeCsvWorldEventTime getWorldEventTime(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvWorldEventTime getWorldEventTime(int idCsv)" + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvWorldEventTime getWorldEventTime(int idCsv)").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvWorldEventTime)_csvItemMemory[key];
		/// 返回值
		TypeCsvWorldEventTime result = _csvTables["WorldEventTime"].searchAndNew<TypeCsvWorldEventTime>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得 活动 活动信息
	public static TypeCsvActivEvent getActivEvent(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvActivEvent getActivEvent(int idCsv)" + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvActivEvent getActivEvent(int idCsv)").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvActivEvent)_csvItemMemory[key];
		/// 返回值
		TypeCsvActivEvent result = _csvTables["ActivEvent"].searchAndNew<TypeCsvActivEvent>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 活动 活动时间
	public static TypeCsvActivEventTime getActivEventTime(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvActivEventTime getActivEventTime(int idCsv)" + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvActivEventTime getActivEventTime(int idCsv)").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvActivEventTime)_csvItemMemory[key];
		/// 返回值
		TypeCsvActivEventTime result = _csvTables["ActivEventTime"].searchAndNew<TypeCsvActivEventTime>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 活动 活动时间
	public static TypeCsvCombatAuto getCombatAuto(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCombatAuto getCombatAuto(int idCsv)" + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvCombatAuto getCombatAuto(int idCsv)").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvCombatAuto)_csvItemMemory[key];
		/// 返回值
		TypeCsvCombatAuto result = _csvTables["CombatAuto"].searchAndNew<TypeCsvCombatAuto>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	
	/// add by ssy
	/// get hero res data
	public static TypeCsvHeroRes getHeroRes(int id_csv)
	{
//		string key = "public static TypeCsvHeroRes getHeroRes(int id_csv)" + id_csv;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroRes getHeroRes(int id_csv)").Append(id_csv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvHeroRes)_csvItemMemory[key];
		}

		TypeCsvHeroRes ret = _csvTables["HeroRes"].searchAndNew<TypeCsvHeroRes>("id", id_csv);
		_csvItemMemory.Add(key, ret);
		return ret;
	}
	/// add end
	
	
	
	/// <summary>
	///  get any csv you want
	/// </summary>
	/// <returns>The csv reader.</returns>
	/// <param name="csv_name">Csv_name.</param>
	public static UtilCsvReader getCsvReader(string csv_name)
	{
		if(!_csvTables.ContainsKey(csv_name))
		{
			if(UtilLog.isBulidLog)UtilLog.LogError(" the csv table you want have not been loaded!!");
			return null;
		}

		return _csvTables[csv_name];
	}
	
	// add by ssy 
	/// 根据normal副本 csv id ，获取大陆 csv id。需要便利fbmuster表格的所有行

	/// cache serach result
	private static Dictionary<int, int> _dirCacheLandID = new Dictionary<int, int> ();
	/// cache fb muster table
	private static List<TypeCsvFBMuster> _listCacheFBMusterTable  = null;

	/// <summary>
	/// 根据fbcsv id 获得land id
	/// 如果副本 id 为0 那么表示所有副本都通关了，那么就返回表中最后一个land的id
	/// </summary>
	/// <returns>
	/// The land I.
	/// </returns>
	/// <param name='fb_csv_id'>
	/// Fb_csv_id.
	/// </param>
	public static int getLandID(int fb_csv_id)
	{
		int ret = 0;
		
		int max_land_id = 0;
		// 判断缓存
		if(_dirCacheLandID.ContainsKey(fb_csv_id))
		{
			return _dirCacheLandID[fb_csv_id];
		}

		// check cache
		if(null == _listCacheFBMusterTable)
		{
			UtilCsvReader csv_reader = getCsvReader("FBMuster");
			_listCacheFBMusterTable =	csv_reader.getTableAndNew<TypeCsvFBMuster> ();
		}

		// foreache
		foreach(TypeCsvFBMuster csv_muster in _listCacheFBMusterTable)
		{
			if(csv_muster.id >= max_land_id)
			{
				max_land_id = csv_muster.id;
			}
			
			// when array is #
			if(csv_muster.fbIDs == null)
			{
				continue;
			}
			
			foreach(string i in csv_muster.fbIDs)
			{
				if(i == fb_csv_id.ToString())
				{
					_dirCacheLandID[GMath.toInt(fb_csv_id)] = csv_muster.id;
					ret = csv_muster.id;
					break;
				}
			}
			
			foreach(string i in csv_muster.fbElitIDs)
			{
				if(i == fb_csv_id.ToString())
				{
					_dirCacheLandID[GMath.toInt(fb_csv_id)] = csv_muster.id;
					ret = csv_muster.id;
					break;
				}
			}
			
			foreach(string i in csv_muster.fbChallengeIDs)
			{
				if(i == fb_csv_id.ToString())
				{
					_dirCacheLandID[GMath.toInt(fb_csv_id)] = csv_muster.id;
					ret = csv_muster.id;
					break;
				}
			}

		}
		
		if(fb_csv_id == 0)
		{
			ret = max_land_id;
		}

		if(0 == ret)
		{
			UtilLog.LogError("not find the fb id in any land fb id = " + fb_csv_id); 
		}
		return ret;

	}
	// add end
	
	
	public static TypeCsvPropEquip getPropEquip(int idCsvProp)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvPropEquip getPropEquip(int idCsvProp) = " + idCsvProp;
		string key = string.Intern(new StringBuilder("public static TypeCsvPropEquip getPropEquip(int idCsvProp) = ").Append(idCsvProp).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvPropEquip)_csvItemMemory[key];
		}
		/// 返回值
//		TypeCsvPropEquip result = _csvTables["PropEquip"].searchAndNew<TypeCsvPropEquip>("id", idCsvProp);
//		if(null != result)
//			_csvTables["Prop"].searchAndSet<TypeCsvPropEquip>(result, "id", idCsvProp);
		TypeCsvPropEquip result = _csvTables["Prop"].searchAndNew<TypeCsvPropEquip>("id", idCsvProp);
		if(null != result && result.type == 1)
			_csvTables["PropEquip"].searchAndSet<TypeCsvPropEquip>(result, "id", idCsvProp);
		else
			result = null;
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	// add by ssy
	public static TypeCsvFBReward getFBReward(int idReward)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvFBReward getFBReward(int idReward) = " + idReward;
		string key = string.Intern(new StringBuilder("public static TypeCsvFBReward getFBReward(int idReward) = ").Append(idReward).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvFBReward)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvFBReward result = _csvTables["FbReward"].searchAndNew<TypeCsvFBReward>("id", idReward);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	// add end
	
	//add by yxh
	///获得升级到lv时 需要的经验值
	public static ulong getExp(int lv)
	{	
		string key = string.Intern(new StringBuilder("public static ulong getExp(int lv) = ").Append(lv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (ulong)_csvItemMemory[key];

		ulong exp = 0;
		TypeCsvHeroLv herolv = null;
		for(int i = 1; i < lv+1; i++)
		{
			herolv = getHeroLv(i);
			if(herolv != null)
			{
				exp += herolv.exp;
			}
		}
		_csvItemMemory.Add(key, exp);
		return exp;
	}
	/// 获得人物升级到lv时 需要的经验值
	public static ulong getPlayerExp(int lv)
	{	
		string key = string.Intern(new StringBuilder("public static ulong getPlayerExp(int lv) = ").Append(lv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (ulong)_csvItemMemory[key];

		ulong exp = 0;
		TypeCsvHeroLv herolv = null;
		for(int i = 1; i < lv+1; i++)
		{
			herolv = getHeroLv(i);
			if(herolv != null)
			{
				exp += herolv.expPlayer;
			}
		}
		_csvItemMemory.Add(key, exp);
		return exp;
	}
	/// 获得卡牌当前等级 需要卡牌 经验和品质 和加几
	public static int getPlayerLv(ulong exp, int grade, int addNum)
	{



		int lv = 1;
		bool isRun = true;
		TypeCsvHeroLv herolv = null;
		
		if(exp == 0)
		{
			return 1;	
		}
		
		while(isRun)
		{
			herolv = getHeroLv(lv);
			if(herolv != null)
			{	
				if(exp < herolv.exp)
				{					
					isRun = false;
				}
				else
				{
					exp -= herolv.exp;
					lv++;
				}
			}
		}
		
//		TypeCsvHeroGrade gradeCsv = ManagerCsv.getCsvHeroGrade(grade, addNum);
//		/// 品质 最高等级
//		if(lv >= gradeCsv.lvMax)
//			lv = gradeCsv.lvMax;
		
		return lv;
	}
	
	/// 检索军衔数据
	public static TypeCsvNobility getNobility(int lv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvNobility getNobility(int lv) = " + lv;
		string key = string.Intern(new StringBuilder("public static TypeCsvNobility getNobility(int lv) = ").Append(lv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvNobility)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvNobility result = _csvTables["Nobility"].searchAndNew<TypeCsvNobility>("id", lv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static TypeCsvHeroCreat[] getAllHeroCreat()
	{
		string keyList = "public static TypeCsvHeroCreat[] getAllHeroCreat()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (TypeCsvHeroCreat[])_csvItemMemory[keyList];

		List<TypeCsvHeroCreat> list = new List<TypeCsvHeroCreat>();
		int index = 1;
		while(true)
		{
//			string key = "public static TypeCsvHeroCreat[] getAllHeroCreat()" + index;
//			string key = string.Intern(new StringBuilder("public static TypeCsvHeroCreat[] getAllHeroCreat()").Append(index).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				list.Add((TypeCsvHeroCreat)_csvItemMemory[key]);
//				index++;
//				continue;
//			}
			/// 返回值
			TypeCsvHeroCreat result = _csvTables["HeroCreate"].searchAndNew<TypeCsvHeroCreat>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);
//			/// 值存储
//			_csvItemMemory.Add(key, result);
			/// 返回
			index++;
		}
		_csvItemMemory.Add(keyList, list.ToArray());
		return list.ToArray();
	}
	
	/// 获得塔的信息
	public static TypeCsvFBTower getFBTower(int idCsv)
	{
		/// 数据缓存部分
		string key = "public static TypeCsvFBTower getFBTower(int idCsv) = " + idCsv;
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvFBTower)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvFBTower result = _csvTables["FBTower"].searchAndNew<TypeCsvFBTower>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得塔中层的信息
	public static TypeCsvFBTowerFloor getFBTowerFloor(int idCsvTower, int idCsvFloor)
	{
		/// 数据缓存部分
		string key = "public static TypeCsvFBTowerFloor getFBTowerFloor(int idCsvTower, int idCsvFloor) = " + idCsvTower + "/" + idCsvFloor;
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvFBTowerFloor)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvFBTowerFloor result = _csvTables["FBTowerFloor"].searchAndNew<TypeCsvFBTowerFloor>("idTower", idCsvTower, "idFloor", idCsvFloor);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得塔的信息
	public static TypeCsvFBTowerBase getFBTowerBase()
	{
		/// 数据缓存部分
		string key = "public static TypeCsvFBTowerBase getFBTowerBase() = " + 1;
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvFBTowerBase)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvFBTowerBase result = _csvTables["FBTowerBase"].searchAndNew<TypeCsvFBTowerBase>("id", 1);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 开启系统的功能
	public static TypeCsvSystem getSystem(InfoSystem idCsvSystem)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvSystem getSystem(int idCsvSystem) idCsvSystem = " + idCsvSystem;
		string key = string.Intern(new StringBuilder("public static TypeCsvSystem getSystem(int idCsvSystem) idCsvSystem = ").Append(idCsvSystem).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvSystem)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvSystem result = _csvTables["System"].searchAndNew<TypeCsvSystem>("id", (int)idCsvSystem);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得商店要出售的道具
	public static TypeCsvShop getShop(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvShop getShop(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvShop getShop(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvShop)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvShop result = _csvTables["Shop"].searchAndNew<TypeCsvShop>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得ui特效
	public static TypeCsvCartoon getCartoon(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCartoon getCartoon(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvCartoon getCartoon(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvCartoon)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvCartoon result = _csvTables["Cartoon"].searchAndNew<TypeCsvCartoon>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	
	
	/// 获得商店要出售的道具
	public static TypeCsvCntBuy getCntBuy(int cnt)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCntBuy getCntBuy(int cnt) cnt = " + cnt;
		string key = string.Intern(new StringBuilder("public static TypeCsvCntBuy getCntBuy(int cnt) cnt = ").Append(cnt).ToString());
		if(_csvItemMemory.ContainsKey(key))
		{
			return (TypeCsvCntBuy)_csvItemMemory[key];
		}
		/// 返回值
		TypeCsvCntBuy result = _csvTables["CntBuy"].searchAndNew<TypeCsvCntBuy>("cnt", cnt);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	public static List<TypeCsvEquipCreat> getEquipCreat()
	{
		string keyList = "public static List<TypeCsvEquipCreat> getEquipCreat()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvEquipCreat>)_csvItemMemory[keyList];

		List<TypeCsvEquipCreat> temp = new List<TypeCsvEquipCreat>();
		int id = 0;
		while(true)
		{
			id++;
//			string key = "List<TypeCsvEquipCreat> getEquipCreat()" + id;
//			string key = string.Intern(new StringBuilder("List<TypeCsvEquipCreat> getEquipCreat()").Append(id).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				temp.Add((TypeCsvEquipCreat)_csvItemMemory[key]);
//				continue;
//			}
			
			TypeCsvEquipCreat result = _csvTables["EquipCreat"].searchAndNew<TypeCsvEquipCreat>("id",id);
			if(result == null)
			{
				break;
			}
//			_csvItemMemory.Add(key, result);
			temp.Add(result);
		}
		_csvItemMemory.Add(keyList, temp);
		return temp;
	}
	/// 获得引导数据
	public static TypeCsvGuide[] getGuide()
	{
		string keyList = "public static TypeCsvGuide[] getGuide()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (TypeCsvGuide[])_csvItemMemory[keyList];


		List<TypeCsvGuide> list = new List<TypeCsvGuide>();
		int index = 1;
		while(true)
		{
			TypeCsvGuide result = _csvTables["Guide"].searchAndNew<TypeCsvGuide>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);
			/// 值存储
//			_csvItemMemory.Add(key, result);
			/// 返回
			index++;
		}
		_csvItemMemory.Add(keyList, list.ToArray());
		return list.ToArray();
	}
	
	public static TypeCsvLuckyShop getLuckShop(int idCsvLuckyShopId)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvLuckyShop getLuckShop(int idCsvLuckyShopId)" + idCsvLuckyShopId;
		string key = string.Intern(new StringBuilder("public static TypeCsvLuckyShop getLuckShop(int idCsvLuckyShopId)").Append(idCsvLuckyShopId).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvLuckyShop)_csvItemMemory[key];
		/// 返回值
		TypeCsvLuckyShop result = _csvTables["LuckyShop"].searchAndNew<TypeCsvLuckyShop>("id", idCsvLuckyShopId);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static TypeCsvDictionary getDictionary(int Id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvDictionary getDictionary(int Id)" + Id;
		string key = string.Intern(new StringBuilder("public static TypeCsvDictionary getDictionary(int Id)").Append(Id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvDictionary)_csvItemMemory[key];
		/// 返回值
		TypeCsvDictionary result = _csvTables["Dictionary"].searchAndNew<TypeCsvDictionary>("id", Id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static TypeCsvCharge getCharge(int idCharge)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvCharge getCharge(int idCharge)" + idCharge;
		string key = string.Intern(new StringBuilder("public static TypeCsvCharge getCharge(int idCharge)").Append(idCharge).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvCharge)_csvItemMemory[key];
		/// 返回值
		TypeCsvCharge result = _csvTables["Charge"].searchAndNew<TypeCsvCharge>("id", idCharge);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static List<TypeCsvTips> getTips()
	{
		string keyList = "public static List<TypeCsvTips> getTips()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvTips>)_csvItemMemory[keyList];


		List<TypeCsvTips> temp = new List<TypeCsvTips>();
		int id = 1;
		while(true)
		{	
			/// 数据缓存部分
//			string key = "public static List<TypeCsvTips> getTips()" + id;
//			string key = string.Intern(new StringBuilder("public static List<TypeCsvTips> getTips()").Append(id).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				temp.Add((TypeCsvTips)_csvItemMemory[key]);
//			}
//			else
			{
				/// 返回值
				TypeCsvTips result = _csvTables["Tips"].searchAndNew<TypeCsvTips>("id", id);
				if(result != null)
				{
					/// 值存储
//					_csvItemMemory.Add(key, result);
					temp.Add(result);
				}
				else
				{
					break;
				}
			}	
			id++;
		}
		_csvItemMemory.Add(keyList, temp);
		return temp;
	}
	
	public static TypeCsvBadgeShop getBadgeShop(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvBadgeShop getBadgeShop(int id)" + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvBadgeShop getBadgeShop(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvBadgeShop)_csvItemMemory[key];
		/// 返回值
		TypeCsvBadgeShop result = _csvTables["BadgeShop"].searchAndNew<TypeCsvBadgeShop>("id", id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static List<TypeCsvClientConfig> getClientConfig()
	{
		string keyList = "public static List<TypeCsvClientConfig> getClientConfig()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvClientConfig>)_csvItemMemory[keyList];

		List<TypeCsvClientConfig> temp = new List<TypeCsvClientConfig>();
		
		int index = 1;
		
		while(true)
		{
//			string key = "public static List<TypeCsvClientConfig> getClientConfig()" + index.ToString();
//			string key = string.Intern(new StringBuilder("public static List<TypeCsvClientConfig> getClientConfig()").Append(index).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				temp.Add((TypeCsvClientConfig)_csvItemMemory[key]);
//			}
//			else
			{
				TypeCsvClientConfig result = null;

				if(_csvTables.ContainsKey(ConfigUrl.CLIENT_CONFIG))
				{
					result = _csvTables[ConfigUrl.CLIENT_CONFIG].searchAndNew<TypeCsvClientConfig>("id", index);
				}

				if(result != null)
				{
					/// 值存储
//					_csvItemMemory.Add(key, result);
					temp.Add(result);
				}
				else
				{
					break;
				}
			}
			index++;
		}
		_csvItemMemory.Add(keyList, temp);
		return temp;
	}
	
	public static TypeCsvAccountConfig getAccountConfig(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvAccountConfig getAccountConfig(int id)" + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvAccountConfig getAccountConfig(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvAccountConfig)_csvItemMemory[key];
		/// 返回值
		TypeCsvAccountConfig result = null;
		if(_csvTables.ContainsKey("AccountConfig"))
		{
			result = _csvTables["AccountConfig"].searchAndNew<TypeCsvAccountConfig>("id", id);
		}
		/// 值存储
		_csvItemMemory.Add(key, result);

		/// 返回
		return result;
	}
	
	public static TypeCsvAnnounce getAnnounce(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvAnnounce getAnnounce(int id)" + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvAnnounce getAnnounce(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvAnnounce)_csvItemMemory[key];
		/// 返回值
		TypeCsvAnnounce result = null;
		if(_csvTables.ContainsKey(ConfigUrl.ANNOUNCE))
		{
			result = _csvTables[ConfigUrl.ANNOUNCE].searchAndNew<TypeCsvAnnounce>("id", id);
		}
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static TypeCsvLoginWarn getLoginWarn(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvLoginWarn getLoginWarn(int id)" + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvLoginWarn getLoginWarn(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvLoginWarn)_csvItemMemory[key];
		/// 返回值
		TypeCsvLoginWarn result = _csvTables[ConfigUrl.LOGINWARN].searchAndNew<TypeCsvLoginWarn>("id", id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static List<TypeCsvLoginWarn> getLoginWarnList()
	{   
		string keyList = "public static List<TypeCsvLoginWarn> getLoginWarnList()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvLoginWarn>)_csvItemMemory[keyList];


		List<TypeCsvLoginWarn> list = new List<TypeCsvLoginWarn>();
		int index = 1;
		while(true)
		{
//			string key = "public static List<TypeCsvLoginWarn> getLoginWarnList()" + index;
//			string key = string.Intern(new StringBuilder("public static List<TypeCsvLoginWarn> getLoginWarnList()").Append(index).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				list.Add((TypeCsvLoginWarn)_csvItemMemory[key]);
//				index++;
//				continue;
//			}
			/// 返回值
			TypeCsvLoginWarn result = _csvTables[ConfigUrl.LOGINWARN].searchAndNew<TypeCsvLoginWarn>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);
			/// 值存储
//			_csvItemMemory.Add(key, result);
			/// 返回
			index++;
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}
	
	public static List<TypeCsvPropEquip> getAllEquip(string location)
	{
//		string key = "public static List<TypeCsvPropEquip> getAllEquip()";
		string key = string.Intern(new StringBuilder("public static List<TypeCsvPropEquip> getAllEquip()").Append(location).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvPropEquip>)_csvItemMemory[key];
		List<TypeCsvPropEquip> list = _csvTables["PropEquip"].searchsT<TypeCsvPropEquip>("local",location);
		_csvItemMemory.Add(key, list);
		return list;
	}
	
	public static TypeCsvRewardSum getRewardSum(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvRewardSum getRewardSum(int id)" + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvRewardSum getRewardSum(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvRewardSum)_csvItemMemory[key];
		/// 返回值
		TypeCsvRewardSum result = _csvTables["RewardSum"].searchAndNew<TypeCsvRewardSum>("id", id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	public static List<TypeCsvSign> getSign(int month)
	{
//		string key = "public static List<TypeCsvSign> getSign()"+month;
		string key = string.Intern(new StringBuilder("public static List<TypeCsvSign> getSign()").Append(month).ToString());
		List<TypeCsvSign> list = _csvTables["Sign"].searchsT<TypeCsvSign>("month",month);
		return list;
		
	}
	
	/*	public static List<TypeCsvTask> getMissionList(int type)
	{
		string key = "public static List<TypeCsvTask> getMissionList()";
		List<TypeCsvTask> list = _csvTables["Task"].searchsT<TypeCsvTask>("type",type);
		return list;
		
	}*/
	 
	public static TypeCsvTask getMission(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvTask getMission(int id)" + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvTask getMission(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvTask)_csvItemMemory[key];
		/// 返回值
		TypeCsvTask result = _csvTables["Task"].searchAndNew<TypeCsvTask>("id", id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得可以被合成的所有英雄 id是sameId
	public static TypeCsvHeroList[] getHeroList()
	{
		string keyList = "public static TypeCsvHeroList[] getHeroList())";
		if(_csvItemMemory.ContainsKey(keyList))
			return (TypeCsvHeroList[])_csvItemMemory[keyList];
		List<TypeCsvHeroList> list = new List<TypeCsvHeroList>();

		int index = 1;
		while(true)
		{
//			string key = string.Intern(new StringBuilder("public static TypeCsvHeroList[] getHeroList()").Append(index).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				list.Add((TypeCsvHeroList)_csvItemMemory[key]);
//				index++;
//				continue;
//			}
			/// 返回值
			TypeCsvHeroList result = _csvTables["HeroList"].searchAndNew<TypeCsvHeroList>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);
			/// 值存储
//			_csvItemMemory.Add(key, result);
			/// 返回
			index++;
		}
		_csvItemMemory.Add(keyList, list.ToArray());
		return list.ToArray();
	}
	
	/// 所有的英雄
	public static List<TypeCsvHero> getAllHero()
	{
		string keyList = "public static List<TypeCsvHero> getAllHero()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvHero>)_csvItemMemory[keyList];

		List<TypeCsvHero> list = new List<TypeCsvHero>();
		List<TypeCsvHero> temp = null;
		
		TypeCsvHeroList[] heros = getHeroList();
		
		for(int i = 0; i < heros.Length; i++)
		{
			temp = getHeroSameItem(heros[i].idCsv);
			
			for(int j = 0; j < temp.Count; j++)
			{
				list.Add(temp[j]);
			}
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}
	
	
	/// 获得 剧情 组
	public static TypeCsvStory getStory(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvStory getStory(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvStory getStory(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvStory)_csvItemMemory[key];
		/// 返回值
		TypeCsvStory result = _csvTables["Story"].searchAndNew<TypeCsvStory>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 剧情 组中成员
	public static TypeCsvStoryItem getStoryItem(int idCsv)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvStoryItem getStoryItem(int idCsv) idCsv = " + idCsv;
		string key = string.Intern(new StringBuilder("public static TypeCsvStoryItem getStoryItem(int idCsv) idCsv = ").Append(idCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvStoryItem)_csvItemMemory[key];
		/// 返回值
		TypeCsvStoryItem result = _csvTables["StoryItem"].searchAndNew<TypeCsvStoryItem>("id", idCsv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	
	/// 获得 角色AI中的自动技能AI
	public static List<TypeCsvHeroAI> getHeroAI(int idCsvHero)
	{
//		string key = "public static List<TypeCsvHeroAI> getHeroAI(int idCsvHero) idCsv = " + idCsvHero;
		string key = string.Intern(new StringBuilder("public static List<TypeCsvHeroAI> getHeroAI(int idCsvHero) idCsv = ").Append(idCsvHero).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvHeroAI>)_csvItemMemory[key];
		/// 返回值
		List<TypeCsvHeroAI> result = _csvTables["HeroAI"].searchsT<TypeCsvHeroAI>("idHero", idCsvHero);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得 角色AI中的自动技能AI
	public static TypeCsvSummon getSummon(int idCsvSummon)
	{
//		string key = "public static List<TypeCsvSummon> getSummon(int idCsvSummon) idCsvSummon = " + idCsvSummon;
		string key = string.Intern(new StringBuilder("public static List<TypeCsvSummon> getSummon(int idCsvSummon) idCsvSummon = ").Append(idCsvSummon).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvSummon)_csvItemMemory[key];
		/// 返回值
		TypeCsvSummon result = _csvTables["Summon"].searchAndNew<TypeCsvSummon>("id", idCsvSummon);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	/// 获得 远征属性
	public static TypeCsvMot getMot(int idCsvMot)
	{
//		string key = "public static TypeCsvSummon getMot(int idCsvMot) idCsvMot = " + idCsvMot;
		string key = string.Intern(new StringBuilder("public static TypeCsvSummon getMot(int idCsvMot) idCsvMot = ").Append(idCsvMot).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvMot)_csvItemMemory[key];
		/// 返回值
		TypeCsvMot result = _csvTables["Mot"].searchAndNew<TypeCsvMot>("id", idCsvMot);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 远征地图
	public static TypeCsvMotMap getMotMap(int idCsvMotMap)
	{
//		string key = "public static TypeCsvMotMap getMotMap(int idCsvMotMap) idCsvMotMap = " + idCsvMotMap;
		string key = string.Intern(new StringBuilder("public static TypeCsvMotMap getMotMap(int idCsvMotMap) idCsvMotMap = ").Append(idCsvMotMap).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvMotMap)_csvItemMemory[key];
		/// 返回值
		TypeCsvMotMap result = _csvTables["MotMap"].searchAndNew<TypeCsvMotMap>("id", idCsvMotMap);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 远征BUFF
	public static TypeCsvMotBuff getMotBuff(int idCsvMotBuff)
	{
//		string key = "public static TypeCsvMotBuff getMotBuff(int idCsvMotBuff) idCsvMotBuff = " + idCsvMotBuff;
		string key = string.Intern(new StringBuilder("public static TypeCsvMotBuff getMotBuff(int idCsvMotBuff) idCsvMotBuff = ").Append(idCsvMotBuff).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvMotBuff)_csvItemMemory[key];
		/// 返回值
		TypeCsvMotBuff result = _csvTables["MotBuff"].searchAndNew<TypeCsvMotBuff>("id", idCsvMotBuff);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 远征怪物成长系数
	public static TypeCsvMotEnemyAttri getMotEnemyAttri(int idCsvMot)
	{
//		string key = "public static TypeCsvMotEnemyAttri getMotEnemyAttri(int idCsvMot) idCsvMot = " + idCsvMot;
		string key = string.Intern(new StringBuilder("public static TypeCsvMotEnemyAttri getMotEnemyAttri(int idCsvMot) idCsvMot = ").Append(idCsvMot).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvMotEnemyAttri)_csvItemMemory[key];
		/// 返回值
		TypeCsvMotEnemyAttri result = _csvTables["MotEnemyAttri"].searchAndNew<TypeCsvMotEnemyAttri>("id", idCsvMot);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得镶嵌石头的背景
	public static TypeCsvHeroUpView getHeroUpView(int stoneId)
	{
//		string key = "public static TypeCsvHeroUpView getHeroUpView(int stoneId) = " + stoneId;
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroUpView getHeroUpView(int stoneId) = ").Append(stoneId).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroUpView)_csvItemMemory[key];
		/// 返回值
		TypeCsvHeroUpView result = _csvTables["HeroUpView"].searchAndNew<TypeCsvHeroUpView>("id", stoneId);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	/// 获得 魂兽
	public static TypeCsvBeast getBeast(int sIDCsvBeast, int sLv)
	{
//		string key = "public static TypeCsvBeast getBeast(int sIDCsvBeast) = " + sIDCsvBeast + "/" + sLv;
		string key = string.Intern(new StringBuilder("public static TypeCsvBeast getBeast(int sIDCsvBeast) = ").Append(sIDCsvBeast).Append("/").Append(sLv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvBeast)_csvItemMemory[key];
		/// 返回值
		TypeCsvBeast result = _csvTables["Beast"].searchAndNew<TypeCsvBeast>("id", sIDCsvBeast, "lv", sLv);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	/// 获得 魂兽 装备
	public static TypeCsvPropEquipBeast getPropEquipBeast(int sIDCsvProp)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvPropEquipBeast getPropEquipBeast(int sIDCsvProp, int sLv) " + sIDCsvProp;
		string key = string.Intern(new StringBuilder("public static TypeCsvPropEquipBeast getPropEquipBeast(int sIDCsvProp, int sLv) ").Append(sIDCsvProp).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvPropEquipBeast)_csvItemMemory[key];
		/// 返回值
		TypeCsvPropEquipBeast result = _csvTables["PropEquipBeast"].searchAndNew<TypeCsvPropEquipBeast>("id", sIDCsvProp);
		if(null != result)
			_csvTables["Prop"].searchAndSet<TypeCsvPropEquipBeast>(result, "id", sIDCsvProp);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	//下一个将要开启的系统
	public static List<TypeCsvOpenSystem> getNextOpenSystemList()
	{
		string keyList = "public static List<TypeCsvOpenSystem> getNextOpenSystemList()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvOpenSystem>)_csvItemMemory[keyList];
		
		List<TypeCsvOpenSystem> list = new List<TypeCsvOpenSystem>();
		int index = 1;
		while(true)
		{	
			TypeCsvOpenSystem result = _csvTables["opensystem"].searchAndNew<TypeCsvOpenSystem>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);		
			index++;
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}

	public static List<TypeCsvBeastList> getBeastList()
	{
		string keyList = "public static List<TypeCsvBeastList> getBeastList()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvBeastList>)_csvItemMemory[keyList];

		List<TypeCsvBeastList> list = new List<TypeCsvBeastList>();
		int index = 1;
		while(true)
		{
//			string key = "public static List<TypeCsvBeastList> getBeastList()" + index;
//			string key = string.Intern(new StringBuilder("public static List<TypeCsvBeastList> getBeastList()").Append(index).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				list.Add((TypeCsvBeastList)_csvItemMemory[key]);
//				index++;
//				continue;
//			}
			/// 返回值
			TypeCsvBeastList result = _csvTables["BeastList"].searchAndNew<TypeCsvBeastList>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);
			/// 值存储
//			_csvItemMemory.Add(key, result);
			/// 返回
			index++;
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}

	//获得公会属性
	public static TypeCsvUnionControl getUnionControlValue(int csvID)
	{
		/// 数据缓存部分
		//		string key = "public static TypeCsvPropEquipBeast getPropEquipBeast(int sIDCsvProp, int sLv) " + sIDCsvProp;
		string key = string.Intern(new StringBuilder("public static TypeCsvUnionControl getUnionControlValue(int csvID) ").Append(csvID).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvUnionControl)_csvItemMemory[key];
		/// 返回值
		TypeCsvUnionControl result = _csvTables["GUBackControl"].searchAndNew<TypeCsvUnionControl>("id", csvID);
		if(null != result)
			_csvTables["GUBackControl"].searchAndSet<TypeCsvUnionControl>(result, "id", csvID);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	//获得某个公会图标
	public static TypeCsvUnionIconBg getUnionIconBg(int csvID)
	{
		/// 数据缓存部分
		//		string key = "public static TypeCsvPropEquipBeast getPropEquipBeast(int sIDCsvProp, int sLv) " + sIDCsvProp;
		string key = string.Intern(new StringBuilder("public static TypeCsvUnionIcon getUnionIconBg(int csvID) ").Append(csvID).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvUnionIconBg)_csvItemMemory[key];
		/// 返回值
		TypeCsvUnionIconBg result = _csvTables["unionIconBg"].searchAndNew<TypeCsvUnionIconBg>("id", csvID);
		if(null != result)
			_csvTables["unionIconBg"].searchAndSet<TypeCsvUnionIconBg>(result, "id", csvID);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}
	
	//获得所有公会图标
	public static List<TypeCsvUnionIconBg> getAllUnionIconBg()
	{
		string keyList = "public static List<TypeCsvUnionIconBg> getAllUnionIconBg()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvUnionIconBg>)_csvItemMemory[keyList]; 
		
		List<TypeCsvUnionIconBg> list = new List<TypeCsvUnionIconBg>();
		int index = 1;
		while(true)
		{	
			/// 返回值
			TypeCsvUnionIconBg result = _csvTables["unionIconBg"].searchAndNew<TypeCsvUnionIconBg>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);	
			index++;
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}


	//获得某个公会图标
	public static TypeCsvUnionIcon getUnionIcon(int csvID)
	{
		/// 数据缓存部分
		//		string key = "public static TypeCsvPropEquipBeast getPropEquipBeast(int sIDCsvProp, int sLv) " + sIDCsvProp;
		string key = string.Intern(new StringBuilder("public static TypeCsvUnionIcon getUnionIcon(int csvID) ").Append(csvID).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvUnionIcon)_csvItemMemory[key];
		/// 返回值
		TypeCsvUnionIcon result = _csvTables["UnionIcon"].searchAndNew<TypeCsvUnionIcon>("id", csvID);
		if(null != result)
			_csvTables["UnionIcon"].searchAndSet<TypeCsvUnionIcon>(result, "id", csvID);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	//获得所有公会图标
	public static List<TypeCsvUnionIcon> getAllUnionIcon()
	{
		string keyList = "public static List<TypeCsvUnionIcon> getAllUnionIcon()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvUnionIcon>)_csvItemMemory[keyList]; 

		List<TypeCsvUnionIcon> list = new List<TypeCsvUnionIcon>();
		int index = 1;
		while(true)
		{	
			/// 返回值
			TypeCsvUnionIcon result = _csvTables["UnionIcon"].searchAndNew<TypeCsvUnionIcon>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);	
			index++;
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}

	public static List<TypeCsvBeastShop> getBeastShop()
	{
		string keyList = "public static List<TypeCsvBeastShop> getBeastShop()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvBeastShop>)_csvItemMemory[keyList];

		List<TypeCsvBeastShop> list = new List<TypeCsvBeastShop>();
		int index = 1;
		while(true)
		{
//			string key = "public static List<TypeCsvBeastShop> getBeastShop()" + index;
//			string key = string.Intern(new StringBuilder("public static List<TypeCsvBeastShop> getBeastShop()").Append(index).ToString());
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				list.Add((TypeCsvBeastShop)_csvItemMemory[key]);
//				index++;
//				continue;
//			}
			/// 返回值
			TypeCsvBeastShop result = _csvTables["BeastShop"].searchAndNew<TypeCsvBeastShop>("id", index);
			if(result == null)
			{
				break;
			}
			list.Add(result);
			/// 值存储
//			_csvItemMemory.Add(key, result);
			/// 返回
			index++;
		}
		_csvItemMemory.Add(keyList, list);
		return list;
	}

	public static TypeCsvBeastElement getBeastElement(int id)
	{
		/// 数据缓存部分
//		string key = "public static TypeCsvBeastElement getBeastElement(int id) " + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvBeastElement getBeastElement(int id) ").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvBeastElement)_csvItemMemory[key];
		/// 返回值
		TypeCsvBeastElement result = _csvTables["BeastShopElement"].searchAndNew<TypeCsvBeastElement>("id", id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}

	public static TypeCsvGiftBox getGiftBox(int id)
	{
//		string key = "public static TypeCsvGiftBox getGiftBox(int id) " + id;
		string key = string.Intern(new StringBuilder("public static TypeCsvGiftBox getGiftBox(int id) ").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvGiftBox)_csvItemMemory[key];
		/// 返回值
		TypeCsvGiftBox result = _csvTables["GiftBox"].searchAndNew<TypeCsvGiftBox>("idProp", id);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}


	/// 获得 护送队伍信息
	public static TypeCsvEscortNote getEscortNote(int sType)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvEscortNote getEscortNote(int sType) = ").Append(sType).ToString());
//		string key = "public static TypeCsvEscortNote getEscortNote(int sType) = " + sType;
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvEscortNote)_csvItemMemory[key];
		/// 返回值
		TypeCsvEscortNote result = _csvTables["EscortNote"].searchAndNew<TypeCsvEscortNote>("type", sType);
		/// 值存储
		_csvItemMemory.Add(key, result);
		/// 返回
		return result;
	}


	public static List<TypeCsvSignBeast> getSignBeast()
	{
		string keyList = "public static List<TypeCsvSignBeast> getSignBeast()";
		if(_csvItemMemory.ContainsKey(keyList))
			return (List<TypeCsvSignBeast>)_csvItemMemory[keyList];


		List<TypeCsvSignBeast> temp = new List<TypeCsvSignBeast>();
		int index = 1;

		while(true)
		{
//			string key = string.Intern(new StringBuilder("public static List<TypeCsvSignBeast> getSignBeast()").Append(index).ToString());
////			string key = "public static List<TypeCsvSignBeast> getSignBeast()" + index;
//			if(_csvItemMemory.ContainsKey(key))
//			{
//				temp.Add((TypeCsvSignBeast)_csvItemMemory[key]);
//				index++;
//				continue;
//			}
			TypeCsvSignBeast result = _csvTables["SignBeast"].searchAndNew<TypeCsvSignBeast>("id",index);
			if(result == null)
			{
				break;
			}
//			_csvItemMemory.Add(key,result);
			temp.Add(result);
			index++;
		}
		_csvItemMemory.Add(keyList, temp);
		return temp;
	}

	/// 获得世界boss 奖励列表
	public static TypeCsvBossReward getBossReward(int sIDCsv)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvBossReward getBossReward(int sIDCsv) = ").Append(sIDCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvBossReward)_csvItemMemory[key];
		TypeCsvBossReward result = _csvTables["BossReward"].searchAndNew<TypeCsvBossReward>("id", sIDCsv);
		_csvItemMemory.Add(key, result);
		return result;
	}


	/// 2015.08.10 gus 
	/// 获得vip支付的条目
	public static TypeCsvVIPPay getVIPPay(int sIDCsv)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvVIPPay getVIPPay(int sIDCsv) = ").Append(sIDCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvVIPPay)_csvItemMemory[key];
		TypeCsvVIPPay result = _csvTables["VIPPay"].searchAndNew<TypeCsvVIPPay>("id", sIDCsv);
		_csvItemMemory.Add(key, result);
		return result;
	}
	/// 获得vip支付的条目 列表
	public static List<TypeCsvVIPPay> getVIPPayList()
	{
		string key = "public static TypeCsvVIPPay getVIPPayList()";
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvVIPPay>)_csvItemMemory[key];
		List<TypeCsvVIPPay> result = new List<TypeCsvVIPPay>();
		int index = 0;
		while(true)
		{
			index++;
			TypeCsvVIPPay csvVIPPay = getVIPPay(index);
			if(null != csvVIPPay)
			{
				result.Add(csvVIPPay);
				continue;
			}
			break;
		}
		_csvItemMemory.Add(key, result);
		return result;
	}
	/// 2015.08.26 gus 
	/// 获得主页面需要展示鞋啥玩意
	public static TypeCsvActiveMissionPageInfo getActiveMissionPageInfo(int sIDCsv)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvActiveMissionPageInfo getActiveMissionPageInfo(int sIDCsv) = ").Append(sIDCsv).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvActiveMissionPageInfo)_csvItemMemory[key];
		TypeCsvActiveMissionPageInfo result = _csvTables["ActiveMissionPageInfo"].searchAndNew<TypeCsvActiveMissionPageInfo>("id", sIDCsv);
		_csvItemMemory.Add(key, result);
		return result;
	}
	/// 2015.08.27 gus 
	/// 获得竞技排名奖励表
	public static TypeCsvPkReward getPKReward(int sRank)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvPkReward getPKReward(int sRank) = ").Append(sRank).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvPkReward)_csvItemMemory[key];
		TypeCsvPkReward result = _csvTables["PkReward"].searchAndNew<TypeCsvPkReward>("id", sRank);
		_csvItemMemory.Add(key, result);
		return result;
	}

	/// 2015.09.09
	/// 技能扩展属性
	public static TypeCsvHeroSkillAITarget getHeroSkillAITarget(int sSkillTarget)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvPkReward getHeroSkillAITarget(int sSkillTarget) = ").Append(sSkillTarget).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroSkillAITarget)_csvItemMemory[key];
		TypeCsvHeroSkillAITarget result = _csvTables["HeroSkillAITarget"].searchAndNew<TypeCsvHeroSkillAITarget>("atkTarget", sSkillTarget);
		_csvItemMemory.Add(key, result);
		return result;
	}

	/// 2015.09.14
	/// 获得首充属性
	public static TypeCsvFirstRechargeAct getFirstRechargeAct()
	{
		string key = "public static TypeCsvFirstRechargeAct getFirstRechargeAct()";
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvFirstRechargeAct)_csvItemMemory[key];
		TypeCsvFirstRechargeAct result = _csvTables["FirstRechargeAct"].searchAndNew<TypeCsvFirstRechargeAct>("id", 1);
		_csvItemMemory.Add(key, result);
		return result;
	}
	/// 2015.09.14 gus 
	/// 获得 单独界面信息配置
	public static TypeCsvActiveMissionPage getActiveMissionPage(int sType)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvActiveMissionPage getActiveMissionPage(int sType) = ").Append(sType).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvActiveMissionPage)_csvItemMemory[key];
		TypeCsvActiveMissionPage result = _csvTables["ActiveMissionPage"].searchAndNew<TypeCsvActiveMissionPage>("type", sType);
		_csvItemMemory.Add(key, result);
		return result;
	}

	public static TypeCsvTBCShop getTBCShop(int id)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvTBCShop getTBCShop(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvTBCShop)_csvItemMemory[key];
		TypeCsvTBCShop result = _csvTables["TBCShop"].searchAndNew<TypeCsvTBCShop>("id", id);
		_csvItemMemory.Add(key, result);
		return result;
	}
	/// 获得TBC巫医商店所有的效果
	public static List<TypeCsvTBCShop> getTBCShopList()
	{
		string key = "public static List<TypeCsvTBCShop> getTBCShopList()";
		if(_csvItemMemory.ContainsKey(key))
			return (List<TypeCsvTBCShop>)_csvItemMemory[key];
		List<TypeCsvTBCShop> result = new List<TypeCsvTBCShop>();
		int index = 0;
		while(true)
		{
			index++;
			TypeCsvTBCShop csvTBCShop = getTBCShop(index);
			if(null != csvTBCShop)
			{
				result.Add(csvTBCShop);
				continue;
			}
			break;
		}
		_csvItemMemory.Add(key, result);
		return result;
	}
	/// 2015.09.18 gus 
	/// 获得 单独界面信息配置
	public static TypeCsvHeroAttributeMathFrighting getHeroAttributeMathFrighting(int sJob)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvHeroAttributeMathFrighting getHeroAttributeMathFrighting(int sJob) = ").Append(sJob).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvHeroAttributeMathFrighting)_csvItemMemory[key];
		TypeCsvHeroAttributeMathFrighting result = _csvTables["HeroAttributeMathFrighting"].searchAndNew<TypeCsvHeroAttributeMathFrighting>("job", sJob);
		_csvItemMemory.Add(key, result);
		return result;
	}

	public static TypeCsvStoneCreat getStoneCreat(int stoneChipId)
	{
		string key = string.Intern(new StringBuilder("public static List<TypeCsvStoneCreat> getStoneCreat(int stoneChipId)").Append(stoneChipId).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvStoneCreat)_csvItemMemory[key];
		TypeCsvStoneCreat result = _csvTables["StoneCreat"].searchAndNew<TypeCsvStoneCreat>("id", stoneChipId);
		_csvItemMemory.Add(key, result);
		return result;
	}

	public static TypeCsvPropMagic getPropMagic(int id)
	{
		string key = string.Intern(new StringBuilder("public static TypeCsvPropMagic getPropMagic(int id)").Append(id).ToString());
		if(_csvItemMemory.ContainsKey(key))
			return (TypeCsvPropMagic)_csvItemMemory[key];
		TypeCsvPropMagic result = _csvTables["PropMagic"].searchAndNew<TypeCsvPropMagic>("id", id);
		_csvItemMemory.Add(key, result);
		return result;
	}
}
