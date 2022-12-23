using UnityEngine;

public class HandleButton : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
         _animator = GetComponent<Animator>();  
    }

    public void HandleButtonPressed(string lsButton)
    {
        if (_animator.GetBool(lsButton))
        {
            _animator.SetBool(lsButton, false);
        }
        else if (!_animator.GetBool(lsButton))
        {
            _animator.SetBool(lsButton, true);
        }
        return;
    }
}