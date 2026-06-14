using System;
using UnityEngine;

[Serializable]
public class TaskDefinition
{
    [Tooltip("Unique id used by ObjectiveTriggerZone")]
    public string id;

    [Tooltip("Text shown in the checklist")]
    public string description;
}
