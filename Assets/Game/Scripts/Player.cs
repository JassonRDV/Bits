using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField] private bool useRootMotion;
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Slider _lifeBar;
    private float currentLife;
    public Animator Anim { get; private set; }
    private Rigidbody rigg;

    // Guarda direção calculada em Update e usada em FixedUpdate
    private Vector3 movementDirection;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Anim.applyRootMotion = useRootMotion;
        _lifeBar.value = _lifeBar.maxValue = currentLife = 100;
        rigg = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var mobilejoystick = MobileJoystick.GetJoystickAxis();
        var joystick = mobilejoystick.magnitude > 0 ? mobilejoystick : JoystickAxis();
        movementDirection = new Vector3(joystick.x, 0, joystick.y);

        // animação (mantém o uso do damping)
        Anim.SetFloat("Movement", joystick.magnitude, .25f, Time.deltaTime);

        // rotação suave no Update (não afeta física)
        if (movementDirection.magnitude != 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);

        // atualiza barra de vida no mundo (assume Canvas em World Space)
        _lifeBar.transform.position = new Vector3(transform.position.x, _lifeBar.transform.position.y, transform.position.z);
        _lifeBar.value = Mathf.Lerp(_lifeBar.value, currentLife, Time.deltaTime * 2.5f);
    }

    private void FixedUpdate()
    {
        if (!useRootMotion && rigg != null)
        {
            // Atribui velocidade em unidades/segundo (não multiplicar por Time.deltaTime)
            rigg.velocity = movementDirection * speed;
            // Alternativa mais física e controlada:
            // rigg.MovePosition(rigg.position + movementDirection * speed * Time.fixedDeltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        if (currentLife <= 0) Anim.SetTrigger("Death");
        else Anim.SetTrigger("Hit");
    }
    private Vector2 JoystickAxis()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        return new Vector2(x, y);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GroundButton Event))
        {
            Event.Cancell();
            Event.StartCoroutine(Event.Fill(transform));
        }
        if (other.TryGetComponent(out EnemyDamage Enemy))
        {
            Enemy.gameObject.SetActive(false);
            TakeDamage(30);
        }
    }
    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.TryGetComponent(out GroundButton Event))
    //        Event.Cancell();
    //}
}
