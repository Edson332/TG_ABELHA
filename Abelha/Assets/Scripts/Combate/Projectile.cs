// Scripts/CombatSystem/Projectile.cs
using UnityEngine;
public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage;
    public Combatant3D target;

    void Update()
    {
        if (target == null || !target.IsAlive())
        {
            Destroy(gameObject); // Destrói o projétil se o alvo morreu
            return;
        }

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.transform.position) < 0.5f)
        {
            // Chegou ao alvo
            target.TakeDamage(damage);
            // Opcional: Instanciar um efeito de impacto aqui
            Destroy(gameObject);
        }
    }
}