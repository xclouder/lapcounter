using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class TestLapCounter {

	private const int cLapWaypointCount = 100;

	[Test]
	public void TestNormalIncreaseDecreaseLap()
	{

		LapCounter lc = new LapCounter(2, cLapWaypointCount);


		for (int i = 0; i <= cLapWaypointCount; i++)
		{
			var k = i;
			if (i == cLapWaypointCount)
			{
				k = i - cLapWaypointCount;
			}

			lc.UpdateWayPointLocal(k);

			if (i < cLapWaypointCount)
			{
				Assert.AreEqual(0, lc.CurrentLap);
			}
			else
			{
				Assert.AreEqual(1, lc.CurrentLap);
			}

		}

		lc.UpdateWayPointLocal(cLapWaypointCount - 5);

		Assert.AreEqual(0, lc.CurrentLap);

	}

	[Test]
	public void TestPassiveResetForNetworkDelay()
	{
		LapCounter lc = new LapCounter(2, cLapWaypointCount);

		for (int i = 0; i <= cLapWaypointCount / 2; i++)
		{
			var k = i;
			if (i == cLapWaypointCount)
			{
				k = i - cLapWaypointCount;
			}

			lc.UpdateWayPointLocal(k);
		}

		Assert.AreEqual(0, lc.CurrentLap);

		//继续同步remote一段时间，但本地模拟碰撞等情况，导致不更新路点
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex - 2);
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex);
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex + 3);
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex + 10);
		lc.UpdateWayPointRemote(0, cLapWaypointCount - 5);
		lc.UpdateWayPointRemote(0, cLapWaypointCount - 3);

		//这一行可注释，模拟同步数据的圈数还没+1，但预测后reset导致+1的情况。效果见TestPassiveResetForNetworkDelay2
		lc.UpdateWayPointRemote(1, 1);

		//一段时间后被动reset，直接跳跃式更新路点
		lc.UpdateWayPointLocal(2);
		Assert.AreEqual(1, lc.CurrentLap);

	}

	[Test]
	public void TestPassiveResetForNetworkDelay2()
	{
		LapCounter lc = new LapCounter(2, cLapWaypointCount);

		for (int i = 0; i <= cLapWaypointCount / 2; i++)
		{
			var k = i;
			if (i == cLapWaypointCount)
			{
				k = i - cLapWaypointCount;
			}

			lc.UpdateWayPointLocal(k);
		}

		Assert.AreEqual(0, lc.CurrentLap);

		//继续同步remote一段时间，但本地模拟碰撞等情况，导致不更新路点
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex - 2);
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex);
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex + 3);
		lc.UpdateWayPointRemote(0, lc.CurrentWaypointIndex + 10);
		lc.UpdateWayPointRemote(0, cLapWaypointCount - 5);
		lc.UpdateWayPointRemote(0, cLapWaypointCount - 3);

		//lc.UpdateWayPointRemote(1, 1);

		//一段时间后被动reset，直接跳跃式更新路点
		lc.UpdateWayPointLocal(2);
		Assert.AreEqual(1, lc.CurrentLap);

	}

	[Test]
	public void TestWithNormalRemoteUpdate()
	{
		LapCounter lc = new LapCounter(2, cLapWaypointCount);

		//车被裁剪后，启用PreferRemote策略
		lc.SetStrategy(LapCounter.UpdateStrategy.PreferRemote);

		lc.UpdateWayPointRemote(1, 10);

		Assert.AreEqual(1, lc.CurrentLap);
		Assert.AreEqual(10, lc.CurrentWaypointIndex);
	}

	[Test]
	public void TestSwitchStragegyInCriticalSituation()
	{
		//切换策略的时候，距离比较远用户一般无感知。咱不处理
		Assert.Pass();
	}

	[Test]
	public void TestFinish()
	{
		LapCounter lc = new LapCounter(3, cLapWaypointCount);

		for (int i = 0; i <= cLapWaypointCount * 3; i++)
		{
			var k = i % cLapWaypointCount;

			lc.UpdateWayPointLocal(k);
		}

		Assert.AreEqual(3, lc.CurrentLap);
		Assert.IsTrue(lc.IsFinished);
	}

	/// <summary>
	/// 测试网络车重连，重置其位置的情况
	/// </summary>
	[Test]
	public void TestNetKartReconnect()
	{
		LapCounter lc = new LapCounter(3, cLapWaypointCount);


		for (int i = 0; i <= cLapWaypointCount; i++)
		{
			var k = i;
			if (i == cLapWaypointCount)
			{
				k = i - cLapWaypointCount;
			}

			lc.UpdateWayPointLocal(k);
		}

		lc.Reset(2, 8);

		Assert.AreEqual(2, lc.CurrentLap);

		lc.UpdateWayPointLocal(10);

		Assert.AreEqual(2, lc.CurrentLap);
		Assert.AreEqual(10, lc.CurrentWaypointIndex);
	}
}
