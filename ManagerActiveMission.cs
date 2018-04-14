using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerActiveMission
{
	// 35-WILL DONE 登录次数送大礼
	// 36-WILL DONE 累计儲值 
	// 37-WILL DONE 单笔儲值
	// 38-WILL DONE 限时VIP
	// 39-WILL DONE 战斗力排名送奖励
	// 40-WILL DONE 累计消费送奖励
	// 41-WILL DONE 冲级排名奖励
	// 42-WILL DONE 开服基金
	// 43-WILL DONE 全民福利
	
	/// 是否开启这个类型
	public static bool isHasMission(int sType)
	{
		bool result = (null != DataMode.getActiveMission(sType) && (DataMode.getActiveMission(sType).isBegin || DataMode.getActiveMission(sType).isBeginReward));
		/// 活动开始 是永久活动
		if(result && DataMode.getActiveMission(sType).timeStampEnd <= 0)
		{
			/// 登录次数 
			if(sType == 35)
			{
				bool isHasReward = false;
				for(int index = 0; index < DataMode.getActiveMission(sType).finishedLimit.Length; index++)
				{
					if(DataMode.getActiveMission(sType).finishedLimit[index] <= 0)
						break;
					if(!DataMode.getActiveMission(sType).isLoginLoopReward(index + 1))
					{
						isHasReward = true;
						break;
					}
				}
				result = result && isHasReward;
			}
			// 44- 53 收集白绿篮紫橙卡获得奖励
			if(sType == 44)
			{
				bool isHasReward = false;
				for(int i = 44; i <= 53; i++)
				{
					if(null == DataMode.getActiveMission(i) || !DataMode.getActiveMission(i).isBegin)
						continue;
					isHasReward = isHasReward || !DataMode.getActiveMission(i).isReward2Right(0);
				}
				result = result && isHasReward;
			}
		}

		return result;
	}
	/// 是否有开启的活动
	public static bool isOpenMission
	{
		get
		{
			if(isHasMission(1))
				return true;
			if(isHasMission(2))
				return true;
			if(isHasMission(3))
				return true;

			if(isHasMission(35))
				return true;
			if(isHasMission(36))
				return true;
			if(isHasMission(37))
				return true;
			if(isHasMission(38))
				return true;
			if(isHasMission(39))
				return true;
			if(isHasMission(40))
				return true;
			if(isHasMission(41))
				return true;
			if(isHasMission(42))
				return true;
			if(isHasMission(43))
				return true;
			/// 收集品阶卡牌
			if(isHasMission(44))
				return true;

			for(int i = 0; i < UIActiveMissionPage1.TYPE_MISSION.Length; i++)
			{
				if(isHasMission(UIActiveMissionPage1.TYPE_MISSION[i]))
					return true;
			}
			return false;
		}
	}
	/// 是否 有购买体力活动
	public static bool isHasMissionBuyPower
	{
		get
		{
			return DataMode.infoActiveMissionBuyPower.isHasMission;
		}
	}
	/// 是否 有首充活动
	public static bool isHasMissionPayFirst
	{
		get
		{
			return !DataMode.infoActiveMissionPayFirst.isReward;
		}
	}


	/// 有哪些接口能领取奖励
	public static List<int> hasRewardType
	{
		get
		{
			List<int> result = new List<int>();
			if(null == DataMode.myPlayer)
				return result;
			/// 1七天登陆送大礼
			if(isHasMission(1))
			{
				for(int index = 1; index <= DataMode.getActiveMission(1).day; index++)
				{
					if(DataMode.getActiveMission(1).isLogin(index) && !DataMode.getActiveMission(1).isReward(index))
					{
						result.Add(1);
						break;
					}
				}
			}
			
			/// 2连续登陆送大礼
			if(isHasMission(2))
			{
				for(int index = 1; index <= DataMode.getActiveMission(2).dayLoopMax; index++)
				{
					if(!DataMode.getActiveMission(2).isReward(index))
					{
						result.Add(2);
						break;
					}
				}
			}
			
			/// 3冲级送好礼
			if(isHasMission(3))
			{
				for(int index = 0; index < DataMode.getActiveMission(3).finishedLimit.Length; index++)
				{
					if(DataMode.getActiveMission(3).finishedLimit[index] <= 0)
						continue;
					if(DataMode.getActiveMission(3).finishedLimit[index] > DataMode.myPlayer.lv)
						break;
					if(!DataMode.getActiveMission(3).isReward(index + 1))
					{
						result.Add(3);
						break;
					}
				}
			}
			List<int> _rewardData = null;

			// 35-登录次数送大礼
			if(isHasMission(35))
			{
				for(int index = 0; index < DataMode.getActiveMission(35).finishedLimit.Length; index++)
				{
					if(DataMode.getActiveMission(35).finishedLimit[index] <= 0)
						break;
					if(DataMode.getActiveMission(35).isLoginLoop(index + 1) && !DataMode.getActiveMission(35).isLoginLoopReward(index + 1))
					{
						result.Add(35);
						break;
					}
				}
			}

			// 36-累计儲值 
			if(isHasMission(36))
			{
				_rewardData = DataMode.getActiveMission(36).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(36).isReward2Right(index) && DataMode.getActiveMission(36).isFinished2Right(index))
					{
						result.Add(36);
						break;
					}
				}
			}

			// 37-单笔儲值
			if(isHasMission(37))
			{
				_rewardData = DataMode.getActiveMission(37).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(37).isReward2Right(index) && DataMode.getActiveMission(37).isFinished2Right(index))
					{
						result.Add(37);
						break;
					}
				}
			}
			// 38-限时VIP
			if(isHasMission(38))
			{
				_rewardData = DataMode.getActiveMission(38).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(38).isReward2Right(index) && DataMode.getActiveMission(38).isFinished2Right(index))
					{
						result.Add(38);
						break;
					}
				}
			}
			// 39-WILL DONE 战斗力排名送奖励
			if(Data.serverTime >  DataMode.getActiveMission(39).timeStampEnd && Data.serverTime <= DataMode.getActiveMission(39).getLimitID4(0))
			{
				_rewardData = DataMode.getActiveMission(39).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(39).isReward2Right(index) && DataMode.getActiveMission(39).isFinished2Right(index))
					{
						result.Add(39);
						break;
					}
				}
			}
			// 40-累计消费送奖励
			if(isHasMission(40))
			{
				_rewardData = DataMode.getActiveMission(40).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(40).isReward2Right(index) && DataMode.getActiveMission(40).isFinished2Right(index))
					{
						result.Add(40);
						break;
					}
				}
			}
			// 41-WILL DONE 冲级排名奖励
			if(Data.serverTime >  DataMode.getActiveMission(41).timeStampEnd && Data.serverTime <= DataMode.getActiveMission(41).getLimitID4(0))
			{
				_rewardData = DataMode.getActiveMission(41).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(41).isReward2Right(index) && DataMode.getActiveMission(41).isFinished2Right(index))
					{
						result.Add(41);
						break;
					}
				}
			}
			// 42-开服基金
			if(isHasMission(42))
			{
				_rewardData = DataMode.getActiveMission(42).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(42).isReward2Right(index) && DataMode.getActiveMission(42).isFinished2Right(index))
					{
						result.Add(42);
						break;
					}
				}
			}
			// 43-全民福利
			if(isHasMission(43))
			{
				_rewardData = DataMode.getActiveMission(43).reward.getAllRewardKeyBatch();
				for(int index = 0; index < _rewardData.Count; index++)
				{
					if(!DataMode.getActiveMission(43).isReward2Right(index) && DataMode.getActiveMission(43).isFinished2Right(index))
					{
						result.Add(43);
						break;
					}
				}
			}
			// 43-全民福利
			for(int type = 44; type <= 53; type++)
			{
				if(isHasMission(type))
				{
					if(!DataMode.getActiveMission(type).isReward2Right(0) && DataMode.getActiveMission(type).isFinished2Right(0))
					{
						result.Add(type);
						//change by yxh 这个地方要是break了 我还咋往下做......
						//break;
					}
				}
			}




			/// 其他情况
			for(int index = 0; index < UIActiveMissionPage1.TYPE_MISSION.Length; index++)
			{
				if(!isHasMission(UIActiveMissionPage1.TYPE_MISSION[index]))
					continue;
				for(int i = 1; i <= DataMode.getActiveMission(UIActiveMissionPage1.TYPE_MISSION[index]).day; i++)
				{
					if(1 == UIActiveMissionPage1.TYPE_MISSION[index])
						continue;
					if(2 == UIActiveMissionPage1.TYPE_MISSION[index])
						continue;
					if(3 == UIActiveMissionPage1.TYPE_MISSION[index])
						continue;

					if(!DataMode.getActiveMission(UIActiveMissionPage1.TYPE_MISSION[index]).isFinished2Left(i))
						continue;
					if(DataMode.getActiveMission(UIActiveMissionPage1.TYPE_MISSION[index]).isReward2Left(i))
						continue;
					result.Add(UIActiveMissionPage1.TYPE_MISSION[index]);
					break;
				}

			}
			return result;
		}
	}



	////寻路
	public static bool FindPath(int sType)
	{
		///index需要打开的哪个页面
		WindowsMSG.ReopenActiveMission reopen = new WindowsMSG.ReopenActiveMission();
		int type = sType;
		if(sType>=14&&sType<=21)
		{
			//攻打指定的精英副本
		
			if(isHasMission(type))
			{
				InfoActiveMission mission =  DataMode.getActiveMission(type);
				int fbId = mission.getLimitID4(0);
				bool ishasFb = false;
				InfoFB fbInfo = DataMode.myPlayer.infoFBListElit.getFBInfoByCsv(fbId);
				if(fbInfo !=null)
				{
					WindowsID id = WindowsID.GUS_ACTIVMENU;
					if(WindowBase.reopenWindowList == null)
					{
						WindowBase.reopenWindowList = new Hashtable();
					}
					if(WindowBase.reopenWindowList.ContainsKey(id))
					{
						WindowBase.reopenWindowList.Remove(id);
						WindowBase.reopenListKey.Remove(id);
					}
					
					WindowBase.reopenWindowList.Add(id,reopen);
					WindowBase.reopenListKey.Add(id);
					
					WindowsMngr.getInstance().showFBWindowSpecify(fbId);//.showLastFBWindow(Config.EFBType.EElit);
					return true;
				}
				else
				{
					if(DataMode.myPlayer.infoFBListElit.getMaxComFBId() > 0)
					{
						WindowsID id = WindowsID.GUS_ACTIVMENU;
						if(WindowBase.reopenWindowList == null)
						{
							WindowBase.reopenWindowList = new Hashtable();
						}
						if(WindowBase.reopenWindowList.ContainsKey(id))
						{
							WindowBase.reopenWindowList.Remove(id);
							WindowBase.reopenListKey.Remove(id);
						}
						
						WindowBase.reopenWindowList.Add(id,reopen);
						WindowBase.reopenListKey.Add(id);
						WindowsMngr.getInstance().showLastFBWindow(Config.EFBType.EElit);
						return true;
					}
					else
					{
						//PROP_INFO_NOT_OPEN
						WindowsMngr.getInstance().showTextInfo(ConfigComment.PROP_INFO_NOT_OPEN);
						return false;
					}
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}

		}
		else if(sType>=22&&sType<=29)
		{
			sType = 8;
		}
		else if(sType>=30&&sType<=34)
		{
			sType = 10;
		}

		switch(sType)
		{
		case 1:
		{
			//7天登录送大礼
			if(isHasMission(type))
			{
				SuperUI.getUI<UIActiveMissionMenu>().setMenu(2);
				return true;
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}
			return false;
		}
		case 2:
		{
			//连续登录送大礼
			if(isHasMission(type))
			{
				SuperUI.getUI<UIActiveMissionMenu>().setMenu(4);
				return true;
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}
			return false;
		}
		case 3:
		{
			//冲级送好礼
			if(isHasMission(type))
			{
				SuperUI.getUI<UIActiveMissionMenu>().setMenu(5);
				return true;
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}
			return false;
		}
		case 4:
		{
			//召唤达人
			/*
			if(isHasMission(4))
			{
				WindowsMngr.getInstance().openWindow(WindowsID.SHOP , reopen);
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}*/
			break;
		}
		case 5:
		{
			//世界boss等你挑战
			/*
			if(isHasMission(5))
			{
				if(DataMode.hasSystem(InfoSystem.WORLD_BOSS))
				{
					WindowsMngr.getInstance().showWorldBossList(reopen);
				}
				else
				{
					WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_SYSTEM);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}*/
			break;
		}
		case 6:
		{
			//金币大放送，完成2次金娃沼泽    改了
		/*	if(isHasMission(6))
			{
				if(DataMode.hasSystem(InfoSystem.FB_ACTIVE_GAMEMOENY))
				{
					WindowsMngr.getInstance().showActivList(reopen);
				}
				else
				{
					WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_SYSTEM);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}*/
			break;
		}
		case 7:
		{
			//完成2次指定的精英本

			if(isHasMission(type))
			{
				if(DataMode.myPlayer.infoFBListElit.getMaxComFBId() > 0)
				{
					WindowsID id = WindowsID.GUS_ACTIVMENU;
					if(WindowBase.reopenWindowList == null)
					{
						WindowBase.reopenWindowList = new Hashtable();
					}
					if(WindowBase.reopenWindowList.ContainsKey(id))
					{
						WindowBase.reopenWindowList.Remove(id);
						WindowBase.reopenListKey.Remove(id);
					}
					
					WindowBase.reopenWindowList.Add(id,reopen);
					WindowBase.reopenListKey.Add(id);

					WindowsMngr.getInstance().showLastFBWindow(Config.EFBType.EElit);
					return true;
				}
				else
				{
					//PROP_INFO_NOT_OPEN
					WindowsMngr.getInstance().showTextInfo(ConfigComment.PROP_INFO_NOT_OPEN);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}
			return false;
		}
		case 8:
		{
			//通关n次太阳井
			if(isHasMission(type))
			{
				if(DataMode.hasSystem(InfoSystem.SUN_MINE))
				{
					reopen.param = 1;
					WindowsMngr.getInstance().openWindow(WindowsID.SUNWELLFB, reopen);
				}
				else
				{
					WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_SYSTEM);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}

			return true;
		}
		case 9:
		{
			//每天消耗n点面包  商队
			if(isHasMission(type))
			{
				if(DataMode.hasSystem(InfoSystem.CARDTEAM))
				{
					WindowsMngr.getInstance().closeAllUI();
					WindowsMngr.getInstance().openWindow(WindowsID.CARAVAN_MAP, reopen);
				}                                                                                                                                                                                                                                                                                                                                                                                                                                                          
				else
				{
					WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_SYSTEM);
					return false;
				}

			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}


			return true;
		}
		case 10:
		{
			//击败任意一个海山奖励关boss
			if(isHasMission(type))
			{
				if(DataMode.hasSystem(InfoSystem.TBC))
				{
				//	WindowsMngr.getInstance(). showActiv(reopen);
					WindowsMngr.getInstance().ShowExpedition(reopen);
				}
				else
				{
					WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_SYSTEM);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}

			return true;
		}
		case 11:
		{
			//海山防守奖励

			if(isHasMission(11))
			{
				if(DataMode.hasSystem(InfoSystem.TBC))
				{
				//	WindowsMngr.getInstance(). showActiv(reopen);
					WindowsMngr.getInstance().ShowExpedition(reopen);
				}
				else
				{
					WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_SYSTEM);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}

			break;
		}
		case 12:
		{
			//通关n次挑战副本
			if(isHasMission(type))
			{
				if(DataMode.myPlayer.infoFBListChallenge.getMaxComFBId() > 0)
				{
					WindowsID id = WindowsID.GUS_ACTIVMENU;
					if(WindowBase.reopenWindowList == null)
					{
						WindowBase.reopenWindowList = new Hashtable();
					}
					if(WindowBase.reopenWindowList.ContainsKey(id))
					{
						WindowBase.reopenWindowList.Remove(id);
						WindowBase.reopenListKey.Remove(id);
					}
					
					WindowBase.reopenWindowList.Add(id,reopen);
					WindowBase.reopenListKey.Add(id);
					WindowsMngr.getInstance().showLastFBWindow(Config.EFBType.EChallenge);
					//public void showFBWindowSpecify(int fb_csv_id)
				}
				else
				{
					//PROP_INFO_NOT_OPEN
					WindowsMngr.getInstance().showTextInfo(ConfigComment.PROP_INFO_NOT_OPEN);
					return false;
				}
			}
			else
			{
				WindowsMngr.getInstance().showTextInfo(ConfigComment.HAS_NO_ACTIVITY);
				return false;
			}

			return true;
		}
		}
		return false;
	}


}
