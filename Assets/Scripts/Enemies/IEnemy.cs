using System.Collections;

public interface IEnemy
{
    public void TakeDamage(int damage);
    public void Die();
    public IEnumerator Reason();
}
