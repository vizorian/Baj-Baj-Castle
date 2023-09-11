using JetBrains.Annotations;
using UnityEngine;

namespace Game_Logic;

[UsedImplicitly]
public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;

    [UsedImplicitly]
    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Loader.LoaderCallback(Loader.LoadState);
        }
    }
}