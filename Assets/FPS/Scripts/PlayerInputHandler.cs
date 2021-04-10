using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [Tooltip("Sensitivity multiplier for moving the camera around")]
    public float lookSensitivity = 1f;
    [Tooltip("Additional sensitivity multiplier for WebGL")]
    public float webglLookSensitivityMultiplier = 0.25f;
    [Tooltip("Limit to consider an input when using a trigger on a controller")]
    public float triggerAxisThreshold = 0.4f;
    [Tooltip("Used to flip the vertical input axis")]
    public bool invertYAxis = false;
    [Tooltip("Used to flip the horizontal input axis")]
    public bool invertXAxis = false;

    GameFlowManager m_GameFlowManager;
    PlayerCharacterController m_PlayerCharacterController;
    bool m_FireInputWasHeld;

    //ML-Specific
    public Agent myAgent;
    public bool autonomus = false;
    private ActionBuffers m_ActionBuffers;
    
    //ML-Controlled variables
    float horizontalMove = 0; // Keyboard will always use -1 - move right, 0 - do not move or 1 - move left
    float mouseVertical = 0; //TODO
    float mouseHorizontal = 0; //TODO
    float verticalMove = 0; // Keyboard will always use -1, 0 or 1
#pragma warning disable 649
    bool sprintButton; // 0 - false, 1 - truea
    bool jumpButton; // 0 - false, 1 - true
    bool fireButton; // 0 - false, 1 - true
    bool aimButton; // 0 - false, 1 - true
    bool crouchButton; // 0 - false, 1 - true
#pragma warning restore 649
    
    private void Start()
    {
        m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerInputHandler>(m_PlayerCharacterController, this, gameObject);
        m_GameFlowManager = FindObjectOfType<GameFlowManager>();
        DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(m_GameFlowManager, this);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
private void FixedUpdate()
    {
        if (autonomus)
        {
            //Get ML-Inputs
            m_ActionBuffers = myAgent.GetStoredActionBuffers();
            float tempAction;
            //**** CONTINOUS *****
            //Movement Horizontal
            //Debug.Log(m_ActionBuffers.ContinuousActions[0]);
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[0], -1f, 1f);  
            if (tempAction > 0.33)
            {
                horizontalMove = 1;
            }else if(tempAction < -0.33)
            {
                horizontalMove = -1;
            }
            else // >= -0.33 && <= 0.33
            {
                //Debug.Log("No movement left/right");
                horizontalMove = 0;
            }
            
            //Movement Vertical
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[1], -1f, 1f);
            if (tempAction > 0.33)
            {
                //Debug.Log("Move forwards");
                verticalMove = 1;
            }else if(tempAction < -0.33)
            {
                //Debug.Log("Move backwards");
                verticalMove = -1;
            }
            else // >= -0.33 && <= 0.33
            {
                //Debug.Log("No movement forward/backwars");
                verticalMove = 0;
            }
            
            /* Commented out as of 18.01.2020 to reduce inputs //Jump
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[2], -1f, 1f);
            if (tempAction > 0)
            {
                jumpButton = true;
            }else if(tempAction <= 0)
            {
                jumpButton = false;
            }
            else
            {
                jumpButton = false;
            }*/
            
            /* Commented out as of 18.01.2020 to reduce inputs //Crouch
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[3], -1f, 1f);
            if (tempAction > 0)
            {
                crouchButton = true;
            }else if(tempAction <= 0)
            {
                crouchButton = false;
            }
            else
            {
                crouchButton = false;
            }*/
            
            
            //Shoot
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[2], -1f, 1f);
            if (tempAction > 0)
            {
                fireButton = true;
            }else if(tempAction <= 0)
            {
                fireButton = false;
            }
            else //not needed
            {
                fireButton = false;
            }
            
            /* Commented out as of 18.01.2020 to reduce inputs //Aim
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[5], -1f, 1f);
            if (tempAction > 0)
            {
                aimButton = true;
            }else if(tempAction <= 0)
            {
                aimButton = false;
            }
            else
            {
                aimButton = false;
            }*/
            
            /* Commented out as of 18.01.2020 to reduce inputs //Sprint
            tempAction = Mathf.Clamp(m_ActionBuffers.ContinuousActions[6], -1f, 1f);
            if (tempAction >= 0)
            {
                sprintButton = true;
            }else if(tempAction < 0)
            {
                sprintButton = false;
            }
            else
            {
                sprintButton = false;
            }*/

            //Mouse Look Horizontal
            //mouseHorizontal = Mathf.Clamp(m_ActionBuffers.ContinuousActions[7],-1f,1f);
            mouseHorizontal = m_ActionBuffers.ContinuousActions[3];
            
            //Mouse Look Vertical
            //mouseVertical = Mathf.Clamp(m_ActionBuffers.ContinuousActions[8],-1f,1f);
            mouseVertical = m_ActionBuffers.ContinuousActions[4];

            //****** CONTINOUS END*****

            //*****DISCRETE******
            /*
            //***Parse ML Inputs***
            //Horizontal Movementx
            switch (m_ActionBuffers.DiscreteActions[0])
            {
                case 0:
                    horizontalMove = 0;
                    break;
                case 1:
                    horizontalMove = 1;
                    break;
                case 2:
                    horizontalMove = -1;
                    break;
            }

            //Vertical Movement
            switch (m_ActionBuffers.DiscreteActions[1])
            {
                case 0:
                    verticalMove = 0;
                    break;
                case 1:
                    verticalMove = 1;
                    break;
                case 2:
                    verticalMove = -1;
                    break;
            }

            //Jump
            switch (m_ActionBuffers.DiscreteActions[2])
            {
                case 0:
                    jumpButton = false;
                    break;
                case 1:
                    jumpButton = true;
                    break;
            }

            //Crouch
            switch (m_ActionBuffers.DiscreteActions[3])
            {
                case 0:
                    crouchButton = false;
                    break;
                case 1:
                    crouchButton = true;
                    break;
            }

            //Shoot
            switch (m_ActionBuffers.DiscreteActions[4])
            {
                case 0:
                    fireButton = false;
                    break;
                case 1:
                    fireButton = true;
                    break;
            }

            //Aim
            switch (m_ActionBuffers.DiscreteActions[5])
            {
                case 0:
                    aimButton = false;
                    break;
                case 1:
                    aimButton = true;
                    break;
            }

            //Sprint
            switch (m_ActionBuffers.DiscreteActions[6])
            {
                case 0:
                    sprintButton = false;
                    break;
                case 1:
                    sprintButton = true;
                    break;
            }

            //Mouse horizontal
            switch (m_ActionBuffers.DiscreteActions[7])
            {
                case 0:
                    mouseHorizontal = 0;
                    break;
                case 1:
                    mouseHorizontal = 1f;
                    break;
                case 2:
                    mouseHorizontal = 0.8f;
                    break;
                case 3:
                    mouseHorizontal = 0.6f;
                    break;
                case 4:
                    mouseHorizontal = 0.4f;
                    break;
                case 5:
                    mouseHorizontal = 0.2f;
                    break;
                case 6:
                    mouseHorizontal = -0.2f;
                    break;
                case 7:
                    mouseHorizontal = -0.4f;
                    break;
                case 8:
                    mouseHorizontal = -0.6f;
                    break;
                case 9:
                    mouseHorizontal = -0.8f;
                    break;
                case 10:
                    mouseHorizontal = -1f;
                    break;
                
            }

            //Mouse vertical
            switch (m_ActionBuffers.DiscreteActions[8])
            {                
                case 0:
                    mouseVertical = 0;
                    break;
                case 1:
                    mouseVertical = 1f;
                    break;
                case 2:
                    mouseVertical = 0.8f;
                    break;
                case 3:
                    mouseVertical = 0.6f;
                    break;
                case 4:
                    mouseVertical = 0.4f;
                    break;
                case 5:
                    mouseVertical = 0.2f;
                    break;
                case 6:
                    mouseVertical = -0.2f;
                    break;
                case 7:
                    mouseVertical = -0.4f;
                    break;
                case 8:
                    mouseVertical = -0.6f;
                    break;
                case 9:
                    mouseVertical = -0.8f;
                    break;
                case 10:
                    mouseVertical = -1f;
                    break;
                
            }*/
            //****DISCRETE END*****
        }

        /*
        Debug.Log("Move horizontal: " + horizontalMove);
        Debug.Log("Move vertical: " + verticalMove);
        Debug.Log("Jump: " + jumpButton);
        Debug.Log("Crouch: " + crouchButton);
        Debug.Log("Shoot: " + fireButton);
        Debug.Log("Aim: " + aimButton);
        Debug.Log("Sprint: " + sprintButton);
        Debug.Log("Mouse horizontal: " + mouseHorizontal);
        Debug.Log("Mouse vertical: " + mouseVertical);
        
         Debug.Log("Branch 0: " + m_ActionBuffers.DiscreteActions[0]);
         Debug.Log("Branch 1: " + m_ActionBuffers.DiscreteActions[1]);
         Debug.Log("Branch 2: " + m_ActionBuffers.DiscreteActions[2]);
         Debug.Log("Branch 3: " + m_ActionBuffers.DiscreteActions[3]);
         Debug.Log("Branch 4: " + m_ActionBuffers.DiscreteActions[4]);
         Debug.Log("Branch 5: " + m_ActionBuffers.DiscreteActions[5]);
         Debug.Log("Branch 6: " + m_ActionBuffers.DiscreteActions[6]);
         Debug.Log("Branch 7: " + m_ActionBuffers.DiscreteActions[7]);
         Debug.Log("Branch 8: " + m_ActionBuffers.DiscreteActions[8]);
         */
        m_FireInputWasHeld = GetFireInputHeld();
    }
    

    float GetMlAxisRaw(string axisName)
    {
        if (axisName.ToLower().Equals("horizontal"))
        {
            return horizontalMove;
        } 
        if(axisName.ToLower().Equals("vertical"))
        {
            return verticalMove;
        }

        if (axisName.ToLower().Equals("mouse x"))
        {
            return mouseHorizontal;
        }
        
        if (axisName.ToLower().Equals("mouse y"))
        {
            return mouseVertical;
        }

        return 0;
    }
    
    public bool CanProcessInput()
    {
        return Cursor.lockState == CursorLockMode.Locked && !m_GameFlowManager.gameIsEnding;
    }

    public Vector3 GetMoveInput()
    {
        if (CanProcessInput())
        {
            Vector3 move;
            if (autonomus)
            {
                move = new Vector3(GetMlAxisRaw(GameConstants.k_AxisNameHorizontal), 0f, GetMlAxisRaw(GameConstants.k_AxisNameVertical));
            }
            else
            {
                move = new Vector3(Input.GetAxisRaw(GameConstants.k_AxisNameHorizontal), 0f,
                    Input.GetAxisRaw(GameConstants.k_AxisNameVertical));
            }

            //Debug.Log("Movement before clamp: " + move.ToString());
            // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
            
            move = Vector3.ClampMagnitude(move, 1);
            
            //Debug.Log("Movement returned: " + move.ToString());
            return move;
        }

        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        return GetMouseOrStickLookAxis(GameConstants.k_MouseAxisNameHorizontal, GameConstants.k_AxisNameJoystickLookHorizontal);
    }

    public float GetLookInputsVertical()
    {
        return GetMouseOrStickLookAxis(GameConstants.k_MouseAxisNameVertical, GameConstants.k_AxisNameJoystickLookVertical);
    }

    public bool GetJumpInputDown()
    {
        if (CanProcessInput())
        {
            if (autonomus)
            {
                return jumpButton;
            }
            return Input.GetButtonDown(GameConstants.k_ButtonNameJump);
        }

        return false;
    }

    public bool GetJumpInputHeld()
    {
        if (CanProcessInput())
        {
            if (autonomus) //TODO: Maybe not working
            {
                return jumpButton;
            }
            return Input.GetButton(GameConstants.k_ButtonNameJump);
        }

        return false;
    }

    public bool GetFireInputDown()
    {
        return GetFireInputHeld() && !m_FireInputWasHeld;
    }

    public bool GetFireInputReleased()
    {
        return !GetFireInputHeld() && m_FireInputWasHeld;
    }

    public bool GetFireInputHeld()
    {
        if (CanProcessInput())
        {
            if (autonomus)
            {
                return fireButton;
            }
            bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadFire) != 0f;
            if (isGamepad)
            {
                return Input.GetAxis(GameConstants.k_ButtonNameGamepadFire) >= triggerAxisThreshold;
            }
            else
            {
                return Input.GetButton(GameConstants.k_ButtonNameFire);
            }
        }

        return false;
    }

    public bool GetAimInputHeld()
    {
        if (CanProcessInput())
        {
            if (autonomus)
            {
                return aimButton;
            }
            bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadAim) != 0f;
            bool i = isGamepad ? (Input.GetAxis(GameConstants.k_ButtonNameGamepadAim) > 0f) : Input.GetButton(GameConstants.k_ButtonNameAim);
            return i;
        }

        return false;
    }

    public bool GetSprintInputHeld()
    {
        if (CanProcessInput())
        {
            if (autonomus)
            {
                return sprintButton;
            }
            return Input.GetButton(GameConstants.k_ButtonNameSprint);
        }

        return false;
    }

    public bool GetCrouchInputDown()
    {
        if (CanProcessInput())
        {
            if (autonomus)
            {
                return crouchButton;
            }
            return Input.GetButtonDown(GameConstants.k_ButtonNameCrouch);
        }

        return false;
    }

    public bool GetCrouchInputReleased()
    {
        if (CanProcessInput())
        {
            if (autonomus)
            {
                return crouchButton;
            }
            return Input.GetButtonUp(GameConstants.k_ButtonNameCrouch);
        }

        return false;
    }

    public int GetSwitchWeaponInput()
    {
        if (CanProcessInput())
        {
            if (!autonomus)
            {
                bool isGamepad = Input.GetAxis(GameConstants.k_ButtonNameGamepadSwitchWeapon) != 0f;
                string axisName = isGamepad ? GameConstants.k_ButtonNameGamepadSwitchWeapon : GameConstants.k_ButtonNameSwitchWeapon;

                if (Input.GetAxis(axisName) > 0f)
                    return -1;
                else if (Input.GetAxis(axisName) < 0f)
                    return 1;
                else if (Input.GetAxis(GameConstants.k_ButtonNameNextWeapon) > 0f)
                    return -1;
                else if (Input.GetAxis(GameConstants.k_ButtonNameNextWeapon) < 0f)
                    return 1;
            }
        }

        return 0;
    }

    public int GetSelectWeaponInput()
    {
        if (CanProcessInput())
        {
            if (!autonomus)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    return 1;
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                    return 2;
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                    return 3;
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                    return 4;
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                    return 5;
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                    return 6;
                else
                    return 0;
            }
        }

        return 0;
    }
    
    float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName)
    {
        if (CanProcessInput())
        {
            float i;
            bool isGamepad = false;
            if (autonomus)
            {
                //Debug.Log("Mouse X " + Input.GetAxisRaw("Mouse X"));
                //i = Lerp(-15, 15,GetMlAxisRaw(mouseInputName));
                i = GetMlAxisRaw(mouseInputName);
                //Debug.Log("Mouse input in InputHandler: " + i);
            }
            else
            {
                // Check if this look input is coming from the mouse
                isGamepad = Input.GetAxis(stickInputName) != 0f;
                i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);
                //Debug.Log("Mouse input in InputHandler (non autonomous): " + i);
                
            }

            // handle inverting vertical input
            if (invertYAxis)
                i *= -1f;

            // apply sensitivity multiplier
            i *= lookSensitivity;

            if (isGamepad)
            {
                // since mouse input is already deltaTime-dependant, only scale input with frame time if it's coming from sticks
                i *= Time.deltaTime;
            }
            else
            {
                // reduce mouse input amount to be equivalent to stick movement
                //i *= 0.01f;
                i *= Time.deltaTime;
#if UNITY_WEBGL
                // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                i *= webglLookSensitivityMultiplier;
#endif
            }

            return i;
        }

        return 0f;
    }
}
