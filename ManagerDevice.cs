using UnityEngine;
using System.Collections;

public class ManagerDevice
{

	#region static var
	/// <summary>
	/// The is open vig. in fb muster 
	/// </summary>
	public static bool isOpenVig = true;

	/// <summary>
	/// good water in fbglobal
	/// </summary>
	public static bool isOpenGoodWater = true;

	/// <summary>
	/// The is open shaow. in sence
	/// </summary>
	public static bool isOpenShaow = true;

	public static bool isOpenMotionBlur = true;

	public static bool isOpenGoodTown = true;

	public static bool isOpenGoodEffCurLand = true;

	public static bool isOpenGlobalEffAndAni = true;
	
	public static bool isOpenGoodLoginPage = true;
	
	public static bool isOpenGoodLandDisable = true;
	public static bool isOpenFishEye = true;

	#endregion static var



	#region static fun
	public static void init()
	{
		// set default high
		if(UtilLog.isBulidLog)UtilLog.Log("device init!!");

		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
#if UNITY_IOS		

			switch(iPhone.generation)
			{
			case iPhoneGeneration.iPad1Gen:
			case iPhoneGeneration.iPad2Gen:
			
				isOpenVig = false;
				isOpenShaow = false;
				isOpenGoodWater = false;
				isOpenMotionBlur = false;
				isOpenGoodTown = false;
				isOpenGoodEffCurLand = false;
				isOpenGlobalEffAndAni = false;
				isOpenGoodLoginPage = false;
				break;

			case iPhoneGeneration.iPad3Gen:
				isOpenVig = false;
				isOpenGoodWater = false;
				isOpenMotionBlur = false;
				isOpenGoodEffCurLand = false;
				isOpenGlobalEffAndAni = false;
				break;



			case iPhoneGeneration.iPadMini1Gen:
				isOpenVig = false;
				isOpenShaow = false;
				isOpenGoodWater = false;
				isOpenMotionBlur = false;
				isOpenGoodTown = false;
				break;



			case iPhoneGeneration.iPhone:
			case iPhoneGeneration.iPhone3G:
			case iPhoneGeneration.iPhone3GS:
			case iPhoneGeneration.iPhone4:
				isOpenVig = false;
				isOpenShaow = false;
				isOpenGoodWater = false;
				isOpenMotionBlur = false;
				isOpenGoodTown = false;
				isOpenGoodEffCurLand = false;
				isOpenGlobalEffAndAni = false;
				isOpenGoodLoginPage = false;
				break;

			case iPhoneGeneration.iPhone4S:
				isOpenVig = false;
				isOpenGoodWater = false;
				isOpenMotionBlur = false;
				break;
			default:
				break;
			}
#endif

		}
		else if(Application.platform == RuntimePlatform.Android)
		{
			if(UtilLog.isBulidLog)UtilLog.Log("android device setting!!");
			isOpenVig = false;
			isOpenMotionBlur = false;
			isOpenGoodTown = false;


			if(SystemInfo.systemMemorySize <  1000)
			{
				// low android
				if(UtilLog.isBulidLog)UtilLog.Log("low android device !!");
				isOpenGoodWater = false;
				isOpenGlobalEffAndAni = false;
				isOpenShaow = false;
				isOpenGoodEffCurLand = false;
				isOpenGoodTown = false;
				isOpenGoodLandDisable = false;
				isOpenFishEye = false;



//				QualitySettings.SetQualityLevel((int)QualityLevel.Simple);
			}


		}

	}

	#endregion static fun



}
