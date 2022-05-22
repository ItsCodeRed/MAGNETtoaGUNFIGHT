using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    int leaderboardID = 2816;

    public Animation boardAnim;
    public TextMeshProUGUI playerNames;
    public TextMeshProUGUI playerScores;

    public PlayerManager playerManager;

    private bool leaderboardShowing = false;

    public void ShowLeaderboard()
    {
        if (!boardAnim.isPlaying)
        {
            leaderboardShowing = !leaderboardShowing;

            if (leaderboardShowing)
            {
                boardAnim.Play("ShowLeaderboard");
                StartCoroutine(FetchLeaderboardRoutine());
            }
            else
            {
                boardAnim.Play("HideLeaderboard");
            }
        }
    }

    public void HideLeaderboard()
    {
        if (leaderboardShowing)
        {
            leaderboardShowing = false;
            boardAnim.Play("HideLeaderboard");
        }
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        bool done = false;
        string playerId = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerId, scoreToUpload, leaderboardID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully submitted score! :)");
            }
            else
            {
                Debug.LogWarning("Score submittion failed! ): " + response.Error);
            }
            done = true;
        });

        yield return new WaitWhile(() => !done);
    }

    public IEnumerator FetchLeaderboardRoutine()
    {
        bool done = false;

        if (!playerManager.loggedIn)
        {
            Debug.LogWarning("Failed to generate leaderboard! ): LootLocker failed to log in.");
            done = true;
        }

        LootLockerSDKManager.GetScoreListMain(leaderboardID, 100, 0, (response) =>
        {
            if (response.success)
            {
                string tempPlayerNames = "Names\n";
                string tempPlayerScores = "Scores\n";

                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    tempPlayerNames += members[i].rank + ". ";
                    if (members[i].player.name != "")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score + "\n";
                    tempPlayerNames += "\n";
                }

                playerNames.text = tempPlayerNames;
                playerScores.text = tempPlayerScores;
            }
            else
            {
                Debug.LogWarning("Failed to generate leaderboard! ): " + response.Error);
            }

            done = true;
        });

        yield return new WaitWhile(() => !done);
    }
}
