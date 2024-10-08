﻿using System;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;
using StockSharp.BusinessEntities;

namespace StockSharp.Samples.Strategies.HistoryTrend
{
	public class OneCandleCountertrendStrategy : Strategy
	{
		private readonly CandleSeries _candleSeries;
		private Subscription _subscription;

		public OneCandleCountertrendStrategy(CandleSeries candleSeries)
		{
			_candleSeries = candleSeries;
		}

		protected override void OnStarted(DateTimeOffset time)
		{
			CandleReceived += OnCandleReceived;
			_subscription = this.SubscribeCandles(_candleSeries);

			base.OnStarted(time);
		}

		protected override void OnStopped()
		{
			if (_subscription != null)
			{
				UnSubscribe(_subscription);
				_subscription = null;
			}

			base.OnStopped();
		}

		private void OnCandleReceived(Subscription subscription, ICandleMessage candle)
		{
			if (subscription != _subscription)
				return;

			if (candle.State != CandleStates.Finished) return;

			if (candle.OpenPrice < candle.ClosePrice && Position >= 0)
			{
				SellMarket(Volume + Math.Abs(Position));
			}

			else
			if (candle.OpenPrice > candle.ClosePrice && Position <= 0)
			{
				BuyMarket(Volume + Math.Abs(Position));
			}
		}
	}
}