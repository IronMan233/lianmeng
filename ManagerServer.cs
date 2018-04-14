using UnityEngine;
using System.Collections;
using net;
using net.unity3d;
using System.Collections.Generic;
using net.unity3d;
using System.Net.Sockets;

/// 连接服务器用的类
/// 
/// 设备标示符 还有个方法是用KeyChain来保存 
/// 不过可以肯定的是 最好不要去动  Bundle Identifier
/// 另外 账号和密码是保存在本地的 用unity 自带的 玩家偏好 函数  安全性为止


public class ManagerServer 
{
	//单例
	private static ManagerServer _server;
	private ManagerServer(){}
	public static ManagerServer getInstance()
	{
		if(_server == null)
		{
			_server = new ManagerServer();
		}
		return _server;
	}

	public List<ServerPlayerInfo> serverPlayerInfoList = new List<ServerPlayerInfo>();
	public int lastLoginServerIndex = 0;

	private readonly string macIdServerStr = "net.mac_id_server";
	
	/// load数据挖掘 是否是新账号
	/// 为true说明需要创建账号
	public static bool isLoadStatueNew = false;
	public static bool isLoadStatueBefor = false;
	/// 是否开始发送统计数据
	public static bool isLoadStatue = false;
		
	/// 是不是已经绑定过的账号
	public bool isBinded = false;
#region server
	/// 这个是服务器登陆account之后返回的一个id用来连login
	private string _accountID = "";
	/// 频道ID
	private string _channelID = "";
	/// 账号
	public string _account = "";
	/// 密码
	public string _password = "";
	
	public string _sessionID = "";
	/// 设备标示位
	private string _macid = "";
	//选择的服务器id
	private int _ServerIndex = 0; 

	private WindowsMSG.Login msg;

	public enum LOGIN_STATE
	{
		//未登录
		UN_LOGIN,
		//account登录完了
		ACCOUNT_LOGINED,
	}

	public LOGIN_STATE login_state = LOGIN_STATE.UN_LOGIN;

	/// 跨日登陆踢号用的
	public double loginTime = 0;
	/// 选择服务器的id
	public int ServerIndex
	{
		get
		{
			return _ServerIndex;
		}
	}
	/// add by gus 返回服务器名字
	public string ServerName
	{
		get
		{
			List<TypeCsvClientConfig> csvtemp = ManagerCsv.getClientConfig();
			if(null == csvtemp)
				return "";
			for(int index = 0; index < csvtemp.Count; index++)
			{
				if(csvtemp[index].server_id == _ServerIndex)
				{
					return csvtemp[index].server_name;
				}
			}
			return "";
		}
	}

	public string ServerNumber
	{
		get
		{
			List<TypeCsvClientConfig> csvtemp = ManagerCsv.getClientConfig();
			if(null == csvtemp)
				return "";
			for(int index = 0; index < csvtemp.Count; index++)
			{
				if(csvtemp[index].server_id == _ServerIndex)
				{
					return csvtemp[index].id.ToString();
				}
			}
			return "";
		}
	}
	
	public void setServerIndx(int index)
	{
//		PlayerPrefs.SetInt(key,index);
//		PlayerPrefs.Save();
		_ServerIndex = index;
	}
	
	/// account服务器返回的id
	public string AccountId
	{
		set
		{
			_accountID = value;
		}
		get
		{
			return _accountID;
		}
	}

	/// SDK返回的渠道ID
	public string ChannelId
	{
		set
		{
			_channelID = value;
		}
		get
		{
			return _channelID;
		}
	}
	
	/// 设备标识id
	public string MacId
	{
		get
		{
			return _macid;
		}
	}
	
//	private List<int> usedServerId = new List<int>();
	
	#endregion
	public void iniServer(Function complete,bool isOutLine)
	{
		List<NoteServer> temp = new List<NoteServer>();
		NoteServer note;
		///设置服务器第二步 读取本地的account 
		#region account config
		note = new NoteServer();
		TypeCsvAccountConfig csvacc = ManagerCsv.getAccountConfig(0); 
		//如果服务器列表没有下载下来 就初始化 退出游戏
		if(csvacc == null)
		{
			DialogMngr.getInstance().showSubmitDialog(ConfigLabel.LOADING_FILE_ERROR, callBackAlert);
			return;
		}
		//如果是外网 并且是本地的类型 才初始化外网服务器
		
		note._serverId = csvacc.server_id;
		note._serverIp = csvacc.getIp();
		note._serverProt = (short)csvacc.server_listen_port;
		note._timeOut = csvacc.server_connect_timeout;
		temp.Add(note);
		//设置
		net.unity3d.AgentNet.getInstance().initAccountNote(temp);
		if(UtilLog.isBulidLog)UtilLog.Log("Ini Account Server ip : " + note._serverIp + " prot : " + note._serverProt);
		#endregion
		
		
		temp = new List<NoteServer>();
		
		
	 	#region client config
		List<TypeCsvClientConfig> csvtemp = ManagerCsv.getClientConfig();
		if(csvtemp == null || csvtemp.Count == 0)
		{
			DialogMngr.getInstance().showSubmitDialog(ConfigLabel.LOADING_FILE_ERROR, callBackAlert);
			return;
		}
		#if SDK_92_APP_STORE && UNITY_IOS
		UConnectionAddressFamily.UseCustomAddress = true;
		UConnectionAddressFamily.FindAddressFunc = GetAddress;
		UConnectionAddressFamily.ClearData();
		#endif

		foreach(TypeCsvClientConfig config in csvtemp)
		{
			note = new NoteServer();
			note._serverId = config.server_id;
#if SDK_92_APP_STORE && UNITY_IOS
			string ip = config.getIp();
			AddressFamily address = AddressFamily.InterNetwork;
			string ipNew = string.Empty;
			AddressFamily addressNew = AddressFamily.InterNetwork; 
			SdkConector92AppStore.GetIPType(ip, config.server_listen_port.ToString(), out ipNew, out addressNew);
			note._serverIp = ipNew;
			UConnectionAddressFamily.AddData(ipNew, config.server_listen_port.ToString(), addressNew);
#else
			note._serverIp = config.getIp();
#endif
			note._serverProt = (short)config.server_listen_port;
			note._timeOut = config.server_connect_timeout;
			temp.Add(note);
		}
		//设置
		net.unity3d.AgentNet.getInstance().initClientNoetNew(temp);
		#endregion
		
		//初始化 所有接受协议侦听
		DataModeRecv.initListener();
		if(complete != null)
		{
			complete();
		}
	}

	public AddressFamily GetAddress(string ip, string port)
	{
#if SDK_92_APP_STORE && UNITY_IOS
		string ipNew = string.Empty;
		AddressFamily temp = AddressFamily.InterNetwork;
		SdkConector92AppStore.GetIPType(ip, port, out ipNew, out temp);
		return temp;
#endif
		return AddressFamily.InterNetwork;
	}

	//服务器初始化错误处理函数
	private void callBackAlert(bool isAct)
	{
		Application.Quit();
	}
	

	/// 连接account
	public void contactAccount()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("in func contactAccount");
		_macid = "";
		//如果macid 为空 说明本地客户端没有记录过临时的账号 在连接account服务器的时候 recv account 不会返回
		//而是会返回AC2C_MAC_ID 协议，这个是服务器自动分配的id 然后这个id会被保存在本地 用于以后按照步骤登录使用
		if(PlayerPrefs.HasKey(macIdServerStr))
		{
			_macid = PlayerPrefs.GetString(macIdServerStr);
		}
		if(UtilLog.isBulidLog)UtilLog.Log("(1)连接Account >> MacId = " + _macid);
//		_macid = "00100391111";
		login_state = LOGIN_STATE.UN_LOGIN;
		if(UtilLog.isBulidLog)UtilLog.Log("(2)account = " + _account);
		if(UtilLog.isBulidLog)UtilLog.Log("(3)Config.VERIFY_TYPE = " + Config.VERIFY_TYPE.ToString());
		if(UtilLog.isBulidLog)UtilLog.Log("(4)sessionID = " + _sessionID);
		if(UtilLog.isBulidLog)UtilLog.Log("(5)channelID = " + _channelID);
		if(UtilLog.isBulidLog)UtilLog.Log("(6)accountID = " + _accountID);
		
		AgentNet.getInstance().close();
		///设置登陆信息
        AgentNet.getInstance().setLoginInfo(_account, "1.0.2.0", _macid, (byte)Config.VERIFY_TYPE, _sessionID, _channelID, _accountID);
		
		AgentNet.getInstance().bingAccountSer();
		if(UtilLog.isBulidLog)UtilLog.Log("连接Account执行完成");
	}
	
	public void recvAccount(int iResult)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("Recv account result = " + iResult);

		if(iResult == 1)
		{
			//account登入完毕
			login_state = LOGIN_STATE.ACCOUNT_LOGINED;
		}

		//如果是无SDK的情况
		if(Config.VERIFY_TYPE == Config.EVERIFY_TYPE.EV_LOCAL)
		{
			if(!LoginWindow._isTempLogin)
			{
				LoginWindow._isTempLogin = true;
				msg = new WindowsMSG.Login();
				msg.result = iResult != 1 ? false : true;
				WindowsMngr.getInstance().showLoginWindow(msg);
				DataModeServer3.sendRecentServerInfo(_account, _password, _macid, (byte)Config.VERIFY_TYPE, _sessionID, _channelID, _accountID, IniPlayerInfo);
			}
			else
			{
				connecteLogin();
			}
		}
		else
		{
			//通知SDK 登入状态
			Document.valueObj.notifySDKLoginState(iResult);
			if(iResult == 1)
			{
				DataModeServer3.sendRecentServerInfo(_account, _password, _macid, (byte)Config.VERIFY_TYPE, _sessionID, _channelID, _accountID, IniPlayerInfo);
			}
			else
			{
				ErrorAccount(iResult);
			}
		}
	}

	//
	public void IniPlayerInfo(UtilListenerEvent sData)
	{

		AC2C_RECENT_SERVER_INFO recv = (AC2C_RECENT_SERVER_INFO)sData.eventArgs;
		if(UtilLog.isBulidLog)UtilLog.Log("AC2C_RECENT_SERVER_INFO = " + recv.vctRecentInfo.Length);
		if(recv.cIsBegin == 1)
		{
			serverPlayerInfoList.Clear();
		}

		for(int i = 0; i < recv.vctRecentInfo.Length; i++)
		{
			InfoPlayer player = new InfoPlayer();
			player.exp = (ulong)recv.vctRecentInfo[i].m_luiExp;
			player.vipExp = (ulong)recv.vctRecentInfo[i].m_uiVipExp;
			player.name = recv.vctRecentInfo[i].GetName();
			ServerPlayerInfo info = new ServerPlayerInfo();
			info.csv = (int)recv.vctRecentInfo[i].m_uiIdCsvPet;
			info.player = player;
			info.player.idServer = recv.vctRecentInfo[i].m_uiMasterID;
			info.serverIndex = (int)recv.vctRecentInfo[i].m_uiServerID;

			if(info.csv > 0)
			{
				serverPlayerInfoList.Add(info);
			}
		}

		if(recv.cIsOver == 1 && LoginWindow.view != null)
		{
			if(lastLoginServerIndex > 0)
			{
				setServerIndx(lastLoginServerIndex);
			}
			LoginWindow.view.Reset();
		}
	}

	/// 选择并登陆对应的服务器
	public void connecteLogin()
	{
		if(UtilLog.isBulidLog)UtilLog.Log("connecteLogin server id = " + _ServerIndex.ToString());
		if(UtilLog.isBulidLog)UtilLog.Log("_channelID = " + _channelID);
		if(UtilLog.isBulidLog)UtilLog.Log("AccountId " + AccountId);
		AgentNet.getInstance().getRelamInfoByAreaId(_channelID, AccountId,  _ServerIndex);
	}
	
	/// 登陆realm
	public void connecteRealm()
	{
		AgentNet.getInstance().openLogicServer(_channelID, AccountId, _account);
	}
	
	/// 掉线重连用的函数
	public void reConnecte()
	{
		AgentNet.getInstance().logic_reopen(_channelID,_accountID,_macid);
	}
	
	
	public void ErrorAccount(int iResult)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("connecte account server faild, result = " + iResult.ToString());
		ManagerServer.getInstance()._account = "";
		ManagerServer.getInstance()._password = "";
	}


	/// 保存最近登入信息
	//服务器返回的最近登录信息 返回的是ServerId 并不是表中的id
	public void SaveAccount(object obj)
	{
//		AC2C_ACCOUNT_INFO recv = (AC2C_ACCOUNT_INFO)obj;
//		if(recv.iResult == 1)
//		{
//			int serverid = 0;
//			usedServerId.Clear();
//			for(int i = 0; i < recv.sAccountAC2C.uiServer.Length; i++)
//			{
//				serverid = (int)recv.sAccountAC2C.uiServer[i];
//				if(serverid != 0)
//				{
//					usedServerId.Add((int)recv.sAccountAC2C.uiServer[i]);
//				}
//			}
//		}
	}
	
	public void SaveMacIdServer(object obj)
	{
		AC2C_MAC_ID recv = (AC2C_MAC_ID)obj;
		string tempMacIdServer = System.Text.Encoding.ASCII.GetString(recv.cDeviceInfo);
		Debug.Log("account mac id = " + tempMacIdServer);
		if(tempMacIdServer.Length == 0)
		{
			Debug.LogError("错误的mac id");
			return;
		}

		PlayerPrefs.SetString(macIdServerStr, tempMacIdServer);
		PlayerPrefs.Save();
		_macid = PlayerPrefs.GetString(macIdServerStr);
		AgentNet.getInstance().setMacId(_macid);
		AgentNet.getInstance().sendAccountLogin();
	}

	public void Reset()
	{
		serverPlayerInfoList.Clear();
		lastLoginServerIndex = 0;
		isLoadStatueNew = false;
		isLoadStatueBefor = false;
		/// 是否开始发送统计数据
		isLoadStatue = false;

		_accountID = "";
		_channelID = "";
		_account = "";
		_password = "";
		_sessionID = "";
		 _macid = "";
//		_ServerIndex = 0; 
		login_state = ManagerServer.LOGIN_STATE.UN_LOGIN;
	}






	//禁言时间 如果时间为0的话 则永久不能发言
	public double lockWordTime = 0;
	//是否禁言 true为禁言
	public bool lockWord = false;
	//禁言原因
	public string lockWordReason = "";

	//禁言时间 如果时间为0的话 则永久不能发言
	public double lockLoginTime = 0;
	//是否禁言 true为禁言
	public bool lockLogin = false;
	//禁言原因
	public string lockLoginReason = "";

	public enum LOCK_STATE_TYPE
	{
		WORD,
		LOGIN
	}

	public void SetLockWordState(double time,bool locked, string reason)
	{
		lockWordTime = time;
		lockWord = locked;
		lockWordReason = reason;
	}

	public void SetLockLoginState(double time,bool locked, string reason)
	{
		lockLoginTime = time;
		lockLogin = locked;
		lockLoginReason = reason;
	}

	public bool CheckStateByType(LOCK_STATE_TYPE type)
	{
		if(type == LOCK_STATE_TYPE.WORD)
		{
			if(lockWord)
			{
				if(lockWordTime == 0)
				{
					ShowLockWarning();
					return false;
				}
				else
				{
					if(Data.serverTime > lockWordTime)
					{
						return true;
					}
					else
					{
						ShowLockWarning();
						return false;
					}
				}
			}
			else
			{
				return true;
			}
		}
		else
		{
			if(lockLogin)
			{
				if(lockLoginTime == 0)
				{
					ShowLockLogin();
					return false;
				}
				else
				{
					if(Data.serverTime > lockLoginTime)
					{
						return true;
					}
					else
					{
						ShowLockLogin();
						return false;
					}
				}
			}
			else
			{
				return true;
			}
		}
	}

	//禁言提示
	private void ShowLockWarning()
	{
		UIUtilPopup.CContentText text1 = new UIUtilPopup.CContentText ();
		text1.pivot = UIUtilPopup.EPivot.EMid;

//		
//		else 
		if(lockWordTime == 0)
		{
			text1.text = ConfigComment.parseText(ConfigComment.TICK_LOCK_WORD);
		}
		else
		{
			int time = (int)(lockWordTime - Data.serverTime);
			int day = time/(24*60*60);
			int hour = (time-day*24*60*60)/(60*60);
			int minute = (time-hour*60*60-day*24*60*60)/60;
			
			System.Text.StringBuilder builder1 = new System.Text.StringBuilder();
            builder1.Append(ConfigLabel.TICK_LOCK_WORD);
			builder1.Append(day);
			builder1.Append(ConfigLabel.STR_DAY);
			builder1.Append(hour);
			builder1.Append(ConfigLabel.STR_HOUR);
			builder1.Append(minute);
			builder1.Append(ConfigLabel.STR_MINUTE);
            builder1.Append(ConfigLabel.TICK_LOCK_WORD_END);
			
			text1.text = builder1.ToString();
		}

		if(!string.IsNullOrEmpty(lockWordReason))
		{
			text1.text += lockWordReason;
		}

		UIUtilPopup.CBut but = new UIUtilPopup.CBut ();
		but.butName = ConfigLabel.CHAT_INPUT_OK;
		
		List<UIUtilPopup.CBut> buts = new List<UIUtilPopup.CBut>();
		buts.Add(but);
		
		WindowsMngr.getInstance().showPopupNormalText(ConfigLabel.POPUP_TITLE_TISHI, text1, buts, false);
	}


	private void ShowLockLogin()
	{
		Document.valueObj.sendping = false;
		WindowsMngr.getInstance().showLoginWindow();
		
		UIUtilPopup.CContentText text = new UIUtilPopup.CContentText ();
		text.pivot = UIUtilPopup.EPivot.EMid;

		if(lockLoginTime == 0)
		{
			text.text = ConfigComment.parseText(ConfigComment.TICK_BY_ADMIN_CLOSE);
		}
		else
		{
			int time = (int)(lockLoginTime - Data.serverTime);
			int day = time/(24*60*60);
			int hour = (time-day*24*60*60)/(60*60);
			int minute = (time-hour*60*60-day*24*60*60)/60;
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			builder.Append(ConfigLabel.TICK_LOCK_LOGIN);
			builder.Append(day);
			builder.Append(ConfigLabel.STR_DAY);
			builder.Append(hour);
			builder.Append(ConfigLabel.STR_HOUR);
			builder.Append(minute);
			builder.Append(ConfigLabel.STR_MINUTE);
			builder.Append(ConfigLabel.TICK_LOCK_LOGIN_END);
			
			text.text = builder.ToString();
		}

		if(!string.IsNullOrEmpty(lockLoginReason))
		{
			text.text += lockLoginReason;
		}

		UIUtilPopup.CBut but = new UIUtilPopup.CBut ();
		but.butName = ConfigLabel.CHAT_INPUT_OK;
		
		List<UIUtilPopup.CBut> buts = new List<UIUtilPopup.CBut>();
		buts.Add(but);
		
		WindowsMngr.getInstance().showPopupNormalText(ConfigLabel.POPUP_TITLE_TISHI, text, buts, false);
	}
}
