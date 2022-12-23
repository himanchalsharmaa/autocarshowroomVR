using UnityEngine;


public class MenuAnim : MonoBehaviour
{
    public GameObject Panel;

    public void CarSettigns()
    {
        if (Panel != null)
        {
            Panel.GetComponent<Animator>().SetTrigger("Pop");
        }
    }
}
