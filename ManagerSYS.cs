using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// 管理系统工具
public class ManagerSYS
{

	public static List<string> clearListener = new List<string>();
	/// 清理场景 有选择清理,或者无序清理
	public static void clear()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> clear()");
		ConfigUrl._MEMORY_ATLAS_ALL.Clear();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 1");
		ManagerCsv.clearSenceCsv();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 2");
		ManagerSence.clearAnimation();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 3");
		ManagerSence.isLockSence = false;
		CameraMoveData.valueData.fogTween.clear();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 4");
		/// 删除全部不是 gameobject
		CameraMoveData.valueData.changeMove(null);
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 5");
		CameraMoveData.valueData.camera.enabled = false;
		DataMode.infoSetting.isRelSkill = 0;
		/// 清除战斗中缓存的对象
		ManagerCombat.hashCombatClear();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 6");
		/// 清除技能数据
		AICombatSkillRelease.clear();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 7");
		/// 进行销毁
		clearSenceObject();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 8");
		/// 关闭gus的全部ui
		clearUI();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 9");
		/// 清除战斗数据
		AICombat.clearCombat();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 10");
		/// 清除内容
		ControlTouch.clearStateMultiTwo();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 11");
		AIPlayerTown.indexSelect = 0;
		
		LoadMngr.getInstance().clearAllLoader();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 12");
		WindowsMngr.getInstance().ResetAllUI();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 13");
		foreach(string listener in clearListener)
		{
			UtilListener.removeListener(listener);
		}
		clearListener.Clear();
		/// 各种数据恢复
		DataMode.infoSetting.isPause = false;
		DataMode.infoSetting.speed = 1f;
		DataMode.infoSetting.unPause.Clear();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 14");
		/// 各种销毁
		ManagerCombatBeast.clear();
		if(UtilLog.isBulidLog)UtilLog.Log("ManagerSYS >> 15");
	}
	/// 删除所有UI
	public static void clearUI()
	{
		SuperUI.closeAll();
		WindowsMngr.getInstance().closeWindowExcept(WindowsID.LOADING);
		/// 防止强关
		ManagerMouse.valueData.isWithOut = true;
	}
	/// 删除场景玩意
	public static void clearSenceObject()
	{
		/// 删除控制器中的内容
		SuperObject[] controlSuperObject = (SuperObject[])GameObject.FindObjectsOfType(typeof(SuperObject));
		if(null != controlSuperObject)
		{
			foreach(SuperObject obj in controlSuperObject)
			{
				if(null != obj)
					obj.destroy();
			}
		}
		/// 删除控制器中的内容
		EffectParticleSystem[] controlEffectParticleSystem = (EffectParticleSystem[])GameObject.FindObjectsOfType(typeof(EffectParticleSystem));
		if(null != controlEffectParticleSystem)
		{
			foreach(EffectParticleSystem obj in controlEffectParticleSystem)
			{
				if(null != obj)
					obj.destroy();
			}
		}
		
		/// 删除控制器中的内容
		Bullet[] controlBullet = (Bullet[])GameObject.FindObjectsOfType(typeof(Bullet));
		if(null != controlBullet)
		{
			foreach(Bullet obj in controlBullet)
			{
				if(null != obj)
					obj.destroy();
			}
		}

		/// 删除控制器中的内容
		DropBox[] controlDropBox = (DropBox[])GameObject.FindObjectsOfType(typeof(DropBox));
		if(null != controlDropBox)
		{
			foreach(DropBox obj in controlDropBox)
			{
				if(null != obj)
					obj.destroy();
			}
		}

		/// 删除全部不是 gameobject
		GameObject[] monos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
		CameraMoveData.valueData.camera.enabled = false;
		DataMode.infoSetting.isRelSkill = 0;
		/// 进行销毁
		foreach(GameObject obj in monos)
		{
			try
			{
				/// 是空的
				if(null == obj)
					continue;
				/// 不删除
				if(obj.tag == Config.UNDESTROY)
					continue;
				if(obj.tag == Config.MAIN_CAMERA)
					continue;
			}
			catch(Exception e){
				UtilLog.LogError(obj.name);
			}
			
			/// 父级单位不为空,下一个
			if(null != obj.transform.parent)
				continue;
			/// 进行完全销毁
			GameObject.Destroy(obj);
		}
		
		UICombatEffectText.clear();
		
		/// 清除lightmap
		LightmapSettings.lightmaps = new LightmapData[0]{};
	}
}

