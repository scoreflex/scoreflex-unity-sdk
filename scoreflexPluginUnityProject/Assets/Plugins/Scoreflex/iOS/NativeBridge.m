#import <Scoreflex/Scoreflex.h>

NSString * scoreflexUnityObjectName = @"Scoreflex";
UIView * scoreflexRankPanelView = nil;
NSMutableDictionary *scoreflexPanelViews = nil;

NSString * fromUnichar(const unichar *source)
{
	size_t length = 0; while(source[length] != 0) length++;
	NSString *string = [NSString stringWithCharacters:source length:length];
	return string;
}

NSString * stringOrNil(const unichar *source)
{
	NSString *result = source == NULL ? nil : fromUnichar(source);
	return result;
}

id kvccFromUnichar(const unichar *source)
{
	id result = nil;

	NSString *stage1 = stringOrNil(source);
	
	if(stage1 != nil)
	{
		NSError *error = nil;
	
		NSData *stage2 = [stage1 dataUsingEncoding:NSUTF8StringEncoding];
		result = [NSJSONSerialization JSONObjectWithData:stage2 options:kNilOptions error:&error];
		
		if(error != nil)
		{
			NSLog(@"Scoreflex JSON Failure: '%@', while attempting to parse '%@'.", [error localizedDescription], stage1);
		}
	}
		
	return result;
}

id kvccWithScore(const unichar *source, int score)
{
	id parsed = kvccFromUnichar(source);
	id result = nil;
	
	NSString * key = @"score";
	NSString * value = [NSString stringWithFormat:@"%d", score];
	
	if(parsed == nil || ![parsed isMemberOfClass:[NSDictionary class]])
	{
		result = [NSDictionary dictionaryWithObject:value forKey:key];
	}
	else
	{
		result = [NSMutableDictionary dictionaryWithCapacity:([parsed count] + 1)];
		[result addEntriesFromDictionary:parsed];
		[result setObject:value forKey:key];
	}
	
	return result;
}

void scoreflexSetUnityObjectName(const unichar *_unityObjectName)
{
	scoreflexUnityObjectName = fromUnichar(_unityObjectName);
}

void scoreflexInitialize(const unichar *_id, const unichar *_secret, int _sandbox)
{
	NSString *identification = fromUnichar(_id);
	NSString *secret = fromUnichar(_secret);
	BOOL sandbox = _sandbox == 1 ? YES : NO;

	NSLog(@"Initializing:\nid = %@\nsecret = %@\n_sandbox = %d", identification, secret, _sandbox);

	[Scoreflex setClientId:identification secret:secret sandboxMode:sandbox];
}

void scoreflexGetPlayerId(void *buffer, int bufferLength)
{
	NSString *playerId = [Scoreflex getPlayerId];
	[playerId getCString:buffer maxLength:bufferLength encoding:NSUnicodeStringEncoding];
}

float scoreflexGetPlayingTime()
{
	NSNumber *playTime = [Scoreflex getPlayingTime];
	return [playTime floatValue];
}

void scoreflexShowFullscreenView(const unichar *_resource, const unichar *_params)
{
	NSString *resource = fromUnichar(_resource);
	id params = kvccFromUnichar(_params);
	[Scoreflex showFullScreenView:resource params:params];
}

int scoreflexShowPanelView(const unichar *_resource, const unichar *_params, int isOnTop)
{
	if(scoreflexPanelViews == nil)
		scoreflexPanelViews = [[NSMutableDictionary alloc] init];
		
	SXGravity gravity = isOnTop ? SXGravityTop : SXGravityBottom;

	NSString *resource = fromUnichar(_resource);
	id params = kvccFromUnichar(_params);
	SXView *view = [Scoreflex showPanelView:resource params:params gavity:gravity]; // sic
	
	int key;
	NSNumber *keyAsNumber;
	do {
		key = rand();
		keyAsNumber = [NSNumber numberWithInt:key];
	} while ([scoreflexPanelViews objectForKey:keyAsNumber] != nil);
	
	[scoreflexPanelViews setObject:view forKey:keyAsNumber];
	
	return key;
}

void scoreflexHidePanelView(int key)
{
	if(scoreflexPanelViews != nil)
	{
		NSNumber *keyAsNumber = [NSNumber numberWithInt:key];
		
		SXView *view = [scoreflexPanelViews objectForKey:keyAsNumber];
		
		if(view != nil)
		{
			[view close];
			[scoreflexPanelViews removeObjectForKey:keyAsNumber];
		}
	}
}

// handleNotification?

// handleURL?

// isReachable?

// languageCode?

// location?

void scoreflexSetDeviceToken(const unichar *_deviceToken)
{
	NSString *deviceToken = fromUnichar(_deviceToken);
	[Scoreflex setDeviceToken:deviceToken];
}

void scoreflexShowDeveloperGames(const unichar *_developerId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);

	NSString *developerId = fromUnichar(_developerId);
	
	[Scoreflex showDeveloperGames:developerId params:params];
}

void scoreflexShowDeveloperProfile(const unichar *_developerId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *developerId = fromUnichar(_developerId);
	
	[Scoreflex showDeveloperProfile:developerId params:params];
}

void scoreflexShowGameDetails(const unichar *_gameId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *gameId = fromUnichar(_gameId);
	
	[Scoreflex showGameDetails:gameId params:params];
}

void scoreflexShowGamePlayers(const unichar *_gameId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *gameId = fromUnichar(_gameId);
	
	[Scoreflex showGamePlayers:gameId params:params];
}

void scoreflexShowLeaderboard(const unichar *_leaderboardId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *leaderboardId = fromUnichar(_leaderboardId);
	[Scoreflex showLeaderboard:leaderboardId params:params];
}


void scoreflexShowLeaderboardOverview(const unichar *_leaderboardId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *leaderboardId = fromUnichar(_leaderboardId);
	[Scoreflex showLeaderboardOverview:leaderboardId params:params];
}

void scoreflexShowPlayerChallenges(const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	[Scoreflex showPlayerChallenges:params];
}

void scoreflexShowPlayerFriends(const unichar *_playerId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *playerId = stringOrNil(_playerId);
	[Scoreflex showPlayerFriends:playerId params:params];
}

void scoreflexShowPlayerNewsFeed(const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	[Scoreflex showPlayerNewsFeed:params];
}

void scoreflexShowPlayerProfile(const unichar *_playerId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	
	NSString *playerId = stringOrNil(_playerId);

	[Scoreflex showPlayerProfile:playerId params:params];
}

void scoreflexShowPlayerProfileEdit(const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	[Scoreflex showPlayerProfileEdit:params];
}

void scoreflexShowPlayerRating(const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	[Scoreflex showPlayerRating:params];
}

void scoreflexShowPlayerSettings(const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	[Scoreflex showPlayerSettings:params];
}

void scoreflexShowRanksPanel(const unichar *_leaderboardId, int _score, const unichar *_params, int isOnTop)
{
	SXGravity gravity = isOnTop ? SXGravityTop : SXGravityBottom;
	id params = kvccWithScore(_params, _score);
	NSString *leaderboardId = fromUnichar(_leaderboardId);
	scoreflexRankPanelView = [Scoreflex showRanksPanel:leaderboardId params:params gravity:gravity];
}

void scoreflexHideRanksPanel()
{
	if(scoreflexRankPanelView != nil)
		[scoreflexRankPanelView removeFromSuperview];
}

void scoreflexShowSearch(const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	[Scoreflex showSearch:params];
}

void scoreflexStartPlayingSession()
{
	[Scoreflex startPlayingSession];
}

void scoreflexStopPlayingSession()
{
	[Scoreflex stopPlayingSession];
}

void scoreflexAPICallback(SXResponse *response, NSError *error, NSString *handler)
{
	if(handler != nil)
	{
		NSString *json = nil;
		
		if(error == nil)
		{
			NSData *_json = [NSJSONSerialization dataWithJSONObject:[response object] options:0 error:&error];
			json = [[NSString alloc] initWithData:_json encoding:NSUTF8StringEncoding];
		}
		
		NSString *message;
		
		if(error == nil)
		{
			message = [NSString stringWithFormat:@"%@:success:%@", handler, json];
		}
		else
		{
			message = [NSString stringWithFormat:@"%@:failure:%@", handler, [error localizedDescription]];
		}
		
		UnitySendMessage([scoreflexUnityObjectName UTF8String], "HandleAPICallback", [message UTF8String]);
	}
}

void scoreflexGet(const unichar *_resource, const unichar *_params, const unichar *_handler)
{
	id params = kvccFromUnichar(_params);
	NSString *resource = fromUnichar(_resource);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex get:resource params:params handler:^(SXResponse *response, NSError *error) {
			scoreflexAPICallback(response, error, handler);
	}];
}

void scoreflexPut(const unichar *_resource, const unichar *_params, const unichar *_handler)
{
	id params = kvccFromUnichar(_params);
	NSString *resource = fromUnichar(_resource);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex put:resource params:params handler:^(SXResponse *response, NSError *error) {
			scoreflexAPICallback(response, error, handler);
	}];
}

void scoreflexPost(const unichar *_resource, const unichar *_params, const unichar *_handler)
{
	id params = kvccFromUnichar(_params);
	NSString *resource = fromUnichar(_resource);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex post:resource params:params handler:^(SXResponse *response, NSError *error) {
			scoreflexAPICallback(response, error, handler);
	}];
}

void scoreflexPostEventually(const unichar *_resource, const unichar *_params, const unichar *_handler)
{
	id params = kvccFromUnichar(_params);
	NSString *resource = fromUnichar(_resource);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex postEventually:resource params:params handler:^(SXResponse *response, NSError *error) {
			scoreflexAPICallback(response, error, handler);
	}];
}

void scoreflexDelete(const unichar *_resource, const unichar *_params, const unichar *_handler)
{
	id params = kvccFromUnichar(_params);
	NSString *resource = fromUnichar(_resource);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex delete:resource params:params handler:^(SXResponse *response, NSError *error) {
			scoreflexAPICallback(response, error, handler);
	}];
}

void scoreflexSubmitTurn(const unichar *_challengeInstanceId, const unichar *_params, const unichar *_handler)
{
	id params = kvccFromUnichar(_params);
	NSString *challengeInstanceId = fromUnichar(_challengeInstanceId);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex submitTurn:challengeInstanceId params:params
		handler:^(SXResponse *response , NSError *error) {
			if(handler != nil)
			{
				NSString *message;
				if(error == nil)
				{
					message = [NSString stringWithFormat:@"%@:success", handler];
				}
				else
				{
					message = [NSString stringWithFormat:@"%@:failure: %@", handler, [error localizedDescription]];
				}
				UnitySendMessage([scoreflexUnityObjectName UTF8String], "HandleSubmit", [message UTF8String]);
			}
			NSLog(@"SubmitTurn Returned");
		}
	];
}

void scoreflexSubmitScore(const unichar *_leaderboardId, int _score, const unichar *_params, const unichar *_handler)
{
	id params = kvccWithScore(_params, _score);
	NSString *leaderboardId = fromUnichar(_leaderboardId);
	NSString *handler = stringOrNil(_handler);
	[Scoreflex submitScore:leaderboardId params:params
		handler:^(SXResponse *response , NSError *error) {
			if(handler != nil)
			{
				NSString *message;
				if(error == nil)
				{
					message = [NSString stringWithFormat:@"%@:success", handler];
				}
				else
				{
					message = [NSString stringWithFormat:@"%@:failure: %@", handler, [error localizedDescription]];
				}
				UnitySendMessage([scoreflexUnityObjectName UTF8String], "HandleSubmit", [message UTF8String]);
			}
			NSLog(@"SubmitScore Returned");
		}
	];
}

void scoreflexSubmitScoreAndShowRanksPanel(const unichar *_leaderboardId, int _score, const unichar *_params, int isOnTop)
{
	SXGravity gravity = isOnTop ? SXGravityTop : SXGravityBottom;
	id params = kvccWithScore(_params, _score);
	NSString *leaderboardId = fromUnichar(_leaderboardId);
	scoreflexRankPanelView = [Scoreflex submitScoreAndShowRanksPanel:leaderboardId params:params gravity:gravity];
}

void scoreflexSubmitTurnAndShowChallengeDetail(const unichar *_challengeInstanceId, const unichar *_params)
{
	id params = kvccFromUnichar(_params);
	NSString *challengeInstanceId = fromUnichar(_challengeInstanceId);
	[Scoreflex submitTurnAndShowChallengeDetail:challengeInstanceId params:params];
}
