using UnityEngine;

namespace Photon.Pun
{
	[AddComponentMenu("Photon Networking/Photon Transform View")]
	[HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
	[RequireComponent(typeof(PhotonView))]
	public class PhotonTransformView : MonoBehaviour, IPunObservable
	{
		private float m_Distance;

		private float m_Angle;

		private PhotonView m_PhotonView;

		private Vector3 m_Direction;

		private Vector3 m_NetworkPosition;

		private Vector3 m_StoredPosition;

		private Quaternion m_NetworkRotation;

		public bool m_SynchronizePosition = true;

		public bool m_SynchronizeRotation = true;

		public bool m_SynchronizeScale;

		public void Awake()
		{
			m_PhotonView = GetComponent<PhotonView>();
			m_StoredPosition = base.transform.position;
			m_NetworkPosition = Vector3.zero;
			m_NetworkRotation = Quaternion.identity;
		}

		public void Update()
		{
			if (!m_PhotonView.IsMine)
			{
				base.transform.position = Vector3.MoveTowards(base.transform.position, m_NetworkPosition, m_Distance * (1f / (float)PhotonNetwork.SerializationRate));
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, m_NetworkRotation, m_Angle * (1f / (float)PhotonNetwork.SerializationRate));
			}
		}

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.IsWriting)
			{
				if (m_SynchronizePosition)
				{
					m_Direction = base.transform.position - m_StoredPosition;
					m_StoredPosition = base.transform.position;
					stream.SendNext(base.transform.position);
					stream.SendNext(m_Direction);
				}
				if (m_SynchronizeRotation)
				{
					stream.SendNext(base.transform.rotation);
				}
				if (m_SynchronizeScale)
				{
					stream.SendNext(base.transform.localScale);
				}
				return;
			}
			if (m_SynchronizePosition)
			{
				m_NetworkPosition = (Vector3)stream.ReceiveNext();
				m_Direction = (Vector3)stream.ReceiveNext();
				float num = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
				m_NetworkPosition += m_Direction * num;
				m_Distance = Vector3.Distance(base.transform.position, m_NetworkPosition);
			}
			if (m_SynchronizeRotation)
			{
				m_NetworkRotation = (Quaternion)stream.ReceiveNext();
				m_Angle = Quaternion.Angle(base.transform.rotation, m_NetworkRotation);
			}
			if (m_SynchronizeScale)
			{
				base.transform.localScale = (Vector3)stream.ReceiveNext();
			}
		}
	}
}
