using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameTransition : MonoBehaviour
{
    [SerializeField] private VideoPlayer player;
    [SerializeField] private string gameSceneName;

    private void Start()
    {
        StartCoroutine(WaitForIntroRoutine());
    }

    IEnumerator WaitForIntroRoutine()
    {
        yield return new WaitForSeconds(10);
        yield return new WaitWhile(() => player.isPlaying);

        SceneManager.LoadScene(gameSceneName);
    }
}
