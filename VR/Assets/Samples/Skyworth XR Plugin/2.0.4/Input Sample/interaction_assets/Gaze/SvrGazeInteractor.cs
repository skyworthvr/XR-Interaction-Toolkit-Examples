using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class SvrGazeInteractor : MonoBehaviour
{
    /// The constants below are expsed for testing. Minimum inner angle of the reticle (in degrees).
    public const float RETICLE_MIN_INNER_ANGLE = 0.0f;

    /// Minimum outer angle of the reticle (in degrees).
    public const float RETICLE_MIN_OUTER_ANGLE = 0.25f;

    /// Angle at which to expand the reticle when intersecting with an object (in degrees).
    public const float RETICLE_GROWTH_ANGLE = 0.7f;

    /// Minimum distance of the reticle (in meters).
    public const float RETICLE_DISTANCE_MIN = 0.45f;

    /// Maximum distance of the reticle (in meters).
    public float maxReticleDistance = 20.0f;

    /// Number of segments making the reticle circle.
    public int reticleSegments = 20;

    /// Growth speed multiplier for the reticle/
    public float reticleGrowthSpeed = 8.0f;

    /// Sorting order to use for the reticle's renderer.
    /// Range values come from https://docs.unity3d.com/ScriptReference/Renderer-sortingOrder.html.
    /// Default value 32767 ensures gaze reticle is always rendered on top.
    [Range(-32767, 32767)]
    public int reticleSortingOrder = 32767;

    public Material MaterialComp { private get; set; }

    // Current inner angle of the reticle (in degrees).
    // Exposed for testing.
    public float ReticleInnerAngle { get; private set; }
    // Current outer angle of the reticle (in degrees).
    // Exposed for testing.
    public float ReticleOuterAngle { get; private set; }

    // Current distance of the reticle (in meters).
    // Getter exposed for testing.
    public float ReticleDistanceInMeters { get; private set; }

    // Current inner and outer diameters of the reticle, before distance multiplication.
    // Getters exposed for testing.
    public float ReticleInnerDiameter { get; private set; }

    public float ReticleOuterDiameter { get; private set; }

    private XRRayInteractor m_XRRayInteractor;
    private Renderer rendererComponent;
    // Start is called before the first frame update
    void Start()
    {
        rendererComponent = GetComponent<Renderer>();
        rendererComponent.sortingOrder = reticleSortingOrder;
        MaterialComp = rendererComponent.material;
        CreateReticleVertices();

        m_XRRayInteractor = GetComponentInParent<XRRayInteractor>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastResult raycastResult;
#if UNITY_INTERACTION_TOOLKIT_200
        if (m_XRRayInteractor && m_XRRayInteractor.TryGetCurrentUIRaycastResult(out raycastResult))
        {
            Vector3 targetLocalPosition = transform.InverseTransformPoint(raycastResult.worldPosition);
            ReticleDistanceInMeters =
            Mathf.Clamp(targetLocalPosition.z, RETICLE_DISTANCE_MIN, maxReticleDistance);
            ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE + RETICLE_GROWTH_ANGLE;
            ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE + RETICLE_GROWTH_ANGLE;
        }
        else
#endif
        {
            ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
            ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
        }

        InputDevice leftControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!leftControllerDevice.isValid && !rightControllerDevice.isValid)
        {
            m_XRRayInteractor.enabled = true;
            rendererComponent.enabled = true;
        }
        else 
        {
            m_XRRayInteractor.enabled = false;
            rendererComponent.enabled = false;
        }
        UpdateDiameters();
    }
    public void UpdateDiameters()
    {
        ReticleDistanceInMeters =
          Mathf.Clamp(ReticleDistanceInMeters, RETICLE_DISTANCE_MIN, maxReticleDistance);

        if (ReticleInnerAngle < RETICLE_MIN_INNER_ANGLE)
        {
            ReticleInnerAngle = RETICLE_MIN_INNER_ANGLE;
        }

        if (ReticleOuterAngle < RETICLE_MIN_OUTER_ANGLE)
        {
            ReticleOuterAngle = RETICLE_MIN_OUTER_ANGLE;
        }

        float inner_half_angle_radians = Mathf.Deg2Rad * ReticleInnerAngle * 0.5f;
        float outer_half_angle_radians = Mathf.Deg2Rad * ReticleOuterAngle * 0.5f;

        float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
        float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

        ReticleInnerDiameter =
          Mathf.Lerp(ReticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
        ReticleOuterDiameter =
          Mathf.Lerp(ReticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);

        MaterialComp.SetFloat("_InnerDiameter", ReticleInnerDiameter * ReticleDistanceInMeters);
        MaterialComp.SetFloat("_OuterDiameter", ReticleOuterDiameter * ReticleDistanceInMeters);
        MaterialComp.SetFloat("_DistanceInMeters", ReticleDistanceInMeters);
    }
    private void CreateReticleVertices()
    {
        Mesh mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().mesh = mesh;

        int segments_count = reticleSegments;
        int vertex_count = (segments_count + 1) * 2;

        #region Vertices

        Vector3[] vertices = new Vector3[vertex_count];

        const float kTwoPi = Mathf.PI * 2.0f;
        int vi = 0;
        for (int si = 0; si <= segments_count; ++si)
        {
            // Add two vertices for every circle segment: one at the beginning of the
            // prism, and one at the end of the prism.
            float angle = (float)si / (float)(segments_count) * kTwoPi;

            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);

            vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
            vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
        }
        #endregion

        #region Triangles
        int indices_count = (segments_count + 1) * 3 * 2;
        int[] indices = new int[indices_count];

        int vert = 0;
        int idx = 0;
        for (int si = 0; si < segments_count; ++si)
        {
            indices[idx++] = vert + 1;
            indices[idx++] = vert;
            indices[idx++] = vert + 2;

            indices[idx++] = vert + 1;
            indices[idx++] = vert + 2;
            indices[idx++] = vert + 3;

            vert += 2;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
#if !UNITY_5_5_OR_NEWER
    // Optimize() is deprecated as of Unity 5.5.0p1.
    mesh.Optimize();
#endif  // !UNITY_5_5_OR_NEWER
    }
}
