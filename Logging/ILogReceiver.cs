namespace StockSharp.Logging;

using System;

using Ecng.Common;

/// <summary>
/// Logs recipient interface.
/// </summary>
public interface ILogReceiver : ILogSource
{
	/// <summary>
	/// To record a message to the log.
	/// </summary>
	/// <param name="message">A debug message.</param>
	void AddLog(LogMessage message);
}

/// <summary>
/// The base implementation <see cref="ILogReceiver"/>.
/// </summary>
public abstract class BaseLogReceiver : BaseLogSource, ILogReceiver
{
	/// <summary>
	/// Initialize <see cref="BaseLogReceiver"/>.
	/// </summary>
	protected BaseLogReceiver()
	{
	}

	void ILogReceiver.AddLog(LogMessage message)
	{
		RaiseLog(message);
	}
}

/// <summary>
/// Global logs receiver.
/// </summary>
public class GlobalLogReceiver : ILogReceiver
{
	private ILogReceiver App => LogManager.Instance?.Application;

	private GlobalLogReceiver()
	{
	}

	/// <summary>
	/// Instance.
	/// </summary>
	public static GlobalLogReceiver Instance { get; } = new GlobalLogReceiver();

	Guid ILogSource.Id => App?.Id ?? default;

	string ILogSource.Name
	{
		get => App?.Name;
		set { }
	}
	
	ILogSource ILogSource.Parent
	{
		get => App?.Parent;
		set => throw new NotSupportedException();
	}

	/// <inheritdoc />
	public event Action<ILogSource> ParentRemoved
	{
		add { }
		remove { }
	}
	
	LogLevels ILogSource.LogLevel
	{
		get => App?.LogLevel ?? default;
		set
		{
			var app = App;

			if (app == null)
				return;

			app.LogLevel = value;
		}
	}

	DateTimeOffset ILogSource.CurrentTime => App?.CurrentTime ?? default;

	bool ILogSource.IsRoot => true;

	event Action<LogMessage> ILogSource.Log
	{
		add
		{
			var app = App;

			if (app == null)
				return;

			app.Log += value;
		}
		remove
		{
			var app = App;

			if (app == null)
				return;

			app.Log -= value;
		}
	}

	void ILogReceiver.AddLog(LogMessage message)
	{
		App?.AddLog(message);
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
	}
}

/// <summary>
/// <see cref="BaseLogReceiver"/>.
/// </summary>
public class LogReceiver : BaseLogReceiver
{
	/// <summary>
	/// Create instance.
	/// </summary>
	/// <param name="name">Name.</param>
	public LogReceiver(string name = null)
	{
		if (!name.IsEmptyOrWhiteSpace())
			// ReSharper disable once VirtualMemberCallInConstructor
			Name = name;
	}
}