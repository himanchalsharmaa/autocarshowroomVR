using System.Collections.Generic;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Unity.XR.SDK
{
    public class DisplaySampleXRLoader : XRLoaderHelper
    {
        private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors =
            new List<XRDisplaySubsystemDescriptor>();

        public override bool Initialize()
        {
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "Display Sample");
            return true;
        }

        public override bool Start()
        {
            StartSubsystem<XRDisplaySubsystem>();

            return true;
        }

        public override bool Stop()
        {
            StopSubsystem<XRDisplaySubsystem>();

            return true;
        }

        public override bool Deinitialize()
        {
            DestroySubsystem<XRDisplaySubsystem>();

            return true;
        }
    }
}
