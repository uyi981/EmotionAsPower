using System;
using UnityEngine;

[Serializable]
public class AIActionData
{
    public NewAIController ai;
    public GameObject targetObject;
    public ActionState state;
}