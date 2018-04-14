using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// 战斗数据
public class ManagerCombatData
{
	/// 我的战斗数据缓存
	private static List<TypeCombatData> _itemCombatData = new List<TypeCombatData>();
	/// 出手次序
	private static int _indexRel = 0;
	public static int indexRelSign{get{return _indexRel++;}}
	
	
	/// 清除数据
	public static void clearData()
	{
		_itemCombatData.Clear();
	}
	/// 还原ID
	public static void clearIndex()
	{
		_indexRel = 0;
	}
	/// 添加数据
	public static void addData(TypeCombatData combatData)
	{
		if(!AICombat.isStartCombat)
			return;
		_itemCombatData.Add(combatData);
		if(_itemCombatData.Count % 60 == 0)
		{
			switch(AICombat.combatType)
			{
				/// 副本
			case 1:
				DataModeServer.sendFBCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 竞技
			case 2:
				DataModeServer.sendPKCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 世界boss
			case 3:
				DataModeServer.sendWorldBossCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 爬塔战斗
			case 4:
				DataModeServer.sendTowerCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 羞辱好友
			case 5:
				DataModeServer.sendFriendCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 好友复仇
			case 6:
				DataModeServer.sendFriendCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 活动副本
			case 8:
				DataModeServer.sendActiveCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 燃烧的远征
			case 9:
				DataModeServer2.sendTBCCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 太阳井
			case 10:
				DataModeServer2.sendActiveCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 护送掠夺
			case 11:
				DataModeServer3.sendEscortCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;
				/// 燃烧的远征 奖励关
			case 12:
				DataModeServer3.sendTBCRewardCombatData(_itemCombatData.GetRange((_itemCombatData.Count / 60 - 1) * 60, 60), _itemCombatData.Count / 60);
				break;



			}
		}
	}
	/// 获得属性
	public static List<TypeCombatData> getCombatLast()
	{
		if(_itemCombatData.Count % 60 == 0) 
			return null;
		return _itemCombatData.GetRange(_itemCombatData.Count / 60 * 60, _itemCombatData.Count - (_itemCombatData.Count / 60 * 60));
	}
	/// 获得属性
	public static List<TypeCombatData> getCombat()
	{
		return _itemCombatData;
	}
	
}
/// 我的战斗数据
public class TypeCombatData : SuperType
{	
	/// 被攻击怪物位置
	public int standIndexAttack;
	/// 攻击怪物位置
	public int standIndexBeaten;
	
	/// 攻击技能/或者buff
	public int idCsv;
	
	/// 出手次序
	public int indexRel;
	/// 技能的第几波伤害
	public int indexRelBatch;
	
	/// 被攻击者状态（位操作1111 0闪避 1暴击 2死亡）
	public byte state = new byte();
	
	
	/// 伤害类型 0技能 1buff反弹 2buff掉血(仅死亡有) 3复活 4吸血技能 5吸血buff 6召唤生物放的技能伤害 7空放技能
	public byte atkHPType = new byte();
	///正伤害量/负加血量
	public int atkHP = new int();
	///时间 
	public double timeTeamp = new double();
}
