using System;
using System.Collections.Generic;
using Core.StateMachine;
using Player;
using UnityEngine;

public class KaralamaStateMachine : StateMachineRunner
{

    [SerializeField] private bool debugMode = false;

    #region SharedData
    [System.Serializable]
    public class SharedData
    {
        public Animator animator;
        public Rigidbody2D rigidbody;
        public GameObject player;
        public MonoBehaviour coroutineRunner;

        public bool isDashing = false;
        public bool idleBlackActive = false;
        public bool idleWhiteActive = false;
        public bool exploitActive = false;
        public bool hammerActive = false;
        public bool spearActive = false;
        public bool shadow = false;
        public SharedData(
            Animator animator,
            Rigidbody2D rigidbody,
            GameObject player,
            MonoBehaviour coroutineRunner)
        {
            this.rigidbody = rigidbody;
            this.player = player;
            this.animator = animator;
            this.coroutineRunner = coroutineRunner;
        }
    }

    public class InternalSharedData
    {
        public int h_damage;
        public int h_TrapDamage;

        public int s_Damage;

        public int shadowSpeed;
        public GameObject player;
        public Exploit exploit;
        public GameObject exploitPos;
        public GameObject[] dashPositions;
        public bool currentPos;

        public GameObject trapPrefab;
        public GameObject razePrefab;
        public GameObject currSpear = null;

        public InternalSharedData(int dh_Hammer, int dh_Trap, int s_Damage, int shadowSpeed,
        GameObject[] dashPositions, GameObject player, GameObject exploitPos, GameObject trapPrefab, GameObject razePrefab,
        Exploit exploit)
        {
            this.h_damage = dh_Hammer;
            this.h_TrapDamage = dh_Trap;
            this.player = player;
            this.shadowSpeed = shadowSpeed;
            this.s_Damage = s_Damage;
            this.exploit = exploit;
            this.exploitPos = exploitPos;
            this.dashPositions = dashPositions;
            this.trapPrefab = trapPrefab;
            this.currentPos = false;
            this.razePrefab = razePrefab;
        }
    }

    public SharedData sharedData;
    public InternalSharedData internalSharedData;

    private void InitSharedDatas()
    {
        sharedData = new SharedData(
            GetComponent<Animator>(),
            GetComponent<Rigidbody2D>(),
            sharedData.player,
            this);

        internalSharedData = new InternalSharedData(
            h_damage,
            h_trapDamage,
            s_damage,
            shadowSpeed,
            dashPositions,
            sharedData.player,
            e_Pos,
            trapPrefab,
            razePrefab,
            _exploit);
    }

    #endregion

    #region Instance

    public static KaralamaStateMachine Instance;

    private void InitInstances()
    {
        if (Instance == null)
            Instance = this;
    }

    #endregion

    #region Hammer
    [Header("Hammer")]
    [SerializeField] private GameObject h_Prefab;
    [SerializeField] private GameObject h_SpawnPos;
    [SerializeField] private int h_damage = 3;
    [SerializeField] private int h_trapDamage = 2;

    [SerializeField] private GameObject trapPrefab;

    #endregion

    #region Spear
    [Header("Spear")]
    [SerializeField] private GameObject s_Prefab;
    [SerializeField] private GameObject s_SpawnPos;
    [SerializeField] private int s_damage = 2;
    #endregion 

    #region  Exploit
    [Header("Exploit")]
    [SerializeField] private GameObject e_Pos;
    [SerializeField] private List<GameObject> e_Objects;
    private Dictionary<Vector2, GameObject> e_Dictionary;
    private Dictionary<Vector2, ChainManager> e_ChainDictionary;
    public Dictionary<Vector2, GameObject> GetExploitDictionary() { return e_Dictionary; }
    public Dictionary<Vector2, ChainManager> GetChainDictionary() { return e_ChainDictionary; }

    private void DestroyExploitObject() => internalSharedData.exploit.DestroyExploitObject();

    private void E_Init()
    {
        e_Dictionary = new Dictionary<Vector2, GameObject>();
        e_ChainDictionary = new Dictionary<Vector2, ChainManager>();

        foreach (var obj in e_Objects)
        {
            var chainManager = obj.GetComponent<ChainManager>();
            var lantern = chainManager.lantern;

            e_Dictionary.Add(lantern.transform.position, lantern);
            e_ChainDictionary.Add(lantern.transform.position, chainManager);
        }
        //Extra slot
        e_Dictionary.Add(new Vector2(0, e_Objects[0].transform.position.y), null);

    }
    #endregion

    #region Dash
    [Header("Dash")]
    [SerializeField] private GameObject[] dashPositions;
    #endregion

    #region Raze

    [SerializeField] GameObject razePrefab;

    #endregion

    #region Shadow

    [SerializeField] private int shadowSpeed = 20;

    private void ShadowIdle()
    {
        _shadow.SetShadow();
    }

    #endregion

    #region States
    private Idle_Black _idleBlack;
    private Idle_White _idleWhite;
    private Exploit _exploit;
    private Hammer _hammer;
    private Spear _spear;
    private Dash _dash;
    private Shadow _shadow;

    protected override void InitializeStates()
    {
        base.InitializeStates();

        _idleBlack = new Idle_Black(gameObject, sharedData);
        _idleWhite = new Idle_White(gameObject, sharedData);
        _exploit = new Exploit(gameObject, sharedData, internalSharedData);
        _hammer = new Hammer(gameObject, sharedData);
        _spear = new Spear(gameObject, sharedData);
        _dash = new Dash(gameObject, sharedData, internalSharedData);
        _shadow = new Shadow(gameObject, sharedData, internalSharedData);


        _idleBlack.AddTransition(() => sharedData.isDashing, _dash);
        _idleBlack.AddTransition(() => sharedData.hammerActive, _hammer);
        _idleBlack.AddTransition(() => sharedData.spearActive, _spear);
        _idleBlack.AddTransition(() => sharedData.shadow, _shadow);

        _hammer.AddTransition(() => sharedData.idleWhiteActive, _idleWhite);
        _spear.AddTransition(() => sharedData.idleWhiteActive, _idleWhite);

        _dash.AddTransition(() => sharedData.idleBlackActive, _idleBlack);
        _shadow.AddTransition(() => sharedData.idleBlackActive, _idleBlack);

        _idleWhite.AddTransition(() => sharedData.exploitActive, _exploit);

        _exploit.AddTransition(() => sharedData.idleBlackActive, _idleBlack);

        Machine.ChangeState(_idleBlack);

        internalSharedData.exploit = _exploit;
    }
    #endregion

    #region Monobehaviour

    void Awake()
    {
        E_Init();
        InitInstances();
        InitSharedDatas();

        base.OnAwake();
    }

    void Update()
    {
        if (debugMode)
            print(Machine.GetCurrentState());
        base.OnUpdate();

        if (sharedData.idleWhiteActive)
        {
            if (!lastIdleWhiteActive)
            {
                ResetActionFlags();
                sharedData.exploitActive = true;
                waitingForStateChange = true;
                nextDecisionTime = 0f;
            }
            lastIdleWhiteActive = true;
            return;
        }

        lastIdleWhiteActive = false;

        if (!sharedData.idleBlackActive)
        {
            waitingForStateChange = false;
            nextDecisionTime = 0f;
            return;
        }

        if (nextDecisionTime <= 0f)
        {
            nextDecisionTime = Time.time + .3f;//UnityEngine.Random.Range(decisionIntervalMin, decisionIntervalMax);
        }

        if (waitingForStateChange)
        {
            return;
        }

        if (Time.time >= nextDecisionTime)
        {
            MakeRandomAttackDecision();
            waitingForStateChange = true;
            nextDecisionTime = 0f;
        }
    }

    #endregion

    #region HelperFunctions
    private void FaceToPlayer()
    {
        if (sharedData.player == null) return;

        Vector3 localScale = transform.localScale;

        if (sharedData.player.transform.position.x < transform.position.x)
            localScale.x = -Mathf.Abs(localScale.x);
        else
            localScale.x = Mathf.Abs(localScale.x);

        transform.localScale = localScale;
    }

    private bool isLookingRight()
    {
        return transform.localScale.x > 0;
    }

    private void ResetActionFlags()
    {
        sharedData.hammerActive = false;
        sharedData.spearActive = false;
        sharedData.exploitActive = false;
        sharedData.isDashing = false;
    }
    #endregion

    #region AnimEvents
    private void CreateSpear()
    {
        var temp = Instantiate(s_Prefab, s_SpawnPos.transform.position, new Quaternion());
        temp.transform.localScale = transform.localScale;
    }

    private void CreateHammer()
    {
        var temp = Instantiate(h_Prefab, h_SpawnPos.transform.position, new Quaternion());
        temp.transform.localScale = transform.localScale;
    }

    #endregion

    #region AI

    private float nextDecisionTime = 2f;
    private bool waitingForStateChange = false;
    private bool lastIdleWhiteActive = false;
    [SerializeField] private float dashDecideDistance = 3f;

    int decisionId;

    private void MakeRandomAttackDecision()
    {
        ResetActionFlags();
        FaceToPlayer();

        //Count exploit objects
        int e_count = 0;
        foreach (var exp in e_Dictionary)
        {
            if (exp.Value != null)
                e_count++;
        }

        int id;

        do
        {
            id = UnityEngine.Random.Range(0, 4);
        } while (decisionId == id || ((e_count == 1 || e_count == 0) && id == 1));

        if (Vector2.Distance(transform.position, sharedData.player.transform.position) <= dashDecideDistance && UnityEngine.Random.Range(0  ,100) > 70)
            id = 2;

        decisionId = id;

        //decisionId = 3;
        switch (decisionId)
        {
            case 0:
                sharedData.hammerActive = true;
                break;
            case 1:
                sharedData.spearActive = true;
                break;
            case 2:
                sharedData.isDashing = true;
                break;
            case 3:
                sharedData.shadow = true;
                break;
        }
    }

    #endregion

    #region Debug

    void PrintAnimName()
    {
        Animator animator = sharedData.animator;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (stateInfo.IsName(clip.name))
            {
                Debug.Log("Current Animation: " + clip.name);
                break;
            }
        }
    }

    #endregion
}