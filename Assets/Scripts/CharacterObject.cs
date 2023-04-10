using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    [HideInInspector]
    private string myName;

    public Vector3 velocity; // applied to object to make it move

    public float gravity = -0.01f;

    public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);

    public CharacterController myController;
    public Animator myAnimator;

    public int currentState;
    public float currentStateTime;
    public float prevStateTime;

    public GameObject character;
    public GameObject draw;

    public enum ControlType { AI, PLAYER }
    public ControlType controlType;

    public Hitbox hitbox;

    public bool canCancel;
    public int hitConfirm;
    
    // Start is called before the first frame update
    void Start()
    {
        myName = this.gameObject.name;
        myController = GetComponent<CharacterController>();
        //myAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
       
    }

    // Update is called once per frame
    void FixedUpdate() // Input -> Command -> State -> Physics
    {
        if (GameEngine.hitStop <= 0)
        {
            // Update Input Buffer


            // Update Input
            switch (controlType)
            {
                case ControlType.PLAYER:
                    UpdateInput();
                    break;

                case ControlType.AI:
                    UpdateAI();
                    break;
            }

            // Update State Machine
            UpdateState();

            // Update Physics
            UpdatePhysics();
            
        }
        // when hitStop is occuring, we'll want to slow down animation too
        UpdateAnimation();
    }

    void UpdateAI()
    {

    }

    public float aniMoveSpeed;

    void UpdateAnimation()
    {
        Vector3 latSpeed = new Vector3(velocity.x, 0, velocity.z);
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        myAnimator.SetFloat("moveSpeed", aniMoveSpeed);
        myAnimator.SetFloat("aerialState", aniAerialState);
        myAnimator.SetFloat("fallSpeed", aniFallSpeed);
    }

    bool CheckVelocityDeadzone()
    {
        if (velocity.x > 0.001f) { return true; }
        if (velocity.x < -0.001f) { return true; }
        if (velocity.z > 0.001f) { return true; }
        if (velocity.z < -0.001f) { return true; }
        return false;
    }

    void FaceVelocity()
    {
        if (CheckVelocityDeadzone())
        {
            character.transform.rotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z), Vector3.up);
        }
    }

    public bool aerialFlag;
    public float aerialTimer;
    public float aniAerialState;
    public float aniFallSpeed;
    void UpdatePhysics()
    {
        velocity.y += gravity;

        myController.Move(velocity);

        velocity.Scale(friction);

        // Aerial Check
        if ((myController.collisionFlags & CollisionFlags.Below) != 0)
        {
            aerialFlag = false;
            aerialTimer = 0;
            aniAerialState *= 0.75f;
            velocity.y = 0;
        }
        else
        {
            if (!aerialFlag)
            {
                aerialTimer++;
            }
            if (aerialTimer >= 3)
            {
                aerialFlag = true;
                if (aniAerialState <= 1f)
                {
                    aniAerialState += 0.1f;
                }
            }
        }

        if (hitStun <= 0)
        {
            FaceVelocity();
        }
    }

    void StartState(int _newState)
    {
        currentState = _newState;
        prevStateTime = -1;
        currentStateTime = 0;

        //Attack
        hitActive = 0;
        hitConfirm = 0;

        SetAnimation(GameEngine.coreData.characterStates[currentState].stateName);
    }

    void SetAnimation(string aniName)
    {
        //myAnimator.CrossFadeInFixedTime(aniName, GameEngine.coreData.characterStates[currentState].blendRate);
        Debug.Log(myName + " Start: " + aniName);
    }

    void UpdateState()
    {
        CharacterState myCurrentState = GameEngine.coreData.characterStates[currentState];

        UpdateStateEvents();
        UpdateStateAttacks();

        prevStateTime = currentStateTime;
        currentStateTime++;

        if (currentStateTime >= myCurrentState.length)
        {
            if (myCurrentState.loop) { LoopState(); }
            else{ EndState(); }
        }
    }

    void LoopState()
    {
        currentStateTime = 0;
        prevStateTime = -1;
    }

    void EndState()
    {
        currentStateTime = 0;
        currentState = 0;
        prevStateTime = -1;
        StartState(currentState);
    }

    void UpdateStateEvents()
    {
        foreach(StateEvent _ev in GameEngine.coreData.characterStates[currentState].events)
        { 
            /*
            if (currentStateTime <= _ev.start && currentStateTime <= _ev.end) // not ideal, should fix eventually
            {
                DoEventScript(_ev.script, _ev.variable);
            }
            */
            if (currentStateTime >= _ev.start && currentStateTime <= _ev.end)
            {
                DoEventScript(_ev.script, _ev.variable);
            }
            
        }
    }

    public float hitActive;
    public int currentAttackIndex;
    void UpdateStateAttacks()
    {
        int _cur = 0;
        foreach (Attack _atk in GameEngine.coreData.characterStates[currentState].attacks)
        {
            if (currentStateTime == _atk.start) 
            {
                hitActive = _atk.length;
                hitbox.transform.localScale = _atk.hitboxScale;
                hitbox.transform.localPosition = _atk.hitboxPos;
                currentAttackIndex = _cur;
            }

            if (currentStateTime == _atk.start + _atk.length)
            {
                hitActive = 0;
            }

            _cur++;
        }
    }

    void HitCancel()
    {

    }

    void DoEventScript(int _index, float _var)
    {
        switch (_index)
        {
            case 0: 
                VelocityY(_var);
                break;
            case 1:
                FrontVelocity(_var);
                break;
            case 3:
                CameraRelativeStickMove(_var);
                break;
            case 4:
                GettingHit();
                break;
        }
    }

    void FrontVelocity(float _pow)
    {
        velocity = character.transform.forward * _pow;
    }


    void VelocityY(float _pow)
    {
        velocity.y = _pow;
    }

    public Vector2 leftStick;
    void CameraRelativeStickMove(float _val)
    {
        Vector3 velHelp = new Vector3(0, 0, 0);
        Vector3 velDir;

        //leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (leftStick.x > deadzone || leftStick.x < -deadzone || leftStick.y > deadzone || leftStick.y < -deadzone)
        {
            if (leftStick.sqrMagnitude > 1) { leftStick.Normalize(); }

            velDir = Camera.main.transform.forward;
            velDir.y = 0;
            velDir.Normalize();
            velHelp += velDir * leftStick.y;

            velHelp += Camera.main.transform.right * leftStick.x;
            velHelp.y = 0;

            velHelp *= _val;


            velocity += velHelp;
        }
    }

    public float deadzone = 0.2f;
    public float moveSpeed = 0.01f;
    public float jumpPow = 1;
    void UpdateInput()
    {
        leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        foreach(InputCommand c in GameEngine.coreData.commands)
        {
            if (c.inputString != "")
            {
                if (Input.GetButtonDown(c.inputString))
                {
                    StartState(c.state);
                    break;
                    // Continue from here
                    // --> Hold state until out of command list and then check if you can Cancel or not
                }
            }
        }
    }

    public void SetVelocity(Vector3 _pow)
    {
        velocity = _pow;
    }

    public float hitAniX;
    public float hitAniY;
    public void GetHit(CharacterObject attacker)
    {
        // set knockback to the velocity and make it relative to the oncoming attack
        Attack curAtk = GameEngine.coreData.characterStates[attacker.currentState].attacks[attacker.currentAttackIndex];

        attacker.hitActive = 0;
        //Vector3 targetOffset = 
        Vector3 nextKnockback = curAtk.knockback;
        Vector3 knockOrientation = attacker.character.transform.forward;

        // we'll do a look rotation to rotate the vector3 based on knockback
        nextKnockback = Quaternion.LookRotation(knockOrientation) * nextKnockback;
        SetVelocity(nextKnockback);

        hitAniX = curAtk.hitAni.x;
        hitAniY = curAtk.hitAni.y;

        GameEngine.SetHitStop(curAtk.hitStop);

        hitStun = curAtk.hitstun;

        StartState(5); // start hitstun state
    }

    public float hitStun;
    public void GettingHit()
    {
        hitStun--;
    }

}
