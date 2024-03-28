//Loads and saves high scores to an external server

using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

[System.Serializable]
public class HighScore {
    public string name;
    public string score;
    public HighScore(string _name, string _score) {
        name = _name;
        score = _score;
    }
}

[System.Serializable]
public class HighScoreData {
    public List<HighScore> scores;
}

public class HighScoreManager: MonoBehaviour {

    /**************************************************************************************************
      IMPORTANT - TO AVOID INTERFERING WITH OTHER GAMES USING THE SCORE SERVER, GAME_ID MUST BE UNIQUE
    **************************************************************************************************/

    const string GAME_ID = "ludum52-postjam";

    /**************************************************************************************************
    **************************************************************************************************/


    //High Scores
    const string SERVER_URL ="https://ldjam51.rob6566.click/LudumDare-server/";
    const string SCORES_URL="scores.php?game_id=";
    const string ADD_SCORE_URL="add_score.php";
    public GameObject scorePrefab;
    public GameObject scoreHolder;
    string playerName;
    public TMP_InputField playerNameInput;
    public GameObject invalidName;

    //Saves a high score to server
    public IEnumerator SaveScore(int score) {
            
            WWWForm form = new WWWForm();
            form.AddField("user_name", playerName);
            form.AddField("score", score);
            form.AddField("game_id", GAME_ID);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(SERVER_URL+ADD_SCORE_URL, form)) {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                switch (webRequest.result) {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                        break;
                    case UnityWebRequest.Result.Success:
                        break;
                }
            }
    }

    //Loads high scores from server
    public IEnumerator LoadScores() {
        string uri=SERVER_URL+SCORES_URL+GAME_ID;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    //Position high scores on the splash page                    
                    string jsonString=webRequest.downloadHandler.text;
                    var data = JsonUtility.FromJson<HighScoreData>(jsonString);
                    int scoreUpto=0;
                    foreach (HighScore thisScore in data.scores) {
                        //We only load 10 high scores
                        if (scoreUpto>10) {
                            break;
                        }

                        GameObject gameObject = Instantiate(scorePrefab);
                        gameObject.transform.SetParent(scoreHolder.transform);      
                        gameObject.transform.localPosition=new Vector3(-25, 35-(50*scoreUpto), 0);
                        gameObject.transform.localScale=new Vector3(1f, 1f, 1f);
                        TextMeshProUGUI txtName = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                            txtName.text=thisScore.name;
                        TextMeshProUGUI txtScore = gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
                            txtScore.text=thisScore.score;
                        scoreUpto++;
                    }
                    break;
                }
            }
        }

        //Ensures that the player enters a valid name before starting the game
        public bool validateName() {

            string tempPlayerName=playerNameInput.text;
            tempPlayerName=tempPlayerName.Trim();
            if (tempPlayerName.Length>30 || tempPlayerName.Length<2) {
                invalidName.SetActive(true);
                return false;
            }
            else if (!Regex.IsMatch(tempPlayerName, "^[a-zA-Z0-9 ]*$")) {
                invalidName.SetActive(true);
                return false;
            }
            else {
                playerName=tempPlayerName;
                return true;
            }
        }
}