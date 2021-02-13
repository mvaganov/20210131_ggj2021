using System;
using UnityEngine;

public class ConditionCheck : MonoBehaviour
{
	public Func<bool> condition;
	public Action action;
	public bool IsActivate() { return condition.Invoke(); }
	public void DoActivateTrigger() { action.Invoke(); }
	public void DoActivateTest() { if (IsActivate()) { DoActivateTrigger(); } }
}
