using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISelectCycle : MonoBehaviour
{
	private EventSystem system;

	private void Start()
	{
		system = EventSystem.current;
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Tab) || system.currentSelectedGameObject == null)
		{
			return;
		}
		Selectable selectable = ((!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) ? system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown() : system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp());
		if (selectable != null)
		{
			InputField component = selectable.GetComponent<InputField>();
			if (component != null)
			{
				component.OnPointerClick(new PointerEventData(system));
			}
			system.SetSelectedGameObject(selectable.gameObject, new BaseEventData(system));
		}
	}
}
