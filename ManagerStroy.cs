using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerStroy
{
	
	public enum InfoStroy
	{
		NULL = -1,
		FB_LAND_0 = 0,
		FB_LAND_1 = 1,
		FB_LAND_2 = 2,
		FB_LAND_3 = 3,
		FB_LAND_4 = 4,
		FB_LAND_5 = 5,
		FB_LAND_6 = 6,
		FB_LAND_7 = 7,
		FB_LAND_8 = 8,
		FB_LAND_9 = 9,
		FB_LAND_10 = 10,
		FB_LAND_11 = 11,
		FB_LAND_12 = 12,
		FB_LAND_13 = 13,
		FB_LAND_14 = 14,
		FB_LAND_15 = 15,
		FB_LAND_16 = 16,
		FB_LAND_17 = 17,
		FB_LAND_18 = 18,
		FB_LAND_19 = 19,
		FB_LAND_20 = 20,
		
		
		BEAST_SKILL_OVER = 47,
		BEAST_SKILL = 48,
		BEAST_SKILL_FORE = 49,

		/// 追加技能引导
		SKILL_SAME_6_OVER = 50,
		SKILL_SAME_7_OVER = 51,
		
		SKILL_SAME_6 = 52,
		SKILL_SAME_7 = 53,
		
		/// 追加技能引导
		SKILL_SAME_4_OVER = 54,
		SKILL_SAME_5_OVER = 55,
		
		SKILL_SAME_4 = 56,
		SKILL_SAME_5 = 57,
		/// 技能引导
		SKILL_SAME_1_OVER = 58,
		SKILL_SAME_2_OVER = 59,
		SKILL_SAME_3_OVER = 60,

		SKILL_SAME_1 = 61,
		SKILL_SAME_2 = 62,
		SKILL_SAME_3 = 63
		
	}
	private static int[] _STROY_LAND_ID = new int[]{
		/// 副本大陆剧情
		0000001,
		0100001,
		0200001,
		0300001,
		0400001,
		0500001,
		0600001,
		0700001,
		0800001,
		0900001,
		1000001,
		1100001,
		1200001,
		1300001,
		1400001,
		1500001,
		1600001,
		1700001,
		1800001,
		1900001,
		2000001
	}; 
	/// 打开ui 是不是第一次使用
	public static bool isStroy(InfoStroy idStroy)
	{
		/// 如果是空的，没有剧情返回
		if(null == getCsvStroy(idStroy))
			return false;
//		return true;
		/// 如果是副本中技能引导
		if( idStroy == InfoStroy.SKILL_SAME_1 || 
			idStroy == InfoStroy.SKILL_SAME_2 || 
			idStroy == InfoStroy.SKILL_SAME_3 || 
			idStroy == InfoStroy.SKILL_SAME_4 ||
			idStroy == InfoStroy.SKILL_SAME_5 ||
			idStroy == InfoStroy.SKILL_SAME_6 ||
			idStroy == InfoStroy.SKILL_SAME_7
			)
			/// 如果战斗没开始 如果不是副本战斗 如果主角是空 如果主句等级大于10 有一个成立就不引导
			if(!AICombat.isStartCombat || AICombat.combatType != 1 || DataMode.myPlayer == null || DataMode.myPlayer.lv > 20)
				return false;

		/// 副本中引导魂兽技能
		if(idStroy == InfoStroy.BEAST_SKILL || idStroy == InfoStroy.BEAST_SKILL_OVER)
			if(!AICombat.isStartCombat || AICombat.combatType != 1 || DataMode.myPlayer == null)
				return false;
		/// 副本中引导魂兽技能 前置
		if(idStroy == InfoStroy.BEAST_SKILL_FORE)
			if(AICombat.combatType != 1 || DataMode.myPlayer == null)
				return false;

		/// 如果展示过 返回
		if(!DataMode.infoSetting.isStroy((int)idStroy))
			return false;
		
		/// 如果不是第一次
		return true;
	}
	/// 显示剧情
	private static void showStroy(InfoStroy sInfoStroy)
	{
		/// 如果有剧情
		if(isStroy(sInfoStroy))
		{
			SuperUI.showNew<UIStory>("GusUIStory");
			SuperUI.getUI<UIStory>().setStory(getCsvStroy(sInfoStroy));
			SuperUI.getUI<UIStory>().finishedActionHD = _finishedActionHD;
			SuperUI.getUI<UIStory>().infoStroy = sInfoStroy;
			_isLoadShowStroy = false;
		}
	}
	/// 获得引导的资源地址
	public static void loadShowStroy(InfoStroy sInfoStroy, Function sFinishedActionHD)
	{
		/// 如果有剧情
		if(isStroy(sInfoStroy))
		{
			_infoStroy = sInfoStroy;
			_finishedActionHD = sFinishedActionHD;
			_isLoadShowStroy = true;
			
			List<string> urls = new List<string>();
			urls.Add(ConfigUrl.getAtlasUrl("guide"));
			
			ConfigUrl.getAssetsStory(urls, getCsvStroy(sInfoStroy).id);
			
			/// 资源剔除
			for(int index = urls.Count - 1; index >= 0; index--)
			{
				if(LoadMngr.getInstance().isHasAsset(urls[index]))
					urls.RemoveAt(index);
			}
			/// 下载
			_loadID = -1d;
			if(urls.Count > 0)
				_loadID = LoadMngr.getInstance().load(urls.ToArray(), loadStroyHD);
			else 
				loadStroyHD(-1d);
		}
	}
	
	/// 返回是哪个剧情的ID
	private static TypeCsvStory getCsvStroy(InfoStroy sInfoStroy)
	{
		/// 板块引导
		if((int)sInfoStroy < _STROY_LAND_ID.Length)
			return ManagerCsv.getStory(_STROY_LAND_ID[(int)sInfoStroy]);




		/// 技能1的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_1)
			return ManagerCsv.getStory(4000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_1_OVER)
			return ManagerCsv.getStory(4100001);
		/// 技能2的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_2)
			return ManagerCsv.getStory(5000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_2_OVER)
			return ManagerCsv.getStory(5100001);
		/// 技能3的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_3)
			return ManagerCsv.getStory(6000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_3_OVER)
			return ManagerCsv.getStory(6100001);
		
		/// 技能4的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_4)
			return ManagerCsv.getStory(7000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_4_OVER)
			return ManagerCsv.getStory(7100001);
		
		/// 技能5的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_5)
			return ManagerCsv.getStory(8000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_5_OVER)
			return ManagerCsv.getStory(8100001);
		
		/// 技能6的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_6)
			return ManagerCsv.getStory(9000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_6_OVER)
			return ManagerCsv.getStory(9100001);
		
		/// 技能7的引导
		if(sInfoStroy == InfoStroy.SKILL_SAME_7)
			return ManagerCsv.getStory(10000001);
		if(sInfoStroy == InfoStroy.SKILL_SAME_7_OVER)
			return ManagerCsv.getStory(10100001);

		/// 魂兽技能引导 进副本的时候 提示 
		if(sInfoStroy == InfoStroy.BEAST_SKILL_FORE)
			return ManagerCsv.getStory(11000001);
		/// 魂兽技能引导 释放技能 
		if(sInfoStroy == InfoStroy.BEAST_SKILL)
			return ManagerCsv.getStory(11100001);
		/// 魂兽技能引导 释放技能 结束
		if(sInfoStroy == InfoStroy.BEAST_SKILL_OVER)
			return ManagerCsv.getStory(11200001);

		/// 没有 返回空
		return null;
	}
	
	/// 下载显示
	private static void loadStroyHD(double sLoadID)
	{
		if(sLoadID != _loadID)
			return;
		showStroy(_infoStroy);
	}
	
	
	private static bool _isLoadShowStroy = false;
	public static bool isLoadShowStroy{get{return _isLoadShowStroy;}}
	private static Function _finishedActionHD;
	private static InfoStroy _infoStroy;
	private static double _loadID;
}
