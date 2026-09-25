using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class HealthManager : MonoBehaviour, IDamagable
    {
        [SerializeField] private Controller stateMachine;
        private static bool isImmune = false;
        public GameObject lastDamageSource {get ; private set;}
        [SerializeField] private Slider slider;

        public static void setImmunity(bool state) { isImmune = state; }
        [SerializeField] private uint m_maxHealth;
        private uint m_health;

        void Awake()
        {
            slider.maxValue = m_maxHealth;
            slider.value = m_maxHealth;
            m_health = m_maxHealth;
            stateMachine = GetComponent<Controller>();
        }

        //Overrides from IDamagable
        public void Damage(GameObject source, uint amount, int damageId = 0)
        {
            if (source == this.gameObject)
                return;

            if (isImmune)
                return;

            lastDamageSource = source;

            Debug.Log(m_health);
            if (m_health == 0)
                return;

            if (amount >= m_health)
            {
                m_health = 0;
                //TODO : DIE
                return;
            }

            m_health -= amount;
            slider.value = m_health;
            StartCoroutine(SpriteFlashing());
            TimeManager.ChangeTime(.3f);

            //TODO : for getting damage play post process effect (vignette)
            //TODO : if health is getting lower than some kind of threshold start post process effects (Vignette , lens distortion , color adjustments , film grain)
        }

        private float spriteFlashTime = .1f;

        IEnumerator SpriteFlashing()
        {
            isImmune = true;
            Color defaultColor = stateMachine.sr.color;
            stateMachine.sr.color = Color.red;
            yield return new WaitForSeconds(spriteFlashTime);
            stateMachine.sr.color = defaultColor;
            isImmune = false;
        }
    }
}