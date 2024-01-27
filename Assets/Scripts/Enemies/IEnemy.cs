using System.Collections;

public interface IEnemy
{
    public void Kill();
    public bool IsAlive();
    public IEnumerator Reason();
}
