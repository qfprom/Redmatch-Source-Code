using UnityEngine;

namespace ExitGames.Demos.DemoPunVoice
{
	public class OrthographicController : ThirdPersonController
	{
		public float smoothing = 5f;

		private Vector3 offset;

		protected override void Init()
		{
			base.Init();
			ControllerCamera = Camera.main;
		}

		protected override void SetCamera()
		{
			base.SetCamera();
			offset = camTrans.position - base.transform.position;
		}

		protected override void Move(float h, float v)
		{
			base.Move(h, v);
			CameraFollow();
		}

		private void CameraFollow()
		{
			Vector3 b = base.transform.position + offset;
			camTrans.position = Vector3.Lerp(camTrans.position, b, smoothing * Time.deltaTime);
		}
	}
}
