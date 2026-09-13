using UnityEngine;

namespace EZCameraShake
{
	public static class CameraShakePresets
	{
		public static CameraShakeInstance Bump
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(2.5f, 4f, 0.1f, 0.75f);
				cameraShakeInstance.PositionInfluence = Vector3.one * 0.15f;
				cameraShakeInstance.RotationInfluence = Vector3.one;
				return cameraShakeInstance;
			}
		}

		public static CameraShakeInstance Explosion
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(5f, 10f, 0f, 1.5f);
				cameraShakeInstance.PositionInfluence = Vector3.one * 0.25f;
				cameraShakeInstance.RotationInfluence = new Vector3(4f, 1f, 1f);
				return cameraShakeInstance;
			}
		}

		public static CameraShakeInstance Earthquake
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(0.6f, 3.5f, 2f, 10f);
				cameraShakeInstance.PositionInfluence = Vector3.one * 0.25f;
				cameraShakeInstance.RotationInfluence = new Vector3(1f, 1f, 4f);
				return cameraShakeInstance;
			}
		}

		public static CameraShakeInstance BadTrip
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(10f, 0.15f, 5f, 10f);
				cameraShakeInstance.PositionInfluence = new Vector3(0f, 0f, 0.15f);
				cameraShakeInstance.RotationInfluence = new Vector3(2f, 1f, 4f);
				return cameraShakeInstance;
			}
		}

		public static CameraShakeInstance HandheldCamera
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(1f, 0.25f, 5f, 10f);
				cameraShakeInstance.PositionInfluence = Vector3.zero;
				cameraShakeInstance.RotationInfluence = new Vector3(1f, 0.5f, 0.5f);
				return cameraShakeInstance;
			}
		}

		public static CameraShakeInstance Vibration
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(0.4f, 20f, 2f, 2f);
				cameraShakeInstance.PositionInfluence = new Vector3(0f, 0.15f, 0f);
				cameraShakeInstance.RotationInfluence = new Vector3(1.25f, 0f, 4f);
				return cameraShakeInstance;
			}
		}

		public static CameraShakeInstance RoughDriving
		{
			get
			{
				CameraShakeInstance cameraShakeInstance = new CameraShakeInstance(1f, 2f, 1f, 1f);
				cameraShakeInstance.PositionInfluence = Vector3.zero;
				cameraShakeInstance.RotationInfluence = Vector3.one;
				return cameraShakeInstance;
			}
		}
	}
}
