using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamMember : MonoBehaviour
{
	public Action<Team> onJoinTeam;
	public Action<Team> onLeaveTeam;
}
