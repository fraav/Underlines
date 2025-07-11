using UnityEngine;

public class BotonCarta : MonoBehaviour
{
    public Animator anim;

    public void SetearBoolTrue(string state)
    {
        anim.SetBool(state, true);
    }

        public void SetearBoolFalse(string state)
    {
        anim.SetBool(state, false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
