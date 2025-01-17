using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
public class EnemyBossBody : Entity
{
    static Camera m_mainCamera;
    public Transform target;

    public delegate void KilledEnemy();
    public event KilledEnemy OnKilledEnemy;
    public delegate void BossMusic();
    public event BossMusic OnBossMusic;
    //bullet
    private float shootRate = 500f;
    public GameObject bulletPrefab;
    public int paternFlag = 0;
    private float patern = 0f;
    private float paternLimiter = 0f;
    private bool flag = false;
    private bool flagLimiter = false;
    //fin bullet

    private Color couleur;
    public GameObject body;
    private bool isnotDied = true;

    public Slider lifeBar;

    public Stopwatch timer;
    public Stopwatch timerDeath;
    private Stopwatch timerPatern;
    private bool timerFlag = false;

    public Transform[] bulletSpawn = new Transform[3];
    private bool shield = true;

    public delegate void InterfaceVictory();
    public event InterfaceVictory OnInterfaceVictory;



    private void Awake()
    {
        timer = new Stopwatch();
        timer.Start();
        timerPatern = new Stopwatch();
        timerPatern.Start();
        lifeBar.value = 1;
        timerDeath = new Stopwatch();

        timerDeath.Start();
    }

    public void Initalize(PlayerController player, MusicManager zicManager, UserInterface userInterface)
    {
        OnKilledEnemy += player.OnBulletHit;
        ////////On mute la musique de fond et on met la musique de BOSS
        OnBossMusic += zicManager.BossOnMap;
        OnBossMusic?.Invoke();
        OnBossMusic += zicManager.BossNoMoreOnMap;

        OnInterfaceVictory += userInterface.setVictoryScene;
        ////////
        maxHealth = 1000;
        currentHealth = maxHealth;
        m_mainCamera = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != 0 && isnotDied)
        {
            lifeBar.value = ((float)currentHealth / (float)maxHealth);

            Vector3 screenPos = EnemyBossBody.m_mainCamera.WorldToViewportPoint(target.position);
            if (this.transform.position.y > 12) // on fais stagner le boss � cette hauteur
            {
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - (5F * Time.deltaTime), this.transform.position.z);
            }
            if (screenPos.y < 0.0F)
            {
                Destroy(gameObject);
            }
            if (timer.ElapsedMilliseconds >= 1000 / (shootRate * Time.timeScale)) // on g�re les tirs en fonction du shootRate
            {
                //ON ECHANGE LES PATERNES DE TIRS, le 2eme paterne doit jouer un certains nb de secondes
                if (Random.Range(0, 10000) > 9700 && timerFlag == false)
                {
                    paternFlag = 1;
                    timerFlag = true;
                    timerPatern.Restart();
                }
                if (timerPatern.ElapsedMilliseconds >= 2000 && timerFlag == true)
                {
                    timerFlag = false;
                    paternFlag = 0;
                }

                if (currentHealth > 500 && paternFlag == 0)
                {
                    body.GetComponent<AudioSource>().volume = 0.05f;
                    if (body.GetComponent<AudioSource>().isPlaying == false)
                        body.GetComponent<AudioSource>().Play();
                    shootRate = 10F;
                    Instantiate(bulletPrefab, bulletSpawn[0].position, Quaternion.Euler(0f, 0f, Random.Range(-50,50)));
                }
                else if (currentHealth > 500 && paternFlag == 1)
                {
                    shootRate = 500F;
                    if (body.GetComponent<AudioSource>().isPlaying == false)
                    {
                        body.GetComponent<AudioSource>().Play();
                    }
                    if (flag == false)
                        patern = patern + 20f;
                    else
                        patern = patern - 20f;

                    if (patern > 80f || patern < -80f)
                    {
                        if (flag == false)
                            flag = true;
                        else
                            flag = false;
                    }
                    Instantiate(bulletPrefab, bulletSpawn[0].position, Quaternion.Euler(0f, 0f, patern));
                }
                //ATTAQUE ULTIME DU BOSS SI IL EST A LA MOITIE DE SA VIE
                if (currentHealth <= 500)
                {
                    shootRate = 500F;
                    if (body.GetComponent<AudioSource>().isPlaying == false)
                    {
                        body.GetComponent<AudioSource>().Play();
                    }
                    if (flag == false)
                        patern = patern + 20f + paternLimiter;
                    else
                        patern = patern - 20f + paternLimiter;

                    if (flagLimiter == false)
                        paternLimiter = paternLimiter + 0.1f;
                    else
                        paternLimiter = paternLimiter - 0.1f;

                    if (paternLimiter > 20f || paternLimiter < -20f)
                    {
                        if (flagLimiter == false)
                            flagLimiter = true;
                        else
                            flagLimiter = false;
                    }
                    if (patern > 80f + paternLimiter || patern < -80f + paternLimiter)
                    {
                        if (flag == false)
                            flag = true;
                        else
                            flag = false;
                    }
                    Instantiate(bulletPrefab, bulletSpawn[0].position, Quaternion.Euler(0f, 0f, patern));
                    
                }
                //FIN ATTAQUE ULTIME DU BOSS
                timer.Restart();
            }
        }
    }

    IEnumerator Hurt()//coroutine pour faire clignoter le monstre lorsqu'il subit des d�gats
    {
        if (isnotDied != false)
        {
            couleur = body.GetComponent<MeshRenderer>().material.GetColor("_BaseColor");
            couleur.r = 1f;
            body.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", couleur);
            yield return new WaitForSeconds(0.15f);
            couleur.r = 0.302f;
            if (isnotDied)
                body.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", couleur);
        }
    }

    public void NoShield()
    {
        shield = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet" && shield == false)
        {
            if (isnotDied)
                StartCoroutine("Hurt");
            currentHealth -= other.GetComponent<Bullet>().GetBulletDamage();

        }

        if (currentHealth <= 0)
        {
            OnKilledEnemy?.Invoke();
            body.GetComponent<SphereCollider>().enabled = false;
            ////////////On d�sactive la musique de Boss et on d�mute la musique de fond
            OnBossMusic?.Invoke();
            ////////////
            StartCoroutine("died");

        }

    }
    IEnumerator died() //La coroutine sert � d�sactiver partiellement le monstre pour jouer le son de mort avant de le supprimer pour de bons � la fin
    {
        isnotDied = false;
        this.GetComponent<AudioSource>().Play();
        Destroy(body);
        PlayerPrefs.SetInt("BossDead", 1);
        if(timerDeath.ElapsedMilliseconds <= 20000)
        PlayerPrefs.SetInt("Success9", 1);

        yield return new WaitForSeconds(1f);
        OnInterfaceVictory?.Invoke();
        Destroy(gameObject);
    }
}
