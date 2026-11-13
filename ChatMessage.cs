namespace CyberSecurityAwarenessChatbot;

public sealed record ChatMessage(string Sender, string Text, bool IsBot, DateTime Timestamp)
{
    public ChatMessage(string sender, string text, bool isBot) : this(sender, text, isBot, DateTime.Now) { }
}