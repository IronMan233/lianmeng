using UnityEngine;
using System.Collections;

public class ManagerDeviceAuto : MonoBehaviour 
{
	private static readonly float allTime = 5f;
	private static readonly float lowFrame = 28f;
	private static readonly float delayTime = 2f;

	public static bool isBegin = false;

	private float _time = 0f;
	private float _frame = 0f;
	private int _tick = 0;
	private bool _isSetFb = false;
	private bool _isSetTown = false;
	private bool _isSetGlobal = false;

	// Use this for initialization
	void Start () 
	{
		_time = 0f;
		_frame = 0f;
		_tick = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(DataMode.myPlayer == null)
		{
			return;
		}

		if(!isBegin)
		{
			return;
		}

		if(_isSetFb &&
		   _isSetTown &&
		   _isSetGlobal)
		{
			return;
		}

		_time += Time.deltaTime;
		if(_time <= delayTime)
		{
			return;
		}
		_tick++;
		if(_time >= allTime)
		{

			_frame = _tick/(allTime - delayTime);
			UtilLog.Log("fps = " + _frame);

			if(_frame <= lowFrame)
			{
				setUpLow(true);
			}
			else
			{
				setUpLow(false);
			}

			_time = 0f;
			_tick = 0;
			isBegin = false;

		}

	
	}

	public void setUpLow(bool is_low)
	{
		if(DataMode.myPlayer.isInFB || DataMode.myPlayer.isInFBWorldBoss || DataMode.myPlayer.isInPK ||
		   DataMode.myPlayer.isInSunWell)
		{
			_isSetFb = true;
			if(is_low)
			{
				DataMode.infoSetting.setValue(InfoSettingEnum.isCloseLight, true);
				ManagerSence.setLight(false);
			}


		}
		else if(DataMode.myPlayer.isInTown)
		{
			_isSetTown = true;
			if(is_low)
			{
				ManagerDevice.isOpenGoodTown = false;
			}

			if(WindowsMngr.getInstance().isVisible(WindowsID.FB_Global))
			{
				_isSetGlobal = true;
				if(is_low)
				{
					ManagerDevice.isOpenFishEye = false;
					ManagerDevice.isOpenGlobalEffAndAni = false;
					ManagerDevice.isOpenGoodEffCurLand = false;
					ManagerDevice.isOpenGoodWater = false;
					ManagerDevice.isOpenVig = false;
					ManagerDevice.isOpenGoodLandDisable = false;
				}
			}
		}



	}
}
