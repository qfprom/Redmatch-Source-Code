using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoPunVoice
{
	public class ChangePOV : MonoBehaviour, IMatchmakingCallbacks
	{
		public delegate void OnCameraChanged(Camera newCamera);

		private FirstPersonController firstPersonController;

		private ThirdPersonController thirdPersonController;

		private OrthographicController orthographicController;

		private Vector3 initialCameraPosition;

		private Quaternion initialCameraRotation;

		private Camera defaultCamera;

		[SerializeField]
		private GameObject ButtonsHolder;

		[SerializeField]
		private Button FirstPersonCamActivator;

		[SerializeField]
		private Button ThirdPersonCamActivator;

		[SerializeField]
		private Button OrthographicCamActivator;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static OnCameraChanged CameraChanged__BackingField;

		public static event OnCameraChanged CameraChanged
		{
			add
			{
				OnCameraChanged onCameraChanged = CameraChanged__BackingField;
				OnCameraChanged onCameraChanged2;
				do
				{
					onCameraChanged2 = onCameraChanged;
					onCameraChanged = Interlocked.CompareExchange(ref CameraChanged__BackingField, (OnCameraChanged)Delegate.Combine(onCameraChanged2, value), onCameraChanged);
				}
				while ((object)onCameraChanged != onCameraChanged2);
			}
			remove
			{
				OnCameraChanged onCameraChanged = CameraChanged__BackingField;
				OnCameraChanged onCameraChanged2;
				do
				{
					onCameraChanged2 = onCameraChanged;
					onCameraChanged = Interlocked.CompareExchange(ref CameraChanged__BackingField, (OnCameraChanged)Delegate.Remove(onCameraChanged2, value), onCameraChanged);
				}
				while ((object)onCameraChanged != onCameraChanged2);
			}
		}

		private void OnEnable()
		{
			CharacterInstantiation.CharacterInstantiated += OnCharacterInstantiated;
			PhotonNetwork.AddCallbackTarget(this);
		}

		private void OnDisable()
		{
			CharacterInstantiation.CharacterInstantiated -= OnCharacterInstantiated;
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		private void Start()
		{
			defaultCamera = Camera.main;
			initialCameraPosition = new Vector3(defaultCamera.transform.position.x, defaultCamera.transform.position.y, defaultCamera.transform.position.z);
			initialCameraRotation = new Quaternion(defaultCamera.transform.rotation.x, defaultCamera.transform.rotation.y, defaultCamera.transform.rotation.z, defaultCamera.transform.rotation.w);
			FirstPersonCamActivator.onClick.AddListener(FirstPersonMode);
			ThirdPersonCamActivator.onClick.AddListener(ThirdPersonMode);
			OrthographicCamActivator.onClick.AddListener(OrthographicMode);
		}

		private void OnCharacterInstantiated(GameObject character)
		{
			firstPersonController = character.GetComponent<FirstPersonController>();
			firstPersonController.enabled = false;
			thirdPersonController = character.GetComponent<ThirdPersonController>();
			thirdPersonController.enabled = false;
			orthographicController = character.GetComponent<OrthographicController>();
			ButtonsHolder.SetActive(true);
		}

		private void FirstPersonMode()
		{
			ToggleMode(firstPersonController);
		}

		private void ThirdPersonMode()
		{
			ToggleMode(thirdPersonController);
		}

		private void OrthographicMode()
		{
			ToggleMode(orthographicController);
		}

		private void ToggleMode(BaseController controller)
		{
			if (!(controller == null) && !(controller.ControllerCamera == null))
			{
				controller.ControllerCamera.gameObject.SetActive(true);
				controller.enabled = true;
				FirstPersonCamActivator.interactable = !(controller == firstPersonController);
				ThirdPersonCamActivator.interactable = !(controller == thirdPersonController);
				OrthographicCamActivator.interactable = !(controller == orthographicController);
				BroadcastChange(controller.ControllerCamera);
			}
		}

		private void BroadcastChange(Camera camera)
		{
			if (!(camera == null) && CameraChanged__BackingField != null)
			{
				CameraChanged__BackingField(camera);
			}
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
		}

		public void OnCreatedRoom()
		{
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
		}

		public void OnJoinedRoom()
		{
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
		}

		public void OnLeftRoom()
		{
			defaultCamera.gameObject.SetActive(true);
			FirstPersonCamActivator.interactable = true;
			ThirdPersonCamActivator.interactable = true;
			OrthographicCamActivator.interactable = false;
			defaultCamera.transform.position = initialCameraPosition;
			defaultCamera.transform.rotation = initialCameraRotation;
			ButtonsHolder.SetActive(false);
		}
	}
}
