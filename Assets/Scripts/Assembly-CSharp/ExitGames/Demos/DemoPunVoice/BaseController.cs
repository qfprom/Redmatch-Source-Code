using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ExitGames.Demos.DemoPunVoice
{
	[RequireComponent(typeof(PhotonView))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Animator))]
	public abstract class BaseController : MonoBehaviour
	{
		public Camera ControllerCamera;

		protected Rigidbody rigidBody;

		protected Animator animator;

		protected Transform camTrans;

		private float h;

		private float v;

		[SerializeField]
		protected float speed = 5f;

		[SerializeField]
		private float cameraDistance;

		protected virtual void OnEnable()
		{
			ChangePOV.CameraChanged += ChangePOV_CameraChanged;
		}

		protected virtual void OnDisable()
		{
			ChangePOV.CameraChanged -= ChangePOV_CameraChanged;
		}

		protected virtual void ChangePOV_CameraChanged(Camera camera)
		{
			if (camera != ControllerCamera)
			{
				base.enabled = false;
				HideCamera(ControllerCamera);
			}
			else
			{
				ShowCamera(ControllerCamera);
			}
		}

		protected virtual void Start()
		{
			PhotonView component = GetComponent<PhotonView>();
			if (component.IsMine)
			{
				Init();
				SetCamera();
			}
			else
			{
				base.enabled = false;
			}
		}

		protected virtual void Init()
		{
			rigidBody = GetComponent<Rigidbody>();
			animator = GetComponent<Animator>();
		}

		protected virtual void SetCamera()
		{
			camTrans = ControllerCamera.transform;
			camTrans.position += cameraDistance * base.transform.forward;
		}

		protected virtual void UpdateAnimator(float h, float v)
		{
			bool value = h != 0f || v != 0f;
			animator.SetBool("IsWalking", value);
		}

		protected virtual void FixedUpdate()
		{
			h = CrossPlatformInputManager.GetAxisRaw("Horizontal");
			v = CrossPlatformInputManager.GetAxisRaw("Vertical");
			UpdateAnimator(h, v);
			Move(h, v);
		}

		protected virtual void ShowCamera(Camera camera)
		{
			if (camera != null)
			{
				camera.gameObject.SetActive(true);
			}
		}

		protected virtual void HideCamera(Camera camera)
		{
			if (camera != null)
			{
				camera.gameObject.SetActive(false);
			}
		}

		protected abstract void Move(float h, float v);
	}
}
