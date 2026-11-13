namespace CyberSecurityAwarenessChatbot;

/// <summary>
/// Keeps the chatbot's conversation state and selects useful cybersecurity guidance.
/// The user interface subscribes to <see cref="ResponseGenerated"/> instead of needing
/// to know how keywords, memory, or sentiment are handled.
/// </summary>
internal sealed class ChatbotEngine
{
    private readonly Random random = new();
    private readonly Dictionary<string, string[]> topicResponses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["phishing"] =
        [
            "Phishing messages try to rush you into sharing a password, OTP, or banking detail. Pause before clicking anything, check the complete sender address, and contact the organisation through its official website or phone number.\n\nNever share a password or one-time code: legitimate support staff will not ask for them.",
            "Treat an unexpected request for information as suspicious, even if the message uses a familiar logo. Look for misspelled domains, strange grammar, threats, or pressure to act immediately.\n\nSafest action: do not use the message link; open your browser and visit the organisation’s official site yourself.",
            "A convincing phishing message may pretend to be your bank, delivery company, lecturer, or a friend. Verify it using a separate method before responding.\n\nIf you already clicked a suspicious link, change the affected password from the real website and tell a trusted adult or IT support person."
        ],
        ["password"] =
        [
            "Use a long, unique passphrase for every account — for example, four unrelated words with numbers or symbols added. A password manager can generate and safely remember strong passwords.\n\nReusing one password is risky because a leak on one site can expose all your accounts.",
            "Avoid names, birthdays, favourite teams, and common words in passwords. Turn on multi-factor authentication wherever it is available; an authenticator app is usually safer than an SMS code.\n\nNever approve a login notification you did not request.",
            "Aim for at least 12 characters and make every password unique. Do not share it with anyone, including a person claiming to be support.\n\nIf you think a password was exposed, change it immediately and sign out of other devices in that account’s security settings."
        ],
        ["link"] =
        [
            "Do not click unexpected links. On a computer, hover over the link to preview the full address; on a phone, press and hold it carefully.\n\nIf it seems urgent, open the official website or app yourself instead of using the link.",
            "Shortened, misspelled, or look-alike web addresses can hide dangerous sites. Check that the spelling after the final dot is correct, such as the real company domain.\n\nWhen uncertain, search for the organisation independently or ask a trusted person before opening it.",
            "A link is not automatically safe because it came from a friend: their account could have been compromised. Verify using a different conversation before opening attachments or links.\n\nNever enter a password on a page reached from an unexpected message."
        ],
        ["malware"] =
        [
            "Malware is harmful software that can steal data, show unwanted adverts, spy on activity, or lock files for money. Keep Windows, browsers, and apps updated, and install software only from trusted stores or official websites.\n\nUse reputable security protection and back up important work regularly.",
            "Avoid cracked software, unknown USB drives, and unexpected email attachments: these are common ways malware reaches a device. Scan downloads before opening them.\n\nA backup stored separately from your device can protect your work if ransomware locks your files.",
            "Warning signs include sudden pop-ups, a very slow device, unknown programs, or files you cannot open. If this happens after a download, disconnect from the internet and do not enter passwords.\n\nAsk a trusted adult, lecturer, or IT support person for help rather than trying random fixes."
        ],
        ["privacy"] =
        [
            "Protect privacy by sharing less personal information online and reviewing social-media privacy settings. A post can reveal your location, routines, school, family, or other details even when it looks harmless.\n\nBefore posting, ask: would I be comfortable if a stranger saw this?",
            "Choose the smallest audience setting that makes sense for a post. Keep your address, school, phone number, ID details, and travel plans private.\n\nTurn off location sharing for apps that do not genuinely need it, especially when posting photos in real time.",
            "Review app permissions regularly. A torch app, for example, does not need your contacts or microphone. Remove permissions that are not necessary and uninstall apps you no longer use.\n\nUse a screen lock and keep accounts signed out on shared devices."
        ],
        ["scam"] =
        [
            "Scams often create urgency, promise a prize, or ask for payment or personal information. Slow down and verify before acting — pressure is a warning sign.\n\nDo not pay, click, or reply until you independently confirm who contacted you.",
            "Never send money, passwords, PINs, banking details, or OTP codes to someone who contacted you unexpectedly. Real banks and delivery companies do not need your secret code to help you.\n\nTalk to a trusted person if you are unsure, especially before making a payment.",
            "If an offer sounds too good to be true, it probably is. Search independently for the company, check its real contact details, and look for trustworthy reviews.\n\nIf you have paid a scammer, contact your bank quickly and preserve screenshots of the conversation."
        ]
    };

    private readonly Dictionary<string, string> memory = new(StringComparer.OrdinalIgnoreCase);
    private string? currentTopic;

    /// <summary>A delegate-based notification that lets any UI receive a completed reply.</summary>
    public event Action<ChatMessage>? ResponseGenerated;

    public void RememberName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            memory["name"] = name.Trim();
        }
    }

    public void Process(string? rawInput)
    {
        string input = rawInput?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            Reply("I did not receive a question. Try a topic such as phishing, passwords, scams, links, malware, or privacy.");
            return;
        }

        string lower = input.ToLowerInvariant();
        RememberDetails(lower, input);

        if (IsExit(lower))
        {
            string name = memory.GetValueOrDefault("name", "there");
            Reply($"Goodbye, {name}. Think before you click, and stay safe online!");
            return;
        }

        string? matchedTopic = FindTopic(lower);
        string? sentiment = DetectSentiment(lower);
        if (IsFollowUp(lower) && currentTopic is not null)
        {
            Reply(BuildReply(currentTopic, GetRandomResponse(currentTopic), isFollowUp: true, sentiment));
            return;
        }

        if (matchedTopic is not null)
        {
            currentTopic = matchedTopic;
            Reply(BuildReply(matchedTopic, GetRandomResponse(matchedTopic), isFollowUp: false, sentiment));
            return;
        }

        if (sentiment is not null)
        {
            string tip = currentTopic is not null
                ? GetRandomResponse(currentTopic)
                : "Take one safe step now: do not share passwords or OTP codes, and verify unexpected messages before responding.";
            Reply($"{sentiment} {tip}");
            return;
        }

        Reply("I am not sure I understand. Please rephrase, or ask about phishing, passwords, suspicious links, malware, privacy, or scams.");
    }

    private void RememberDetails(string lowerInput, string originalInput)
    {
        const string namePrefix = "my name is ";
        if (lowerInput.StartsWith(namePrefix, StringComparison.Ordinal))
        {
            string name = originalInput[namePrefix.Length..].Trim(' ', '.', '!', '?');
            if (name.Length >= 2)
            {
                memory["name"] = name;
            }
        }

        if (lowerInput.Contains("interested in") || lowerInput.Contains("favourite topic") || lowerInput.Contains("favorite topic"))
        {
            string? interest = FindTopic(lowerInput);
            if (interest is not null)
            {
                memory["interest"] = interest;
            }
        }
    }

    private string? FindTopic(string input) => topicResponses.Keys.FirstOrDefault(topic => topic switch
    {
        "phishing" => input.Contains("phishing") || input.Contains("phish"),
        "password" => input.Contains("password") || input.Contains("passphrase"),
        "link" => input.Contains("link") || input.Contains("url") || input.Contains("suspicious website"),
        "malware" => input.Contains("malware") || input.Contains("virus") || input.Contains("ransomware"),
        "privacy" => input.Contains("privacy") || input.Contains("personal information") || input.Contains("social media"),
        "scam" => input.Contains("scam") || input.Contains("fraud") || input.Contains("otp"),
        _ => false
    });

    private static bool IsExit(string input) => input is "exit" or "quit" or "bye" or "goodbye";

    private static bool IsFollowUp(string input) =>
        input.Contains("another tip") || input.Contains("tell me more") || input.Contains("explain more") ||
        input.Contains("more detail") || input.Contains("more information");

    private static string? DetectSentiment(string input)
    {
        if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious"))
        {
            return "It is completely understandable to feel worried. You are doing the right thing by checking first.";
        }

        if (input.Contains("frustrated") || input.Contains("overwhelmed") || input.Contains("confused"))
        {
            return "Cybersecurity can feel overwhelming, so let us keep it simple and take one step at a time.";
        }

        if (input.Contains("curious") || input.Contains("interested"))
        {
            return "Great question — curiosity is one of the best ways to build safer online habits.";
        }

        return null;
    }

    private string GetRandomResponse(string topic)
    {
        string[] choices = topicResponses[topic];
        return choices[random.Next(choices.Length)];
    }

    private string BuildReply(string topic, string advice, bool isFollowUp, string? sentiment)
    {
        string lead = isFollowUp ? "Here is another tip: " : string.Empty;
        if (sentiment is not null)
        {
            lead += sentiment + " ";
        }
        if (memory.TryGetValue("interest", out string? interest) && interest == topic)
        {
            lead += $"Since you told me you are interested in {topic}, ";
        }

        return lead + advice;
    }

    private void Reply(string text) => ResponseGenerated?.Invoke(new ChatMessage("Cyber Bot", text, IsBot: true));
}
