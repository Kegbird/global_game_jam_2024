using System.Collections;

public interface IEnemy
{
    public void TakeDamage(int damage);
    public void Kill();
    public bool IsAlive();
    public IEnumerator Reason();
}
