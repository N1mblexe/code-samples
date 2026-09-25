public class WeaponManager : MonoBehaviour
{
    private class WeaponInfo
    {
        public GameObject weapon;
        public IWeapon iWeapon;
        public float shootRate;
        public Sprite icon;
        public int bulletAmount;

        public WeaponInfo(GameObject weapon)
        {
            this.weapon = weapon;
            iWeapon = this.weapon.GetComponent<IWeapon>();
            shootRate = iWeapon.GetShootRate();
            icon = weapon.GetComponent<IWeapon>().GetImage();
            bulletAmount = iWeapon.GetAmmoAmount();
        }
    }

    private const int SLOT_SIZE = 3;
    private WeaponInfo[] weapons = new WeaponInfo[SLOT_SIZE];
    private WeaponInfo currentGun;

    [SerializeField] private int currentSlot = 0;

    [SerializeField] private GameObject[] icons = new GameObject[SLOT_SIZE];
    private Vector2 iconDefaultScale;

    #region Instance

    public static WeaponManager manager;

    #endregion Instance

    #region Shoot

    [SerializeField] public GameObject ShootPos;

    public GameObject startGun;

    public bool canShoot = true;

    private float lastShootTime = 0;

    private void Awake()
    {
        foreach (var weapon in weapons)
            SetGun(startGun);

        iconDefaultScale = icons[0].transform.localScale;
    }

    private void Start()
    {
        manager = this;

        ChangeSlot(0);

        try
        {
            GunManager.Instance.SetBulletCount(currentGun.bulletAmount);
        }catch(Exception e) { }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canShoot == true)
        {
            if (Time.timeSinceLevelLoad - lastShootTime > currentGun.shootRate)
            {
                if (currentGun.weapon == null)
                {
                    Debug.LogError("NO GUN AVAILABLE");
                    return;
                }
                currentGun.iWeapon.Shoot(PlayerManager.Instance.characterData.damage);
                lastShootTime = Time.timeSinceLevelLoad;

                FillByTime(icons[currentSlot].GetComponent<Image>(), currentGun.iWeapon.GetShootRate());

                try
                {
                    SetGunManagerControllerAhmedoHe.Instance.SetBulletCount(--currentGun.bulletAmount);
                }
                catch(Exception ex) { }

                if (currentGun.bulletAmount <= 0)
                    SetSlot(startGun, currentSlot);
            }
        }

        float scrollValue = Input.GetAxis("Mouse ScrollWheel");

        if (scrollValue > 0)
            ChangeSlot(currentSlot + 1);
        if (scrollValue < 0)
            ChangeSlot(currentSlot - 1);

        if (Input.GetKeyDown(KeyCode.Alpha1) && currentSlot != 0) ChangeSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && currentSlot != 1) ChangeSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && currentSlot != 2) ChangeSlot(2);
    }

    #endregion Shoot

    #region GunManager

    public void SetGun(GameObject gunPrefab)
    {
        GameObject gun = Instantiate(gunPrefab, ShootPos.transform);

        int id = 0;
        foreach (var weapon in weapons)
        {
            if (weapon == null)
            {
                SetSlot(new WeaponInfo(gun), id);
                return;
            }
            id++;
        }

        id = 0;
        int leastValueableId = 0;
        WeaponInfo leastValueable = weapons[0];
        foreach (var weapon in weapons)
        {
            if (leastValueable.iWeapon.GetValue() > weapon.iWeapon.GetValue())
            {
                leastValueable = weapon;
                leastValueableId = id;
            }
            id++;
        }

        SetSlot(new WeaponInfo(gun), leastValueableId);
    }

    private void SetSlot(WeaponInfo weapon, int slot)
    {
        if (slot > SLOT_SIZE)
            return;

        if (currentSlot == slot)
            currentGun = weapon;

        if (weapons[slot] != null)
            Destroy(weapons[slot].weapon);

        weapons[slot] = weapon;

        icons[slot].GetComponent<Image>().sprite = weapon.icon;
    }

    private void SetSlot(GameObject prefab, int slot)
    {
        GameObject gun = Instantiate(prefab, ShootPos.transform);

        SetSlot(new WeaponInfo(gun), slot);
    }

    private float animTime = 0.3f;

    private void ChangeSlot(int to)
    {
        AnimIn();

        currentSlot = to % SLOT_SIZE;

        if (currentSlot < 0)
            currentSlot = SLOT_SIZE - 1;

        currentGun = weapons[currentSlot];

        lastShootTime = Time.timeSinceLevelLoad - currentGun.iWeapon.GetShootRate() / 1.3f;

        FillByTime(icons[currentSlot].GetComponent<Image>(), currentGun.iWeapon.GetShootRate() - currentGun.iWeapon.GetShootRate() / 1.3f);

        AnimOut();
    }

    #endregion GunManager

    private void AnimIn() => icons[currentSlot].transform.DOScale(iconDefaultScale, animTime).SetEase(Ease.OutQuad).OnComplete(() => { icons[currentSlot].transform.localScale = iconDefaultScale; });

    private void AnimOut() => icons[currentSlot].transform.DOScale(iconDefaultScale * 1.3f, animTime).SetEase(Ease.OutQuad).OnComplete(() => { icons[currentSlot].transform.localScale = iconDefaultScale * 1.3f; });

    private void FillByTime(UnityEngine.UI.Image image, float cd)
    {
        image.fillAmount = 0;

        DOTween.To(
            () => image.fillAmount,  
            x => image.fillAmount = x, 
            1.0f,                     
            cd  
        );
    }
}