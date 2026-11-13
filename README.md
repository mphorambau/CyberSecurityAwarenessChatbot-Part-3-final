# Cybersecurity Awareness Chatbot — Complete Part 3

Windows Forms C# application combining the original Part 1 foundation, Part 2 conversational enhancements and Part 3 assignment requirements.

## Part 1 functionality carried forward
- Cyber Bot welcome screen and stay-safe-online ASCII artwork in the GUI.
- Personalised name prompt with validation.
- Original topics: phishing, strong passwords, suspicious links, malware/viruses and online privacy.
- Optional recorded WAV greeting playback with graceful fallback if the file is unavailable.

## Part 2 functionality carried forward
- Windows Forms conversational interface and quick-topic buttons.
- Varied cybersecurity advice for phishing, passwords, suspicious links, malware, privacy and scams.
- In-session name and interest memory.
- Sentiment-aware responses for worried, anxious, scared, frustrated, overwhelmed, confused and curious users.
- Contextual follow-ups such as "tell me more" and "another tip".

## Part 3 requirements
- MySQL task assistant: create, list, set date/time reminders, mark complete and delete tasks using parameterized SQL.
- 12-question quiz with multiple-choice and true/false questions, one at a time, immediate feedback/explanations, score and encouraging final feedback.
- NLP simulation using normalization, regular expressions, keyword matching and varied phrases for task, reminder, quiz, activity and cybersecurity requests.
- Timestamped activity log records application start, chatbot/NLP actions, task changes, quiz start/completion and activity requests. Latest ten entries are viewable.

## Requirements
- Windows 10/11
- .NET 10 SDK
- MySQL 8.x for persistent task management
- Visual Studio with .NET desktop development workload, or VS Code with C# Dev Kit

## MySQL setup
1. Run `database/schema.sql` in MySQL Workbench or your MySQL client.
2. Set a Windows environment variable named `CYBERBOT_MYSQL`, for example:
   `Server=localhost;Database=cyber_awareness;User ID=root;Password=YOUR_PASSWORD;`
3. Restart Visual Studio after setting the variable. Never commit real credentials.

Chat and quiz work without MySQL; task persistence requires a configured database.

## Build and run in Visual Studio
1. Clone this repository and open `CyberSecurityAwarenessChatbot.csproj`.
2. Allow NuGet restore to complete, or run `dotnet restore`.
3. Build with `dotnet build CyberSecurityAwarenessChatbot.csproj`.
4. Run with `dotnet run --project CyberSecurityAwarenessChatbot.csproj`.

## Voice greeting
The application looks for `Assets/greeting.wav` and copies it to the output folder when present. The voice-player integration is included; the original WAV binary must be added to `Assets/greeting.wav` to enable playback. The Part 1 and Part 2 repositories remain unchanged.

## Continuous integration
The `.github/workflows/build.yml` workflow runs restore and a Release build on Windows for pushes, pull requests and manual dispatch. Check the Actions tab for a green run before treating the project as build-verified.

## Source references
- Part 1: https://github.com/mphorambau/CyberSecurityAwarenessChatbot
- Part 2: https://github.com/mphorambau/CyberSecurityAwarenessChatbot-Part2

## Presentation
Add the unlisted YouTube voice-over presentation URL here after recording the final running application. Demonstrate the Part 1 welcome and chat, Part 2 sentiment/follow-up behaviour, Part 3 task operations and reminders, quiz/scoring, NLP examples and activity history.
