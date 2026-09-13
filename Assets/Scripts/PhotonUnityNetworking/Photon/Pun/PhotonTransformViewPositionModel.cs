using System;

namespace Photon.Pun
{
	[Serializable]
	public class PhotonTransformViewPositionModel
	{
		public enum InterpolateOptions
		{
			Disabled = 0,
			FixedSpeed = 1,
			EstimatedSpeed = 2,
			SynchronizeValues = 3,
			Lerp = 4
		}

		public enum ExtrapolateOptions
		{
			Disabled = 0,
			SynchronizeValues = 1,
			EstimateSpeedAndTurn = 2,
			FixedSpeed = 3
		}

		public bool SynchronizeEnabled;

		public bool TeleportEnabled = true;

		public float TeleportIfDistanceGreaterThan = 3f;

		public InterpolateOptions InterpolateOption = InterpolateOptions.EstimatedSpeed;

		public float InterpolateMoveTowardsSpeed = 1f;

		public float InterpolateLerpSpeed = 1f;

		public ExtrapolateOptions ExtrapolateOption;

		public float ExtrapolateSpeed = 1f;

		public bool ExtrapolateIncludingRoundTripTime = true;

		public int ExtrapolateNumberOfStoredPositions = 1;
	}
}
