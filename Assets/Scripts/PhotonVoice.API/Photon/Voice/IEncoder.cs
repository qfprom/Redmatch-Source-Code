using System;

namespace Photon.Voice
{
	public interface IEncoder : IDisposable
	{
		string Error { get; }
	}
}
