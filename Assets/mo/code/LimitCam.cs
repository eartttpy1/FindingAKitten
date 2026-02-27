using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LimitCam : MonoBehaviour
{
    public GameObject Player;

    void LateUpdate() {
        transform.position = new Vector3(Player.transform.position.x, 14, Player.transform.position.z);
    }

}
