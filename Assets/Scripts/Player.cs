using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

    public float moveSpeed;
    public float rotateAmount;
    public GameObject winText;

    Rigidbody2D rb;

    float rot;  

    int score;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
    }

    void Update() {
        if (Input.GetMouseButton(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (mousePos.x < 0) {
                rot = rotateAmount;
            } else {
                rot = -rotateAmount;
            }

            transform.Rotate(0, 0, rot);
        }
    }

    void FixedUpdate() {
        rb.velocity = transform.up * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Food") {
            Destroy(collision.gameObject);

            score++;

            if (score >= 5) {
                print("Level Complete");
                winText.SetActive(true);
            }
        } else if (collision.gameObject.tag == "Danger") {
            SceneManager.LoadScene("GameScene");
        }
    } 
}