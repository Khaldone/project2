// =============================================================================
// PlayFab CloudScript: moderate-avatar.js
// Server-authoritative PG13 image moderation via OpenAI omni-moderation-latest.
//
// DEPLOY:
//   1. PlayFab Game Manager → Automation → CloudScript → Revisions
//   2. Paste the contents of this file (or merge with existing CloudScript)
//   3. Click "Save as new revision" then "Deploy"
//
// PREREQUISITES (one-time):
//   1. PlayFab Game Manager → Title Settings → Internal Title Data
//      Key:   OpenAIApiKey
//      Value: sk-proj-...   (your OpenAI API key — NEVER ship in the client)
//
//   2. (Optional) Tune PG13_THRESHOLDS below before deploying. Whatever you set
//      here is the canonical, hack-proof gate — clients cannot override it.
//
//   3. Bump SERVER_VERSION on each deploy so the client log proves it ran your
//      newest revision (and not a stale cached one).
// =============================================================================

var SERVER_VERSION = "moderate-avatar-1.1";

var PG13_THRESHOLDS = {
    "sexual":           0.09,
    "sexual/minors":    0.00, // zero-tolerance
    "violence":         0.25,
    "violence/graphic": 0.10,
    "harassment":       0.25,
    "hate":             0.25,
    "hate/threatening": 0.15,
    "self-harm":        0.02
};

// Severity order — first rejection wins. Matches Pg13ImagePolicy.cs on the client.
var SEVERITY_ORDER = [
    "sexual/minors",
    "sexual",
    "self-harm",
    "violence/graphic",
    "hate/threatening",
    "violence",
    "hate",
    "harassment"
];

handlers.ModerateAvatarImage = function (args, context) {
    var imageBase64 = args && args.imageBase64;
    var mimeType = (args && args.mimeType) || "image/jpeg";

    var nowIso = (new Date()).toISOString();

    if (!imageBase64) {
        return errorReply("imageBase64 parameter is required.", nowIso);
    }

    // Server-only secret. Stored in Title Internal Data, never sent to the client.
    var titleData;
    try {
        titleData = server.GetTitleInternalData({ Keys: ["OpenAIApiKey"] });
    } catch (e) {
        return errorReply("Could not read Title Internal Data: " + (e.message || e), nowIso);
    }

    var apiKey = titleData && titleData.Data && titleData.Data["OpenAIApiKey"];
    if (!apiKey) {
        return errorReply("OpenAIApiKey is not set in PlayFab Title Internal Data.", nowIso);
    }

    var requestBody = JSON.stringify({
        input: [{
            type: "image_url",
            image_url: { url: "data:" + mimeType + ";base64," + imageBase64 }
        }],
        model: "omni-moderation-latest"
    });

    var raw;
    try {
        raw = http.request(
            "https://api.openai.com/v1/moderations",
            "post",
            requestBody,
            "application/json",
            { "Authorization": "Bearer " + apiKey }
        );
    } catch (e) {
        return errorReply("OpenAI request failed: " + (e.message || e), nowIso);
    }

    var parsed;
    try {
        parsed = JSON.parse(raw);
    } catch (e) {
        return errorReply("Could not parse OpenAI response: " + (e.message || e), nowIso);
    }

    if (!parsed.results || parsed.results.length === 0) {
        return errorReply("OpenAI returned no moderation results.", nowIso);
    }

    var r = parsed.results[0];
    var s = r.category_scores || {};

    // Evaluate against PG13 thresholds in severity order.
    var offendingCategory = null;
    var offendingScore = 0.0;
    for (var i = 0; i < SEVERITY_ORDER.length; i++) {
        var name = SEVERITY_ORDER[i];
        var score = s[name] || 0;
        if (score > PG13_THRESHOLDS[name]) {
            offendingCategory = name;
            offendingScore = score;
            break;
        }
    }
    if (!offendingCategory && r.flagged) {
        offendingCategory = "provider-flagged";
        offendingScore = highest(s);
    }

    var allowed = (offendingCategory === null);

    return {
        allowed: allowed,
        offendingCategory: offendingCategory,
        offendingScore: offendingScore,
        scores: {
            sexual:          s.sexual                 || 0,
            sexualMinors:    s["sexual/minors"]       || 0,
            violence:        s.violence               || 0,
            violenceGraphic: s["violence/graphic"]    || 0,
            harassment:      s.harassment             || 0,
            hate:            s.hate                   || 0,
            hateThreatening: s["hate/threatening"]    || 0,
            selfHarm:        s["self-harm"]           || 0
        },
        // Echo the thresholds back so the client can detect drift between client-side
        // inspector values and the deployed CloudScript values. Mapping the slash/dash
        // keys to camelCase keeps the C# typed POCO simple.
        thresholds: {
            sexual:          PG13_THRESHOLDS["sexual"],
            sexualMinors:    PG13_THRESHOLDS["sexual/minors"],
            violence:        PG13_THRESHOLDS["violence"],
            violenceGraphic: PG13_THRESHOLDS["violence/graphic"],
            harassment:      PG13_THRESHOLDS["harassment"],
            hate:            PG13_THRESHOLDS["hate"],
            hateThreatening: PG13_THRESHOLDS["hate/threatening"],
            selfHarm:        PG13_THRESHOLDS["self-harm"]
        },
        providerFlagged: r.flagged === true,
        serverVersion: SERVER_VERSION,
        serverTimestamp: nowIso,
        error: ""
    };
};

function errorReply(message, nowIso) {
    return {
        allowed: false,
        offendingCategory: null,
        offendingScore: 0,
        scores: null,
        providerFlagged: false,
        serverVersion: SERVER_VERSION,
        serverTimestamp: nowIso,
        error: message
    };
}

function highest(scores) {
    var max = 0;
    for (var key in scores) {
        if (Object.prototype.hasOwnProperty.call(scores, key)) {
            if (scores[key] > max) max = scores[key];
        }
    }
    return max;
}