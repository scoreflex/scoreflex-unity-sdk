﻿using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

public partial class Scoreflex
{
	#if UNITY_ANDROID

	private const string OPERATION_KEY_RESPONSE = "response";
	private const string OPERATION_KEY_SUCCESS = "success";
	private const string OPERATION_KEY_CALLBACK = "callback";
	private const string OPERATION_KEY_INTENT = "intent";

	class ResponseHandler: AndroidJavaProxy {
		private Scoreflex Instance;
		private Callback Callback;

		public ResponseHandler(Scoreflex instance, Callback callback)
			: base("com.scoreflex.unity3d.IResponseHandler") {
			Callback = callback;
			Instance = instance;
		}

		private void handle(bool success, AndroidJavaObject response)
		{
			var figures = new Dictionary<string,object> {
				{ OPERATION_KEY_RESPONSE, response },
				{ OPERATION_KEY_SUCCESS, success },
				{ OPERATION_KEY_CALLBACK, Callback }
			};
			Instance.EnqueuePendingOperation(Instance.ProcessResponse, figures);
		}

		public void onFailure(AndroidJavaObject response)
		{
			handle(false, response);
		}

		public void onSuccess(AndroidJavaObject response)
		{
			handle(true, response);
		}

		public AndroidJavaObject ToBridge()
		{
			var bridge = new AndroidJavaObject("com.scoreflex.unity3d.ResponseHandler", this);
			return bridge;
		}
	}

	class ChallengeBroadcastReceiver: AndroidJavaProxy {
		Scoreflex Instance;

		public ChallengeBroadcastReceiver(Scoreflex instance): base("com.scoreflex.unity3d.IBroadcastReceiver")
		{
			Instance = instance;
		}
		void onReceive(AndroidJavaObject context, AndroidJavaObject intent)
		{
			var figures = new Dictionary<string,object> {
				{ Scoreflex.OPERATION_KEY_INTENT, intent }
			};
			Instance.EnqueuePendingOperation(Instance.ProcessChallengeBroadcast, figures);
		}
	}

	class PlaySoloBroadcastReceiver: AndroidJavaProxy {
		Scoreflex Instance;
		public PlaySoloBroadcastReceiver(Scoreflex instance): base("com.scoreflex.unity3d.IBroadcastReceiver")
		{
			Instance = instance;
		}
		void onReceive(AndroidJavaObject context, AndroidJavaObject intent)
		{
			var figures = new Dictionary<string,object> {
				{ Scoreflex.OPERATION_KEY_INTENT, intent }
			};
			Instance.EnqueuePendingOperation(Instance.ProcessPlaySoloBroadcast, figures);
		}
	}

	void ProcessResponse(Dictionary<string,object> figures)
	{
		var response = (AndroidJavaObject) figures[Scoreflex.OPERATION_KEY_RESPONSE];
		var boolean = (bool) figures[Scoreflex.OPERATION_KEY_SUCCESS];
		var callback = (Callback) figures[Scoreflex.OPERATION_KEY_CALLBACK];

		var parsedResponse = PullFiguresFromResponse(response);

		if(callback != null) callback(boolean, parsedResponse);
	}

	void ProcessChallengeBroadcast(Dictionary<string,object> figures)
	{
		var intent = (AndroidJavaObject) figures[Scoreflex.OPERATION_KEY_INTENT];
		var scoreflexClass = new AndroidJavaClass("com.scoreflex.Scoreflex");
		var constantID = AndroidJNI.GetStaticFieldID(scoreflexClass.GetRawClass(), "INTENT_START_CHALLENGE_EXTRA_INSTANCE", "Ljava/lang/String;");
		Debug.Log("Kappakappa: Attempt to pull key '" + constantID + "' from provided Intent.");
		string constantValue = AndroidJNI.GetStaticStringField(scoreflexClass.GetRawClass(), constantID);
		var jsonParcelable = intent.Call<AndroidJavaObject>("getParcelableExtra", constantValue);
		var jsonObject = jsonParcelable.Call<AndroidJavaObject>("getJSONObject");
		var jsonString = jsonObject.Call<string>("toString");
		var result = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string,object>;
		ChallengeHandlers(result);
	}

	void ProcessPlaySoloBroadcast(Dictionary<string,object> figures)
	{
		var intent = (AndroidJavaObject) figures[Scoreflex.OPERATION_KEY_INTENT];
		var scoreflexClass = new AndroidJavaClass("com.scoreflex.Scoreflex");
		var constantID = AndroidJNI.GetStaticFieldID(scoreflexClass.GetRawClass(), "INTENT_PLAY_LEVEL_EXTRA_LEADERBOARD_ID", "Ljava/lang/String;");
		Debug.Log("Bungabunga: Attempt to pull key '" + constantID + "' from provided Intent.");
		string constantValue = AndroidJNI.GetStaticStringField(scoreflexClass.GetRawClass(), constantID);
		string leaderboardId = intent.Call<string>("getStringExtra", constantValue);
		PlaySoloHandlers(leaderboardId);
	}

	//These figures are derived from the Android SDK manual for android.view.Gravity.
	//http://developer.android.com/reference/android/view/Gravity.html
	private readonly Dictionary<Gravity,int> androidGravity = new Dictionary<Gravity,int>() {
		{ Gravity.Bottom, 80 },
		{ Gravity.Top, 48 }
	};

	AndroidJavaObject unityActivity;
	AndroidJavaClass scoreflex;

	void Awake()
	{
		if(Instance == null)
		{
			try
			{
				AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
				unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

				scoreflex = new AndroidJavaClass("com.scoreflex.Scoreflex");
				scoreflex.CallStatic("initialize", unityActivity, ClientId, ClientSecret, Sandbox);

				AndroidJavaClass localBroadcastManagerClass = new AndroidJavaClass("android.support.v4.content.LocalBroadcastManager");
				var localBroadcastManager = localBroadcastManagerClass.CallStatic<AndroidJavaObject>("getInstance", unityActivity);

				var challengeBroadcastReceiver = new ChallengeBroadcastReceiver(this);
				var challengeBroadcastReceiverBridge = new AndroidJavaObject("com.scoreflex.unity3d.BroadcastReceiver", challengeBroadcastReceiver);
				var INTENT_START_CHALLENGE_ID = AndroidJNI.GetStaticFieldID(scoreflex.GetRawClass(), "INTENT_START_CHALLENGE", "Ljava/lang/String;");
				string INTENT_START_CHALLENGE = AndroidJNI.GetStaticStringField(scoreflex.GetRawClass(), INTENT_START_CHALLENGE_ID);
				AndroidJavaObject challengeIntentFilter = new AndroidJavaObject("android.content.IntentFilter", INTENT_START_CHALLENGE);
				localBroadcastManager.Call("registerReceiver", challengeBroadcastReceiverBridge, challengeIntentFilter);

				var playSoloBroadcastReceiver = new PlaySoloBroadcastReceiver(this);
				var playSoloBroadcastReceiverBridge = new AndroidJavaObject("com.scoreflex.unity3d.BroadcastReceiver", playSoloBroadcastReceiver);
				var INTENT_PLAY_LEVEL_ID = AndroidJNI.GetStaticFieldID(scoreflex.GetRawClass(), "INTENT_PLAY_LEVEL", "Ljava/lang/String;");
				string INTENT_PLAY_LEVEL = AndroidJNI.GetStaticStringField(scoreflex.GetRawClass(), INTENT_PLAY_LEVEL_ID);
				AndroidJavaObject playSoloIntentFilter = new AndroidJavaObject("android.content.IntentFilter", INTENT_PLAY_LEVEL);
				localBroadcastManager.Call("registerReceiver", playSoloBroadcastReceiverBridge, playSoloIntentFilter);

				initialized = true;
			}
			catch(System.Exception ex)
			{
				Debug.LogWarning("Failed to boot Scoreflex.");
				Debug.LogException(ex);
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

	#region Internal queue to handler callbacks from Java runtime on the Unity main thread.
	private class PendingOperation {
		public System.Action<Dictionary<string,object>> method;
		public Dictionary<string,object> figures;
	};
	private readonly List<PendingOperation> PendingOperations = new List<PendingOperation>();
	private void EnqueuePendingOperation(System.Action<Dictionary<string,object>> method, Dictionary<string,object> figures)
	{
		var pendingOperation = new PendingOperation {
			method = method,
			figures = figures
		};
		lock(PendingOperations) {
			PendingOperations.Add(pendingOperation);
		}
	}
	void Update()
	{
		lock(PendingOperations)
		{
			foreach(var pendingOperation in PendingOperations)
			{
				try {
					pendingOperation.method(pendingOperation.figures);
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			PendingOperations.Clear();
		}
	}
	#endregion

	private static Dictionary<string,object> PullFiguresFromResponse(AndroidJavaObject response)
	{
		AndroidJavaObject mJson = response.Call<AndroidJavaObject>("getJSONObject");

		string jsonString = mJson.Call<string>("toString");
		var result = MiniJSON.Json.Deserialize(jsonString) as Dictionary<string,object>;

		return result;
	}

	private string GetScoreflexActivityConstant(string constantName)
	{
		AndroidJavaClass scoreflexActivityClass = new AndroidJavaClass("com.scoreflex.ScoreflexActivity");
		var constantID = AndroidJNI.GetStaticFieldID(scoreflexActivityClass.GetRawClass(), constantName, "Ljava/lang/String;");
		string constantValue = AndroidJNI.GetStaticStringField(scoreflexActivityClass.GetRawClass(), constantID);
		return constantValue;
	}

	private AndroidJavaObject CreateScoreflexActivityIntent(string showWhat)
	{
		AndroidJavaClass scoreflexActivityClass = new AndroidJavaClass("com.scoreflex.ScoreflexActivity");

		var showWhatKeyID = AndroidJNI.GetStaticFieldID(scoreflexActivityClass.GetRawClass(), "INTENT_SHOW_EXTRA_KEY", "Ljava/lang/String;");
		string showWhatKey = AndroidJNI.GetStaticStringField(scoreflexActivityClass.GetRawClass(), showWhatKeyID);

		var showWhatID = AndroidJNI.GetStaticFieldID(scoreflexActivityClass.GetRawClass(), showWhat, "Ljava/lang/String;");
		string showWhatValue = AndroidJNI.GetStaticStringField(scoreflexActivityClass.GetRawClass(), showWhatID);
		AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", unityActivity, scoreflexActivityClass);
		
		intent.Call<AndroidJavaObject>("putExtra", showWhatKey, showWhatValue);

		return intent;
	}

	private void AddParametersToIntentIfNotNull(AndroidJavaObject intent, Dictionary<string,object> _parameters)
	{
		if(_parameters != null)
		{
			AndroidJavaObject parameters = CreateRequestParamsFromDictionary(_parameters);
			string key = GetScoreflexActivityConstant("INTENT_EXTRA_REQUEST_PARAMS_KEY");
			intent.Call<AndroidJavaObject>("putExtra", key, parameters);
		}
	}

	private void AddFigureToIntentIfNotNull(AndroidJavaObject intent, string figure, string constantName)
	{
		if(figure != null)
		{
			string key = GetScoreflexActivityConstant(constantName);
			intent.Call<AndroidJavaObject>("putExtra", key, figure);
		}
	}

	private AndroidJavaObject CreateRequestParamsFromDictionary(Dictionary<string,object> source, long? score = null)
	{
		AndroidJavaClass mapAssist = new AndroidJavaClass("com.scoreflex.unity3d.MapAssist");

		AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
		if(source != null)
		{
			foreach(KeyValuePair<string,object> kvp in source)
			{
				var value = kvp.Value == null ? null : kvp.Value.ToString();
				mapAssist.CallStatic("put", map, kvp.Key, value);
			}
		}
		if(score.HasValue)
		{
			mapAssist.CallStatic("put", map, "score", score.Value.ToString());
		}
		AndroidJavaObject requestParams = new AndroidJavaObject("com.scoreflex.Scoreflex$RequestParams", map);
		return requestParams;
	}

	private void StartActivityWithIntent(AndroidJavaObject intent)
	{
		unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			unityActivity.Call("startActivity", intent);
		}));
	}

	public string _GetPlayerId()
	{
		return scoreflex.CallStatic<string>("getPlayerId");
	}
	
	public float _GetPlayingTime()
	{
		long l = scoreflex.CallStatic<long>("getPlayingSessionTime");
		return (float) l;
	}
	
	public void _ShowFullscreenView(string resource, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_FULLSCREEN_VIEW");
		AddFigureToIntentIfNotNull(intent, resource, "INTENT_EXTRA_FULLSCREEN_RESOURCE");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}

	private readonly Dictionary<int,AndroidJavaObject> scoreflexViewByHandle = new Dictionary<int,AndroidJavaObject>();

	public View _ShowPanelView(string resource, Dictionary<string,object> parameters = null, Gravity gravity = Gravity.Top)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters);
		AndroidJavaObject view = null;

		unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			view = scoreflex.CallStatic<AndroidJavaObject>("showPanelView", unityActivity, resource, droidParams, androidGravity[gravity]);
		}));

		const int timeout = 5000;
		int countdown = timeout;

		while(view == null && countdown > 0)
		{
			const int rest = 5;
			System.Threading.Thread.Sleep(rest);
			countdown -= rest;
		}

		if(view == null)
		{
			Debug.LogError("Scoreflex.ShowPanelView attempted to call up a panel view but request timed out.");
			return null;
		}
		else
		{
			int newHandle;
			do newHandle = Random.Range(1, int.MaxValue); while(scoreflexViewByHandle.ContainsKey(newHandle) == false);
			scoreflexViewByHandle[newHandle] = view;
			return new View(newHandle);
		}
	}

	private void _HidePanelView(int handle)
	{
		if(scoreflexViewByHandle.ContainsKey(handle))
		{
			AndroidJavaObject view = scoreflexViewByHandle[handle];
			view.Call("close");
			scoreflexViewByHandle.Remove(handle);
		}
	}
	
	public void _SetDeviceToken(string deviceToken)
	{
		Debug.LogWarning("Scoreflex.setDeviceToken called on an Android device; will do nothing.");
	}
	
	public void _ShowDeveloperGames(string developerId, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_DEVELOPER_GAMES");
		AddFigureToIntentIfNotNull(intent, developerId, "INTENT_EXTRA_DEVELOPER_PROFILE_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowDeveloperProfile(string developerId, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_DEVELOPER_PROFILE");
		AddFigureToIntentIfNotNull(intent, developerId, "INTENT_EXTRA_DEVELOPER_PROFILE_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowGameDetails(string gameId, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_GAME_DETAIL");
		AddFigureToIntentIfNotNull(intent, gameId, "INTENT_EXTRA_GAME_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowGamePlayers(string gameId, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_GAME_PLAYERS");
		AddFigureToIntentIfNotNull(intent, gameId, "INTENT_EXTRA_GAME_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowLeaderboard(string leaderboardId, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_LEADERBOARD");
		Debug.Log("LeaderboardID: " + leaderboardId);
		AddFigureToIntentIfNotNull(intent, leaderboardId, "INTENT_EXTRA_LEADERBOARD_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowLeaderboardOverview(string leaderboardId, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_LEADERBOARD_OVERVIEW");
		AddFigureToIntentIfNotNull(intent, leaderboardId, "INTENT_EXTRA_LEADERBOARD_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowPlayerChallenges(Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_CHALLENGES");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowPlayerFriends(string playerId = null, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_PLAYER_FRIENDS");
		AddFigureToIntentIfNotNull(intent, playerId, "INTENT_EXTRA_PLAYER_PROFILE_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowPlayerNewsFeed(Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_PLAYER_NEWS_FEED");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}

		
	public void _ShowPlayerProfile(string playerId = null, Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_PLAYER_PROFILE");
		AddFigureToIntentIfNotNull(intent, playerId, "INTENT_EXTRA_PLAYER_PROFILE_ID");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowPlayerProfileEdit(Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_PLAYER_PROFILE_EDIT");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowPlayerRating(Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_PLAYER_RATING");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowPlayerSettings(Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_PLAYER_SETTINGS");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}
	
	public void _ShowSearch(Dictionary<string,object> parameters = null)
	{
		AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_SEARCH");
		AddParametersToIntentIfNotNull(intent, parameters);
		StartActivityWithIntent(intent);
	}

	private AndroidJavaObject ranksPanelView = null;

	public void _ShowRanksPanel(string leaderboardId, long score, Dictionary<string,object> parameters = null, Gravity gravity = Gravity.Top)
	{
		unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
			var requestParams = CreateRequestParamsFromDictionary(parameters, score);
			ranksPanelView = scoreflex.CallStatic<AndroidJavaObject>("showRanksPanel", unityActivity, leaderboardId, androidGravity[gravity], requestParams, true);
		}));
	}
	
	public void _HideRanksPanel()
	{
		if(ranksPanelView != null)
		{
			unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				ranksPanelView.Call("close");
				ranksPanelView = null;
			}));
		}
	}
	
	public void _StartPlayingSession()
	{
		scoreflex.CallStatic("startPlayingSession");
	}
	
	public void _StopPlayingSession()
	{
		scoreflex.CallStatic("stopPlayingSession");
	}
	
	public void _Get(string resource, Dictionary<string,object> parameters, Callback callback)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters);
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("get", resource, droidParams, droidHandler);
	}
	
	public void _Put(string resource, Dictionary<string,object> parameters, Callback callback)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters);
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("put", resource, droidParams, droidHandler);
	}
	
	public void _Post(string resource, Dictionary<string,object> parameters, Callback callback)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters);
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("post", resource, droidParams, droidHandler);
	}
	
	public void _PostEventually(string resource, Dictionary<string,object> parameters, Callback callback)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters);
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("postEventually", resource, droidParams, droidHandler);
	}
	
	public void _Delete(string resource, Dictionary<string,object> parameters, Callback callback)
	{
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("delete", resource, droidHandler);
	}
	
	public void _SubmitTurn(string challengeInstanceId, long score, Dictionary<string,object> parameters = null, Callback callback = null)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters, score);
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("submitTurn", challengeInstanceId, droidParams, droidHandler);
	}
	
	public void _SubmitScore(string leaderboardId, long score, Dictionary<string,object> parameters = null, Callback callback = null)
	{
		var droidParams = CreateRequestParamsFromDictionary(parameters);
		var droidHandler = new ResponseHandler(this, callback).ToBridge();
		scoreflex.CallStatic("submitScore", leaderboardId, score, droidParams, droidHandler);
	}
	
	public void _SubmitScoreAndShowRanksPanel(string leaderboardId, long score, Dictionary<string,object> parameters = null, Gravity gravity = Gravity.Top)
	{
		_ShowRanksPanel(leaderboardId, score, gravity:gravity);
		_SubmitScore(leaderboardId, score, parameters, (success, dict) => { Debug.Log("Score submission " + (success ? "successful" : "failed")); });
	}
	
	public void _SubmitTurnAndShowChallengeDetail(string challengeInstanceId, long score, Dictionary<string,object> parameters = null)
	{
		_SubmitTurn(challengeInstanceId, score, parameters, (success, dict) => {
			AndroidJavaObject intent = CreateScoreflexActivityIntent("INTENT_EXTRA_SHOW_CHALLENGE_DETAIL");
			AddFigureToIntentIfNotNull(intent, challengeInstanceId, "INTENT_EXTRA_CHALLENGE_INSTANCE_ID");
			StartActivityWithIntent(intent);
		} );
	}

#endif
}


































