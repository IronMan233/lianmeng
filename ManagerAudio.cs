using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagerAudio
{
	public static bool playerCostMoneySound = false;
	/// 音乐
	private static AudioSource _music;
	private static AudioSource _musicOut;
	
	private static double _musicIndex = 0d;
	
	private static string _musicUrl;
	/// 音效
	private static GameObject _sound;
	
	/// 各种音效,尼玛啊
	private static Dictionary<string, AudioSource[]> _soundAudioSource = new Dictionary<string, AudioSource[]>();
	private static Dictionary<string, int> _soundAudioSourceIndex = new Dictionary<string, int>();
	
	private static int _soundSameLength = 3;
	private static int _soundSameLengthMax = 3;
	/// 同一种音效允许存在的个数
	public static int soundSameLength
	{
		get{return _soundSameLength;}
		set
		{
			_soundSameLength = value;
			_soundSameLength = Mathf.Max(_soundSameLength, 1);
			_soundSameLength = Mathf.Min(_soundSameLength, _soundSameLengthMax);
			
		}
	}
	
	
	
	/// ####################################################################################################
	/// 声音播放函数
	
	/// 播放 音效
	public static void playSound(string sName)
	{
		/// 节省开销的资源,如果音量为0.我还播放啥音乐啊
		if(DataMode.infoSetting.volumeSound == 0)
			return;
		
		
		if(string.IsNullOrEmpty(sName))
			return;
		if(sName == "#")
			return;
		
		
		///  音效播放的地方
		if(null == _sound)
		{
			_sound = new GameObject("_AudioSound");
			_sound.tag = Config.UNDESTROY;
		}
		/// 创建缓存
		if(!_soundAudioSource.ContainsKey(sName))
		{
			_soundAudioSource.Add(sName, new AudioSource[_soundSameLengthMax]);
			_soundAudioSourceIndex.Add(sName, 0);
			for(int i = 0; i < _soundAudioSource[sName].Length; i++)
			{
				GameObject obj = new GameObject("_AudioSound_" + sName);
				obj.transform.parent = _sound.transform;
				_soundAudioSource[sName][i] = obj.AddComponent<AudioSource>();
				_soundAudioSource[sName][i].panLevel = 0f;
				_soundAudioSource[sName][i].volume = DataMode.infoSetting.volumeSound;
			}
		}
		
		
		
		/// 加载或者播放
		string url = ConfigUrl.getAudioUrl(sName);
		if(LoadMngr.getInstance().isHasAsset(url))
		{
			AudioClip audioClip = LoadMngr.getInstance().GetAudio(url);
			
			
			/// 循环找各种玩意
			_soundAudioSourceIndex[sName]++;
			if(_soundAudioSourceIndex[sName] >= _soundSameLength)
				_soundAudioSourceIndex[sName] = 0;
			
			AudioSource soundSource = _soundAudioSource[sName][_soundAudioSourceIndex[sName]];
			/// 停止重新播放
			soundSource.Stop();
			soundSource.PlayOneShot(audioClip);
		} else {
			LoadMngr.getInstance().load(url, new LoadEventHandler[0], LoadMngr.ELoadPriority.EBack);
		
		}
	}
	/// 播放 音乐
	public static void playMusic(string sName)
	{
//		return;
		/// 声音空返回
		if(string.IsNullOrEmpty(sName))
			return;
		/// 声音一样返回
		if(_musicUrl == ConfigUrl.getAudioUrl(sName))
			return;
		if(sName == "#")
			return;
		///  音效播放的地方
		if(null == _music)
		{
			GameObject obj = new GameObject("_AudioMusic");
			_music = obj.AddComponent<AudioSource>();
			_music.tag = Config.UNDESTROY;
			_music.loop = true;
			_music.panLevel = 0f;
			_music.volume = DataMode.infoSetting.volumeMusic;
		}
		if(null == _musicOut)
		{
			GameObject obj = new GameObject("_AudioMusicOut");
			_musicOut = obj.AddComponent<AudioSource>();
			_musicOut.tag = Config.UNDESTROY;
			_musicOut.panLevel = 0f;
			_musicOut.loop = true;
			_musicOut.volume = DataMode.infoSetting.volumeMusic;
		}
		/// 加载或者播放
		_musicUrl = ConfigUrl.getAudioUrl(sName);

		if(DataMode.infoSetting.volumeMusic <= 0f)
			return;

		_musicIndex = LoadMngr.getInstance().load(_musicUrl, playMusicHD, LoadMngr.ELoadPriority.EBack);
		
	}
	/// 播放背景音乐
	private static void playMusicHD(double loader_id)
	{
		if(_musicIndex != loader_id)
			return;
		/// 渐变
		if(null != _music.clip)
		{
			_musicOut.clip = _music.clip;
			_musicOut.volume = DataMode.infoSetting.volumeMusic;
			_musicOut.pitch = _music.pitch;
			_musicOut.Play();
			TweenVolume.Begin(_musicOut.gameObject, 1f, 0f).onFinished.Add(new EventDelegate(playMusicTweenHD));
		}
		/// 播放背景音乐
		_music.clip = LoadMngr.getInstance().GetAudio(_musicUrl);
		_music.volume = 0f;
		_music.Play();
		if(DataMode.infoSetting.volumeMusic > 0f)
			TweenVolume.Begin(_music.gameObject, 1f, DataMode.infoSetting.volumeMusic);
	}
	/// 播放背景音乐的渐变
	private static void playMusicTweenHD()
	{
		_musicOut.Stop();
	}
	/// ####################################################################################################
	/// 声音设置
	
	/// 设置音量
	public static void setVolume(float volume)
	{
		setVolumeSound(volume);
		setVolumeMusic(volume);
		
	}
	/// 设置音量 音效
	public static void setVolumeSound(float volume)
	{
		foreach(AudioSource[] sourceArr in _soundAudioSource.Values)
		{
			if(null == sourceArr || sourceArr.Length <= 0)
				continue;
			foreach(AudioSource source in sourceArr)
			{
				if(null == source)
					continue;
				source.volume = volume;
			}
		}
	}
	/// 设置音量 音乐
	public static void setVolumeMusic(float volume)
	{
		if(volume >= 0 && !string.IsNullOrEmpty(_musicUrl))
			_musicIndex = LoadMngr.getInstance().load(_musicUrl, playMusicHD, LoadMngr.ELoadPriority.EBack);

		if(null != _music)
		{
			if(null != _music.gameObject.GetComponent<TweenVolume>())
			{
				if(_music.gameObject.GetComponent<TweenVolume>().enabled)
				{
					_music.gameObject.GetComponent<TweenVolume>().to = volume;
					return;
				}
			}
			_music.volume = volume;
		}
	}
	/// ####################################################################################################
	/// 销毁
	
	/// 清除声音
	public static void clear()
	{
		clearMusic();
		clearSound();
	}
	/// 清除声音 音效
	public static void clearSound()
	{
//		if(null != _sound)
//			_sound.Stop();
	}
	/// 清除声音 音乐
	public static void clearMusic()
	{
		if(null != _music)
			_music.Stop();
		_musicUrl = null;
	}
}
