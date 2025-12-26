using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame.Models;
using System.Security.Cryptography.X509Certificates;

public class UpgradeUnit : MonoBehaviour
{
    public CardStats cardStats;

    public int healthUpgrade = 50;
    public int maxHealthUpgrade = 50;
    public int attackUpgrade = 5;
    private int maxUpgrades = 10;
    private int upgradesApplied = 0;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI HealthAdditionText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI attackAdditionText;

    public Button upgradeButton;


    void Start()
    {
        HealthAdditionText.text = maxHealthUpgrade.ToString();
        attackAdditionText.text = attackUpgrade.ToString();


       
        if (cardStats == null)
        {  
             GameObject cardObject = GameObject.FindGameObjectWithTag("PlayerCard");
             if (cardObject != null) cardStats = cardObject.GetComponent<CardStats>();
                cardStats = new CardStats();
        }

        UpdateUI();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(ApplyUpgrade);
        }


    }

    public void ApplyUpgrade()
    {
        if (upgradesApplied <= maxUpgrades)
        {
            cardStats.health += healthUpgrade;
            cardStats.attack += attackUpgrade;
            cardStats.maxHealth += maxHealthUpgrade;

            upgradesApplied++;
        }

        else
        {
            Debug.Log("Максимум апгрейдов хватит тыкать");
        }

            UpdateUI();

    }

    private void UpdateUI()
    {
        if (cardStats == null) return;

        if (healthText != null)
            healthText.text =  cardStats.health.ToString();
    

        if (attackText != null)
            attackText.text = cardStats.attack.ToString();
    }

    public void SetCardStats(CardStats newStats)
    {
        cardStats = newStats;
    }

 
    void Update()
    {
        
    }
}
