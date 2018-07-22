using UnityEngine;
using System.Collections;

/// <summary>
/// 圈数计数器，用于计算局内排名。设计此计数器来支持非触发器方式的路点更新器
/// 分两种更新策略，Local和Remote，当本地有更新的时候，以本地为主；
/// 假设一直有Remote更新，用于确定本地更新时的正确圈数
/// 断线重连的情况直接使用Reset接口
/// </summary>
public class LapCounter {

	public enum UpdateStrategy
	{
		PreferLocal,
		PreferRemote,
	}

	private UpdateStrategy _strategy;

	private int _totalLapCnt;
	private int _waypointsCntPerLap;

	private int _curLapCnt;
	private int _curWaypointIdx;

	private int _curLapCntRemote;
	private int _curWaypointIdxRemote;

	//最大容忍路点误差
	private const int cMaxWaypointError = 12;


	public LapCounter(int lapCount, int waypointsCntPerLap)
	{
		_totalLapCnt = lapCount;
		_waypointsCntPerLap = waypointsCntPerLap;
		_strategy = UpdateStrategy.PreferLocal;
	}

	public void SetStrategy(UpdateStrategy s)
	{
		_strategy = s;
	}

	/// <summary>
	/// 本地路点更新接口，当Local和Remote在接近的时间调用时，以本地为准
	/// </summary>
	/// <param name="newWaypoitIdx">New waypoit index.</param>
	public void UpdateWayPointLocal(int newWaypoitIdx)
	{
		if (_strategy != UpdateStrategy.PreferLocal)
		{
			return;
		}

		var originWpIdx = _curWaypointIdx;

		if (newWaypoitIdx < originWpIdx)
		{
			if (originWpIdx - newWaypoitIdx > (_waypointsCntPerLap - cMaxWaypointError))
			{
				//lap increase
				if (IsFinished)
				{
					Debug.LogWarning("already finished");
				}
				else
				{
					_curWaypointIdx = newWaypoitIdx;
					IncreaseLap();
				}
			}
			else
			{
				//如果是不同步造成的reset，这时可能remote圈数大于当前圈数，则直接使用remote的圈数
				if (_curLapCntRemote > _curLapCnt)
				{
					//increase lap
					IncreaseLap();
				}
				else
				{
					//remote圈数和当前一致的情况，也有可能是预测点过了圈，但remote数据还没来得及同步。也要加圈
					if (_curWaypointIdxRemote - newWaypoitIdx > (_waypointsCntPerLap - cMaxWaypointError))
					{
						_curWaypointIdx = newWaypoitIdx;
						IncreaseLap();
					}
					else
					{
						//正常的往回行驶
						_curWaypointIdx = newWaypoitIdx;
					}
				}
			}
		}
		else
		{
			if (newWaypoitIdx - originWpIdx > (_waypointsCntPerLap - cMaxWaypointError))
			{
				DecreaseLap();

				_curWaypointIdx = newWaypoitIdx;
			}
			else
			{
				//move forward
				_curWaypointIdx = newWaypoitIdx;
			}

		}

		_curWaypointIdx = newWaypoitIdx;
	}

	/// <summary>
	/// 远程路点更新接口，当对象被裁剪时更新路点
	/// </summary>
	/// <param name="newWaypointIdx">New waypoint index.</param>
	public void UpdateWayPointRemote(int lapCount, int newWaypointIdx)
	{
		_curLapCntRemote = lapCount;
		_curWaypointIdxRemote = newWaypointIdx;

		if (_strategy == UpdateStrategy.PreferRemote)
		{
			_curLapCnt = lapCount;
			_curWaypointIdx = newWaypointIdx;
		}
	}

	/// <summary>
	/// 只有在断线重连后调用此借口
	/// </summary>
	/// <param name="lapCount">Lap count.</param>
	/// <param name="waypointIndex">Waypoint index.</param>
	public void Reset(int lapCount, int waypointIndex)
	{
		Debug.Log("Reset to lap:" + lapCount + ", waypointIndex:" + waypointIndex);

		_curLapCnt = lapCount;
		_curWaypointIdx = waypointIndex;
	}

	public int CurrentLap
	{
		get {
			return _curLapCnt;
		}
	}

	public int CurrentWaypointIndex
	{
		get {
			return _curWaypointIdx;
		}
	}

	private bool _isFinished;
	public bool IsFinished
	{
		get 
		{
			return _isFinished;
		}
	}

	private void OnFinished()
	{
		Debug.Log("finished.");
	}

	private void OnDecreaseLap()
	{
		Debug.Log("DecreaseLap to:" + _curLapCnt);
	}

	private void OnIncreaseLap()
	{
		Debug.Log("IncreaseLap to:" + _curLapCnt);
	}

	public void IncreaseLap()
	{
		_curLapCnt++;

		OnIncreaseLap();

		if (_curLapCnt >= _totalLapCnt)
		{
			_isFinished = true;
			OnFinished();
		}
	}

	public void DecreaseLap()
	{
		_curLapCnt--;

		if (_curLapCnt < 0)
			_curLapCnt = 0;
		
		OnDecreaseLap();
	}

	private int GetScore(int lap, int wpIdx)
	{
		return lap * _waypointsCntPerLap + wpIdx;
	}


}
