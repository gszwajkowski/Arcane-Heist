using UnityEngine;
public class TestHealth : MonoBehaviour, IDamageable {
    public void TakeDamage(float amount) => Debug.Log($"Player hit for {amount} damage!");
}
