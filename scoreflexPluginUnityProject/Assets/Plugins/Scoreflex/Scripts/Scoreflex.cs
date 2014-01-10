﻿using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

public class Scoreflex : MonoBehaviour
{
	public class View
	{
		public readonly int handle;

		public View(int _handle)
		{
			handle = _handle;
		}

		public void Close()
		{
			Scoreflex.scoreflexHidePanelView(handle);
		}
	}

	public static Scoreflex Instance { get; private set; }

	public string ClientId;
	public string ClientSecret;
	public bool Sandbox;

	private bool initialized = false;

	public bool Live {
		get {
			return initialized;
		}
	}

	private const string ErrorNotLive = "Scoreflex: Method called while not live.";

	void Awake()
	{
		if(Instance == null)
		{
			try
			{
				scoreflexInitialize(ClientId, ClientSecret, Sandbox);
				scoreflexSetUnityObjectName(gameObject.name);

				initialized = true;
			}
			catch(System.EntryPointNotFoundException)
			{
				Debug.LogWarning("Failed to boot Scoreflex; not linked (EntryPointNotFoundException).");
				initialized = false;
			}
			GameObject.DontDestroyOnLoad(gameObject);
			Instance = this;
		}
		else if(Instance != this)
		{
			GameObject.Destroy(gameObject);
		}
	}

	private readonly Dictionary<string,System.Action<bool,Dictionary<string,object>>> APICallbacks = new Dictionary<string,System.Action<bool,Dictionary<string,object>>>();

	void HandleAPICallback(string figure)
	{
		if(figure.Contains(":"))
		{
			bool success = figure.Contains("success");
			string handlerKey = figure.Split(':')[0];
			if(SubmitCallbacks.ContainsKey(handlerKey))
			{
				string jsonString = figure.Substring(handlerKey.Length + ":success:".Length); // :failure is the same length

				var dict = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string,object>;

				APICallbacks[handlerKey](success, dict);
			}
			APICallbacks.Remove(handlerKey);
		}
		else
		{
			Debug.Log("Scoreflex: Received invalid callback code from native library: " + figure);
		}
	}

	private readonly Dictionary<string,System.Action<bool>> SubmitCallbacks = new Dictionary<string,System.Action<bool>>();

	void HandleSubmit(string figure)
	{
		if(figure.Contains(":"))
		{
			bool success = figure.Contains("success");
			string handlerKey = figure.Split(':')[0];
			if(SubmitCallbacks.ContainsKey(handlerKey))
			{
				SubmitCallbacks[handlerKey](success);
			}
			SubmitCallbacks.Remove(handlerKey);
		}
		else
		{
			Debug.Log("Scoreflex: Received invalid callback code from native library: " + figure);
		}
	}

	public string GetPlayerId()
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return string.Empty;
		}

		var buffer = new byte[512];
		scoreflexGetPlayerId(buffer, buffer.Length);
		int stringLength = 0;
		while(stringLength < buffer.Length && buffer[stringLength] != '\0') stringLength++;
		string result = System.Text.Encoding.Unicode.GetString(buffer);
		return result;
	}
	
	public float GetPlayingTime()
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return 0f;
		}

		return scoreflexGetPlayingTime();
	}
	
	public void ShowFullscreenView(string resource, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowFullscreenView(resource, json);
	}

	public View ShowPanelView(string resource, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return null;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		int handle = scoreflexShowPanelView(resource, json);

		return new View(handle);
	}

	public void SetDeviceToken(string deviceToken)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}

		scoreflexSetDeviceToken(deviceToken);
	}

	public void ShowDeveloperGames(string developerId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}

		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowDeveloperGames(developerId, json);
	}

	public void ShowDeveloperProfile(string developerId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowDeveloperProfile(developerId, json);
	}

	public void ShowGameDetails(string gameId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowGameDetails(gameId, json);
	}

	public void ShowGamePlayers(string gameId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowGamePlayers(gameId, json);
	}

	public void ShowLeaderboard(string leaderboardId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowLeaderboard(leaderboardId, json);
	}

	public void ShowLeaderboardOverview(string leaderboardId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowLeaderboardOverview(leaderboardId, json);
	}

	public void ShowPlayerChallenges(Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerChallenges(json);
	}

	public void ShowPlayerFriends(string playerId = null, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerFriends(playerId, json);
	}

	public void ShowPlayerNewsFeed(Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerNewsFeed(json);
	}

	public void ShowPlayerProfile(string playerId = null, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerProfile(playerId, json);
	}
	
	public void ShowPlayerProfileEdit(Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerProfileEdit(json);
	}

	public void ShowPlayerRating(Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerRating(json);
	}
	
	public void ShowPlayerSettings(Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowPlayerSettings(json);
	}

	public void ShowRanksPanel(string leaderboardId, int score, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowRanksPanel(leaderboardId, score, json);
	}

	public void HideRanksPanel()
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}

		scoreflexHideRanksPanel();
	}

	public void ShowSearch(Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexShowSearch(json);
	}

	public void StartPlayingSession()
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}

		scoreflexStartPlayingSession();
	}

	public void StopPlayingSession()
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}

		scoreflexStopPlayingSession();
	}

	private static string CreateKeyForCallbackDictionary(Dictionary<string,System.Action<bool>> dictionary)
	{
		string key;
		var random = new System.Random();	
		do {
			key = random.Next().ToString();
		} while(dictionary.ContainsKey(key));
		return key;
	}

	public void SubmitTurn(string challengeInstanceId, Dictionary<string,object> parameters = null, System.Action<bool> callback = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			if(callback != null) callback(false);
			return;
		}

		string handlerKey = callback == null ? null : CreateKeyForCallbackDictionary(SubmitCallbacks);
		if(handlerKey != null) SubmitCallbacks[handlerKey] = callback;

		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexSubmitTurn(challengeInstanceId, json, handlerKey);
	}

	public void SubmitScore(string leaderboardId, int score, Dictionary<string,object> parameters = null, System.Action<bool> callback = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			if(callback != null) callback(false);
			return;
		}
		
		string handlerKey = callback == null ? null : CreateKeyForCallbackDictionary(SubmitCallbacks);
		if(handlerKey != null) SubmitCallbacks[handlerKey] = callback;
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexSubmitScore(leaderboardId, score, json, handlerKey);
	}

	public void SubmitScoreAndShowRanksPanel(string leaderboardId, int score, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexSubmitScoreAndShowRanksPanel(leaderboardId, score, json);
	}

	public void SubmitTurnAndShowChallengeDetail(string challengeLeaderboardId, Dictionary<string,object> parameters = null)
	{
		if(!Live) {
			Debug.Log(ErrorNotLive);
			return;
		}
		
		string json = parameters == null ? null : MiniJSON.Json.Serialize(parameters);

		scoreflexSubmitTurnAndShowChallengeDetail(challengeLeaderboardId, json);
	}
	
	#region Imports
	
	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexGet(string resource, string json = null, string handler = null);
	
	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexPut(string resource, string json = null, string handler = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexPost(string resource, string json = null, string handler = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexPostEventually(string resource, string json = null, string handler = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexDelete(string resource, string json = null, string handler = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowFullscreenView(string resource, string json = null);
	
	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern int scoreflexShowPanelView(string resource, string json = null);
	
	[DllImport ("__Internal")]
	private static extern void scoreflexHidePanelView(int handle);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexSetUnityObjectName(string unityObjectName);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexInitialize(string clientId, string secret, bool sandbox);

	[DllImport ("__Internal")]
	private static extern void scoreflexGetPlayerId(byte[] buffer, int bufferLength);
	
	[DllImport ("__Internal")]
	private static extern float scoreflexGetPlayingTime();

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexSetDeviceToken(string deviceToken);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowDeveloperGames(string developerId, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowDeveloperProfile(string developerId, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowGameDetails(string gameId, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowGamePlayers(string gameId, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowLeaderboard(string leaderboardId, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowLeaderboardOverview(string leaderboardId, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerChallenges(string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerFriends(string playerId = null, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerNewsFeed(string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerProfile(string playerId = null, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerProfileEdit(string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerRating(string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowPlayerSettings(string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowRanksPanel(string leaderboardId, int score, string json = null);

	[DllImport ("__Internal")]
	private static extern void scoreflexHideRanksPanel();
	
	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexShowSearch(string json = null);
	
	[DllImport ("__Internal")]
	private static extern void scoreflexStartPlayingSession();
	
	[DllImport ("__Internal")]
	private static extern void scoreflexStopPlayingSession();
	
	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexSubmitTurn(string challengeInstanceId, string json = null, string handler = null);
	
	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexSubmitScore(string leaderboardId, int score, string json = null, string handler = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexSubmitScoreAndShowRanksPanel(string leaderboardId, int score, string json = null);

	[DllImport ("__Internal", CharSet = CharSet.Unicode)]
	private static extern void scoreflexSubmitTurnAndShowChallengeDetail(string challengeLeaderboardId, string json = null);
	#endregion
}


































