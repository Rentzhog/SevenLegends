using UnityEngine;

[CreateAssetMenu(fileName = "AttackScriptableObjerct", menuName = "ScriptableObjects/Attack")]
public class AttackScriptableObject : ScriptableObject
{
    public AnimationClip attackAnimation;
    public float damage;
    public float knockbackForce;
    public float selfPushForce;
}
