using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New AI Behavior", menuName = "Scriptable Objects/AI/Behavior")]
public class AIBehaviour : ScriptableObject
{
    [SerializeField] private ActionConfiguration[] actions = new ActionConfiguration[0];
    public List<ActionConfiguration> Actions => actions.ToList<ActionConfiguration>();

    [System.Serializable]
    public class ActionConfiguration
    {
        public AIAction action;
        [Tooltip("Override the action's default priority. Leave as -1 to use default.")]
        public int priorityOverride = -1;
        public bool isEnabled = true;
    }

    private void OnValidate()
    {
        actions = actions.Where(a => a.action != null).ToArray();
    }
}
