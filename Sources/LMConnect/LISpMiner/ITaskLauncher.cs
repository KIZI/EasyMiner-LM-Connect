﻿using System;

namespace LMConnect.LISpMiner
{
	public interface ITaskLauncher : IDisposable
	{
		ExecutableStatus Status { get; }

		LISpMiner LISpMiner { get; }

		string TaskName { get; set; }

		bool TaskCancel { get; set; }

		bool CancelAll { get; set; }

		int? ShutdownDelaySec { get; set; }

		void Execute();
	}
}
