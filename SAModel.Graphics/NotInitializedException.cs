using System;

namespace SonicRetro.SAModel.Graphics
{
	/// <summary>
	/// Gets thrown when something was not initialized
	/// </summary>
	sealed class NotInitializedException : Exception
	{
		public NotInitializedException(string message) : base(message)
		{
		}
	}
}
