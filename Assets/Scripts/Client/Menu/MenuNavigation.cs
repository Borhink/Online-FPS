using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class MenuNavigation : MonoBehaviour {
	public static MenuNavigation instance = null;

	private EventSystem		_system;
	private Selectable[]	_selectable;
	private int				_index;
	private bool			_first = true;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
        	_system = EventSystem.current;
		}
		else
			Destroy(this);
	}

    void Start ()
    {
    }

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (_first)
			{
				Refresh();
				_first = false;
			}
			else
				NextSelectable();
		}
	}

	public void Refresh()
	{
		_selectable = FindObjectsOfType<Selectable>()
			.OrderByDescending(s => s.gameObject.transform.position.y)
			.ThenBy(s => s.gameObject.transform.position.x).ToArray();
		_index = -1;
		NextSelectable();
	}

	public void NextSelectable()
	{
		if (_selectable != null && _selectable.Length > 0)
		{
			_index++;
			if (_index >= _selectable.Length)
				_index = 0;
			_system.SetSelectedGameObject(_selectable[_index].gameObject, new BaseEventData(_system));

			InputField inputfield = _system.currentSelectedGameObject.GetComponent<InputField>();
			if (inputfield != null)
				inputfield.OnPointerClick(new PointerEventData(_system));
		}
	}
}
