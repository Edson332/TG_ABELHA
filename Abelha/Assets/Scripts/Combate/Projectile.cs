// Scripts/CombatSystem/Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("A velocidade com que o projétil se move em direção ao alvo.")]
    public float speed = 15f;
    
    // Variáveis preenchidas pelo CombatManager
    public int damage;
    public Combatant3D target;
    public Transform attacker; 

    void Update()
    {
        // Se o alvo não existe mais (foi derrotado enquanto o projétil voava), se autodestrói.
        if (target == null || !target.IsAlive())
        {
            Destroy(gameObject);
            return;
        }

        // Move o projétil em direção ao alvo
        Vector3 direction = (target.damageTextSpawnPoint.position - transform.position).normalized; // Mira no "peito" do alvo
        transform.position += direction * speed * Time.deltaTime;
        
        // Opcional: faz o projétil sempre encarar o alvo
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Verifica se chegou ao alvo
        if (Vector3.Distance(transform.position, target.damageTextSpawnPoint.position) < 0.5f)
        {
            // Causa o dano, passando a referência de quem atacou
            target.TakeDamage(damage, attacker);
            
            // Se autodestrói ao atingir o alvo
            Destroy(gameObject);
        }
    }
}