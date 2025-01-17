using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class EnemyTorp : Entity
{
    static Camera m_mainCamera;
    public Transform target;
    public Transform torpRotation;
    public BonusScript bonus;
    public BonusScript bonusFire;
    public BonusScript bonusTime;
    public BonusScript bonusHeal;

    public delegate void KilledEnemy();
    public event KilledEnemy OnKilledEnemy;
    //bullet
    public GameObject bulletPrefab;
    public Stopwatch timer;
    public Transform[] bulletSpawn = new Transform[3];

    private Color couleur;
    private float shootRate;

    public GameObject torpedo;
    private bool isnotDied = true;

    private void Awake()
    {
        timer = new Stopwatch();
        timer.Start();
        shootRate = bulletPrefab.GetComponent<BulletEnemy>().GetBulletRate();
    }

    public void Initalize(PlayerController player)
    {
        OnKilledEnemy += player.OnBulletHit;

        maxHealth = 200;
        currentHealth = maxHealth;
        m_mainCamera = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != 0 && isnotDied)
        {
            Vector3 screenPos = EnemyTorp.m_mainCamera.WorldToViewportPoint(target.position);
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - (5F * Time.deltaTime), this.transform.position.z); //on gere les d�placements
            if (screenPos.y < 0.0F) // si il sort de l'�cran il meurt
            {
                Destroy(gameObject);
            }
            if (timer.ElapsedMilliseconds >= 1000 / (shootRate * Time.timeScale)) // on g�re les tirs de l'enemi
            {
                for (int i = 0; i < bulletSpawn.Length; i++) // on tire les 3 balles avec les deux sur les cot�s qui changent d'angle
                {
                    if (i == 0)
                    {
                        Instantiate(bulletPrefab, bulletSpawn[i].position, Quaternion.Euler(0, 0, torpRotation.rotation.z * 90));
                    }
                    else if (i == 1)
                    {
                        Instantiate(bulletPrefab, bulletSpawn[i].position, Quaternion.Euler(0, 0, torpRotation.rotation.z * 90));
                    }
                }
                timer.Restart();
            }
        }
    }

    IEnumerator Hurt()//coroutine pour faire clignoter le monstre lorsqu'il subit des d�gats
    {
        couleur = this.GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
        couleur.r = 1f;

        this.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", couleur);
        yield return new WaitForSeconds(0.15f);
        couleur.r = 0.509434f;
        this.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", couleur);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            currentHealth = 0;
        }
        if (other.gameObject.tag == "Bullet")
        {
            StartCoroutine("Hurt");
            currentHealth -= other.GetComponent<Bullet>().GetBulletDamage();
        }

        if (currentHealth <= 0)
        {
            this.GetComponent<BoxCollider>().enabled = false;
            OnKilledEnemy?.Invoke();
            StartCoroutine("died");
            if (other.GetComponent<BulletFragment>())
            {
                for (int i = 0; i < 8; i++)
                    Instantiate(other.gameObject, other.transform.position, Quaternion.Euler(i * 45, 90f, 90f));
                other.gameObject.SetActive(false);
            }
            //Debug.Log("j'appelle levent");

        }

    }
    IEnumerator died() //La coroutine sert � d�sactiver partiellement le monstre pour jouer le son de mort avant de le supprimer pour de bons � la fin
    {
        isnotDied = false;
        this.GetComponent<MeshRenderer>().enabled = false;
        this.GetComponent<AudioSource>().Play();
        Destroy(torpedo);
        if (Random.Range(0, 10) <= 6)
        {
            BonusScript m = Instantiate(bonus) as BonusScript;
            m.transform.position = target.position;
        }
        else if (Random.Range(0, 10) <= 2)
        {
            BonusScript m = Instantiate(bonusFire) as BonusScript;
            m.transform.position = target.position;
        }
        else if (Random.Range(0, 10) <= 5)
        {
            BonusScript m = Instantiate(bonusTime) as BonusScript;
            m.transform.position = target.position;
        }
        else
        {
            BonusScript m = Instantiate(bonusHeal) as BonusScript;
            m.transform.position = target.position;
        }
        int currKilled = PlayerPrefs.GetInt("Success8") + 1;
        PlayerPrefs.SetInt("Success8", currKilled);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
