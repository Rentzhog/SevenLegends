using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private void Awake() {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), transform.parent.GetComponent<Collider2D>());
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        transform.parent.GetComponent<PlayerCombat>().OnHitboxEnter(GetComponent<Collider2D>(), collision);
    }
}
