using UnityEngine;

public class CameraProjectionAdjuster : MonoBehaviour
{
    private Camera m_camera;
    public Vector3 CamerProjection;
    void OnPreCull()
    {
        m_camera = GetComponent<Camera>();
        m_camera.ResetWorldToCameraMatrix();
        m_camera.ResetProjectionMatrix();
        m_camera.projectionMatrix = m_camera.projectionMatrix * Matrix4x4.Scale(CamerProjection);
    }

    void OnPreRender()
    {
        GL.SetRevertBackfacing(true);
    }

    void OnPostRender()
    {
        GL.SetRevertBackfacing(false);
    }

}
