using System;
using System.Collections.Generic;

namespace Photon.Voice
{
	public class Framer<T>
	{
		private T[] frame;

		private int sizeofT;

		private int framePos;

		public Framer(int frameSize)
		{
			frame = new T[frameSize];
			T[] array = new T[1];
			if (array[0] is byte)
			{
				sizeofT = 1;
				return;
			}
			if (array[0] is short)
			{
				sizeofT = 2;
				return;
			}
			if (array[0] is float)
			{
				sizeofT = 4;
				return;
			}
			throw new Exception("Input data type is not supported: " + array[0].GetType());
		}

		public int Count(int bufLen)
		{
			return (bufLen + framePos) / frame.Length;
		}

		public IEnumerable<T[]> Frame(T[] buf)
		{
			int s = frame.Length;
			if (s == buf.Length && framePos == 0)
			{
				yield return buf;
				yield break;
			}
			int bufPos = 0;
			while (bufPos + s - framePos <= buf.Length)
			{
				int l = s - framePos;
				Buffer.BlockCopy(buf, bufPos * sizeofT, frame, framePos * sizeofT, l * sizeofT);
				bufPos += l;
				framePos = 0;
				yield return frame;
			}
			if (bufPos != buf.Length)
			{
				int num = buf.Length - bufPos;
				Buffer.BlockCopy(buf, bufPos * sizeofT, frame, framePos * sizeofT, num * sizeofT);
				framePos += num;
			}
		}
	}
}
