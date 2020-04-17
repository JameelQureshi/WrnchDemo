using UnityEngine;

public class CameraProjectionAdjuster : MonoBehaviour
{
    private Camera m_camera;
    void OnPreCull()
    {
        m_camera = GetComponent<Camera>();
        m_camera.ResetWorldToCameraMatrix();
        m_camera.ResetProjectionMatrix();
        m_camera.projectionMatrix = m_camera.projectionMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
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
