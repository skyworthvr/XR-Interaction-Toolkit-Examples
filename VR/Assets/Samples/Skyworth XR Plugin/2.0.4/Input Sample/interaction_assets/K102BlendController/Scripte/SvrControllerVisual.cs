using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.XR.Skyworth
{
    public enum BatteryLevel
    {
        CriticalLow = 0,
        Low = 1,
        Medium = 2,
        AlmostFull = 3,
        Full = 4,
        Count = 5
    };
    public class SvrControllerVisual : MonoBehaviour
    {

        public XRNode m_node;
        private Animator s_ControllerAnimatior;
        private Material s_Material;
        private GameObject m_vroot;
        private Transform m_RayOrigin;
        private XRRayInteractor rayInteractor;
        public static List<GameObject> ContollerVisual = new List<GameObject>();
        // Start is called before the first frame update
        void Awake() 
        {
            s_ControllerAnimatior = GetComponentInChildren<Animator>();
            s_Material = GetComponentInChildren<Renderer>().material;
            m_vroot = transform.GetChild(0).gameObject;
            m_RayOrigin = transform.GetChild(1);
            rayInteractor = GetComponentInParent<XRRayInteractor>();
#if UNITY_INTERACTION_TOOLKIT_200
            if (rayInteractor) rayInteractor.rayOriginTransform = m_RayOrigin;
#endif
            ContollerVisual.Add(gameObject);
        }
        void Start()
        {
            
        }
       
        
        // Update is called once per frame
        void Update()
        {
            InputDevice ControllerDevice = InputDevices.GetDeviceAtXRNode(m_node);
            
            if (!ControllerDevice.isValid || ControllerDevice.manufacturer != gameObject.name) 
            {
                if(m_vroot.activeInHierarchy) 
                    m_vroot.SetActive(false);
                return;
            }
            if (ControllerDevice.TryGetFeatureValue(CommonUsages.trackingState, out InputTrackingState trackingState)) 
            {
                if ((trackingState & (InputTrackingState.Position | InputTrackingState.Rotation)) != 0)
                {
                    if (!m_vroot.activeInHierarchy)
                    {
                        m_vroot.SetActive(true);
                        if (ControllerDevice.manufacturer == "SkywortXrV2")
                        {
                            if (SkyworthSettings.Instance.m_ControllerModel == SkyworthSettings.ControllerModel.AIM_MODEL)
                            {
                                m_vroot.transform.localRotation = Quaternion.Euler(58.098f, 0, 0);
                                m_vroot.transform.localPosition = new Vector3(0, -0.05414088f, -0.06013277f);
                                m_RayOrigin.transform.localPosition = new Vector3(0, 0, 0.03738f);
                            }
                            else if (SkyworthSettings.Instance.m_ControllerModel == SkyworthSettings.ControllerModel.GRIP_MODEL)
                            {
                                m_vroot.transform.localRotation = Quaternion.Euler(90.0f, 0, 0);
                                m_vroot.transform.localPosition = new Vector3(0, -0.0078f, -0.0738f);
                                m_RayOrigin.transform.localPosition = new Vector3(0f, -0.0177f, 0.0398f);
                            }
                        }
                        else
                        {
                            if (SkyworthSettings.Instance.m_ControllerModel == SkyworthSettings.ControllerModel.AIM_MODEL)
                            {
                                m_vroot.transform.localRotation = Quaternion.Euler(47.818f, 0, 0);
                                m_vroot.transform.localPosition = new Vector3(0.0065f, -0.0603f, -0.0492f);
                                m_RayOrigin.transform.localPosition = new Vector3(0.00532f, 0.00136f, 0.03633f);
                            }
                            else if (SkyworthSettings.Instance.m_ControllerModel == SkyworthSettings.ControllerModel.GRIP_MODEL)
                            {
                                m_vroot.transform.localRotation = Quaternion.Euler(90.0f, 0, 0);
                                m_vroot.transform.localPosition = new Vector3(0, -0.0078f, -0.0738f);
                                m_RayOrigin.transform.localPosition = new Vector3(0f, -0.0177f, 0.0398f);
                            }
                        }
                    }
                }
                else 
                {
                    if (m_vroot.activeInHierarchy)
                        m_vroot.SetActive(false);
                }
            }
            

            if (ControllerDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool PrimaryButtonvalue))
            {
                s_ControllerAnimatior.SetFloat("primaryButton", PrimaryButtonvalue ? 1 : 0);
            }
            else
            {
                s_ControllerAnimatior.SetFloat("primaryButton", 0);
            }

            if (ControllerDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButtonvalue))
            {
                s_ControllerAnimatior.SetFloat("secondaryButton", secondaryButtonvalue ? 1 : 0);
            }
            else
            {
                s_ControllerAnimatior.SetFloat("secondaryButton", 0);
            }

            if (ControllerDevice.TryGetFeatureValue(CommonUsages.menuButton, out bool menuButtonvalue))
            {
                s_ControllerAnimatior.SetFloat("menuButton", menuButtonvalue ? 1 : 0);
            }
            else
            {
                s_ControllerAnimatior.SetFloat("menuButton", 0);
            }

            if (ControllerDevice.TryGetFeatureValue(CommonUsages.grip, out float gripvalue))
            {
                s_ControllerAnimatior.SetFloat("grip", gripvalue);
            }
            else
            {
                s_ControllerAnimatior.SetFloat("grip", 0);
            }
            if (ControllerDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggervalue))
            {
                s_ControllerAnimatior.SetFloat("trigger", triggervalue);
            }
            else
            {
                s_ControllerAnimatior.SetFloat("trigger", 0);
            }

            if (ControllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxisvalue))
            {
                s_ControllerAnimatior.SetFloat("primary2DAxisX", primary2DAxisvalue.x);
                s_ControllerAnimatior.SetFloat("primary2DAxisY", primary2DAxisvalue.y);
            }
            else
            {
                s_ControllerAnimatior.SetFloat("primary2DAxisX", 0);
                s_ControllerAnimatior.SetFloat("primary2DAxisY", 0);
            }

            if (ControllerDevice.TryGetFeatureValue(CommonUsages.batteryLevel, out float batteryvalue))
            {

                s_Material.SetFloat("_Battery", batteryvalue);
            }
            else
            {
                s_Material.SetFloat("_Battery", 0);
            }
        }
    }
}
