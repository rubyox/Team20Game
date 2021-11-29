using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Unit : MonoBehaviourPun
{
    public int curHp;               // current health
    public int maxHp;               // maximum health
    public float moveSpeed;         // units per second when moving
    public int minDamage;           // minimum damage
    public int maxDamage;           // maximum damage

    public int maxMoveDistance;     // max distance we can move per turn
    public int maxAttackDistance;   // max distance we can attack

    public bool usedThisTurn;       // has this unit been used this turn?

    public GameObject selectedVisual;   // selection circle sprite
    public SpriteRenderer spriteVisual; // sprite of the unit

    [Header("UI")]
    public Image healthFillImage;       // health bar fill image

    [Header("Sprite Variants")]
    public Sprite leftPlayerSprite;     // left player sprite (blue)
    public Sprite rightPlayerSprite;    // right player sprite (red)

    // called when the unit is spawned in
    [PunRPC]
    void Initialize (bool isMine)
    {
        if(isMine) PlayerController.me.units.Add(this);
        else GameManager.instance.GetOtherPlayer(PlayerController.me).units.Add(this);

        healthFillImage.fillAmount = 1.0f;

        // set sprite variant
        spriteVisual.sprite = transform.position.x < 0 ? leftPlayerSprite : rightPlayerSprite;

        // rotate the unit
        spriteVisual.transform.up = transform.position.x < 0 ? Vector3.left : Vector3.right;
    }

    // can we be selected?
    public bool CanSelect ()
    {
        if(usedThisTurn) return false;
        else return true;
    }

    // can we move to this position?
    public bool CanMove (Vector3 movePos)
    {
        if(Vector3.Distance(transform.position, movePos) <= maxMoveDistance)
            return true;
        else return false;
    }

    // can we attack this position?
    public bool CanAttack (Vector3 attackPos)
    {
        if(Vector3.Distance(transform.position, attackPos) <= maxAttackDistance)
            return true;
        else return false;
    }

    // called we either select or un-select the unit
    public void ToggleSelect (bool selected)
    {
        selectedVisual.SetActive(selected);
    }

    public void Move (Vector3 targetPos)
    {
        usedThisTurn = true;

        // rotate sprite
        Vector3 dir = (transform.position - targetPos).normalized;
        spriteVisual.transform.up = dir;

        StartCoroutine(MoveOverTime());

        IEnumerator MoveOverTime ()
        {
            while(transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    // attacks the requested enemy unit
    public void Attack (Unit unitToAttack)
    {
        usedThisTurn = true;
        unitToAttack.photonView.RPC("TakeDamage", PlayerController.enemy.photonPlayer, Random.Range(minDamage, maxDamage + 1));
    }

    // called when we get attacked by another unit
    [PunRPC]
    void TakeDamage (int damage)
    {
        curHp -= damage;

        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
        else
        {
            // update health UI
            photonView.RPC("UpdateHealthBar", RpcTarget.All, (float)curHp / (float)maxHp);
        }
    }

    // updates the UI health bar
    [PunRPC]
    void UpdateHealthBar (float fillAmount)
    {
        healthFillImage.fillAmount = fillAmount;
    }

    // called when the unit's health reaches 0
    [PunRPC]
    void Die ()
    {
        if(!photonView.IsMine)
            PlayerController.enemy.units.Remove(this);
        else
        {
            PlayerController.me.units.Remove(this);

            // check the win condition
            GameManager.instance.CheckWinCondition();

            // destroy the unit across the network
            PhotonNetwork.Destroy(gameObject);
        }
    }
}