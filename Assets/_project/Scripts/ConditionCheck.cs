using System;
using UnityEngine;

public class ConditionCheck : MonoBehaviour
{
	public Func<bool> condition;
	public Action action;
	public bool IsActivate() { return condition != null ? condition.Invoke() : false; }
	public void DoActivateTrigger() { if (action != null) { action.Invoke(); }  }
	public void DoActivateTest() { if (IsActivate()) { DoActivateTrigger(); } }
}
