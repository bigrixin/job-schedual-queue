﻿using System;

namespace MyAbilityFirst.Services
{
	/// <summary>
	/// Log <see cref="Exception"/> objects.
	/// </summary>
	public interface ILoggingService
	{

		/// <summary>
		/// Logs the specified exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		void Log(Exception exception);

	}
}