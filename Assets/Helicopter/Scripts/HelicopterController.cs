using UnityEngine;
using UnityEngine.UI;

public class HelicopterController : MonoBehaviour
{
    public bool is_train = false;
    public AudioSource HelicopterSound;
    public ControlPanel ControlPanel;
    public Rigidbody HelicopterModel;
    public HeliRotorController MainRotorController;
    public HeliRotorController MainRotorController01;
    public HeliRotorController MainRotorController02;
    public HeliRotorController MainRotorController03;
    public HeliRotorController SubRotorController;

    public float TurnForce = 3f;
    public float ForwardForce = 10f;
    public float ForwardTiltForce = 20f;
    public float TurnTiltForce = 30f;
    public float EffectiveHeight = 100f;

    public float turnTiltForcePercent = 1.5f;
    public float turnForcePercent = 1.3f;

    private float _engineForce;
    public float EngineForce
    {
        get { return _engineForce; }
        set
        {
           
            if(MainRotorController) MainRotorController.RotarSpeed = value * 80;
            if (MainRotorController01) MainRotorController01.RotarSpeed = value * 80;
            if (MainRotorController02) MainRotorController02.RotarSpeed = value * 80;
            if (MainRotorController03) MainRotorController03.RotarSpeed = value * 80;
            if(SubRotorController) SubRotorController.RotarSpeed = value * 40;
            //HelicopterSound.pitch = Mathf.Clamp(value / 40, 0, 1.2f);
            //if (UIGameController.runtime.EngineForceView != null)
            //    UIGameController.runtime.EngineForceView.text = string.Format("Engine value [ {0} ] ", (int)value);

            _engineForce = value;
        }
    }

    private Vector2 hMove = Vector2.zero;
    private Vector2 hTilt = Vector2.zero;
    private float hTurn = 0f;
    public bool IsOnGround = true;

    // Use this for initialization
	void Start ()
	{
        if(!is_train) { ControlPanel.KeyPressed += OnKeyPressed; }
	}

	void Update () {}
  
    void FixedUpdate()
   {
        LiftProcess();
       MoveProcess();
        TiltProcess();
   }

    private void MoveProcess()
    {
        var turn = TurnForce * Mathf.Lerp(hMove.x, hMove.x * (turnTiltForcePercent - Mathf.Abs(hMove.y)), Mathf.Max(0f, hMove.y));
        hTurn = Mathf.Lerp(hTurn, turn, Time.fixedDeltaTime * TurnForce);
        HelicopterModel.AddRelativeTorque(0f, hTurn * HelicopterModel.mass, 0f);
        HelicopterModel.AddRelativeForce(Vector3.forward * Mathf.Max(0f, hMove.y * ForwardForce * HelicopterModel.mass));
    }

    private void LiftProcess()
    {
        var upForce = 1 - Mathf.Clamp(HelicopterModel.transform.position.y / EffectiveHeight, 0, 1);
        upForce = Mathf.Lerp(0f, EngineForce, upForce) * HelicopterModel.mass;
        HelicopterModel.AddRelativeForce(Vector3.up * upForce);
    }

    private void TiltProcess()
    {
        hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * TurnTiltForce, Time.deltaTime);
        hTilt.y = Mathf.Lerp(hTilt.y, hMove.y * ForwardTiltForce, Time.deltaTime);
        HelicopterModel.transform.localRotation = Quaternion.Euler(hTilt.y, HelicopterModel.transform.localEulerAngles.y, -hTilt.x);
    }

    private void OnKeyPressed(PressedKeyCode[] obj)
    {
        float tempY = 0;
        float tempX = 0;

        // stable forward
        if (hMove.y > 0)
            tempY = - Time.fixedDeltaTime;
        else
            if (hMove.y < 0)
                tempY = Time.fixedDeltaTime;

        // stable lurn
        if (hMove.x > 0)
            tempX = -Time.fixedDeltaTime;
        else
            if (hMove.x < 0)
                tempX = Time.fixedDeltaTime;


        foreach (var pressedKeyCode in obj)
        {
            switch (pressedKeyCode)
            {
                case PressedKeyCode.SpeedUpPressed:

                    EngineForce += 0.1f;
                    break;
                case PressedKeyCode.SpeedDownPressed:

                    EngineForce -= 0.12f;
                    if (EngineForce < 0) EngineForce = 0;
                    break;

                    case PressedKeyCode.ForwardPressed:

                    if (IsOnGround) break;
                    tempY = Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.BackPressed:

                    if (IsOnGround) break;
                    tempY = -Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.LeftPressed:

                    if (IsOnGround) break;
                    tempX = -Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.RightPressed:

                    if (IsOnGround) break;
                    tempX = Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.TurnRightPressed:
                    {
                        if (IsOnGround) break;
                        var force = (turnForcePercent - Mathf.Abs(hMove.y))*HelicopterModel.mass;
                        HelicopterModel.AddRelativeTorque(0f, force, 0);
                    }
                    break;
                    case PressedKeyCode.TurnLeftPressed:
                    {
                        if (IsOnGround) break;
                        
                        var force = -(turnForcePercent - Mathf.Abs(hMove.y))*HelicopterModel.mass;
                        HelicopterModel.AddRelativeTorque(0f, force, 0);
                    }
                    break;

            }
        }

        hMove.x += tempX;
        hMove.x = Mathf.Clamp(hMove.x, -1, 1);

        hMove.y += tempY;
        hMove.y = Mathf.Clamp(hMove.y, -1, 1);

    }

    private void OnCollisionEnter()
    {
        IsOnGround = true;
    }

    private void OnCollisionExit()
    {
        IsOnGround = false;
    }

    public void ReceiveDecision(int discrete_branch)
    {
       
            float tempY = 0;
            float tempX = 0;

            // stable forward
            if (hMove.y > 0)
                tempY = -Time.fixedDeltaTime;
            else
                if (hMove.y < 0)
                tempY = Time.fixedDeltaTime;

            // stable lurn
            if (hMove.x > 0)
                tempX = -Time.fixedDeltaTime;
            else
                if (hMove.x < 0)
                tempX = Time.fixedDeltaTime;



        switch (discrete_branch)
        {
            case -1:

                EngineForce += 0.1f;
                break;
            case -2:

                EngineForce -= 0.12f;
                if (EngineForce < 0) EngineForce = 0;
                break;

            case 0:

                if (IsOnGround) break;
                tempY = Time.fixedDeltaTime;
                break;
            case 1:
                if (IsOnGround) break;
                tempY = -Time.fixedDeltaTime;
                break;
            case 2:
                if (IsOnGround) break;
                tempX = -Time.fixedDeltaTime;
                break;
            case 3:
                if (IsOnGround) break;
                tempX = Time.fixedDeltaTime;
                break;
            case 4:
                {
                    if (IsOnGround) break;
                    var force = (turnForcePercent - Mathf.Abs(hMove.y)) * HelicopterModel.mass;
                    HelicopterModel.AddRelativeTorque(0f, force, 0);
                }
                break;
            case 5:
                {
                    if (IsOnGround) break;

                    var force = -(turnForcePercent - Mathf.Abs(hMove.y)) * HelicopterModel.mass;
                    HelicopterModel.AddRelativeTorque(0f, force, 0);
                }
                break;


        }
            hMove.x += tempX;
            hMove.x = Mathf.Clamp(hMove.x, -1, 1);
            hMove.y += tempY;
            hMove.y = Mathf.Clamp(hMove.y, -1, 1);       
    }
  
    //
}