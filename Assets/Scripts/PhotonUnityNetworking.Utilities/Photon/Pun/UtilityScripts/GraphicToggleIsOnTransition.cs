using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts
{
	[RequireComponent(typeof(Graphic))]
	public class GraphicToggleIsOnTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public Toggle toggle;

		private Graphic _graphic;

		public Color NormalOnColor = Color.white;

		public Color NormalOffColor = Color.black;

		public Color HoverOnColor = Color.black;

		public Color HoverOffColor = Color.black;

		private bool isHover;

		public void OnPointerEnter(PointerEventData eventData)
		{
			isHover = true;
			_graphic.color = ((!toggle.isOn) ? HoverOffColor : HoverOnColor);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			isHover = false;
			_graphic.color = ((!toggle.isOn) ? NormalOffColor : NormalOnColor);
		}

		public void OnEnable()
		{
			_graphic = GetComponent<Graphic>();
			OnValueChanged(toggle.isOn);
			toggle.onValueChanged.AddListener(OnValueChanged);
		}

		public void OnDisable()
		{
			toggle.onValueChanged.RemoveListener(OnValueChanged);
		}

		public void OnValueChanged(bool isOn)
		{
			_graphic.color = (isOn ? ((!isHover) ? HoverOnColor : HoverOnColor) : ((!isHover) ? NormalOffColor : NormalOffColor));
		}
	}
}
