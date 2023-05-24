using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region variable
    Rigidbody2D myrigidbody;
    Animator animator;
    [HideInInspector]
    public bool isControl = true;

    [Header("Walk")]
    public float walk_speed = 3;

    [Header("Jump")]
    public Transform jump_Pos;
    public float checkRadius;
    public float jumpPower = 1;
    public LayerMask isTile;
    private bool isGround;
    #endregion


    FixedJoint2D fixjoint;

    void Start()
    {
        fixjoint = GetComponent<FixedJoint2D>();
        myrigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fixjoint.connectedBody = null;
            fixjoint.enabled = false;
        }
    }
    private void FixedUpdate()
    {
        if (isControl)
        {
            Walk();
            isGround = Physics2D.OverlapCircle(jump_Pos.position, checkRadius, isTile);
                animator.speed = 1;
                Jump();            
        }
        else
        {
            myrigidbody.velocity = Vector2.zero;
        }
    }
    private void Walk()
    {
        float hor = Input.GetAxis("Horizontal");
        myrigidbody.velocity= (new Vector2((hor) *walk_speed * Time.deltaTime, myrigidbody.velocity.y));
       
        if (hor > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            animator.SetBool("walk", true);
        }
        else if (hor < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            animator.SetBool("walk", true);
        }
        else
        {
            animator.SetBool("walk", false);
            
        }
    }
    private void Jump()
    {
        if (isGround == true) {
            if (Input.GetAxis("Jump") != 0) {
                myrigidbody.velocity = Vector2.up * jumpPower;
                animator.SetTrigger("Jump");
            }
            isRope = false;
        }
    }

    bool isRope = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Rope") && !isRope)
        {            
            Rigidbody2D rig = collision.gameObject.GetComponent<Rigidbody2D>();
            fixjoint.enabled = true;
            fixjoint.connectedBody = rig;
            isRope = true;
        }
    }
}
