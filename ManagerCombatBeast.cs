using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerCombatBeast
{
	/// 我放的魂兽
	public static AIBeastData beastSlef;
	/// 敌方的魂兽
	public static AIBeastData beastEnemy;

	/// 魂兽添加的光环
	public static void addBuffBeast(AICombatData sCombatData)
	{
		/// 友方 光环
		if(null != beastSlef && null != beastSlef.csvBeast.idBeastBuff)
		{
			List<int> buffArr = new List<int>();
			for(int index = 0; index < beastSlef.csvBeast.idBeastBuff.Length; index++)
			{
				/// 没解锁的光环加不上
				if(beastSlef.csvBeast.beastBuffLimitLen <= index)
					break;
				/// 判断敌我获取光环
				string[] buffInfo = beastSlef.csvBeast.idBeastBuff[index];
				if(sCombatData.standIndex < 9 && "0" == buffInfo[1])
					buffArr.Add(int.Parse(buffInfo[0]));
				if(sCombatData.standIndex >= 9 && "1" == buffInfo[1])
					buffArr.Add(int.Parse(buffInfo[0]));
			}
			if(null != buffArr)
			{
				foreach(int idBuff in buffArr)
				{
					TypeCombatBuff combatBuff = new TypeCombatBuff();
					combatBuff.csvBuff = ManagerCsv.getBuff(idBuff);
					combatBuff.attackerBeast = beastSlef.dataAttCount;
					/// buff出生的地方
					combatBuff.self = sCombatData;
					/// 计算buff增加的属性 
					combatBuff.mathAttCountBeast();
					/// 下载资源
					if(null != combatBuff.csvBuff.view && "#" != combatBuff.csvBuff.view && (!DataMode.myPlayer.isInFBWorldBoss || sCombatData.myServerHero != null))
					{
						EffMngr3.getInstance().getNoBindEff(combatBuff.csvBuff.view, combatBuff.assetViewComplete, new Vector3(1000f,1000f,1000f), Quaternion.identity);
						UtilListener.addEventListener("EventAnimationSpeed", combatBuff.changeSpeedHD);
					}
					if(null != combatBuff.csvBuff.viewBoss && "#" != combatBuff.csvBuff.viewBoss && (DataMode.myPlayer.isInFBWorldBoss && sCombatData.myServerHero == null))
					{
						EffMngr3.getInstance().getNoBindEff(combatBuff.csvBuff.viewBoss, combatBuff.assetViewComplete, new Vector3(1000f,1000f,1000f), Quaternion.identity);
						UtilListener.addEventListener("EventAnimationSpeed", combatBuff.changeSpeedHD);
					}
					sCombatData.myBuffsBeast.Add(combatBuff);
				}
			}
		}
		/// 敌方 光环
		if(null != beastEnemy && null != beastEnemy.csvBeast.idBeastBuff)
		{

			List<int> buffArr = new List<int>();
			for(int index = 0; index < beastEnemy.csvBeast.idBeastBuff.Length; index++)
			{
				/// 没解锁的光环加不上
				if(beastEnemy.csvBeast.beastBuffLimitLen <= index)
					break;

				/// 判断敌我获取光环
				string[] buffInfo = beastEnemy.csvBeast.idBeastBuff[index];
				if(sCombatData.standIndex >= 9 && "0" == buffInfo[1])
					buffArr.Add(int.Parse(buffInfo[0]));
				if(sCombatData.standIndex < 9 && "1" == buffInfo[1])
					buffArr.Add(int.Parse(buffInfo[0]));
			}
			if(null != buffArr)
			{
				foreach(int idBuff in buffArr)
				{
					TypeCombatBuff combatBuff = new TypeCombatBuff();
					combatBuff.csvBuff = ManagerCsv.getBuff(idBuff);
					combatBuff.attackerBeast = beastEnemy.dataAttCount;
					/// buff出生的地方
					combatBuff.self = sCombatData;
					/// 计算buff增加的属性 
					combatBuff.mathAttCountBeast();
					/// 下载资源
					if(null != combatBuff.csvBuff.view && "#" != combatBuff.csvBuff.view && (!DataMode.myPlayer.isInFBWorldBoss || sCombatData.myServerHero != null))
					{
						EffMngr3.getInstance().getNoBindEff(combatBuff.csvBuff.view, combatBuff.assetViewComplete, new Vector3(1000f,1000f,1000f), Quaternion.identity);
						UtilListener.addEventListener("EventAnimationSpeed", combatBuff.changeSpeedHD);
					}
					if(null != combatBuff.csvBuff.viewBoss && "#" != combatBuff.csvBuff.viewBoss && (DataMode.myPlayer.isInFBWorldBoss && sCombatData.myServerHero == null))
					{
						EffMngr3.getInstance().getNoBindEff(combatBuff.csvBuff.viewBoss, combatBuff.assetViewComplete, new Vector3(1000f,1000f,1000f), Quaternion.identity);
						UtilListener.addEventListener("EventAnimationSpeed", combatBuff.changeSpeedHD);
					}
					sCombatData.myBuffsBeast.Add(combatBuff);
				}
			}
		}
	}
	/// 隐藏
	public static void hide()
	{
		if(null != beastSlef)
		{
			beastSlef.releaseOver();
		}
		if(null != beastEnemy)
		{
			beastEnemy.releaseOver();
		}
	}
	/// 清除
	public static void clear()
	{
		if(null != beastSlef)
		{
			GameObject.Destroy(beastSlef);
			GameObject.Destroy(beastSlef.gameObject);
			beastSlef = null;
		}
		if(null != beastEnemy)
		{
			GameObject.Destroy(beastEnemy);
			GameObject.Destroy(beastEnemy.gameObject);
			beastEnemy = null;
		}
	}
}
