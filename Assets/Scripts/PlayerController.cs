using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EZCameraShake;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	public GameObject playerCameraHolder;

	[SerializeField]
	private GameObject playerCamera;

	private Camera playerCam;

	[SerializeField]
	private SafeFloat walkSpeed;

	[SerializeField]
	private SafeFloat runSpeed;

	[SerializeField]
	private float jumpForce;

	[SerializeField]
	private float smoothTime;

	[SerializeField]
	private LayerMask groundedMask;

	[SerializeField]
	private Renderer graphicsRenderer;

	[SerializeField]
	private GameObject bulletHitPrefab;

	[SerializeField]
	private Text usernameText;

	[SerializeField]
	private Collider meleeBox;

	[SerializeField]
	private Crosshair crosshair;

	[SerializeField]
	private LayerMask bulletMask;

	[SerializeField]
	private GameObject playerCanvas;

	private Animator playerCanvasAnim;

	private bool burstFiring;

	private Animator anim;

	[SerializeField]
	private Item[] allItems;

	[HideInInspector]
	public int itemIndex;

	[HideInInspector]
	public List<Item> ownedItems = new List<Item>();

	[HideInInspector]
	public bool grounded;

	private float verticalLookRotation;

	private Vector3 moveAmount;

	private Vector3 smoothMoveVelocity;

	private Rigidbody rb;

	private AudioManager audio;

	[HideInInspector]
	public PhotonView PV;

	private const float maxHealth = 100f;

	private float currentHealth = 100f;

	private bool reloading;

	private bool sprinting;

	private bool walking;

	private bool shootBuffered;

	private bool interactPrompting;

	private float bulletSpread;

	private float lastHurtTime;

	private float regenDelay = 10f;

	private float regenSpeed = 10f;

	private Vector3 footstepLastPos;

	private float footstepDistance;

	private float nextTimeToShootAfterSwitchingWeapon;

	private float switchingWeaponsShootCooldown = 0.3f;

	[HideInInspector]
	public PhotonPlayer photonPlayer;

	private CameraShaker cs;

	private Dictionary<AmmoType, int> ammoReserve = new Dictionary<AmmoType, int>
	{
		{
			AmmoType.Light,
			0
		},
		{
			AmmoType.Medium,
			0
		},
		{
			AmmoType.Heavy,
			0
		},
		{
			AmmoType.Shotgun,
			0
		}
	};

	private bool meleeing;

	private bool crouching;

	private bool cancelReload;

	private bool climbing;

	private bool dead;

	private string currentSkinName;

	private List<Item> removeItemQueue = new List<Item>();

	public bool aiming { get; private set; }

	private void Awake()
	{
		walkSpeed = new SafeFloat(4f);
		runSpeed = new SafeFloat(6f);
		anim = GetComponent<Animator>();
		audio = GetComponent<AudioManager>();
		PV = GetComponent<PhotonView>();
		rb = GetComponent<Rigidbody>();
		playerCam = playerCamera.GetComponent<Camera>();
		playerCanvasAnim = playerCanvas.GetComponent<Animator>();
		cs = GetComponentInChildren<CameraShaker>(true);
	}

	private void Start()
	{
		if (!PV.IsMine)
		{
			usernameText.text = PV.Owner.NickName;
			Object.Destroy(rb);
			playerCanvas.SetActive(false);
			Camera[] componentsInChildren = GetComponentsInChildren<Camera>(true);
			Camera[] array = componentsInChildren;
			foreach (Camera camera in array)
			{
				Object.Destroy(camera.gameObject);
			}
			return;
		}
		usernameText.gameObject.SetActive(false);
		cs.enabled = true;
		Item[] array2 = allItems;
		foreach (Item item in array2)
		{
			if (item.info.itemType == ItemType.Holstered)
			{
				ownedItems.Add(item);
			}
		}
		if (GameManager.gameManager.GetGameModeInfo().startWithAllItems)
		{
			Item[] array3 = allItems;
			foreach (Item item2 in array3)
			{
				if (item2.info.itemType == ItemType.Gun || item2.info.itemType == ItemType.Melee)
				{
					ownedItems.Add(item2);
				}
			}
		}
		if (photonPlayer.infected)
		{
			PV.RPC("RPC_AssignInfected", RpcTarget.All);
		}
		if (GameMenuManager.menuManager.lockControl)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		if (GameManager.gameManager.gameMode == GameMode.GunGame)
		{
			UpdateGunGameItem();
			SelectItem(1, true);
		}
		else
		{
			SelectItem(0, true);
		}
		UpdateHealthGraphics();
	}

	[PunRPC]
	public void RPC_ApplySkin(string skinName)
	{
		currentSkinName = skinName;
		Skin skin = SkinManager.skinManager.GetSkin(skinName);
		if (!PV.IsMine)
		{
			graphicsRenderer.material = skin.skinMat;
		}
	}

	[PunRPC]
	public void RPC_AssignInfected()
	{
		usernameText.color = Color.green;
		if (!PV.IsMine)
		{
			return;
		}
		PV.RPC("RPC_PlaySound", RpcTarget.All, "infected");
		int num = 0;
		foreach (Item ownedItem in ownedItems)
		{
			if (ownedItem.info.canThrowAway)
			{
				RelieveItemOwnership(num);
			}
			num++;
		}
		Item[] array = allItems;
		foreach (Item item in array)
		{
			if (item.info.itemType == ItemType.Melee)
			{
				ownedItems.Add(item);
			}
		}
	}

	[PunRPC]
	public void RPC_AssignTTTTeam(TTTTeam team)
	{
		switch (team)
		{
		case TTTTeam.Detective:
			usernameText.color = Color.blue;
			break;
		case TTTTeam.Traitor:
			if (GameSetup.gameSetup.player.tttteam == TTTTeam.Traitor)
			{
				usernameText.color = Color.red;
				if (!PV.IsMine)
				{
					WaypointManager.Instance.RequestWaypoint(base.transform, "T", Color.red, WaypointManager.WaypointType.Name);
				}
			}
			break;
		}
	}

	private void UpdateItemInfo()
	{
		switch (ownedItems[itemIndex].info.itemType)
		{
		case ItemType.Gun:
		{
			GameSetup.gameSetup.itemInfoPanel.transform.GetChild(0).GetComponent<Text>().text = ownedItems[itemIndex].info.itemName;
			string[] itemInfo2 = new string[8]
			{
				"Damage: " + ownedItems[itemIndex].info.damage,
				"ROF: " + 60f / ownedItems[itemIndex].info.fireRate,
				"Recoil: " + ownedItems[itemIndex].info.recoil,
				"Magazine Size: " + ownedItems[itemIndex].info.magazineSize,
				"Aimed Spread: " + ownedItems[itemIndex].info.aimedBulletSpread,
				"Hip Spread: " + ownedItems[itemIndex].info.hipBulletSpread,
				"Reload Time: " + ownedItems[itemIndex].info.reloadTime,
				"Ammo Type: " + ownedItems[itemIndex].info.ammoType
			};
			GameSetup.gameSetup.RequestItemInfoRender(itemInfo2);
			break;
		}
		case ItemType.Melee:
		{
			GameSetup.gameSetup.itemInfoPanel.transform.GetChild(0).GetComponent<Text>().text = ownedItems[itemIndex].info.itemName;
			string[] itemInfo = new string[1] { "Damage: " + ownedItems[itemIndex].info.damage };
			GameSetup.gameSetup.RequestItemInfoRender(itemInfo);
			break;
		}
		}
	}

	private void Update()
	{
		if (!PV.IsMine)
		{
			return;
		}
		if (ownedItems[itemIndex].ammo > ownedItems[itemIndex].info.magazineSize + 3)
		{
			ownedItems[itemIndex].ammo = ownedItems[itemIndex].info.magazineSize;
			PleaseDontCheat.Instance.Help();
		}
		footstepDistance += Vector3.Distance(base.transform.position, footstepLastPos);
		footstepLastPos = base.transform.position;
		if (grounded && !crouching && footstepDistance >= 1.1f)
		{
			PV.RPC("RPC_PlaySound", RpcTarget.All, "footstep");
			footstepDistance = 0f;
		}
		if (!GameMenuManager.menuManager.lockControl)
		{
			for (int i = 0; i < ownedItems.Count; i++)
			{
				if (InputManager.IM.GetButtonDown("Slot " + (i + 1)))
				{
					SelectItem(i);
				}
			}
			if (InputManager.IM.GetButtonDown("Inspect") && ownedItems[itemIndex].info.itemType != ItemType.Holstered)
			{
				GameSetup.gameSetup.itemInfoPanel.SetActive(true);
				UpdateItemInfo();
			}
			else if (InputManager.IM.GetButtonUp("Inspect"))
			{
				GameSetup.gameSetup.itemInfoPanel.SetActive(false);
			}
			if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f || InputManager.IM.GetButtonDown("NextSlot"))
			{
				SelectItem(itemIndex - 1);
			}
			else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f || InputManager.IM.GetButtonDown("PreviousSlot"))
			{
				SelectItem(itemIndex + 1);
			}
			Vector2 vector = new Vector2(Input.GetAxisRaw("Controller X"), Input.GetAxisRaw("Controller Y"));
			vector = vector.normalized * Mathf.Clamp01(vector.magnitude);
			base.transform.Rotate(Vector3.up * (vector.x + Input.GetAxisRaw("Mouse X")) * ((!aiming || ownedItems[itemIndex].info.itemType != ItemType.Gun) ? SettingsManager.settings.sensitivity : (SettingsManager.settings.sensitivity * ownedItems[itemIndex].info.aimedSensitivityMultiplier)) * Time.deltaTime);
			verticalLookRotation += (vector.y + Input.GetAxisRaw("Mouse Y")) * ((!aiming || ownedItems[itemIndex].info.itemType != ItemType.Gun) ? SettingsManager.settings.sensitivity : (SettingsManager.settings.sensitivity * ownedItems[itemIndex].info.aimedSensitivityMultiplier)) * Time.deltaTime;
			verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
			Vector2 vector2 = new Vector2(InputManager.IM.GetAxis("Right", "Left"), InputManager.IM.GetAxis("Forward", "Backward"));
			vector2.Normalize();
			Vector3 vector3 = new Vector3(vector2.x, 0f, vector2.y);
			if (vector3.magnitude == 0f)
			{
				if (sprinting)
				{
					OnStopSprinting();
				}
				if (walking)
				{
					OnStopWalking();
				}
			}
			else if (InputManager.IM.GetButton("Sprint") && !shootBuffered)
			{
				if (!sprinting)
				{
					OnStartSprinting();
				}
				if (walking)
				{
					OnStopWalking();
				}
				if (crouching && AbleToStand())
				{
					Stand();
				}
			}
			else
			{
				if (sprinting)
				{
					OnStopSprinting();
				}
				if (!walking)
				{
					OnStartWalking();
				}
			}
			bool flag = false;
			Ray ray = playerCam.ViewportPointToRay((Vector3.right + Vector3.up) * 0.5f);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray.origin, ray.direction, out hitInfo, 2f, bulletMask))
			{
				IInteractable component = hitInfo.collider.GetComponent<IInteractable>();
				if (component != null)
				{
					flag = true;
					GameSetup.gameSetup.interactText.text = component.GetInteractMessage();
					interactPrompting = true;
					GameSetup.gameSetup.canvasAnim.SetBool("interacting", interactPrompting);
					if (InputManager.IM.GetButtonDown("Interact"))
					{
						component.Interact();
					}
				}
			}
			if (!flag && interactPrompting)
			{
				interactPrompting = false;
				GameSetup.gameSetup.canvasAnim.SetBool("interacting", interactPrompting);
			}
			if (GameManager.gameManager.GetGameModeInfo().floorItems)
			{
				if (InputManager.IM.GetButtonDown("Drop") && ownedItems[itemIndex].info.canThrowAway && (GameManager.gameManager.gameMode != GameMode.Infection || !photonPlayer.infected))
				{
					DropItem(itemIndex);
					RelieveItemOwnership(ownedItems[itemIndex].info.itemID);
				}
				if (InputManager.IM.GetButtonDown("DropAmmo") && ownedItems[itemIndex].info.itemType == ItemType.Gun && !GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
				{
					DropAmmo(ownedItems[itemIndex].info.ammoType);
				}
				if (InputManager.IM.GetButtonDown("Shop") && photonPlayer.tttteam == TTTTeam.Traitor)
				{
					GameMenuManager.menuManager.ToggleMenu("shop");
				}
			}
			Vector3 target = vector3 * (((!sprinting) ? walkSpeed.GetValue() : runSpeed.GetValue()) * ((!aiming && !reloading) ? 1f : 0.75f) * ((!crouching) ? 1f : 0.5f) + ownedItems[itemIndex].info.moveSpeedAdditive);
			if (InputManager.IM.GetButtonDown("Jump") && grounded)
			{
				if (crouching && AbleToStand())
				{
					Stand();
				}
				rb.AddForce(base.transform.up * jumpForce);
				UserDataManager.Instance.IncrementProperty("JUMPS");
			}
			moveAmount = Vector3.SmoothDamp(moveAmount, target, ref smoothMoveVelocity, smoothTime);
			if (InputManager.IM.GetButtonDown("Crouch"))
			{
				if (crouching && AbleToStand())
				{
					Stand();
				}
				else
				{
					Crouch();
				}
			}
			if (ownedItems[itemIndex].info.itemType == ItemType.Gun)
			{
				if (InputManager.IM.GetButtonDown("Aim"))
				{
					Aim();
				}
				else if (InputManager.IM.GetButtonUp("Aim"))
				{
					StopAiming();
				}
			}
			if (InputManager.IM.GetButtonDown("ChangeFireMode") && ownedItems[itemIndex].info.itemType == ItemType.Gun && ownedItems[itemIndex].info.fireModes.Length > 1)
			{
				PV.RPC("RPC_PlaySound", RpcTarget.All, "firemode_change");
				ownedItems[itemIndex].fireModeIndex++;
				if (ownedItems[itemIndex].fireModeIndex >= ownedItems[itemIndex].info.fireModes.Length)
				{
					ownedItems[itemIndex].fireModeIndex = 0;
				}
				UpdateFireModeUI();
			}
			if ((ownedItems[itemIndex].info.itemType == ItemType.Gun || ownedItems[itemIndex].info.itemType == ItemType.Melee) && InputManager.IM.GetButton("Attack") && !reloading && Time.time >= ownedItems[itemIndex].nextTimeToFire && !meleeing && Time.time >= nextTimeToShootAfterSwitchingWeapon)
			{
				if (ownedItems[itemIndex].info.itemType == ItemType.Gun)
				{
					if (ownedItems[itemIndex].ammo > 0)
					{
						if (sprinting)
						{
							OnStopSprinting();
						}
						if (ownedItems[itemIndex].info.fireModes[ownedItems[itemIndex].fireModeIndex] == FireMode.Auto)
						{
							Shoot();
						}
						else if (InputManager.IM.GetButtonDown("Attack"))
						{
							if (ownedItems[itemIndex].info.fireModes[ownedItems[itemIndex].fireModeIndex] == FireMode.Burst)
							{
								BurstShoot();
							}
							else
							{
								Shoot();
							}
						}
					}
					else
					{
						PV.RPC("RPC_PlaySound", RpcTarget.All, "dryfire");
					}
				}
				else if (!meleeing)
				{
					StartMelee();
				}
			}
			if (InputManager.IM.GetButtonDown("Melee") && !meleeing)
			{
				StartMelee();
			}
			if (InputManager.IM.GetButtonDown("Reload") && ownedItems[itemIndex].ammo < ownedItems[itemIndex].info.magazineSize && !meleeing && !reloading)
			{
				Reload();
			}
		}
		else
		{
			moveAmount = Vector3.SmoothDamp(moveAmount, Vector3.zero, ref smoothMoveVelocity, smoothTime);
		}
		ownedItems[itemIndex].currentRecoil = Mathf.Clamp(ownedItems[itemIndex].currentRecoil - ownedItems[itemIndex].info.recoilDecrease * ownedItems[itemIndex].currentRecoil * Time.deltaTime - ownedItems[itemIndex].info.recoilDecrease * Time.deltaTime * 0.2f, 0f, ownedItems[itemIndex].info.maxRecoil);
		playerCameraHolder.transform.localEulerAngles = Vector3.left * (verticalLookRotation + ownedItems[itemIndex].currentRecoil * 20f);
		float num;
		if (aiming)
		{
			num = ((!crouching || !grounded) ? ownedItems[itemIndex].info.aimedBulletSpread : Mathf.Max(ownedItems[itemIndex].info.aimedBulletSpread - 0.01f, 0f));
		}
		else
		{
			num = ownedItems[itemIndex].info.hipBulletSpread;
			if ((bool)rb)
			{
				num += Mathf.Min(rb.linearVelocity.magnitude * 0.02f, 0.1f);
			}
			num += Mathf.Min(moveAmount.magnitude * ((!sprinting) ? walkSpeed.GetValue() : runSpeed.GetValue()) * 0.002f, 0.1f);
			if (crouching && grounded)
			{
				num = Mathf.Max(num - 0.05f, 0f);
			}
			num *= ownedItems[itemIndex].info.motionSpreadMultiplier;
		}
		num += ownedItems[itemIndex].currentRecoil;
		bulletSpread = Mathf.Lerp(num, bulletSpread, 15f * Time.deltaTime);
		RenderCrosshair();
		if (Time.time >= lastHurtTime + regenDelay && currentHealth < 100f && GameManager.gameManager.GetGameModeInfo().naturalHealthRegen)
		{
			currentHealth += Time.deltaTime * regenSpeed;
			currentHealth = Mathf.Clamp(currentHealth, 0f, 100f);
			UpdateHealthGraphics();
		}
		GameSetup.gameSetup.healthAmountBackgroundImage.fillAmount = Mathf.Lerp(currentHealth / 100f, GameSetup.gameSetup.healthAmountBackgroundImage.fillAmount, 20f * Time.deltaTime);
		if (base.transform.position.y <= -25f && !dead)
		{
			Die(DeathType.Void);
		}
	}

	private void OnStopWalking()
	{
		walking = false;
		anim.SetBool("walking", false);
	}

	private void OnStartWalking()
	{
		walking = true;
		anim.SetBool("walking", true);
		anim.SetBool("sprinting", false);
	}

	private void OnStartSprinting()
	{
		sprinting = true;
		if (aiming)
		{
			StopAiming();
		}
		anim.SetBool("sprinting", true);
		anim.SetBool("walking", false);
	}

	private void OnStopSprinting()
	{
		sprinting = false;
		if (InputManager.IM.GetButton("Aim") && !aiming)
		{
			Aim();
		}
		anim.SetBool("shootBuffered", true);
		shootBuffered = true;
		CancelInvoke("EndShootBuffer");
		Invoke("EndShootBuffer", 0.5f);
		anim.SetBool("sprinting", false);
	}

	public void PickupItem(int itemID, object[] itemData = null)
	{
		if (!OwnsItem(itemID))
		{
			ObtainItem(itemID);
			if (itemData != null && ownedItems[ItemIndexByID(itemID)].info.itemType == ItemType.Gun)
			{
				ownedItems[ItemIndexByID(itemID)].ammo = (int)itemData[0];
				Debug.Log("Found Ammo: " + (int)itemData[0]);
			}
			RemoveInventorySlots();
			RenderInventorySlots();
		}
	}

	private void StartMelee()
	{
		meleeing = true;
		ownedItems[itemIndex].nextTimeToFire = Time.time + ownedItems[itemIndex].info.fireRate;
		if (reloading)
		{
			CancelReload();
		}
		if (ownedItems[itemIndex].info.itemType == ItemType.Melee)
		{
			anim.SetTrigger("shoot");
		}
		else
		{
			anim.SetTrigger("melee");
		}
		CancelInvoke("CooldownMelee");
		Invoke("CooldownMelee", 1f);
		Invoke("Melee", 0.25f);
	}

	private void CancelMelee()
	{
		meleeing = false;
		CancelInvoke("CooldownMelee");
		CancelInvoke("Melee");
	}

	private void DropItem(int index)
	{
		object[] array = new object[1];
		if (ownedItems[index].info.itemType == ItemType.Gun)
		{
			array[0] = ownedItems[index].ammo;
			PV.RPC("RPC_InstantiateSceneObject", RpcTarget.MasterClient, "FloorItems\\" + ownedItems[index].info.floorItemName, playerCamera.transform.position + playerCamera.transform.forward, playerCamera.transform.rotation, array);
		}
		else
		{
			PV.RPC("RPC_InstantiateSceneObject", RpcTarget.MasterClient, "FloorItems\\" + ownedItems[index].info.floorItemName, playerCamera.transform.position + playerCamera.transform.forward, playerCamera.transform.rotation, array);
		}
	}

	private void DropAmmo(AmmoType ammoType)
	{
		object[] array = new object[1];
		int num = MathPing(Mathf.Max(ownedItems[itemIndex].info.magazineSize, 10), ammoReserve[ownedItems[itemIndex].info.ammoType]);
		ammoReserve[ownedItems[itemIndex].info.ammoType] -= num;
		array[0] = num;
		PV.RPC("RPC_InstantiateSceneObject", RpcTarget.MasterClient, "FloorItems\\" + GameManager.gameManager.ammoFloorItemNames[ammoType], playerCamera.transform.position + playerCamera.transform.forward, playerCamera.transform.rotation, array);
	}

	private bool AbleToStand()
	{
		Collider[] array = Physics.OverlapBox(playerCameraHolder.transform.position + Vector3.up * 0.7f, new Vector3(0.5f, 0.5f, 0.2f), Quaternion.identity, bulletMask);
		return array.Length <= 2;
	}

	private void ObtainItem(int itemID)
	{
		Item[] array = allItems;
		foreach (Item item in array)
		{
			if (item.info.itemID == itemID)
			{
				ownedItems.Add(item);
				break;
			}
		}
	}

	private void RelieveItemOwnership(int itemID)
	{
		int num = 0;
		foreach (Item ownedItem in ownedItems)
		{
			if (ownedItem.info.itemID == itemID)
			{
				removeItemQueue.Add(ownedItem);
			}
			num++;
		}
	}

	private void LateUpdate()
	{
		if (!PV.IsMine)
		{
			return;
		}
		bool flag = false;
		if (removeItemQueue.Count > 0)
		{
			if (GameManager.gameManager.gameMode == GameMode.GunGame)
			{
				SelectItem(0, true);
			}
			else
			{
				SelectItem(Mathf.Max(0, ownedItems.Count - 2));
			}
		}
		while (removeItemQueue.Count > 0)
		{
			ownedItems.Remove(removeItemQueue[0]);
			removeItemQueue.RemoveAt(0);
			flag = true;
		}
		if (flag)
		{
			RemoveInventorySlots();
			RenderInventorySlots();
			if (GameManager.gameManager.gameMode == GameMode.GunGame)
			{
				SelectItem(1, true);
			}
			else
			{
				SelectItem(Mathf.Max(0, ownedItems.Count - 1));
			}
		}
	}

	private void EndShootBuffer()
	{
		anim.SetBool("shootBuffered", false);
		shootBuffered = false;
	}

	private void BurstShoot()
	{
		StartCoroutine(BurstFire(ownedItems[itemIndex].info.fireRate, 3));
	}

	private IEnumerator BurstFire(float interval, int count)
	{
		if (Time.time < ownedItems[itemIndex].nextTimeToBurstFire || burstFiring)
		{
			yield break;
		}
		burstFiring = true;
		Item fireItem = ownedItems[itemIndex];
		for (int i = 0; i < count; i++)
		{
			if (fireItem != ownedItems[itemIndex])
			{
				break;
			}
			if (ownedItems[itemIndex].ammo <= 0)
			{
				break;
			}
			Shoot();
			yield return new WaitForSeconds(interval);
		}
		burstFiring = false;
		fireItem.nextTimeToBurstFire = Time.time + 0.25f;
	}

	private void Shoot()
	{
		anim.SetTrigger("shoot");
		anim.SetBool("shootBuffered", true);
		shootBuffered = true;
		CancelInvoke("EndShootBuffer");
		Invoke("EndShootBuffer", 0.25f);
		PV.RPC("RPC_PlaySound", RpcTarget.All, ownedItems[itemIndex].info.soundName);
		ownedItems[itemIndex].nextTimeToFire = Time.time + ownedItems[itemIndex].info.fireRate;
		cs.ShakeOnce((ownedItems[itemIndex].currentRecoil + 0.08f) * ownedItems[itemIndex].info.cameraShakeMultiplier, ownedItems[itemIndex].info.cameraShakeRoughness, 0.1f, 0.2f);
		ownedItems[itemIndex].ammo--;
		UpdateAmmoGraphics();
		Ray ray = playerCam.ViewportPointToRay((Vector3.right + Vector3.up) * 0.5f);
		UserDataManager.Instance.IncrementProperty("SHOTS_FIRED");
		Dictionary<PlayerController, float> dictionary = new Dictionary<PlayerController, float>();
		for (int i = 0; i < ownedItems[itemIndex].info.shotCount; i++)
		{
			Ray ray2 = ray;
			Vector3 direction = ray2.direction;
			Vector3 vector = Random.insideUnitSphere * bulletSpread;
			direction.x += vector.x;
			direction.y += vector.y;
			direction.z += vector.z;
			ray2.direction = direction;
			PV.RPC("RPC_TracerBullet", RpcTarget.All, OwnedToGlobalItem(itemIndex), ray2.direction, i);
			PV.RPC("RPC_MuzzleFlash", RpcTarget.All, OwnedToGlobalItem(itemIndex));
			RaycastHit hitInfo;
			if (!Physics.Raycast(ray2.origin, ray2.direction, out hitInfo, 500f, bulletMask))
			{
				continue;
			}
			if (hitInfo.collider.CompareTag("Player"))
			{
				PlayerController componentInParent = hitInfo.collider.GetComponentInParent<PlayerController>();
				if (componentInParent != this)
				{
					if (dictionary.ContainsKey(componentInParent))
					{
						dictionary[componentInParent] += ownedItems[itemIndex].info.damage;
					}
					else
					{
						dictionary.Add(componentInParent, ownedItems[itemIndex].info.damage);
					}
				}
			}
			PV.RPC("RPC_InstantiateBulletHit", RpcTarget.All, hitInfo.point, hitInfo.normal);
		}
		ownedItems[itemIndex].currentRecoil += ownedItems[itemIndex].info.recoil;
		bool flag = false;
		foreach (PlayerController key in dictionary.Keys)
		{
			float num = Vector3.Distance(base.transform.position, key.transform.position);
			float num2 = dictionary[key];
			if (num >= ownedItems[itemIndex].info.damageFalloffStart)
			{
				num2 = Mathf.Max(0f, num2 - (num - ownedItems[itemIndex].info.damageFalloffStart) * ownedItems[itemIndex].info.damageFalloff);
			}
			num2 *= ((GameManager.gameManager.gameMode != GameMode.TTT) ? 1f : ((float)photonPlayer.tttkarma / 100f));
			num2 = Mathf.Ceil(num2);
			if (num2 > 0f)
			{
				flag = true;
				PV.RPC("RPC_DealDamage", RpcTarget.All, key.photonPlayer.PV.ViewID, photonPlayer.PV.ViewID, num2, base.transform.position, ownedItems[itemIndex].info.itemName);
			}
		}
		if (flag)
		{
			SuccessfulHit();
		}
		if (ownedItems[itemIndex].info.breaksOnUse)
		{
			RelieveItemOwnership(ownedItems[itemIndex].info.itemID);
		}
		if (ownedItems[itemIndex].ammo == 0)
		{
			Reload();
		}
	}

	private int OwnedToGlobalItem(int ownedIndex)
	{
		int num = 0;
		Item[] array = allItems;
		foreach (Item item in array)
		{
			if (ownedItems[ownedIndex].info.itemID == item.info.itemID)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private void Melee()
	{
		if (!PV.IsMine)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(meleeBox.bounds.center, meleeBox.bounds.extents);
		if (array.Length == 0)
		{
			PV.RPC("RPC_PlaySound", RpcTarget.All, "melee_miss");
			return;
		}
		bool flag = false;
		List<PlayerController> list = new List<PlayerController>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!collider.CompareTag("Player"))
			{
				continue;
			}
			PlayerController componentInParent = collider.GetComponentInParent<PlayerController>();
			if (componentInParent != this && !list.Contains(componentInParent))
			{
				flag = true;
				list.Add(componentInParent);
				SuccessfulHit();
				PV.RPC("RPC_DealDamage", RpcTarget.All, componentInParent.GetComponentInParent<PlayerController>().photonPlayer.PV.ViewID, photonPlayer.PV.ViewID, ((ownedItems[itemIndex].info.itemType != ItemType.Melee) ? 40f : ownedItems[itemIndex].info.damage) * ((GameManager.gameManager.gameMode != GameMode.TTT) ? 1f : ((float)photonPlayer.tttkarma / 100f)), base.transform.position, ownedItems[itemIndex].info.itemName);
				if (ownedItems[itemIndex].info.breaksOnUse)
				{
					RelieveItemOwnership(ownedItems[itemIndex].info.itemID);
				}
			}
		}
		if (!flag)
		{
			PV.RPC("RPC_PlaySound", RpcTarget.All, "melee_miss");
		}
	}

	private void RenderCrosshair()
	{
		float num = 230f;
		Vector2 anchoredPosition = crosshair.top.anchoredPosition;
		anchoredPosition.y = bulletSpread * num + 1f;
		crosshair.top.anchoredPosition = anchoredPosition;
		Vector2 anchoredPosition2 = crosshair.bottom.anchoredPosition;
		anchoredPosition2.y = (0f - bulletSpread) * num - 1f;
		crosshair.bottom.anchoredPosition = anchoredPosition2;
		Vector2 anchoredPosition3 = crosshair.right.anchoredPosition;
		anchoredPosition3.x = bulletSpread * num + 1f;
		crosshair.right.anchoredPosition = anchoredPosition3;
		Vector2 anchoredPosition4 = crosshair.left.anchoredPosition;
		anchoredPosition4.x = (0f - bulletSpread) * num - 1f;
		crosshair.left.anchoredPosition = anchoredPosition4;
	}

	public void GainReserveAmmo(AmmoType ammoType, int amount)
	{
		ammoReserve[ammoType] += amount;
		UpdateAmmoGraphics();
	}

	private void SuccessfulHit()
	{
		audio.Play("successful_hit");
		GameSetup.gameSetup.canvasAnim.SetTrigger("hit");
	}

	private void Crouch()
	{
		crouching = true;
		anim.SetBool("crouching", true);
	}

	private void Stand()
	{
		crouching = false;
		anim.SetBool("crouching", false);
	}

	private void Aim()
	{
		aiming = true;
		HideCrosshair();
		HideMiddleCrosshair();
		if (reloading)
		{
			CancelReload();
		}
		anim.SetBool("aiming", true);
	}

	private void StopAiming()
	{
		aiming = false;
		if (!ownedItems[itemIndex].info.hideCrosshair)
		{
			ShowCrosshair();
		}
		anim.SetBool("aiming", false);
	}

	public bool OwnsItem(int itemID)
	{
		foreach (Item ownedItem in ownedItems)
		{
			if (ownedItem.info.itemID == itemID)
			{
				return true;
			}
		}
		return false;
	}

	[PunRPC]
	public void RPC_SetPhotonPlayer(int photonPlayerViewID)
	{
		photonPlayer = PhotonView.Find(photonPlayerViewID).GetComponent<PhotonPlayer>();
	}

	[PunRPC]
	private void RPC_TracerBullet(int item, Vector3 direction, int index)
	{
		allItems[item].tracerParticles[index].transform.rotation = Quaternion.LookRotation(direction);
		allItems[item].tracerParticles[index].Play();
		allItems[item].muzzleFlassParticles.Play();
	}

	[PunRPC]
	private void RPC_MuzzleFlash(int item)
	{
		allItems[item].muzzleFlassParticles.Play();
	}

	[PunRPC]
	private void RPC_PlaySound(string sound)
	{
		audio.Play(sound);
	}

	[PunRPC]
	private void RPC_InstantiateBulletHit(Vector3 point, Vector3 normal)
	{
		GameObject obj = Object.Instantiate(bulletHitPrefab, point, Quaternion.FromToRotation(Vector3.up, normal));
		if (GameManager.gameManager.gameMode != GameMode.TTT)
		{
			Object.Destroy(obj, 15f);
		}
	}

	[PunRPC]
	private void RPC_DealDamage(int targetViewID, int dealerViewID, float damage, Vector3 dealerPosition, string killedItemName)
	{
		PhotonView photonView = PhotonView.Find(targetViewID);
		if ((bool)photonView)
		{
			PlayerController avatarPlayerController = photonView.GetComponent<PhotonPlayer>().avatarPlayerController;
			if ((bool)avatarPlayerController)
			{
				avatarPlayerController.TakeDamage(damage, dealerViewID, dealerPosition, killedItemName);
			}
		}
	}

	private IEnumerator IndividualReload(float individualReloadTime)
	{
		float timeToFinishInterval = Time.time + individualReloadTime;
		while (ownedItems[itemIndex].ammo < ownedItems[itemIndex].info.magazineSize)
		{
			if ((ammoReserve[ownedItems[itemIndex].info.ammoType] <= 0 && !GameManager.gameManager.GetGameModeInfo().infiniteAmmo) || cancelReload)
			{
				cancelReload = false;
				break;
			}
			if (Time.time >= timeToFinishInterval)
			{
				timeToFinishInterval = Time.time + individualReloadTime;
				ownedItems[itemIndex].ammo++;
				if (!GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
				{
					ammoReserve[ownedItems[itemIndex].info.ammoType]--;
				}
				UpdateAmmoGraphics();
			}
			yield return null;
		}
		anim.SetBool("reloading", false);
		reloading = false;
		if (InputManager.IM.GetButton("Aim"))
		{
			Aim();
		}
	}

	private void Reload()
	{
		if (ammoReserve[ownedItems[itemIndex].info.ammoType] > 0 || GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
		{
			if (aiming)
			{
				StopAiming();
			}
			anim.ResetTrigger("shoot");
			anim.SetBool("reloading", true);
			reloading = true;
			if (ownedItems[itemIndex].info.individualReload)
			{
				StartCoroutine(IndividualReload(ownedItems[itemIndex].info.reloadTime));
			}
			else
			{
				Invoke("EndReload", ownedItems[itemIndex].info.reloadTime);
			}
		}
	}

	private void CancelReload()
	{
		reloading = false;
		anim.SetBool("reloading", false);
		if (ownedItems[itemIndex].info.individualReload)
		{
			cancelReload = true;
		}
		else
		{
			CancelInvoke("EndReload");
		}
	}

	private int MathPing(int num, int max)
	{
		if (num > max)
		{
			return max;
		}
		return num;
	}

	private void EndReload()
	{
		anim.SetBool("reloading", false);
		reloading = false;
		if (GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
		{
			ownedItems[itemIndex].ammo = ownedItems[itemIndex].info.magazineSize;
		}
		else
		{
			int num = ownedItems[itemIndex].info.magazineSize - ownedItems[itemIndex].ammo;
			ownedItems[itemIndex].ammo += MathPing(num, ammoReserve[ownedItems[itemIndex].info.ammoType]);
			ammoReserve[ownedItems[itemIndex].info.ammoType] -= MathPing(num, ammoReserve[ownedItems[itemIndex].info.ammoType]);
		}
		if (InputManager.IM.GetButton("Aim"))
		{
			Aim();
		}
		UpdateAmmoGraphics();
	}

	public void CooldownMelee()
	{
		meleeing = false;
	}

	private void UpdateAmmoGraphics()
	{
		GameSetup.gameSetup.ammoText.text = ownedItems[itemIndex].ammo + "/" + ownedItems[itemIndex].info.magazineSize;
		if (!GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
		{
			GameSetup.gameSetup.totalAmmoText.text = ammoReserve[ownedItems[itemIndex].info.ammoType].ToString();
		}
	}

	private void UpdateHealthGraphics()
	{
		float num = currentHealth / 100f;
		GameSetup.gameSetup.healthAmountImage.fillAmount = num;
		GameSetup.gameSetup.healthAmountImage.color = Color.Lerp(Color.red, Color.white, num);
		GameSetup.gameSetup.healthAmountText.text = Mathf.Ceil(currentHealth / 100f * 100f).ToString("0");
	}

	public void TakeDamage(float damage, int viewID, Vector3 damagerPosition, string killedItemName)
	{
		if (!PV.IsMine)
		{
			return;
		}
		PV.RPC("RPC_PlaySound", RpcTarget.All, "hurt");
		GameSetup.gameSetup.canvasAnim.SetTrigger("hurt");
		if (!GameManager.gameManager.GetGameModeInfo().noDamageOutOfTime || (GameManager.gameManager.gameStarted && !GameManager.gameManager.gameEnded))
		{
			lastHurtTime = Time.time;
			currentHealth -= damage;
			UpdateHealthGraphics();
			if (currentHealth <= 0f)
			{
				float f = Vector3.Distance(base.transform.position, damagerPosition);
				f = Mathf.Round(f);
				Die(DeathType.Player, new object[3] { viewID, killedItemName, f });
			}
		}
	}

	private void TakeEnvironmentDamage(float damage)
	{
		if (!PV.IsMine)
		{
			return;
		}
		PV.RPC("RPC_PlaySound", RpcTarget.All, "hurt");
		GameSetup.gameSetup.canvasAnim.SetTrigger("hurt");
		if (!GameManager.gameManager.GetGameModeInfo().noDamageOutOfTime || (GameManager.gameManager.gameStarted && !GameManager.gameManager.gameEnded))
		{
			lastHurtTime = Time.time;
			currentHealth -= damage;
			UpdateHealthGraphics();
			if (currentHealth <= 0f)
			{
				Die(DeathType.World);
			}
		}
	}

	[PunRPC]
	public void RPC_TakeDamage(float damage, int viewID, Vector3 damagerPosition, string killedItemName)
	{
		if (PV.IsMine)
		{
			TakeDamage(damage, viewID, damagerPosition, killedItemName);
		}
	}

	private void FixedUpdate()
	{
		if (!PV.IsMine || rb == null)
		{
			return;
		}
		if (grounded)
		{
			Collider[] array = Physics.OverlapBox(base.transform.position - Vector3.up * 0.5f, 0.5f * Vector3.one);
			if (array.Length < 5)
			{
				grounded = false;
			}
		}
		if (climbing)
		{
			Collider[] array2 = Physics.OverlapCapsule(base.transform.position - Vector3.up * 0.5f, base.transform.position + Vector3.up * 0.5f, 0.5f);
			if (array2.Length < 5)
			{
				StopClimbing();
			}
		}
		if (climbing)
		{
			rb.MovePosition(rb.position + (base.transform.TransformDirection(Vector3.right * moveAmount.x + Vector3.up * moveAmount.y + Vector3.forward * ((!(moveAmount.z > 1f)) ? 0f : moveAmount.z)) + Vector3.up * moveAmount.z) * Time.fixedDeltaTime);
			rb.linearVelocity = Vector3.zero;
		}
		else
		{
			rb.MovePosition(rb.position + base.transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
		}
	}

	private void Die(DeathType deathType, object[] deathInfo = null)
	{
		if (dead)
		{
			return;
		}
		if (interactPrompting)
		{
			interactPrompting = false;
			GameSetup.gameSetup.canvasAnim.SetBool("interacting", interactPrompting);
		}
		dead = true;
		PhotonView photonView = PV;
		if (deathInfo != null)
		{
			photonView = PhotonView.Find((int)deathInfo[0]);
			PhotonPlayer component = photonView.GetComponent<PhotonPlayer>();
			component.PV.RPC("RPC_GetKill", RpcTarget.All, photonPlayer.PV.ViewID);
			PlayerController avatarPlayerController = component.avatarPlayerController;
		}
		if (deathType == DeathType.Void || deathType == DeathType.World)
		{
			UserDataManager.Instance.IncrementProperty("ENVIRONMENT_DEATHS");
		}
		GameSetup.gameSetup.itemInfoPanel.SetActive(false);
		if (GameManager.gameManager.GetGameModeInfo().dropItemsOnDeath && (GameManager.gameManager.gameMode != GameMode.Infection || !photonPlayer.infected))
		{
			int num = 0;
			foreach (Item ownedItem in ownedItems)
			{
				if (ownedItem.info.canThrowAway)
				{
					DropItem(num);
				}
				num++;
			}
		}
		if (GameManager.gameManager.GetGameModeInfo().bodyOnDeath)
		{
			object[] array = new object[5]
			{
				PhotonNetwork.NickName,
				photonPlayer.tttteam,
				currentSkinName,
				null,
				null
			};
			if (deathInfo != null)
			{
				array[3] = deathInfo[1];
				array[4] = deathInfo[2];
			}
			PV.RPC("RPC_InstantiateSceneObject", RpcTarget.MasterClient, "PlayerBody", base.transform.position, base.transform.rotation, array);
		}
		if (GameManager.gameManager.GetGameModeInfo().sendGameInfo)
		{
			switch (deathType)
			{
			case DeathType.Void:
				photonPlayer.SendGlobalGameInfo("<b>" + PhotonNetwork.NickName + "</b> fell into the void");
				break;
			case DeathType.Player:
				if (GameManager.gameManager.gameMode == GameMode.Infection)
				{
					if (photonPlayer.infected)
					{
						photonPlayer.SendGlobalGameInfo(string.Format("<b><color=#ff8989>{0}</color></b> was killed by <b>{1}</b> from {2} meters away with <b>{3}</b>", PhotonNetwork.NickName, photonView.Owner.NickName, deathInfo[2], deathInfo[1]));
					}
					else
					{
						photonPlayer.SendGlobalGameInfo(string.Format("<b><color=green>{0}</color></b> was infected by <b>{1}</b>", PhotonNetwork.NickName, photonView.Owner.NickName));
					}
				}
				else
				{
					photonPlayer.SendGlobalGameInfo(string.Format("<b><color=#ff8989>{0}</color></b> was killed by <b>{1}</b> from {2} meters away with <b>{3}</b>", PhotonNetwork.NickName, photonView.Owner.NickName, deathInfo[2], deathInfo[1]));
				}
				break;
			case DeathType.Fall:
				photonPlayer.SendGlobalGameInfo("<b>" + PhotonNetwork.NickName + "</b> fell to their death");
				break;
			default:
				photonPlayer.SendGlobalGameInfo("<b>" + PhotonNetwork.NickName + "</b> was killed by the environment");
				break;
			}
		}
		Respawn();
	}

	public void UpdateGunGameItem()
	{
		foreach (Item ownedItem in ownedItems)
		{
			if (ownedItem.info.canThrowAway)
			{
				RelieveItemOwnership(ownedItem.info.itemID);
			}
		}
		ObtainItem(MultiplayerSettings.multiplayerSettings.gunGameWeapons[photonPlayer.currentGunGameItem].itemID);
	}

	[PunRPC]
	private void RPC_InstantiateSceneObject(string name, Vector3 position, Quaternion rotation, object[] data)
	{
		PhotonNetwork.InstantiateSceneObject(Path.Combine("PhotonPrefabs", name), position, rotation, 0, data);
	}

	[PunRPC]
	private void RPC_DestroyObject(int viewID)
	{
		PhotonNetwork.Destroy(PhotonView.Find(viewID));
	}

	private void Respawn()
	{
		photonPlayer.Respawn();
	}

	private int ItemIndexByID(int itemID)
	{
		int num = 0;
		foreach (Item ownedItem in ownedItems)
		{
			if (ownedItem.info.itemID == itemID)
			{
				return num;
			}
			num++;
		}
		return 0;
	}

	private void UpdateFireModeUI()
	{
		FireModeImage[] fireModeImages = GameSetup.gameSetup.fireModeImages;
		foreach (FireModeImage fireModeImage in fireModeImages)
		{
			if (ownedItems[itemIndex].info.fireModes.Contains(fireModeImage.fireMode))
			{
				fireModeImage.image.gameObject.SetActive(true);
			}
			else
			{
				fireModeImage.image.gameObject.SetActive(false);
			}
			if (ownedItems[itemIndex].info.fireModes.Length > 0)
			{
				if (ownedItems[itemIndex].info.fireModes[ownedItems[itemIndex].fireModeIndex] == fireModeImage.fireMode)
				{
					fireModeImage.image.color = Color.white;
				}
				else
				{
					fireModeImage.image.color = Color.grey;
				}
			}
		}
	}

	private void SelectItem(int index, bool repeat = false)
	{
		if (index < 0)
		{
			index = ownedItems.Count - 1;
		}
		else if (index >= ownedItems.Count)
		{
			index = 0;
		}
		if (index == itemIndex && !repeat)
		{
			return;
		}
		nextTimeToShootAfterSwitchingWeapon = Time.time + switchingWeaponsShootCooldown;
		anim.ResetTrigger("shoot");
		if (reloading)
		{
			CancelReload();
		}
		if (meleeing)
		{
			CancelMelee();
		}
		if (InputManager.IM.GetButton("Aim"))
		{
			Aim();
		}
		else if (aiming)
		{
			StopAiming();
		}
		itemIndex = index;
		int num = 0;
		foreach (Item ownedItem in ownedItems)
		{
			if (num == index)
			{
				if (ownedItem.info.hideCrosshair)
				{
					HideCrosshair();
					if (ownedItem.info.hideMiddleCrosshair)
					{
						HideMiddleCrosshair();
					}
				}
				else if (!aiming)
				{
					ShowCrosshair();
				}
				if (ownedItem.info.animationLayers.Length != 0)
				{
					int[] animationLayers = ownedItem.info.animationLayers;
					foreach (int layerIndex in animationLayers)
					{
						anim.SetLayerWeight(layerIndex, 1f);
						anim.SetTrigger(ownedItem.info.enterTriggerName);
					}
				}
				if (ownedItem.info.itemType != ItemType.Holstered)
				{
					PV.RPC("RPC_ActivateItem", RpcTarget.All, OwnedToGlobalItem(num));
				}
			}
			else
			{
				if (ownedItem.info.animationLayers.Length != 0)
				{
					int[] animationLayers2 = ownedItem.info.animationLayers;
					foreach (int layerIndex2 in animationLayers2)
					{
						anim.SetLayerWeight(layerIndex2, 0f);
					}
				}
				if (ownedItem.info.itemType != ItemType.Holstered)
				{
					PV.RPC("RPC_DeactivateItem", RpcTarget.All, OwnedToGlobalItem(num));
				}
			}
			num++;
		}
		if (ownedItems[itemIndex].info.itemType == ItemType.Gun)
		{
			GameSetup.gameSetup.ammoText.gameObject.SetActive(true);
			if (!GameManager.gameManager.GetGameModeInfo().infiniteAmmo)
			{
				GameSetup.gameSetup.totalAmmoText.gameObject.SetActive(true);
			}
		}
		else
		{
			GameSetup.gameSetup.ammoText.gameObject.SetActive(false);
			GameSetup.gameSetup.totalAmmoText.gameObject.SetActive(false);
			GameSetup.gameSetup.itemInfoPanel.SetActive(false);
		}
		if (ownedItems[itemIndex].info.itemType != ItemType.Holstered && InputManager.IM.GetButton("Inspect"))
		{
			GameSetup.gameSetup.itemInfoPanel.SetActive(true);
		}
		RemoveInventorySlots();
		RenderInventorySlots();
		UpdateAmmoGraphics();
		UpdateItemInfo();
		UpdateFireModeUI();
	}

	private void HideCrosshair()
	{
		crosshair.top.gameObject.SetActive(false);
		crosshair.bottom.gameObject.SetActive(false);
		crosshair.left.gameObject.SetActive(false);
		crosshair.right.gameObject.SetActive(false);
	}

	private void HideMiddleCrosshair()
	{
		crosshair.center.gameObject.SetActive(false);
	}

	private void ShowCrosshair()
	{
		crosshair.top.gameObject.SetActive(true);
		crosshair.bottom.gameObject.SetActive(true);
		crosshair.left.gameObject.SetActive(true);
		crosshair.right.gameObject.SetActive(true);
		crosshair.center.gameObject.SetActive(true);
	}

	private void RemoveInventorySlots()
	{
		foreach (Transform item in GameSetup.gameSetup.inventoryContainer)
		{
			Object.Destroy(item.gameObject);
		}
	}

	private void RenderInventorySlots()
	{
		int num = 0;
		foreach (Item ownedItem in ownedItems)
		{
			GameObject gameObject = Object.Instantiate(GameSetup.gameSetup.inventorySlotPrefab, GameSetup.gameSetup.inventoryContainer);
			gameObject.transform.GetChild(0).GetComponent<Text>().text = ownedItem.info.itemName;
			if (num == itemIndex)
			{
				gameObject.GetComponent<Image>().color = Color.white;
			}
			num++;
		}
	}

	[PunRPC]
	private void RPC_ActivateItem(int item)
	{
		allItems[item].itemObj.SetActive(true);
	}

	[PunRPC]
	private void RPC_DeactivateItem(int item)
	{
		allItems[item].itemObj.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!climbing && other.CompareTag("Climbable"))
		{
			StartClimbing();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!climbing && other.CompareTag("Climbable"))
		{
			StartClimbing();
		}
		else if (other.CompareTag("DamagePlayer"))
		{
			TakeEnvironmentDamage(Time.deltaTime * 10f);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (climbing)
		{
			StopClimbing();
		}
	}

	private void StartClimbing()
	{
		climbing = true;
		rb.useGravity = false;
	}

	private void StopClimbing()
	{
		climbing = false;
		rb.useGravity = true;
	}
}
