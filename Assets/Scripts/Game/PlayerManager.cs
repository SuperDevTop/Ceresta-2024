using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static PlayerManager instance;

    [SerializeField] StepDetector stepDetector;
    public bool movePhysically;
    public float physicalRotationValue = 0.1f;
    public float deviceRotationValue = 0.2f;
    public float movePixelValue;
    public float rayRange;
    public float rotationValue;
    public float playerToColliderDistance;
    public float overlapSphereRadius;
    public Transform playerFace;
    public Transform cameraTransform;

    [Space(10)]
    public bool canMoveRight = false;
    public bool canMoveLeft = false;
    public bool canMoveUp = false;
    public bool canMoveDown = false;

    [Space(10)]
    [HideInInspector] public bool movingRight = false;
    [HideInInspector] public bool movingLeft = false;
    [HideInInspector] public bool movingUp = false;
    [HideInInspector] public bool movingDown = false;

    [Space(10)]
    public List<float> accelerometerValues = new List<float>(); //list count should be 4 without initial items.

    [Space(10)]
    float rightAccValue;
    float leftAccValue;
    float upAccValue;
    float downAccValue;

    [Space(10)]
    float maxAccValue;
    int tmpStepCount;

    // Multiplayer parameters
    public static GameObject LocalPlayerInstance;
    public int jobIndex;
    public string itemInventory;
    public string playerName;
    public Vector3 playerRotation;
    private Animator anim;
    public RuntimeAnimatorController[] characterAni;
    public float moveSpeed;
    public bool isWalking;

    private void Awake()
    {
        instance = this;
        isWalking = false;

        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            jobIndex = PlayerPrefs.GetInt("JOB_ID");
            playerName = PhotonNetwork.NickName + ":" + PhotonNetwork.LocalPlayer.UserId;
            GameUI.playerName = playerName;
            playerRotation = new Vector3(0, 0, 0);

            if(jobIndex == 0)
            {
                jobIndex = 1;
            }

            itemInventory = PlayerPrefs.GetString("ITEM_INVENTORY");
            GameUI.ownItems = itemInventory;

            // Animation Play
            anim = this.transform.GetChild(1).gameObject.GetComponent<Animator>();
            SelectAnimation(jobIndex, 0);
        }
        else
        {
            StartCoroutine(DelayToShowAnimations());
        }

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(gameObject);

        if (accelerometerValues.Count <= 0)
        {
            for (int i = 0; i < 4; i++)
            {
                int tmpInt = new int();
                accelerometerValues.Add(tmpInt);
            }
        }        
    }

    void Start()
    {
        if (movePhysically) rotationValue = physicalRotationValue;
        else rotationValue = deviceRotationValue;        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Key")
        {
            Destroy(collision.transform.gameObject);
            GameManager.instance.keyCollected = true;
            UIManager.instance.ShowUIMessage("You found the key! Now, find the exit");
            GameManager.instance.finishLine.SetActive(true);
        }

        if (collision.transform.tag == "Flame")
        {
            Destroy(collision.transform.gameObject);
            UIManager.instance.ShowUIMessage("You found a flame!");

            transform.DOScale(7f/*max value*/, 0.5f);
            playerFace.DOScale(0.045f, 0.5f);
            transform.GetComponent<SphereCollider>().radius = 0.068f;
        }

        if (collision.transform.tag == "Finish")
        {
            if (GameManager.instance.keyCollected)
                UIManager.instance.OpenGameOverPanel();
            else
                UIManager.instance.ShowUIMessage("Collect the key first!");
            //congratulation \n you passed!
        }

        if(collision.transform.tag == "Flag")
        {
            PhotonNetwork.Disconnect();
        }
    }

    void ScaleToBigSize()
    {
        transform.DOScale(4f, 0.5f);
        playerFace.DOScale(0.09f, 0.5f);
    }

    void Update()
    {        
        // Check if there are any colliders inside the sphere
        bool isInsideSphere = Physics.CheckSphere(transform.position, overlapSphereRadius);

        // If there are no colliders inside the sphere, move is true
        bool moveUp = canMoveUp = true;
        bool moveDown = canMoveDown = true;
        bool moveLeft = canMoveLeft = true;
        bool moveRight = canMoveRight = true;

        // Calculate directions for the four cardinal directions
        Vector3 upDirection = Vector3.up;
        Vector3 downDirection = Vector3.down;
        Vector3 leftDirection = -transform.right;
        Vector3 rightDirection = transform.right;

        // Check for colliders within the sphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, overlapSphereRadius);

        // Loop through all colliders found
        foreach (Collider collider in colliders)
        {
            // Check if collider has a "Wall" tag
            if (collider.CompareTag("WallHor") || collider.CompareTag("OuterWall") ||
                collider.CompareTag("WallVer") || collider.CompareTag("WallJoint"))
            {
                // Calculate direction from sphere center to collider's position
                Vector3 direction = (collider.transform.position - transform.position).normalized;

                // Calculate angle between direction and each cardinal direction
                float angleUp = Vector3.Angle(direction, upDirection);
                float angleDown = Vector3.Angle(direction, downDirection);
                float angleLeft = Vector3.Angle(direction, leftDirection);
                float angleRight = Vector3.Angle(direction, rightDirection);

                // Determine which cardinal direction collider is in based on smallest angle
                if (angleUp < angleDown && angleUp < angleLeft && angleUp < angleRight)
                {
                    // Set moveUp to false if collider is above
                    moveUp = canMoveUp = false;
                }
                else if (angleDown < angleUp && angleDown < angleLeft && angleDown < angleRight)
                {
                    // Set moveDown to false if collider is below
                    moveDown = canMoveDown = false;
                }
                else if (angleLeft < angleUp && angleLeft < angleDown && angleLeft < angleRight)
                {
                    // Set moveLeft to false if collider is to the left
                    moveLeft = canMoveLeft = false;
                }
                else if (angleRight < angleUp && angleRight < angleDown && angleRight < angleLeft)
                {
                    // Set moveRight to false if collider is to the right
                    moveRight = canMoveRight = false;
                }
            }
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine && GameMapGenerator.isStarted && MainPun.isPlaying)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) MovePlayerWithKeyboardInput();
            if (Application.platform == RuntimePlatform.Android && !movePhysically) MovePlayerWithAccelerometer();
            if (Application.platform == RuntimePlatform.Android && movePhysically) MovePlayerWithPhysicalInput();

            if (movingRight || movingLeft || movingUp || movingDown)
            {
                anim.Play("Walk");
            }
            else
            {
                anim.Play("Idle");
            }

            // Measure move speed
            Vector3 acceleration = Input.acceleration;
            moveSpeed = acceleration.magnitude;                  

            if(moveSpeed > 1f)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }
        }        

        if (PhotonNetwork.IsConnected && GameMapGenerator.isStarted)
        {
            if (photonView.IsMine)
            {                
                playerRotation = playerFace.localEulerAngles;                
            }
            else
            {
                playerFace.localEulerAngles = playerRotation;
            }                        
        }

        //Test mode
        if (!PhotonNetwork.IsConnected)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor) MovePlayerWithKeyboardInput();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, overlapSphereRadius);
    }

    async void MovePlayerWithAccelerometer()
    {
        // Get the acceleration along the X-axis
        float xAcceleration = Input.acceleration.x;
        float yAcceleration = Input.acceleration.y;
        float multipleValue = 0;

        if (GameUI.isAcceptedPolice)
        {
            if (isWalking)
            {
                multipleValue = 240f;
            }
            else
            {
                multipleValue = 120f;
            }                        
        }
        else
        {
            if (isWalking)
            {
                multipleValue = 120f;
            }
            else
            {
                multipleValue = 60f;
            }
        }

        if (xAcceleration > 0)
        {
            rightAccValue = xAcceleration;
            accelerometerValues[0] = Mathf.Abs(rightAccValue);
        }
        if (xAcceleration < 0)
        {
            leftAccValue = xAcceleration;
            accelerometerValues[1] = Mathf.Abs(leftAccValue);
        }
        if (yAcceleration > 0)
        {
            upAccValue = yAcceleration;
            accelerometerValues[2] = Mathf.Abs(upAccValue);
        }
        if (yAcceleration < 0)
        {
            downAccValue = yAcceleration;
            accelerometerValues[3] = Mathf.Abs(downAccValue);
        }

        GetMaxValue();

        // Determine movement direction based on X-acceleration
        if ((xAcceleration > rotationValue && xAcceleration == maxAccValue) && canMoveRight && /*rightPos &&*/ !movingRight)
        {
            movingRight = true;
            RotatePlayer(new Vector3(0, 0, -90));
            Vector3 targetPos = new Vector3(transform.position.x + movePixelValue * multipleValue, transform.position.y, transform.position.z);

            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveRight) { tween.Kill(); } });

            await Task.Delay(200);
            movingRight = false;
        }
        else if ((xAcceleration < (-rotationValue) && xAcceleration == (-maxAccValue)) && canMoveLeft && /*leftPos &&*/ !movingLeft)
        {
            movingLeft = true;
            RotatePlayer(new Vector3(0, 0, 90));
            Vector3 targetPos = new Vector3(transform.position.x - movePixelValue * multipleValue, transform.position.y, transform.position.z);

            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveLeft) { tween.Kill(); } });

            await Task.Delay(200);
            movingLeft = false;
        }
        else if ((yAcceleration > rotationValue && yAcceleration == maxAccValue) && canMoveUp && /*upPos &&*/ !movingUp)
        {
            movingUp = true;
            RotatePlayer(new Vector3(0, 0, 0));
            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y + movePixelValue * multipleValue, transform.position.z);

            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveUp) { tween.Kill(); } });

            await Task.Delay(200);
            movingUp = false;
        }
        else if ((yAcceleration < (-rotationValue) && yAcceleration == (-maxAccValue)) && canMoveDown && /*downPos &&*/ !movingDown)
        {
            movingDown = true;
            RotatePlayer(new Vector3(0, 0, 180));
            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y - movePixelValue * multipleValue, transform.position.z);

            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveDown) { tween.Kill(); } });

            await Task.Delay(200);
            movingDown = false;
        }
    }

    async void MovePlayerWithPhysicalInput()
    {
        // Get the acceleration along the X-axis
        float xAcceleration = Input.acceleration.x;
        float yAcceleration = Input.acceleration.y;

        if (xAcceleration > 0)
        {
            rightAccValue = xAcceleration;
            accelerometerValues[0] = Mathf.Abs(rightAccValue);
        }
        if (xAcceleration < 0)
        {
            leftAccValue = xAcceleration;
            accelerometerValues[1] = Mathf.Abs(leftAccValue);
        }
        if (yAcceleration > 0)
        {
            upAccValue = yAcceleration;
            accelerometerValues[2] = Mathf.Abs(upAccValue);
        }
        if (yAcceleration < 0)
        {
            downAccValue = yAcceleration;
            accelerometerValues[3] = Mathf.Abs(downAccValue);
        }

        GetMaxValue();

        // Determine movement direction based on X-acceleration
        if (playerFace.localEulerAngles == new Vector3(0, 0, 270) && canMoveRight && /*rightPos &&*/ !movingRight)
        {
            if (stepDetector.stepCount > 0)
            {
                tmpStepCount = stepDetector.stepCount;
                movingRight = true;

                for (int i = 0; i < stepDetector.stepCount; i++)
                {
                    if (tmpStepCount > 0) tmpStepCount--;

                    Vector3 targetPos = new Vector3(transform.position.x + movePixelValue * 100f, transform.position.y, transform.position.z);
                    Tweener tween = transform.DOMove(targetPos, 0.3f);
                    tween.OnUpdate(() => { if (!canMoveRight) { tween.Kill(); } });

                    await Task.Delay(200);

                    if (tmpStepCount == 0)
                    {
                        movingRight = false;
                        stepDetector.stepCount = 0;
                    }
                }
            }
        }
        else if (playerFace.localEulerAngles == new Vector3(0, 0, 90) && canMoveLeft && /*leftPos &&*/ !movingLeft)
        {
            if (stepDetector.stepCount > 0)
            {
                tmpStepCount = stepDetector.stepCount;
                movingLeft = true;

                for (int i = 0; i < stepDetector.stepCount; i++)
                {
                    if (tmpStepCount > 0) tmpStepCount--;

                    Vector3 targetPos = new Vector3(transform.position.x - movePixelValue * 100f, transform.position.y, transform.position.z);
                    Tweener tween = transform.DOMove(targetPos, 0.3f);
                    tween.OnUpdate(() => { if (!canMoveLeft) { tween.Kill(); } });

                    await Task.Delay(200);

                    if (tmpStepCount == 0)
                    {
                        movingLeft = false;
                        stepDetector.stepCount = 0;
                    }
                }
            }
        }
        else if (playerFace.localEulerAngles == new Vector3(0, 0, 0) && canMoveUp && /*upPos &&*/ !movingUp)
        {
            if (stepDetector.stepCount > 0)
            {
                tmpStepCount = stepDetector.stepCount;
                movingUp = true;

                for (int i = 0; i < stepDetector.stepCount; i++)
                {
                    if (tmpStepCount > 0) tmpStepCount--;

                    Vector3 targetPos = new Vector3(transform.position.x, transform.position.y + movePixelValue * 100f, transform.position.z);
                    Tweener tween = transform.DOMove(targetPos, 0.3f);
                    tween.OnUpdate(() => { if (!canMoveUp) { tween.Kill(); } });
                    await Task.Delay(200);

                    if (tmpStepCount == 0)
                    {
                        movingUp = false;
                        stepDetector.stepCount = 0;
                    }
                }
            }
        }
        else if (playerFace.localEulerAngles == new Vector3(0, 0, 180) && canMoveDown && /*downPos &&*/ !movingDown)
        {
            if (stepDetector.stepCount > 0)
            {
                tmpStepCount = stepDetector.stepCount;
                movingDown = true;

                for (int i = 0; i < stepDetector.stepCount; i++)
                {
                    if (tmpStepCount > 0) tmpStepCount--;

                    Vector3 targetPos = new Vector3(transform.position.x, transform.position.y - movePixelValue * 100f, transform.position.z);
                    Tweener tween = transform.DOMove(targetPos, 0.3f);
                    tween.OnUpdate(() => { if (!canMoveDown) { tween.Kill(); } });
                    await Task.Delay(200);

                    if (tmpStepCount == 0)
                    {
                        movingDown = false;
                        stepDetector.stepCount = 0;
                    }
                }
            }
        }
        else
        {
            if (stepDetector.stepCount > 0)
            {
                stepDetector.stepCount = 0;
            }
        }
    }

    async void MovePlayerWithKeyboardInput()
    {
        float multipleValue = 0.6f;        

        if(Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (GameUI.isAcceptedPolice)
            {
                if (isWalking)
                {
                    multipleValue = 240f;
                }
                else
                {
                    multipleValue = 120f;
                }
            }
            else
            {
                if (isWalking)
                {
                    multipleValue = 120f;
                }
                else
                {
                    multipleValue = 60f;
                }
            }
        }
        else
        {
            if (GameUI.isAcceptedPolice)
            {
                if (isWalking)
                {
                    multipleValue = 2.4f;
                }
                else
                {
                    multipleValue = 1.2f;
                }
            }
            else
            {
                if (isWalking)
                {
                    multipleValue = 1.2f;
                }
                else
                {
                    multipleValue = 0.6f;
                }
            }
        }

        if (Input.GetKey(KeyCode.D) && canMoveRight && !movingRight)
        {
            movingRight = true;    

            Vector3 targetPos = new Vector3(transform.position.x + movePixelValue * multipleValue, transform.position.y, transform.position.z);

            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveRight) { tween.Kill(); } });
            playerFace.localEulerAngles = new Vector3(0, 0, -90);

            await Task.Delay(200);
            movingRight = false;
        }
        else if (Input.GetKey(KeyCode.A) && canMoveLeft && !movingLeft)
        {
            movingLeft = true;           

            Vector3 targetPos = new Vector3(transform.position.x - movePixelValue * multipleValue, transform.position.y, transform.position.z);
            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveLeft) { tween.Kill(); } });
            playerFace.localEulerAngles = new Vector3(0, 0, 90);

            await Task.Delay(200);
            movingLeft = false;
        }
        else if (Input.GetKey(KeyCode.W) && canMoveUp && !movingUp)
        {
            movingUp = true;        

            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y + movePixelValue * multipleValue, transform.position.z);
            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveUp) { tween.Kill(); } });
            playerFace.localEulerAngles = new Vector3(0, 0, 0);

            await Task.Delay(200);
            movingUp = false;
        }
        else if (Input.GetKey(KeyCode.S) && canMoveDown && !movingDown)
        {
            movingDown = true;     

            Vector3 targetPos = new Vector3(transform.position.x, transform.position.y - movePixelValue * multipleValue, transform.position.z);
            Tweener tween = transform.DOMove(targetPos, 0.3f);
            tween.OnUpdate(() => { if (!canMoveDown) { tween.Kill(); } });
            playerFace.localEulerAngles = new Vector3(0, 0, 180);

            await Task.Delay(200);
            movingDown = false;
        }   
    }

    public void RotatePlayer(Vector3 mRotation)
    {
        playerFace.localEulerAngles = mRotation;
    }

    public void RotateCamera(Vector3 mRotation, bool trueValue)
    {
        if (movePhysically && trueValue) cameraTransform.DOLocalRotate(mRotation, 0.3f).SetEase(Ease.Linear);
    }

    void GetMaxValue()
    {
        maxAccValue = Mathf.Max(accelerometerValues.ToArray());
    }

    public void SelectAnimation(int indexJob, int indexAnimation)
    {
        anim.runtimeAnimatorController = characterAni[indexJob - 1] as RuntimeAnimatorController;
        
        if(indexAnimation == 0)
        {
            anim.Play("Idle");
        }
        else
        {
            anim.Play("Walk");
        }                
    }  

    IEnumerator DelayToShowAnimations()
    {
        yield return new WaitForSeconds(3f);
        anim = this.transform.GetChild(1).gameObject.GetComponent<Animator>();
        SelectAnimation(jobIndex, 1);
    }

    #region IPunObservable implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.jobIndex);
            stream.SendNext(this.itemInventory);
            stream.SendNext(this.playerName);
            stream.SendNext(this.playerRotation);
        }
        else
        {
            // Network player, receive data
            this.jobIndex = (int)stream.ReceiveNext();
            this.itemInventory = (string)stream.ReceiveNext();
            this.playerName = (string)stream.ReceiveNext();
            this.playerRotation = (Vector3)stream.ReceiveNext();          
        }
    }
    #endregion
}
