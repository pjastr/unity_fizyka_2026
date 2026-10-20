using UnityEngine;

namespace Fizyka
{
    /// <summary>
    /// Postać na komponencie CharacterController (bez Rigidbody!).
    /// Ruch: Player/Move (WASD), skok: Player/Jump (Spacja), bieg: Player/Sprint (Shift).
    /// Grawitację liczymy sami, a CharacterController.Move() rozwiązuje kolizje,
    /// uwzględniając Slope Limit (maks. nachylenie) i Step Offset (maks. wysokość stopnia).
    /// CharacterController sam NIE pcha Rigidbody – robimy to w OnControllerColliderHit.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour
    {
        public float speed = 5f;
        public float sprintSpeed = 9f;
        public float jumpHeight = 1.5f;
        public float pushPower = 2f;
        [Tooltip("Kamera, względem której liczymy kierunek ruchu")]
        public Transform cameraTransform;

        CharacterController m_Controller;
        Vector3 m_Velocity;
        WorldLabel m_Label;
        CollisionFlags m_Flags;

        void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Label = GetComponent<WorldLabel>();
        }

        void OnEnable() => DemoHud.SuppressMainAction = true;
        void OnDisable() => DemoHud.SuppressMainAction = false;

        void Update()
        {
            float gravity = Physics.gravity.y;
            bool grounded = m_Controller.isGrounded;
            if (grounded && m_Velocity.y < 0f)
                m_Velocity.y = -2f; // lekko "dociskamy" do podłoża

            Vector2 input = DemoInput.ReadVector2(DemoInput.Move);
            Vector3 fwd = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            fwd.y = 0f;
            fwd.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, fwd);
            Vector3 move = fwd * input.y + right * input.x;
            if (move.sqrMagnitude > 1f) move.Normalize();

            float s = DemoInput.IsPressed(DemoInput.Sprint) ? sprintSpeed : speed;
            if (move.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 12f * Time.deltaTime);

            if (DemoInput.PressedThisFrame(DemoInput.Jump) && grounded)
                m_Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            m_Velocity.y += gravity * Time.deltaTime;

            // jeden wywołanie Move: ruch poziomy + pionowy
            m_Flags = m_Controller.Move((move * s + Vector3.up * m_Velocity.y) * Time.deltaTime);

            if (transform.position.y < -20f)
            {
                m_Controller.enabled = false;
                transform.position = new Vector3(0, 2, -8);
                m_Controller.enabled = true;
                m_Velocity = Vector3.zero;
            }

            if (m_Label != null)
                m_Label.extra = $"isGrounded = {m_Controller.isGrounded}\nvelocity = {m_Controller.velocity.magnitude:0.0} m/s\nCollisionFlags = {m_Flags}";
        }

        // Wywoływane, gdy kontroler w trakcie Move() uderzy w collider.
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic)
                return;
            if (hit.moveDirection.y < -0.3f)
                return; // nie "wciskamy" obiektów pod sobą

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.linearVelocity = pushDir * pushPower;
        }
    }
}
