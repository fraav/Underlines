using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public int moneyammount = 200;

    public void Addmoney()
    {
        EconomyManager.Instance.AddMoney(moneyammount);
    }
}
