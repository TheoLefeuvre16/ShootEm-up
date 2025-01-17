using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAOE2 : Bullet
{
    // Start is called before the first frame update
    [SerializeField]
    private float speed = 1000f;
    private Camera m_MainCamera;

    private bool onHit = false;
    public AudioSource diedAudio;

    private void Awake()
    {
        m_MainCamera = Camera.main;
        damage = 50 + +PlayerPrefs.GetInt("SecondWeaponWeaponLevel04") *2; //recuperation du niveau de l'arme pour augmenter les degats
        shootRate = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        bulletMovment();

        Vector3 screenPos = m_MainCamera.WorldToViewportPoint(transform.position);
        if (screenPos.y > 1F)
        {
            Destroy(gameObject);
        }
    }
    void bulletMovment()
    {
        Vector3 moveDir = new Vector3(0, 0, 1);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.TransformDirection(new Vector3(0, 0, speed) * 0.007f);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "enemy")
        {
            Destroy(gameObject, 1f);
        }
        else
        {
            //si une b alle la touche, enormes degats et on augmente la taille
            if(other.GetComponent<Bullet>() && onHit == false)
            {
                damage = 250;
                transform.localScale += new Vector3(8, 8, 8);
                onHit = true;
                Destroy(gameObject, 1f);
                // StartCoroutine("died");
                diedAudio.Play();

            }
        }
    }
    IEnumerator died() //La coroutine sert � d�sactiver partiellement le monstre pour jouer le son de mort avant de le supprimer pour de bons � la fin
    {
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

}
