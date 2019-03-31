using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventManager : MonoBehaviour 
{

	private HashSet<string> events = new HashSet<string>();
	private Dictionary<string,float> eventLastFired = new Dictionary<string,float>();
	private float interval = 0.25f;

	public bool RequestEvent(string eventName)
	{
		if (events.Contains(eventName))
		{
			events.Remove(eventName);
			return true;
		}

		return false;
	}

	public void NewEvent(string eventName)
	{
		if (!eventLastFired.ContainsKey(eventName))
		{
			eventLastFired.Add(eventName,Time.time);
			events.Add(eventName);
		}
		else if (Mathf.Abs(eventLastFired[eventName] - Time.time) > interval)
		{
			events.Add(eventName);
			eventLastFired[eventName] = Time.time;
		}
	}
}
