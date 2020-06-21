using UnityEngine;

public class Coaching : MonoBehaviour
{
    public static int m_reps = 0;
    public static int reps
    {
        get { return m_reps; }
        set
        {
            if (m_reps == value) return;
            m_reps = value;
            OnRepsChange?.Invoke(m_reps);
        }
    }
    public delegate void OnRepsChangeDelegate(int newReps);
    public static event OnRepsChangeDelegate OnRepsChange;


    public virtual void AnalyseFrame(JointData[] frame)
    {
        throw new System.NotImplementedException();
    }
}
