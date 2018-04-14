using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
 * 小红点管理类
 * 整体逻辑是晓航写的，有问题可以问他
 * 后来就添加了魂兽小红点，跟晓航思路是一样的
 * 
 * 写一下优化方案：
 * 现在会卡一下，就是每次数据变化的时候，刷新数据的时候，把所有功能涉及到的都刷新了一遍
 * 以后每次打开某个界面，取数据的时候，有新的数据需要刷新，直接就把那个界面的对应数据刷新就好，
 * 其他界面的数据不要刷新
 */

public class InfoBeastRed
{
	public ulong idServer;
	public int idCsv;

	private bool _canDress = false;

	public bool canDress
	{
		get
		{
			return _canDress;
		}
	}

	public void SetDress(bool can)
	{
		_canDress = can;
	}
}
//合成红点类
public class InfoCreatRed
{
	private bool _canCreat = false;
	private long _money;
	public void SetCreat(bool can, long money)
	{
		_canCreat = can;
		_money = money;
	}

	public bool canCreat
	{
		get
		{
			return _canCreat && (DataMode.myPlayer.money_game >= _money);
		}
	}
}

public class InfoHeroRed
{
	//add by yxh 红点逻辑 以下是客户端逻辑 
	
	private bool _canDress = false;
	private bool _canInlay = false;
	private bool _canStoneUp = false;
	private bool _canStarUp = false;
	private long _money_creatStone = 0;
	private long _money_starUp = 0;
	
	#region 接口
	/// 是否有装备可以穿戴的红点 
	public bool canDress
	{
		get
		{
			return _canDress;
		}
	}
	
	/// 是否可以镶嵌的红点
	public bool canInlay
	{
		get
		{
			return _canInlay && (DataMode.myPlayer.money_game >= _money_creatStone);
		}
	}
	
	/// 是否可以进阶的红点
	public bool canStoneUp
	{
		get
		{
			return _canStoneUp;
		}
	}

	/// 是否能进化的红点
	public bool canStarUp
	{
		get
		{
			return _canStarUp && (DataMode.myPlayer.money_game >= _money_starUp);
		}
	}
	#endregion
	
	
	#region 方法
	//装备
	public void SetDress(bool can)
	{
		_canDress = can;
	}
	//镶嵌
	public void SetInlay(bool can,long money)
	{
		_canInlay = can;
		_money_creatStone = money;
	}
	//进阶石进阶
	public void SetStoneUp(bool can)
	{
		_canStoneUp = can;
	}
	//进化 升星
	public void SetStarUp(bool can,long money)
	{
		_canStarUp = can;
		_money_starUp = money;
	}
	#endregion
}

public class ManagerRed{
	private ManagerRed(){}
	private static ManagerRed _self;
	public static ManagerRed getInstance()
	{
		if(_self == null)
		{
			_self = new ManagerRed();
		}
		return _self;
	}
	// 英雄-->进阶 镶嵌 进化 穿戴装备 <--的缓存
	private Dictionary<ulong,InfoHeroRed> _cacheHero = new Dictionary<ulong, InfoHeroRed>();
	//用来和下面碎片合成英雄dic做对比用的
	private List<int> _cacheHeroIdSame = new List<int>();

	//碎片合成英雄
	private Dictionary<int,InfoCreatRed> _cacheHeroCreat = new Dictionary<int,InfoCreatRed>();
	//合成装备的缓存 
	private Dictionary<int,InfoCreatRed> _cacheEquipCreat = new Dictionary<int, InfoCreatRed>();
	//升级装备的缓存
	private Dictionary<int,InfoCreatRed> _cacheEquipLvUp = new Dictionary<int, InfoCreatRed>();

/// 魂兽 穿装备
	private Dictionary<ulong,InfoBeastRed> _cacheBeast = new Dictionary<ulong, InfoBeastRed>();
	//魂兽 装备进阶
	private Dictionary<int,InfoCreatRed> _cacheBeastEquipLvUp = new Dictionary<int, InfoCreatRed>();
	//宝箱
	private bool _haveBoxCanOpen = false;

	/// 全是临时变量 使用前需要设置 省的每次都要new 
	//宝石(往身上放东西被)  材料(装备合成的东西)  强化(傻傻分不清楚啊)
	bool temp_red_prop = false;
	/// 装备
	bool temp_red_equip = false;
	// 宝石碎片
	bool temp_red_stone_chip = false;
	//魂兽装备进阶
	bool temp_red_beast_equipUp = false;
	//魂兽是否有装备
	bool temp_red_beast_hasEquip = false;
	//英雄碎片
	bool temp_red_chip = false;
	//经验
	bool temp_red_exp = false;
	//获得了一个新的英雄
	bool temp_new_hero = false;

	TypeCsvProp temp_csv_prop = null;
	TypeCsvConsume temp_csv_consume = null;
	List<int> propData = null;
	
	public bool Ini = false;


	///接口
	/// 所有外部调用的判断各个地方的红点用
	public bool isBeastHasEquipDress(ulong idServer)
	{
		UpdateRed();
		if(_cacheBeast.ContainsKey(idServer))
		{
			return _cacheBeast[idServer].canDress;
		}
		return false;
	}

	public bool isBeastEquipUp(int csvId)
	{
		UpdateRed();	
		if(_cacheBeastEquipLvUp.ContainsKey(csvId))
			return _cacheBeastEquipLvUp[csvId].canCreat;

		return false;
	}


	/// 是否有英雄能戴装备 
	public bool HeroCanDress(ulong idServer)
	{
		UpdateRed();
		if(_cacheHero.ContainsKey(idServer))
		{
			return _cacheHero[idServer].canDress;
		}
		return false;
	}
	
	/// 是否有英雄能镶嵌
	public bool HeroCanInlay(ulong idServer)
	{
		UpdateRed();
		if(_cacheHero.ContainsKey(idServer))
		{
			if(_cacheHero[idServer].canInlay)
			{
				return true;
			}
			else
			return false;
		}
		return false;
	}
	
	/// 是否有英雄能进阶
	public bool HeroCanStoneUp(ulong idServer)
	{
		UpdateRed();
		if(_cacheHero.ContainsKey(idServer))
		{
			if( _cacheHero[idServer].canStoneUp)
			{
				return true;
			}
			else
				return false;
		//	return _cacheHero[idServer].canStoneUp;
		}
		return false;
	}
	
	/// 是否有英雄能进化
	public bool HeroCanStarUp(ulong idServer)
	{
		UpdateRed();	
		if(_cacheHero.ContainsKey(idServer))
		{
			return _cacheHero[idServer].canStarUp;
		}
		return false;
	}

	//是否有英雄可以合成
	public bool isHeroChipCanCreate()
	{
		UpdateRed();
		foreach(InfoCreatRed red in _cacheHeroCreat.Values)
		{
			if(red.canCreat)
			{
				return true;
			}
		}
		return false;
	}

	
	/// 装备是否可以合成
	public bool EquipCanCreate(int equipId)
	{
		UpdateRed();	
		if(_cacheEquipCreat.ContainsKey(equipId))
		{
			return _cacheEquipCreat[equipId].canCreat;
		}
		return false;
	}
	
	///  装备是否可以强化
	public bool EquipCanUp(int equipId)
	{
		UpdateRed();	
		if(_cacheEquipLvUp.ContainsKey(equipId))
		{
			return _cacheEquipLvUp[equipId].canCreat;
		}
		return false;
	}
	
	/// 判断是否有更强的装备可以装备
	public bool isHasUperEquip(int csvId , InfoHero hero)
	{
		TypeCsvPropEquip equip = ManagerCsv.getPropEquip(csvId);
		TypeCsvHero heroCsv = ManagerCsv.getHero(hero.idCsv);
		List<InfoProp> temp =  DataMode.myPlayer.infoPropList.getProps();
	//	List<InfoProp> proplist = DataMode.myPlayer.infoPropList.getProps();
		///0武器1头盔2胸甲3腿甲4披风5戒指
		/*头盔：防御和
			胸甲：防御和
			护腿：生命
			披风：生命
			戒指：攻击
			武器：攻击*/
		float num = 0;
		if(equip.local == 0||equip.local == 5)
		{
			//武器,戒指
			num+=equip.atk;
			num+=equip.magic;
			
			if(temp != null)
			{
				//遍历所有装备
				foreach(InfoProp subcsvequip in temp)
				{
					if(subcsvequip != null)
					{
						//遍历每件装备的限制职业
						TypeCsvPropEquip csvequip = ManagerCsv.getPropEquip(subcsvequip.idCsv);
						if(csvequip == null)
							continue;
						for(int i = 0; i < csvequip.limitJob.Length; i++)
						{
							//如果职业是可以的
							if(csvequip.limitJob[i].ToString() == heroCsv.job.ToString() && csvequip.limitLv <= hero.lv && csvequip.local == equip.local )
							{
								float aimNum = 0;
								aimNum += csvequip.atk;
								aimNum+= csvequip.magic;
								if(num < aimNum)
								{
									return true;
								}
							
							}
						}
					}
				}
			}
		}
		else if(equip.local == 1||equip.local == 2)
		{
			//头盔,胸甲
			num+=equip.atkDef;
			num+=equip.magicDef;
			if(temp != null)
			{
				//遍历所有装备
				foreach(InfoProp subcsvequip in temp)
				{
					if(subcsvequip != null)
					{
						//遍历每件装备的限制职业
						TypeCsvPropEquip csvequip = ManagerCsv.getPropEquip(subcsvequip.idCsv);
						if(csvequip == null)
							continue;
						for(int i = 0; i < csvequip.limitJob.Length; i++)
						{
							//如果职业是可以的
							if(csvequip.limitJob[i].ToString() == heroCsv.job.ToString() && csvequip.limitLv <= hero.lv && csvequip.local == equip.local )
							{
								float aimNum = 0;
								aimNum += csvequip.atkDef;
								aimNum+= csvequip.magicDef;
								if(num < aimNum)
								{
									return true;
								}
							
							}
						}
					}
				}
			}
		}
		else if(equip.local == 3||equip.local == 4)
		{
			//腿甲，披风
			num+=equip.hp;
			
			if(temp != null)
			{
				//遍历所有装备
				foreach(InfoProp subcsvequip in temp)
				{
					if(subcsvequip != null)
					{
						//遍历每件装备的限制职业
						TypeCsvPropEquip csvequip = ManagerCsv.getPropEquip(subcsvequip.idCsv);
						if(csvequip == null)
							continue;
						for(int i = 0; i < csvequip.limitJob.Length; i++)
						{
							//如果职业是可以的
							if(csvequip.limitJob[i].ToString() == heroCsv.job.ToString() && csvequip.limitLv <= hero.lv && csvequip.local == equip.local )
							{
								float aimNum = 0;
								aimNum += csvequip.hp;
								if(num < aimNum)
								{
									return true;
								}
							
							}
						}
					}
				}
			}
		}
		return false;
	}
	
	public bool isHasShopFree()
	{
		TypeCsvAttribute attr = ManagerCsv.getAttribute();
		int friendlyCD = attr.luckyShopFriend_cd ;
		int stoneCD = attr.luckyShopQMoney_cd;
		int FreeTimes = attr.luckyShopFriendFreeTimes ;
		int friendlyTimes = DataMode.infoLuckyShop.FriendTodayFreeTimes;
		double curtime = Data.serverTime;
		double friendlastTime =(double) DataMode.infoLuckyShop.LastTimeFriend;
		if((int)(curtime -friendlastTime ) >= friendlyCD && ((FreeTimes-friendlyTimes) > 0) )
		{
			return true;
		}
		
		double stoneTime = (double) DataMode.infoLuckyShop.LastTimeQMoney;
		if((int)(curtime -stoneTime ) >= stoneCD )
		{
			return true;
		}
		return false;
	}

	//商队是否完成
	public bool HaveGroupFinish()
	{
		UpdateRed();

		for(int i =0; i<DataMode.infoEscort.idServerEscortSafe.Count; i++)
		{
			InfoEscortSafe _escort = DataMode.getEscortSafe(DataMode.infoEscort.idServerEscortSafe[i]);
			if(_escort.type == 0)
			{
				float _temp = (float)(UIData.getTranSportationTime(_escort.typeSafe) - (Data.serverTime - _escort.timeStamp));
				if(_temp < 0)
					return true;
			}
		}

		return false;
	}

	//发送协议  获取商队信息
	public void sendGetGroupInfo()
	{
		//所有商队ID,获取信息
		DataModeServer3.sendEscortInfoList(null);
	}

	//商队被抢了
	public bool HaveGroupBeRobbed()
	{
		for(int i =0; i<DataMode.infoEscort.idServerEscortSafe.Count; i++)
		{
			InfoEscortSafe _escort = DataMode.getEscortSafe(DataMode.infoEscort.idServerEscortSafe[i]);
			if(_escort.type == 2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveBoxToOpen()
	{
		UpdateRed();
		return _haveBoxCanOpen;
	}






	/// <summary>
	/// 红底维护类 刷新各种红点信息
	/// </summary>



	private void UpdateRed()
	{
		if(DataMode.myPlayer != null)
		{
			if(DataMode.myPlayer.infoPropList.needResetRed || DataMode.infoHeroChip.needResetRed || !Ini ||
			   _cacheHero.Count != DataMode.myPlayer.infoHeroList.GetHeroListCount())
			{
				#region 初始化 设置需要更新的内容
				//没有数据的时候 也就是没有初始化的时候 初始化 刷新所有
				if(!Ini)
				{
					temp_red_equip = true;
					temp_red_prop = true;
					temp_red_stone_chip = true;
					temp_red_chip = true;
					temp_new_hero = false;
					temp_red_exp = true;
					_haveBoxCanOpen = false;
					_cacheHeroCreat.Clear();
					_cacheHero.Clear();
					_cacheHeroIdSame.Clear();
					_cacheEquipCreat.Clear();
					_cacheEquipLvUp.Clear();
					_cacheBeast.Clear();
					_cacheBeastEquipLvUp.Clear();
					Ini = true;
				}
				//如果是背包发生变化
				else 
				{
					temp_red_equip = false;
					temp_red_prop = false;
					temp_red_stone_chip = false;
					temp_red_chip = false;
					temp_new_hero = false;
					temp_red_exp = false;
					if(DataMode.myPlayer.infoPropList.needResetRed)
					{
						propData = DataMode.myPlayer.infoPropList.GetChangeData();
						for(int i = 0; i < propData.Count; i++)
						{
							temp_csv_prop = ManagerCsv.getProp(propData[i]);
							if(temp_csv_prop == null)
								continue;
							
							//进阶石碎片
							if(temp_csv_prop.type == 10)
							{
								temp_red_stone_chip = true;
							}
							
							//装备
							if(temp_csv_prop.type == 1)
							{
								temp_red_equip = true;
							}
							
							//材料
							if(temp_csv_prop.type == 2)
							{
								temp_red_prop = true;
							}

							if(temp_csv_prop.type >= 6 &&
							   temp_csv_prop.type <= 9)
							{
								temp_red_exp = true;
							}
						}
					}

					if(DataMode.infoHeroChip.needResetRed)
					{
						temp_red_chip = true;
					}

					if(_cacheHero.Count != DataMode.myPlayer.infoHeroList.GetHeroListCount())
					{
						temp_new_hero = true;
					}
				}

				#endregion

				#region 英雄相关
				if(temp_red_prop || temp_red_stone_chip || temp_red_equip || temp_red_chip || temp_new_hero || temp_red_exp)
				{

					///先从英雄开始吧~~~~~~~~~~
					///初始化英雄列表 所有需要更新的英雄都在这里
					List<InfoHero> heroList = DataMode.myPlayer.infoHeroList.getHeros();
					//临时变量 
					InfoHero hero = null;
					InfoHeroRed heroRed = null;
					TypeCsvHero csvhero = null;
					for(int i = 0; i < heroList.Count; i++)
					{
						//当初始化或者新合成一个卡牌的时候 这个变量能起到强制更新的作用
						bool needReset = false;
						if(!_cacheHero.ContainsKey(heroList[i].idServer))
						{
							_cacheHero.Add(heroList[i].idServer, new InfoHeroRed());
							needReset = true;
						}
						heroRed = _cacheHero[heroList[i].idServer];
						hero = heroList[i];

						int idsame = ManagerCsv.getHero(heroList[i].idCsv).idSame;
						if(_cacheHeroIdSame.IndexOf(idsame) < 0)
						{
							_cacheHeroIdSame.Add(idsame);
						}

						//物品变化 涉及红点的--->>>装备 进阶 镶嵌 
						if(DataMode.myPlayer.infoPropList.needResetRed || needReset)
						{
							//材料变了 涉及到 装备强化 人物的宝石镶嵌 进阶 
							if(temp_red_prop || temp_red_stone_chip || needReset || temp_red_exp)
							{
								//镶嵌
								IsCanSetInlay(heroRed, hero);
								if(temp_red_prop || needReset)
								{
									//进阶
									heroRed.SetStoneUp(UIData.isCanStoneUp(hero.idServer));
								}
							}
							
							if(temp_red_equip || temp_red_exp || needReset)
							{
								///穿装备
								heroRed.SetDress(UIData.isHasEquipCanDress(hero.idServer,false,0) == 1);
							}

							///装备强化
							if(temp_red_prop || temp_red_equip || needReset)
							{
								ResetHeroEquip(hero);
							}
						}
						
						///碎片的变化涉及红点的---->> 进化
						if(temp_red_chip || needReset)
						{
							IsCanStarUp(heroRed, hero);
						}
					}
				}

				#endregion

				//装备合成
				if(temp_red_prop)
				{
					ResetEquipCreat();
				}

				//碎片数量的变化
				if(temp_red_chip || temp_new_hero)
				{
					ResetHeroChip();
				}

				DataMode.myPlayer.infoPropList.ResetRed();
				DataMode.infoHeroChip.ResetRed();
			}

			//魂兽
			if(DataMode.myPlayer.infoPropBeastList.needResetRed || _cacheBeast.Count != DataMode.myPlayer.infoBeastList.idServerList.Count)
			{
				List<int> tempList = DataMode.myPlayer.infoPropBeastList.GetChangeData();
				TypeCsvPropEquipBeast csvequip = null;
				TypeCsvProp csvprop = null;

				temp_red_beast_equipUp = true;
				temp_red_beast_hasEquip = true;
				_haveBoxCanOpen = false;
				InfoProp box = null;
				for(int i = 0; i < tempList.Count; i++)
				{
					csvprop = ManagerCsv.getProp(tempList[i]);
//					if(csvprop.type == 11)
//					{
//						temp_red_beast_hasEquip = true;
//					}
//
//				    if(csvprop.type == 12)
//					{
//						temp_red_beast_equipUp = true;
//					}
					 
					if(csvprop.type == 15)
					{
						box = DataMode.myPlayer.infoPropBeastList.getProp(csvprop.id);
						if(box != null && box.cnt > 0)
						{
							_haveBoxCanOpen = true;
						}
					}
				}
				//如果获得了新的魂兽 或者装备更新了 或者装备材料更新了 
				if(_cacheBeast.Count != DataMode.myPlayer.infoBeastList.idServerList.Count ||
				   temp_red_beast_equipUp || temp_red_beast_hasEquip)
				{
					//所有魂兽
					List<InfoBeast> bestList = DataMode.myPlayer.infoBeastList.getBeasts();
					int count = bestList.Count;
					InfoBeastRed beastRed = null;
					for(int j = 0; j < count ; j++)
					{
						//设置
						bool needReset = false;
						//如果没这个 说明是新加的魂兽
						if(!_cacheBeast.ContainsKey(bestList[j].idServer))
						{
							_cacheBeast.Add(bestList[j].idServer , new InfoBeastRed());
							
							needReset = true;
						}
						
						_cacheBeast[bestList[j].idServer].idCsv = bestList[j].idCsv;

						//穿装备
						if(temp_red_beast_hasEquip || needReset)
						{
							_cacheBeast[bestList[j].idServer].SetDress( UIData.isBeastHasEquipCanDress(bestList[j].idServer));
						}

						//升级装备
						if(temp_red_beast_equipUp || needReset)
						{
							List<InfoProp> equipList =	bestList[j].infoEquip.getProps();
							foreach(InfoProp p in equipList)
							{
								csvprop = ManagerCsv.getProp(p.idCsv);
								if(!_cacheBeastEquipLvUp.ContainsKey(csvprop.id))
								{
									_cacheBeastEquipLvUp.Add(csvprop.id, new InfoCreatRed());
								}
								InfoCreatRed info = _cacheBeastEquipLvUp[csvprop.id];
								isCanBeastEquipUp(info, csvprop.id);
							}
						}
					}
				}

				DataMode.myPlayer.infoPropBeastList.ResetRed();
			}
		}
	}
	
	/// 装备合成信息刷新
	private void ResetEquipCreat()
	{
		List<TypeCsvEquipCreat>	_usingList = ManagerCsv.getEquipCreat();
		InfoCreatRed info = null;
		foreach(TypeCsvEquipCreat creat in _usingList)
		{
			temp_csv_prop = ManagerCsv.getProp(creat.idEquip);
			if(temp_csv_prop == null)
			{
				continue;
			}
			temp_csv_consume = ManagerCsv.getConsume(temp_csv_prop.idCom);
			if(temp_csv_consume == null || temp_csv_consume.idProps == null)
			{
				continue;
			}
			int cnt = 0;
			for(int i = 0; i < temp_csv_consume.idProps.GetLength(0); i++)
			{
				InfoProp temp = DataMode.myPlayer.infoPropList.getProp(GMath.toInt(temp_csv_consume.idProps[i][0]));
				if(temp != null)
				{
					if( temp.cnt >= GMath.toInt(temp_csv_consume.idProps[i][1]))
					{
						cnt++;
					}	
				}
			}
			
			bool canCreat = (cnt == temp_csv_consume.idProps.GetLength(0));
			if(!_cacheEquipCreat.ContainsKey(creat.idEquip))
			{
				_cacheEquipCreat.Add(creat.idEquip,new InfoCreatRed());	
			}
			info = _cacheEquipCreat[creat.idEquip];
			info.SetCreat(canCreat, temp_csv_consume.money);
		}
	}
	
	/// 装备强化刷新
	private void ResetHeroEquip(InfoHero hero)
	{
		List<InfoProp> tempProp = hero.infoEquip.getProps();
		TypeCsvProp csv = null;
		TypeCsvProp csvNext = null;
		TypeCsvConsume consume = null;
		InfoProp temp = null;
		foreach(InfoProp prop in tempProp )
		{
			csv = ManagerCsv.getProp(prop.idCsv);
			csvNext = ManagerCsv.getProp(csv.idNext);
			if(!_cacheEquipLvUp.ContainsKey(prop.idCsv))
			{
				_cacheEquipLvUp.Add(prop.idCsv,new InfoCreatRed());
			}
			InfoCreatRed info = _cacheEquipLvUp[prop.idCsv];
			if(csvNext != null)
			{
				int cnt = 0;
				consume = ManagerCsv.getConsume(csvNext.idCom);
				for(int i = 0; i < consume.idProps.GetLength(0); i++)
				{
					//强化的时候会消耗掉本身 
					if(consume.idProps[i][0] != prop.idCsv.ToString())
					{
						temp = DataMode.myPlayer.infoPropList.getProp(GMath.toInt(consume.idProps[i][0]));
						if(temp != null)
						{
							if( temp.cnt >= GMath.toInt(consume.idProps[i][1]))
							{
								cnt++;
							}	
						}
					}
					else
					{
						cnt++;
					}
				}
				bool canCreat = (cnt == consume.idProps.GetLength(0));
				info.SetCreat(canCreat, consume.money);
			}
			else
			{
				info.SetCreat(false,0);
			}
		}
	}
	/// 刷新固定英雄的进阶信息
	public void SetHeroRedStone(ulong idserver)
	{
		if(_cacheHero.ContainsKey(idserver))
		{
			InfoHero hero = DataMode.getHero(idserver);
			InfoHeroRed heroRed = _cacheHero[idserver];
			IsCanSetInlay(heroRed, hero);
			heroRed.SetStoneUp(UIData.isCanStoneUp(hero.idServer));
		}
	}

	private List<int> _signBeastKeys = new List<int>();
	/// 魂兽签到中是否有可以领取的魂兽
	public bool HaveSignBeast()
	{
		if(DataMode.infoSignBeast.isOver())
		{
			return false;
		}
		if(_signBeastKeys.Count == 0)
		{
			List<TypeCsvSignBeast> temp = ManagerCsv.getSignBeast();
			for(int i = 0; i < temp.Count; i++)
			{
				_signBeastKeys.Add(temp[i].day);
			}
		}

		for(int i = 0; i < _signBeastKeys.Count; i++)
		{
			if(DataMode.infoSignBeast.IsSign(_signBeastKeys[i]) &&
			   !DataMode.infoSignBeast.IsReward(_signBeastKeys[i]))
			{
				return true;
			}
		}


		return false;
	}

	//刷新合成英雄信息
	private void ResetHeroChip()
	{
		TypeCsvHeroStar csvherostar = null;
		for(int i = 0; i < _cacheHeroIdSame.Count; i++)
		{
			if(_cacheHeroCreat.ContainsKey(_cacheHeroIdSame[i]))
			{
				_cacheHeroCreat.Remove(_cacheHeroIdSame[i]);
			}
		}
		for(int i = 0; i < DataMode.infoHeroChip.chipHeroKey.Count; i++)
		{
			//取得碎片的same id
			int idsame = DataMode.infoHeroChip.chipHeroKey[i];
			//如果这个卡牌出现在了已经有了的卡牌列表中
			if(_cacheHeroIdSame.IndexOf(idsame) >= 0)
			{
				//移除这个数据
				if(_cacheHeroCreat.ContainsKey(idsame))
				{
					_cacheHeroCreat.Remove(idsame);
				}
			}
			else
			{
				//如果这个碎片还没有合成卡牌 添加数据
				if(!_cacheHeroCreat.ContainsKey(idsame))
				{
					_cacheHeroCreat.Add(idsame,new InfoCreatRed());
				}
				//获取这个数据
				InfoCreatRed info = _cacheHeroCreat[idsame];
				//取表
				csvherostar = ManagerCsv.getHeroStar(idsame);
				//如果有的碎片数量大于等于 合成需要的
				if(DataMode.infoHeroChip.chipHero[idsame] >= csvherostar.piece1)
				{
					//赋值
					info.SetCreat(true, csvherostar.moneyGame1);
				}
				else
				{
					info.SetCreat(false,0);
				}
			}
		}
	}

	//刷新进阶石信息
	private void IsCanSetInlay(InfoHeroRed info, InfoHero hero)
	{
		if(info == null || hero == null)
		{
			return;
		}

		TypeCsvHeroUp _csvHeroUp = ManagerCsv.getHeroUp(hero.idCsv);
		InfoProp prop = null;
		TypeCsvProp csvprop = null;
		TypeCsvConsume csvconsume = null;
		List<long> cost = new List<long>();
		bool can = false;
		for(int i = 0; i < 6; i++)
		{
			//如果这个位置有石头的槽
			if(_csvHeroUp.GetStoneId(i) > 0)
			{
				///如果角色的等级大于这个槽位的等级
				if(hero.lv >= _csvHeroUp.GetLv(i))
				{
					///还没镶嵌过
					if(!hero.infoStone.hasStone(i))
					{
						//查看背包中的这个东西
						prop = DataMode.myPlayer.infoPropList.getProp(GMath.toInt(_csvHeroUp.GetStoneId(i)));
						//有这个物品
						if(prop != null)
						{
							//数量1个就够了 
							if(prop.cnt > 0)
							{
								info.SetInlay(true,0);
								return;
							}
						}
						//没这个东西才走到这
						csvprop = ManagerCsv.getProp(_csvHeroUp.GetStoneId(i));
						//如果表中有这个物品
						if(csvprop != null && csvprop.idCom > 0)
						{
							//看看合成信息
							csvconsume = ManagerCsv.getConsume(csvprop.idCom);
							if(csvconsume != null)
							{
								int csvid = GMath.toInt(csvconsume.idProps[0][0]);
								int count = GMath.toInt(csvconsume.idProps[0][1]);
								prop = DataMode.myPlayer.infoPropList.getProp(csvid);
								if(prop != null)
								{
									if(prop.cnt >= count)
									{
										can = true;
										cost.Add(csvconsume.money);
									}
								}
							}
						}
					}
				}
			}	
		}
		if(!can)
		{
			info.SetInlay(false,0);
		}
		else
		{
			long money = -1;
			for(int i = 0; i < cost.Count; i++)
			{
				if(money < 0 || money > cost[i])
				{
					money = cost[i];
				}
			}
			info.SetInlay(true,money);
		}
	}
	//刷新进化信息
	private void IsCanStarUp(InfoHeroRed info, InfoHero hero)
	{
		if(info == null || hero == null)
		{
			return;
		}

		int  needChipCnt = 0;
		int  costMoney = 0;

		TypeCsvHero csvHero = ManagerCsv.getHero(hero.idCsv);
		TypeCsvHeroStar csvStar = ManagerCsv.getHeroStar(csvHero.idSame);
		
		int starCnt = hero.star + 1;
		switch(starCnt)
		{
		case 2:
			needChipCnt = csvStar.piece2;
			costMoney = csvStar.moneyGame2;
			break;
		case 3:
			needChipCnt = csvStar.piece3;
			costMoney = csvStar.moneyGame3;
			break;
		case 4:
			needChipCnt = csvStar.piece4;
			costMoney = csvStar.moneyGame4;
			break;
		case 5:
			needChipCnt = csvStar.piece5;
			costMoney = csvStar.moneyGame5;
			break;
		case 6:
			needChipCnt = csvStar.piece6;
			costMoney = csvStar.moneyGame6;
			break;
		case 7:
			needChipCnt = csvStar.piece7;
			costMoney = csvStar.moneyGame7;
			break;
		case 8:
			needChipCnt = csvStar.piece8;
			costMoney = csvStar.moneyGame8;
			break;
		case 9:
			needChipCnt = csvStar.piece9;
			costMoney = csvStar.moneyGame9;
			break;	
		case 10:
			needChipCnt = csvStar.piece10;
			costMoney = csvStar.moneyGame10;
			break;		
		default:
			//UtilLog.LogError("现在卡牌只能是最高10星");
			info.SetStarUp(false, 0);
			return;
		}
		
		if((int)DataMode.infoHeroChip.getHeroChipCount(csvHero.idSame) >= needChipCnt)
		{
			info.SetStarUp(true, costMoney);
		}		
		else
		{
			info.SetStarUp(false,0);
		}
	}

	//升级魂兽装备
	private void isCanBeastEquipUp(InfoCreatRed info, int _csvId)
	{
		TypeCsvPropEquipBeast _csvT = ManagerCsv.getPropEquipBeast(_csvId);
		if(_csvT.idNext == -1)
		{
			info.SetCreat(false,0);
		}
		else
		{
			TypeCsvPropEquipBeast _csv =  ManagerCsv.getPropEquipBeast(_csvT.idNext);
			TypeCsvConsume consume = ManagerCsv.getConsume(_csv.idCom);
			InfoProp prop = null;
			int cnt = 0;
			for(int i = 0; i < consume.idProps.GetLength(0); i++)
			{
				int idcsv = GMath.toInt(consume.idProps[i][0]);
				int count = GMath.toInt(consume.idProps[i][1]);
				prop = DataMode.myPlayer.infoPropBeastList.getProp(idcsv);
				if(prop != null && prop.cnt >= count)
				{
					cnt++;
				}

			}
			//东西都够
			if(cnt == consume.idProps.GetLength(0))
			{
				//战队等级不足以升到 此品质
				if(ManagerCsv.getHeroLv(DataMode.myPlayer.lv).BeastGrade < _csvT.grade)
				{
					info.SetCreat(false,0);
				}
				else
				{
					info.SetCreat(true,consume.money);
				}
			}
			else
			{
				info.SetCreat(false,0);
			}
		}
	}
}
