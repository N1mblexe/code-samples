using UnityEngine;

public interface IDamagable
{
    public void Damage(GameObject source, uint amount, int damageId = 0);
}
