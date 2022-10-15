using PerformanceCounter;
using PerformanceCounter.Internal;
using UnityEngine;

public class Watcher : MonoBehaviour
{
    public void OnEnable()
    {
        _recorders.SetLogger(new HttpSampleLogger());
        _recorders.Alloc(100);
        _recorders.Start();
    }

    public void OnDisable()
    {
        _recorders.Stop();
    }

    public void LateUpdate()
    {

        _recorders.Record();
    }

    private readonly SampleRecorderGroup _recorders = new SampleRecorderGroup(
        SamplingTarget.TimeFromStartup,
        SamplingTarget.TotalUsedMemory,
        SamplingTarget.MainThreadTime,
        SamplingTarget.SetPassCallsCount,
        SamplingTarget.DrawCallsCount,
        SamplingTarget.BatchesCount,
        SamplingTarget.VerticesCount
    );
}
