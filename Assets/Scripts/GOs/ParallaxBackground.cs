using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] ParralaxLayer[] layers;
    [SerializeField] float spriteWidth;
    private Camera cam;
    private Vector2 camStartPos;

    void Start()
    {
        cam = Camera.main;
        camStartPos = cam.transform.position;
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].startPosition = layers[i].gameObject.transform.position;
        }
    }

    void FixedUpdate() {
        for (int i = 0; i < layers.Length; i++)
        {
            float parallaxX = (cam.transform.position.x - camStartPos.x) * layers[i].parralaxEffectX;
            float parallaxY = (cam.transform.position.y - camStartPos.y) * layers[i].parralaxEffectY;
            float newPosX = layers[i].startPosition.x + parallaxX;

            if (Mathf.Abs(cam.transform.position.x - newPosX) >= spriteWidth)
            {
                layers[i].startPosition.x += spriteWidth * Mathf.Sign(cam.transform.position.x - newPosX);
            }

            Vector3 newPosition = new Vector3(newPosX, layers[i].startPosition.y + parallaxY, layers[i].gameObject.transform.position.z);
            layers[i].gameObject.transform.position = newPosition;
        }
    }
}

[System.Serializable]
public struct ParralaxLayer {
    public Vector3 startPosition;
    public GameObject gameObject;
    public float parralaxEffectX;
    public float parralaxEffectY;
}