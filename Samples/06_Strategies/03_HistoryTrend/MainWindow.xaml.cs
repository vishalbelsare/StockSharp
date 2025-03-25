﻿namespace StockSharp.Samples.Strategies.HistoryTrend;

using System;
using System.Windows;
using System.Windows.Media;

using Ecng.Collections;
using Ecng.Common;
using Ecng.Drawing;
using Ecng.Logging;

using StockSharp.Algo.Commissions;
using StockSharp.Algo.Storages;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Testing;
using StockSharp.BusinessEntities;
using StockSharp.Configuration;
using StockSharp.Messages;
using StockSharp.Xaml;
using StockSharp.Xaml.Charting;
using StockSharp.Charting;

public partial class MainWindow
{
	private HistoryEmulationConnector _connector;
	private readonly string _pathHistory = Paths.HistoryDataPath;

	private Security _security;
	private Portfolio _portfolio;
	private Strategy _strategy;
	
	private readonly LogManager _logManager;
	
	private IChartBandElement _pnl;
	private IChartBandElement _unrealizedPnL;
	private IChartBandElement _commissionCurve;

	public MainWindow()
	{
		InitializeComponent();

		_logManager = new LogManager();
		_logManager.Listeners.Add(new FileLogListener("log.txt"));
		_logManager.Listeners.Add(new GuiLogListener(Monitor));

		DatePickerBegin.SelectedDate = Paths.HistoryBeginDate;
		DatePickerEnd.SelectedDate = Paths.HistoryEndDate;

		CandleDataTypeEdit.DataType = TimeSpan.FromMinutes(5).TimeFrame();
	}

	private void Start_Click(object sender, RoutedEventArgs e)
	{
		_security = new Security
		{
			Id = Paths.HistoryDefaultSecurity,
			PriceStep = 0.01m,
		};
		_portfolio = new Portfolio { Name = "test account", BeginValue = 1000000 };
		var storageRegistry = new StorageRegistry
		{
			DefaultDrive = new LocalMarketDataDrive(_pathHistory),
		};

		_connector = new HistoryEmulationConnector(new[] { _security }, new[] { _portfolio })
		{
			HistoryMessageAdapter =
			{
				StorageRegistry = storageRegistry,
				StorageFormat = StorageFormats.Binary,
				StartDate = DatePickerBegin.SelectedDate.Value.ChangeKind(DateTimeKind.Utc),
				StopDate = DatePickerEnd.SelectedDate.Value.ChangeKind(DateTimeKind.Utc),
			},
			LogLevel = LogLevels.Info,
		};

		_logManager.Sources.Add(_connector);

		Chart.ClearAreas();
		InitPnLChart();
		OrderGrid.Orders.Clear();
		MyTradeGrid.Trades.Clear();

		_connector.OrderBookReceived += (s, b) => MarketDepthControl.UpdateDepth(b);
		_connector.OrderReceived += (s, o) => OrderGrid.Orders.TryAdd(o);
		_connector.OrderRegisterFailReceived += (s, f) => OrderGrid.AddRegistrationFail(f);
		_connector.OwnTradeReceived += (s, t) => MyTradeGrid.Trades.TryAdd(t);

		//
		// !!! IMPORTANT !!!
		// Uncomment the desired strategy
		//
		_strategy = new OneCandleCountertrendStrategy
		//_strategy = new OneCandleTrendStrategy
		//_strategy = new StairsCountertrendStrategy
		//_strategy = new StairsTrendStrategy
		{
			Security = _security,
			Connector = _connector,
			Portfolio = _portfolio,
			CandleType = CandleDataTypeEdit.DataType,
		};
		_logManager.Sources.Add(_strategy);

		_strategy.PnLChanged += Strategy_PnLChanged;
		_strategy.SetChart(Chart);

		_strategy.Start();

		StatisticParameterGrid.Parameters.AddRange(_strategy.StatisticManager.Parameters);

		_connector.Connect();
		_connector.SendInMessage(new CommissionRuleMessage
		{
			Rule = new CommissionPerTradeRule { Value = 0.01m }
		});

		_connector.Start();
	}

	private void InitPnLChart()
	{
		_pnl = EquityCurveChart.CreateCurve("P&L", Colors.Green, DrawStyles.Area);
		_unrealizedPnL = EquityCurveChart.CreateCurve("unrealized", Colors.Black, DrawStyles.Line);
		_commissionCurve = EquityCurveChart.CreateCurve("commission", Colors.Red, DrawStyles.Line);
	}

	private void Strategy_PnLChanged()
	{
		EquityCurveChart.DrawPnL(_strategy, _pnl, _unrealizedPnL, _commissionCurve);
	}
}

