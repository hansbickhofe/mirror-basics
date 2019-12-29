using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour {

    public float Speed = 5f;

    void Update () {
        float h = Input.GetAxisRaw ("Horizontal");

        GetComponent<Rigidbody> ().velocity = new Vector2 (h, 0) * Speed;
    }

}