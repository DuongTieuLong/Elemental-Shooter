using System.Collections;

public interface IWaveStrategy
{
    void Init(SpawnManager manager, WaveConfig config, int waveIndex);
    IEnumerator ExecuteWave();
}
