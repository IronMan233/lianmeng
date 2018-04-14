using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using System.IO;

public class ManagerVersion 
{
	/// del
	public delegate void delMangerVersion();

	/// <summary>
	/// version file name
	/// </summary>
	public static readonly string VER_FILE = "version_list.txt";

	public static readonly string VER_FILE_LOCAL = "version.txt";

	/// <summary>
	/// versino res file name
	/// </summary>
	public static readonly string VER_RES_FILE = "version_#.csv";

	private static List<string> _sResFiles = new List<string> ();

	private static Dictionary<string/*file url*/, string/*md5*/> _sResUrls = new Dictionary<string, string>();

	private static string _sVerLatest  = "";

	private static delMangerVersion _sCallBack = null;

	private static float _updateSize = 0f;





	/// <summary>
	/// load version file
	/// </summary>
	public static void beginVersionUpdate(delMangerVersion call_back)
	{
		if(UtilLog.isBulidLog)UtilLog.Log( "begin version update");

		List<string> url_list = new List<string> ();
		url_list.Add(ConfigUrl.ROOT_UPDATE_WEB_VERSION + VER_FILE);
		url_list.Add(ConfigUrl.ROOT_UPDATE_LOCAL_READ + VER_FILE_LOCAL);	
		_sCallBack = call_back;


#if UNITY_EDITOR 
		//LoadMngr.getInstance().load(url_list.ToArray(), comVersionFile);
//		LoginWindow.ShowLoadingVersion();
		callBack();
		
#elif UNITY_IPHONE || UNITY_ANDROID 
		if(LoadMngr.LOAD_RES_LOCAL)
		{
			LoadMngr.getInstance().load(url_list.ToArray(), comVersionFile);
		}
		else
		{
			callBack();
		}
#else 
		callBack();
#endif

	}

	/// <summary>
	/// complete load version file
	/// judge if need download version res files
	/// </summary>
	public static void comVersionFile(double loader_id)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("load version_list file over!!");

		// compare with local veision
		string url = ConfigUrl.ROOT_UPDATE_WEB_VERSION + VER_FILE;
		string text = LoadMngr.getInstance().GetText(url).Trim();

		// vesion file is empty or not exist
		if(text == null || text == "")
		{
			if(UtilLog.isBulidLog)UtilLog.Log("version  list file is empty or null!!");
			_sVerLatest = "";
			comVersionReses();
			return;
		}


		

		// get local ver file for compare
		string url_local = ConfigUrl.ROOT_UPDATE_LOCAL_READ + VER_FILE_LOCAL;
		string ver_latest = "";

		string text_local = LoadMngr.getInstance().GetText(url_local);
		if(text == null || text == "")
		{
			// local ver file is empty
			ver_latest = "";
		}
		else
		{
			ver_latest = text_local.Trim();
		}


		string[] vers = text.Split(("|").ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
		_sResFiles.Clear();



		if(vers == null || vers.Length <= 0)
		{
			if(UtilLog.isBulidLog)UtilLog.Log("version list have no vers!!");
			// have no version
			_sVerLatest = "";
			comVersionReses();
			return;
		}
		else
		{
			// have version i should judge
			foreach(string str in vers)
			{
				if(UtilLog.isBulidLog)UtilLog.Log(" version str = " + str);
				// version should write in order
				if(ver_latest != "" && ver_latest != str)
				{
					continue;
				}
				else if(ver_latest != "" && ver_latest == str)
				{
					ver_latest = "";
					continue;
				}

				_sVerLatest = str;

				_sResFiles.Add(ConfigUrl.ROOT_UPDATE_WEB_VERSION +  VER_RES_FILE.Replace("#", str));
			}
		}

		 //jugde big version is right big version platefrom can judge
		string big_version = vers[0].Substring(0, vers[0].LastIndexOf("."));
		//if(UtilLog.isBulidLog)UtilLog.Log(utils.Logger.SSYIDS.VERSION, "version = " + big_version);

		float big_version_f = float.Parse(big_version.ToString());

        UtilLog.LogError(big_version_f + "------------" + ConfigUrl.VERSION);
		if(big_version_f.ToString() != ConfigUrl.VERSION)
		{
			// shuold pop up a window!!
			UtilLog.LogError("version is not right!! should down load the latest version from stor!!");

			UIUtilPopup.CContentText tex = new UIUtilPopup.CContentText ();
			tex.pivot = UIUtilPopup.EPivot.EMid;
			tex.text = ConfigLabel.POPUP_CONTENT_VERSION_WRONG;

			List<UIUtilPopup.CBut> buts = new List<UIUtilPopup.CBut> ();

			UIUtilPopup.CBut but_ok = new UIUtilPopup.CBut ();
			but_ok.butName = ConfigLabel.POPUP_BUT_NAME_OK;
			but_ok.callBack = onClickVersionWrong;
			buts.Add(but_ok);



			WindowsMngr.getInstance().showPopupNormalText(ConfigLabel.POPUP_TITLE_VERSION_WRONG, tex, buts, false);
			return;
		}

		if(UtilLog.isBulidLog)UtilLog.Log(" laster version = " + _sVerLatest);


		if(_sResFiles.Count > 0)
		{
			LoadMngr.getInstance().load(_sResFiles.ToArray(), comVersionResFiles);
		}
		else
		{
			comVersionResFiles();
		}

	}

	/// <summary>
	/// COMs the version res file.
	/// load all res files
	/// </summary>
	public static void comVersionResFiles(double loader_id = 0)
	{
		//TODO: how to know version_list or version_files is wrong
		// get all res urls
		_sResUrls.Clear();

		if(UtilLog.isBulidLog)UtilLog.Log(" load version res files over!!");

		// xxKB;
		int all_size = 0; 
		foreach(string str in _sResFiles)
		{

			string text = LoadMngr.getInstance().GetText(str);
			if(text == null || text == "")
			{
				UtilLog.LogError("load version txt file error or empty!!  url = " + str);
				comVersionReses();
				return;
			}

			string[] urls = text.Split(("\n").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if(urls == null || urls.Length <= 0)
			{
				UtilLog.LogError("version txt file !!  url = " + str);

				comVersionReses();
				return;
			}

			string version_str = str.Substring(str.LastIndexOf("/") + 1, str.LastIndexOf(".") - str.LastIndexOf("/") - 1); 

			UtilCsvReader csv_reader = new UtilCsvReader (text);
			int index = 1;
			while(true)
			{
				Dictionary<string, string> search = csv_reader.search("id", index);
				index ++;
				if(null == search)
					break;

				// add file and md5
				string url = search["file"];
				string url_real = url.Replace("\n", "");
				url_real = url_real.Replace("\r","");
				if(!_sResUrls.ContainsKey(ConfigUrl.ROOT_UPDATE_WEB + version_str + "/" + url_real))
				{
					
					_sResUrls.Add(ConfigUrl.ROOT_UPDATE_WEB + version_str + "/" + url_real, search["md5"]);
					if(UtilLog.isBulidLog) UtilLog.Log(" add one url = " + ConfigUrl.ROOT_UPDATE_WEB + version_str + "/" + url_real + " md5 = " + search["md5"]); 				
				}


				// add size
				int size = 0;
				if(int.TryParse(search["size"],out size))
				{
					all_size += size;
				}


			}




		}

		string all_size_real = "";
		if(all_size < 1024)
		{
			all_size_real = all_size + "KB";
		}
		else 
		{
			all_size_real = (all_size/1024).ToString() + "M";
		}

		if(UtilLog.isBulidLog)UtilLog.Log("all size need to load is = " + all_size_real);

		// load
		if(_sResUrls.Count > 0)
		{
			string text = ConfigLabel.POPUP_CONTENT_VERSION_UPDATE + all_size_real;
			// pop up and begin update
//			UIUtilPopup.CContentText text = new UIUtilPopup.CContentText ();
//			text.pivot = UIUtilPopup.EPivot.EMid;
//			text.text = ConfigLabel.POPUP_CONTENT_VERSION_UPDATE + all_size_real;
//			UIUtilPopup.CBut but_cancle = new UIUtilPopup.CBut ();
//			but_cancle.butName = ConfigLabel.POPUP_BUT_NAME_CANCLE;
//			but_cancle.callBack = callBackUpdateCancle;
//
//			UIUtilPopup.CBut but_ok = new UIUtilPopup.CBut ();
//			but_ok.butName = ConfigLabel.POPUP_BUT_NAME_OK;
//			but_ok.callBack = callBackUpdateOK;
//
//			List<UIUtilPopup.CBut> buts = new List<UIUtilPopup.CBut> ();
//			buts.Add(but_cancle);
//			buts.Add(but_ok);

			//WindowsMngr.getInstance().showPopupNormalText(ConfigLabel.POPUP_TITLE_TISHI, text, buts  , false);
			DialogMngr.getInstance().showSelectDialog(ConfigLabel.POPUP_TITLE_TISHI, text, callBackUpdateOK);
		
		}
		else
		{
			_sVerLatest = "";
			comVersionReses();
		}






	}

	public static void callBackUpdateCancle(GameObject go)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("cancle update!");
		Application.Quit();
	}

	public static void callBackUpdateOK(bool sure)
	{
		if(sure)
		{
			LoadMngr.MAX_LOAD_THREAD_FRONT = 1;

			List<string> urls = new List<string> ();
			foreach(string url in _sResUrls.Keys)
			{
				if(!urls.Contains(url))
				{
					urls.Add(url);
				}
			}
			
			LoadMngr.getInstance().load(urls.ToArray(), comVersionReses);
		}
		else
		{
			if(UtilLog.isBulidLog)UtilLog.Log("cancle update!");
			Application.Quit();
		}
	}
	
	/// <summary>
	/// complete load update res
	/// update to document update local version file
	/// update over
	/// </summary>
	public static void comVersionReses(double loader_id = 0)
	{
		if(UtilLog.isBulidLog)UtilLog.Log(" load rees over!!");
		bool is_load_error = false;
		// update local res
		foreach(string url in _sResUrls.Keys)
		{
			// get real path 
			// /version/Version_0.3.1/csv/Hero.csv
			// get version_0.3.1/csv/Hero.csv
			string str_1 = url.Replace(ConfigUrl.ROOT_UPDATE_WEB,"");
			// get version_0.3.1/
			string str_2 = str_1.Substring(0, str_1.IndexOf("/") + 1); 

			string path = url.Replace(ConfigUrl.ROOT_UPDATE_WEB + str_2, ConfigUrl.ROOT_UPDATE_LOCAL_WRITE);


		




			// write file
			try
			{
				if(LoadMngr.getInstance().getBytes(url) == null)
				{
					if(UtilLog.isBulidLog)UtilLog.LogError("file down is null!! url = " + url);
					is_load_error = true;
				}
				else if(LoadMngr.getInstance().getBytes(url).Length <= 0)
				{
					is_load_error = true;
					if(UtilLog.isBulidLog)UtilLog.LogError("file down len is 0!!  url = " + url);
				}
				else if(_sResUrls[url].Trim().ToLower() != UtilMd5.getMd5(LoadMngr.getInstance().getBytes(url)))
				{
					if(UtilLog.isBulidLog)UtilLog.LogError("file down md5  is wrong!!  url = " + url + " file md5 = " + _sResUrls[url].ToLower() + 
					                                       " cal md5 = " + UtilMd5.getMd5(LoadMngr.getInstance().getBytes(url)));
					is_load_error = true;
				}
				else
				{
					createFile(path, LoadMngr.getInstance().getBytes(url));

				}

			}
			catch(Exception ex)
			{
				UtilLog.LogError(" create file exception !! ex = " + ex);
				is_load_error = true;
			}

		}

		if(_sResUrls == null || _sResUrls.Count == 0)
		{
			is_load_error = true;
		}



		// update local version file
		if(_sVerLatest != "" && !is_load_error)
		{
			string path = ConfigUrl.ROOT_UPDATE_LOCAL_WRITE + VER_FILE_LOCAL;

			// messy code will not happen . because all letter and number
			createFile(path, System.Text.Encoding.UTF8.GetBytes(_sVerLatest));

			if(UtilLog.isBulidLog)UtilLog.Log("create version file = " + path);

		}


		// over call back
		try
		{
			callBack();
		}
		catch(Exception ex) 
		{
			UtilLog.LogError("exception ex = " + ex);		
		}

		_sResUrls.Clear();
		


	}

	/// <summary>
	/// create dir if needed
	/// </summary>
	private static void createDir(string path)
	{
		// get dir 
		string dir = path.Substring(0, path.LastIndexOf("/") + 1);


		// create dir
		if(!System.IO.Directory.Exists(dir))
		{
			System.IO.Directory.CreateDirectory(dir);
		}


	}



	/// <summary>
	/// create bin file
	/// </summary>
	/// <param name="path">Path.</param>
	/// <param name="bytes">Bytes.</param>
	private static void createFile(string path, byte[] bytes)
	{
		if(UtilLog.isBulidLog)UtilLog.Log("create file path = " + path);
		if(bytes == null || bytes.Length <= 0)
		{
			UtilLog.LogError("create file byte is null!! path = " + path);
			// TODO: over and warning user!!
//			UIUtilPopup.CContentText tex = new UIUtilPopup.CContentText ();
//			tex.pivot = UIUtilPopup.EPivot.EMid;
//			tex.text = ConfigLabel.POPUP_CONTENT_VERSION_UPDATE_WRONG;
//
//			List<UIUtilPopup.CBut> buts = new List<UIUtilPopup.CBut> ();
//
//			UIUtilPopup.CBut but_ok = new UIUtilPopup.CBut ();
//			but_ok.butName = ConfigLabel.POPUP_BUT_NAME_OK;
//			but_ok.callBack = onClickVersionWrong;
//			buts.Add(but_ok);



//			WindowsMngr.getInstance().showPopupNormalText(ConfigLabel.POPUP_CONTENT_VERSION_UPDATE_WRONG, tex, buts, false);
			return;
		}

		createDir(path);
		FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);

		fs.Write(bytes, 0, bytes.Length);
		fs.Flush();
		fs.Close();
	}

	/// <summary>
	/// quit application
	/// </summary>
	/// <param name="go">Go.</param>
	public static void onClickVersionWrong(GameObject go)
	{
		Application.Quit();
	}
	
	public static void callBack()
	{
		LoadMngr.MAX_LOAD_THREAD_FRONT = 8;

		if(_sCallBack != null)
		{
			_sCallBack();
			_sCallBack = null;
		}
	}


	



}
