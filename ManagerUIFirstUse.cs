using UnityEngine;
using System.Collections;

public class ManagerUIFirstUse
{
	/// 打开ui 是不是第一次使用
	public static bool isFirstUse(InfoSystem idSystem)
	{
		/// 如果是空的 责不是第一次使用
		if(null == ManagerCsv.getSystemFirstUse((int)idSystem))
			return false;
		/// 没有系统
		if(!DataMode.hasSystem(idSystem))
			return false;
		/// 不是第一次使用
		if(!DataMode.infoSetting.isSystemFirstUse(ManagerCsv.getSystemFirstUse((int)idSystem).id))
			return false;
		
		/// 如果是好友
		if(idSystem == InfoSystem.FRIEND)	
			if(DataMode.myPlayer.infoFriendList.getFriends().Count <= 0)
				return false;
		
			
		/// 如果不是第一次
		return true;
	}
	/// 打开ui 看是不是第一次使用
	public static void openUIFirstUse(InfoSystem idSystem)
	{
//		/// 如果不用第一次引导,那么滚蛋
//		if(!isFirstUse(idSystem))
//			return;
//		/// 如果不用
//		SuperUI.showNew<UISystemFirstUse>("_GusUISystemFirstUse").setSystemFirstUse((int)idSystem);
//		/// 设置成假值
//		DataMode.infoSetting.setSystemFirstUse(ManagerCsv.getSystemFirstUse((int)idSystem).id, false);
//		
//		/// 保存至服务器吧
//		DataModeServer.sendSaveSystemUIFirstUse();
	}
}













