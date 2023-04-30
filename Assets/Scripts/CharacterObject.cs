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

    public InputBuffer inputBuffer = new InputBuffer();
    
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

        UpdateTimers();
        UpdateAnimation(); // when hitStop is occuring, we'll want to slow down animation too
    }

    void UpdateTimers() // update meters
    {
        if (dashCooldown > 0) { dashCooldown -= dashCooldownRate; }
    }

    void UpdateAI()
    {

    }

    public float aniMoveSpeed;
    public float aniSpeed;
    void UpdateAnimation()
    {
        Vector3 latSpeed = new Vector3(velocity.x, 0, velocity.z);
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        myAnimator.SetFloat("moveSpeed", aniMoveSpeed);
        myAnimator.SetFloat("aerialState", aniAerialState);
        myAnimator.SetFloat("fallSpeed", aniFallSpeed);
        //myAnimator.SetFloat("hitAniX", curHitAni.x);
        //myAnimator.SetFloat("hitAniX", curHitAni.x);
        //myAnimator.SetFloat("aniSpeed", aniSpeed);
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

    void FaceTarget(Vector3 tarPos)
    {
        Vector3 tarOffset = tarPos - transform.position;
        tarOffset.y = 0;
        character.transform.rotation = Quaternion.LookRotation(tarOffset, Vector3.up);
    }

    public bool aerialFlag;
    public float aerialTimer;
    public float aniAerialState;
    public float aniFallSpeed;

    public int jumps;
    public int jumpMax = 2;

    public float dashCooldown;
    public float dashCooldownMax = 180f;
    public float dashCooldownRate = 1f;

    public float specialMeter;
    public float specialMeterMax = 100f;

    // using a skill for example
    public void UseMeter(float _val) 
    {
        ChangeMeter(-_val); // could also do vfx or sfx here
    }

    // could be from striking an enemy
    public void BuildMeter(float _val)
    {
        ChangeMeter(_val);
    }

    public void ChangeMeter(float _val)
    {
        specialMeter += _val;
        specialMeter = Mathf.Clamp(specialMeter, 0, specialMeterMax);
    }

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
            jumps = jumpMax;
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
                // enable to only allow one jump when airborne
                // if (jumps == jumpMax) { jumps--; } 
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

        if (hitStun <= 0) { FaceStick(1); }
    }

    void SetAnimation(string aniName)
    {
        myAnimator.CrossFadeInFixedTime(aniName, GameEngine.coreData.characterStates[currentState].blendRate);
        Debug.Log(myName + " Start: " + aniName);
    }

    void UpdateState()
    {
        CharacterState myCurrentState = GameEngine.coreData.characterStates[currentState];

        if (hitStun > 0) { GettingHit(); }
        else
        {
            UpdateStateEvents();
            UpdateStateAttacks();

            prevStateTime = currentStateTime;
            currentStateTime++;

            if (currentStateTime >= myCurrentState.length)
            {
                if (myCurrentState.loop) { LoopState(); }
                else { EndState(); }
            }
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

            // HitCancel
            float cWindow = _atk.start + _atk.cancelWindow;
            if (currentStateTime == cWindow)
            {
                if (hitConfirm > 0) { canCancel = true; }
            }

            if (currentStateTime >= cWindow + whiffWindow)
            {
                canCancel = true;
            }

            _cur++;
        }
    }

    public static float whiffWindow = 8f;

    void HitCancel()
    {
        float cWindow = GameEngine.coreData.characterStates[currentState].attacks[currentAttackIndex].start
            + GameEngine.coreData.characterStates[currentState].attacks[currentAttackIndex].cancelWindow;
        
        if (currentStateTime == cWindow)
        {
            if (hitConfirm > 0) { canCancel = true; }
        }

        if (currentStateTime == cWindow + whiffWindow)
        {
            canCancel = true;
        }
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
            case 5:
                GlobalPrefab(_var);
                break;
            case 6:
                CanCancel(_var);
                break;
            case 7:
                Jump(_var);
                break;
            case 8:
                FaceStick(_var);
                break;
        }
    }

    void FaceStick(float _rate)
    {
        Vector3 velHelp = new Vector3(0, 0, 0);
        Vector3 velDir;

        if (leftStick.x > deadzone || leftStick.x < -deadzone || leftStick.y > deadzone || leftStick.y < -deadzone)
        {
            velDir = Camera.main.transform.forward;
            velDir.y = 0;
            velDir.Normalize();
            velHelp += velDir * leftStick.y;

            velHelp += Camera.main.transform.right * leftStick.x;
            velHelp.y = 0;

            character.transform.rotation = Quaternion.Lerp(character.transform.rotation, Quaternion.LookRotation(new Vector3(velHelp.x, 0, velHelp.z), Vector3.up), _rate);
        }
    }

    void Jump(float _pow)
    {
        velocity.y = _pow;
        jumps--;
    }

    void CanCancel(float _val)
    {
        if (_val > 0f) { canCancel = true; }
        else { canCancel = false; }
    }

    void GlobalPrefab(float _index)
    {
        GameEngine.GlobalPrefab((int)_index, gameObject);
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

    public int currentCommandState;
    public int currentCommandStep;
    public void GetCommandState()
    {
        currentCommandState = 0;
        for (int c = 0; c < GameEngine.coreData.commandStates.Count; c++) // (CommandState s in GameEngine.coreData.commandStates)
        {
            CommandState s = GameEngine.coreData.commandStates[c];
            if (s.aerial == aerialFlag)
            {
                currentCommandState = c;
                return;
            }
        }
    }

    int[] cancelStepList = new int[2];
    
    void UpdateInput()
    {
        leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // if (Input.GetButton("RB")) { targeting = true; }
        // else { targeting = false; }

        inputBuffer.Update();

        bool startState = false;

        GetCommandState();
        CommandState comState = GameEngine.coreData.commandStates[currentCommandState];

        if (currentCommandStep >= comState.commandSteps.Count) { currentCommandStep = 0; } // Change this to state-specific or even commandstep specific variables

        cancelStepList[0] = currentCommandStep;
        cancelStepList[1] = 0;
        int finalS = -1;
        int finalF = -1;
        int currentPriority = -1;
        for (int s = 0; s < cancelStepList.Length; s++)
        {
            if (comState.commandSteps[currentCommandStep].strict && s > 0) { break; }
            if (!comState.commandSteps[currentCommandStep].activated) { break; }

            for (int f = 0; f < comState.commandSteps[cancelStepList[s]].followUps.Count; f++) // CommandStep cStep in comState.commandSteps[currentCommandStep])
            {
                CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[s]].followUps[f]];
                InputCommand nextCommand = nextStep.command;

                if (CheckInputCommand(nextCommand))
                {
                    if (canCancel)
                    {
                        if (GameEngine.coreData.characterStates[nextCommand.state].ConditionsMet(this))
                        {
                            if (nextStep.priority > currentPriority) // could do greater than or equal
                            {
                                currentPriority = nextStep.priority;
                                startState = true;
                                finalS = s;
                                finalF = f;
                            }
                        }
                    }
                }
            }
        }

        if (startState)
        {
            CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[finalS]].followUps[finalF]];
            InputCommand nextCommand = nextStep.command;
            inputBuffer.UseInput(nextCommand.input);
            if (nextStep.followUps.Count > 0) { currentCommandStep = nextStep.idIndex; }
            else { currentCommandStep = 0; }
            StartState(nextCommand.state);
        }
    }

    public bool CheckInputCommand(InputCommand _in)
    {
        if (inputBuffer.buttonCommandCheck[_in.input] < 0) { return false; }
        //if (inputBuffer.motionCommandCheck[_in.motionCommand] < 0) { return false; }
        return true;
    }

    /*
    void UpdateInputOld() // NOT WORKING
    {
        leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        inputBuffer.Update();

        bool startState = false;

        GetCommandState();
        CommandState comState = GameEngine.coreData.commandStates[currentCommandState];


        if (currentCommandStep >= comState.commandSteps.Count) { currentCommandStep = 0; } // Change this to state-specific or even commandstep specific variables

        cancelStepList[0] = currentCommandStep;
        cancelStepList[1] = 0;

        for (int s = 0; s < cancelStepList.Length; s++)
        {
            if (comState.commandSteps[currentCommandStep].strict && s > 0) { break; }
            
            for (int f = 0; f < comState.commandSteps[cancelStepList[s]].followUps.Count; f++) // CommandStep cStep in comState.commandSteps[currentCommandStep])
            {
                CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[s]].followUps[f]];
                InputCommand stepCommand = nextStep.command;

                if (startState) { break; }
                
                foreach (InputBufferItem bItem in inputBuffer.inputList)
                {
                    if (startState) { break; }
                    foreach (InputStateItem bState in bItem.buffer)
                    {
                        if (stepCommand.input == bItem.button) // this was when button and input were both strings
                        {
                            if (bState.CanExecute())
                            {
                                if (canCancel)
                                {
                                    if (GameEngine.coreData.characterStates[stepCommand.state].ConditionsMet(this))
                                    {
                                        startState = true;
                                        bState.used = true;

                                        if (nextStep.followUps.Count > 0) { currentCommandStep = nextStep.idIndex; }
                                        else { currentCommandStep = 0; }

                                        Debug.Log("Current Step:" + currentCommandStep);

                                        StartState(stepCommand.state);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    */



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

        FaceTarget(attacker.transform.position);

        GameEngine.SetHitStop(curAtk.hitStop);

        hitStun = curAtk.hitstun;
        attacker.hitConfirm += 1;

        attacker.BuildMeter(10f);

        StartState(5); // magic number, is the index of the hitstun character state
        //GlobalPrefab(0); // magic number, is element 0 of the global prefab list
    }

    public float hitStun;
    public void GettingHit()
    {
        hitStun--;
    }

}
