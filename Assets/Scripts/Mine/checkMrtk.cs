using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkMrtk : MonoBehaviour
{

    private float elapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Rendering.TextureXR.maxViews = 2;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 2.0f)
        {
            elapsed = elapsed % 2f;
            IMixedRealityDataProviderAccess dataProviderAccess = CoreServices.InputSystem as IMixedRealityDataProviderAccess;

            if (dataProviderAccess != null)
            {
                IInputSimulationService inputSimulation =
                    dataProviderAccess.GetDataProvider<IInputSimulationService>();

                if (inputSimulation != null)
                {
                    //Debug.Log(inputSimulation.EyeGazeSimulationMode);
                }
                else { Debug.Log("NO INP SIM"); }
            }
            else
            {
                Debug.Log("NO DATA PROV ACC");
            }
        }
    }

}
