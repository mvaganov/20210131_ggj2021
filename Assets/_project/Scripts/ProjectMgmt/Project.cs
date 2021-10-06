using NonStandard.Extension;
using System;
using System.Collections.Generic;

public class Project
{
	public List<Task> tasks;

	private List<Task> taskListing = new List<Task>();

	public void RecalculateTaskListing() {
		if (taskListing == null) { taskListing = new List<Task>(); }
		taskListing.Clear();
		AddSubTasksToTaskList(taskListing, tasks, true);
	}
	public static void AddSubTasksToTaskList(List<Task> taskListingToAddTo, List<Task> childList, bool recurse) {
		List<Task> recurseIntoThese = recurse ? new List<Task>() : null;
		for(int i = 0; i < childList.Count; ++i) {
			Task t = childList[i];
			int index = taskListingToAddTo.IndexOf(t);
			if (index >= 0) continue;
			taskListingToAddTo.Add(t);
			if (recurse) { recurseIntoThese.Add(t); }
		}
		if (recurse) {
			for(int i = 0; i < recurseIntoThese.Count; ++i) {
				Task t = recurseIntoThese[i];
				if (t.reqFinish != null) AddSubTasksToTaskList(taskListingToAddTo, t.reqFinish, true);
				if (t.reqStart != null) AddSubTasksToTaskList(taskListingToAddTo, t.reqStart, true);
				if (t.parents != null) AddSubTasksToTaskList(taskListingToAddTo, t.parents, true);
				if (t.task != null) AddSubTasksToTaskList(taskListingToAddTo, t.task, true);
			}
		}
	}
	public Task FindById(string id) { return taskListing.Find(t => t.id == id); }
	public Task FindByName(string name) { string n = name.ToLower(); return taskListing.Find(t => {
		if (!t.variables.TryGetValue("name", out object _name)) return false;
		string t_name = _name as string; if (string.IsNullOrEmpty(t_name)) { return false; }
		return t_name.ToLower() == n;
	}); }
	public void CalculateClosest(string text, Func<int> count, Func<int,Task> element, Func<Task,string> nameOfElement, out List<KeyValuePair<Task,int>> results, int maxResults = -1) {
		results = new List<KeyValuePair<Task, int>>();
		int totalElements = count.Invoke();
		results.Capacity = Math.Min(maxResults, totalElements);
		int KvpComparer(KeyValuePair<Task, int> a, KeyValuePair<Task, int> b) => a.Value.CompareTo(b.Value);
		for (int i = 0; i < totalElements; ++i) {
			Task obj = element.Invoke(i);
			string other = nameOfElement.Invoke(obj);
			int dist = text.ComputeDistance(other);
			if (maxResults >= 0 && results.Count >= maxResults && results.Count > 0) {
				if (dist >= results[maxResults-1].Value) { continue; }
				results.RemoveAt(maxResults - 1);
			}
			KeyValuePair<Task, int> kvp = new KeyValuePair<Task, int>(obj, dist);
			results.BinarySearchInsert(kvp, KvpComparer);
		}
	}
	public Task GetTask(string nameOrId) {
		Task t = FindById(nameOrId);
		if (t != null) return t;
		t = FindByName(nameOrId);
		if (t != null) return t;
		CalculateClosest(nameOrId, () => taskListing.Count, i => taskListing[i], tsk=>tsk.id, out List<KeyValuePair<Task, int>> id_results, 1);
		CalculateClosest(nameOrId, () => taskListing.Count, i => taskListing[i], tsk=>tsk.variables.TryGetValue("name", out object n) ? n as string : null,
			out List<KeyValuePair<Task, int>> name_results, 1);
		if (id_results[0].Value <= name_results[0].Value) { t = id_results[0].Key; } else { t = name_results[0].Key; }
		return t;
	}
}
