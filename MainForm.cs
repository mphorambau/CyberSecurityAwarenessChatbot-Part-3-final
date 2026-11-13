using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Main Part 2-style conversational GUI with Part 3 task, quiz, NLP and activity features.</summary>
public sealed class MainForm : Form
{
    private readonly ChatbotEngine _chatbot=new();
    private readonly NlpIntentRouter _router=new();
    private readonly ActivityLog _activity=new();
    private readonly RichTextBox _conversation=new();
    private readonly TextBox _input=new();
    private readonly TaskRepository? _tasks;
    private string _userName="Friend";

    public MainForm()
    {
        Text="Cyber Bot | Cybersecurity Awareness Assistant";
        StartPosition=FormStartPosition.CenterScreen;MinimumSize=new Size(900,650);Size=new Size(1120,760);
        BackColor=Color.FromArgb(12,22,37);ForeColor=Color.White;Font=new Font("Segoe UI",10);
        string? connection=Environment.GetEnvironmentVariable("CYBERBOT_MYSQL");
        if(!string.IsNullOrWhiteSpace(connection))_tasks=new TaskRepository(connection);
        _chatbot.ResponseGenerated+=OnBotReply;
        BuildInterface();
        Shown+=(_,_)=>BeginConversation();
    }

    private void BuildInterface()
    {
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,BackColor=BackColor,ColumnCount=2,RowCount=3};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,265));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,88));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,88));
        Controls.Add(layout);

        var header=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(18,42,66)};
        header.Controls.Add(new Label{AutoSize=true,Text="CYBERSECURITY AWARENESS ASSISTANT",ForeColor=Color.FromArgb(85,224,190),Font=new Font("Segoe UI",19,FontStyle.Bold),Location=new Point(24,12)});
        header.Controls.Add(new Label{AutoSize=true,Text="Stay safe online • Think before you click",ForeColor=Color.FromArgb(184,216,235),Location=new Point(28,53)});
        layout.Controls.Add(header,0,0);layout.SetColumnSpan(header,2);

        var side=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(14),BackColor=Color.FromArgb(17,33,53)};
        side.Controls.Add(new Label{Text="QUICK TOPICS",AutoSize=true,ForeColor=Color.FromArgb(184,216,235),Font=new Font("Segoe UI",10,FontStyle.Bold),Margin=new Padding(4,4,4,10)});
        AddTopic(side,"Phishing scams","Tell me about phishing");
        AddTopic(side,"Strong passwords","How do I create strong passwords?");
        AddTopic(side,"Suspicious links","How can I identify suspicious links?");
        AddTopic(side,"Malware and viruses","Explain malware");
        AddTopic(side,"Online privacy","How can I protect my privacy?");
        AddTopic(side,"Online scams","How do I avoid scams?");
        side.Controls.Add(new Label{Text="PART 3 TOOLS",AutoSize=true,ForeColor=Color.FromArgb(184,216,235),Font=new Font("Segoe UI",10,FontStyle.Bold),Margin=new Padding(4,18,4,8)});
        AddAction(side,"Task assistant",OpenTasks);
        AddAction(side,"Cybersecurity quiz",OpenQuiz);
        AddAction(side,"Activity log",ShowActivity);
        var logo=new Label{Text="  .-----------------.\n  |   STAY SAFE     |\n  |     ONLINE      |\n  '-----------------'\n        ||||\n     ___||||___\n    /  CYBER   \\\n   /   BOT      \\\n  /______________\\",AutoSize=true,ForeColor=Color.FromArgb(85,224,190),Font=new Font("Consolas",10,FontStyle.Bold),Margin=new Padding(4,24,4,4)};
        side.Controls.Add(logo);layout.Controls.Add(side,0,1);

        _conversation.Dock=DockStyle.Fill;_conversation.ReadOnly=true;_conversation.BackColor=Color.FromArgb(12,22,37);
        _conversation.ForeColor=Color.White;_conversation.BorderStyle=BorderStyle.None;_conversation.Font=new Font("Segoe UI",11);
        _conversation.DetectUrls=false;_conversation.ScrollBars=RichTextBoxScrollBars.Vertical;
        var transcriptPanel=new Panel{Dock=DockStyle.Fill,Padding=new Padding(24),BackColor=Color.FromArgb(12,22,37)};
        transcriptPanel.Controls.Add(_conversation);layout.Controls.Add(transcriptPanel,1,1);

        var composer=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(18),ColumnCount=2,BackColor=Color.FromArgb(18,42,66)};
        composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));
        _input.Dock=DockStyle.Fill;_input.Font=new Font("Segoe UI",11);_input.PlaceholderText="Ask about cybersecurity, tasks, reminders or the quiz...";
        var send=MakeButton("Send");send.Dock=DockStyle.Fill;send.Click+=async(_,_)=>await SendAsync();
        _input.KeyDown+=async(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;await SendAsync();}};
        composer.Controls.Add(_input,0,0);composer.Controls.Add(send,1,0);layout.Controls.Add(composer,0,2);layout.SetColumnSpan(composer,2);
    }

    private static Button MakeButton(string text)=>new(){Text=text,AutoSize=true,Height=38,BackColor=Color.FromArgb(29,85,103),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Margin=new Padding(3),Padding=new Padding(8,4,8,4)};
    private void AddTopic(Control parent,string label,string message){var b=MakeButton(label);b.Width=215;b.Click+=(_,_)=>Submit(message);parent.Controls.Add(b);}
    private void AddAction(Control parent,string label,Action action){var b=MakeButton(label);b.Width=215;b.Click+=(_,_)=>action();parent.Controls.Add(b);}

    private void BeginConversation()
    {
        using var prompt=new NamePrompt();
        if(prompt.ShowDialog(this)==DialogResult.OK)_userName=prompt.NameValue;
        _chatbot.RememberName(_userName);
        bool voice=VoicePlayer.TryPlayGreeting();
        Append("Cyber Bot",voice
            ? $"Welcome, {_userName}! The original recorded greeting is playing. I can help with phishing, passwords, suspicious links, malware, privacy and scams."
            : $"Welcome, {_userName}! I am your Cybersecurity Awareness Assistant. Learn how to protect yourself online: ask about phishing, passwords, suspicious links, malware, privacy or scams.");
        _activity.Add("Application started; personalised welcome displayed");
    }

    private async Task SendAsync(){string text=_input.Text.Trim();_input.Clear();Submit(text);await Task.CompletedTask;}
    private void Submit(string text)
    {
        if(string.IsNullOrWhiteSpace(text))return;
        Append(_userName,text);_activity.Add("Chatbot message received");
        ChatIntent intent=_router.Detect(text);_activity.Add($"NLP intent detected: {intent}");
        switch(intent)
        {
            case ChatIntent.TaskAdd: _=AddTaskFromChatAsync(text);return;
            case ChatIntent.TaskList: OpenTasks();return;
            case ChatIntent.TaskDelete: Append("Cyber Bot","Open Task assistant, select the task and choose Delete so the correct record is removed.");OpenTasks();return;
            case ChatIntent.TaskComplete: Append("Cyber Bot","Open Task assistant, select the task and choose Mark Complete.");OpenTasks();return;
            case ChatIntent.Reminder: Append("Cyber Bot","To create a reminder, open Task assistant, add or select a task, then choose Set Reminder and select its date and time.");OpenTasks();return;
            case ChatIntent.Quiz: OpenQuiz();return;
            case ChatIntent.Activity: ShowActivity();return;
            default: _chatbot.Process(text);return;
        }
    }

    private async Task AddTaskFromChatAsync(string text)
    {
        if(_tasks is null){Append("Cyber Bot","MySQL is not configured yet. Set CYBERBOT_MYSQL and execute database/schema.sql. You can still use the chat and quiz.");return;}
        string title=text;
        foreach(string prefix in new[]{"please remind me to","remind me to","remember to","add task","create task","new task"})
            if(title.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)){title=title[prefix.Length..].Trim();break;}
        if(title.Length<2)title="New cybersecurity task";
        try{int id=await _tasks.AddAsync(new TaskItem{Title=title,Description="Created from the chatbot conversation"});_activity.Add($"Task created from chat: {title}");Append("Cyber Bot",$"I saved task #{id}: {title}. Open Task assistant to add details or set a reminder.");}
        catch(Exception ex){Append("Cyber Bot","I could not save the task. Check your MySQL connection and schema. Details: "+ex.Message);}
    }

    private void OpenTasks()
    {
        _activity.Add("Task assistant opened");
        if(_tasks is null){MessageBox.Show(this,"Set the CYBERBOT_MYSQL environment variable and execute database/schema.sql first. Chat and quiz remain available.","MySQL setup required",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}
        using var form=new TaskManagerForm(_tasks,_activity);form.ShowDialog(this);
    }
    private void OpenQuiz(){using var form=new QuizForm(_activity);form.ShowDialog(this);}
    private void ShowActivity(){_activity.Add("Activity history requested");Append("Cyber Bot","Your latest activity:\n"+string.Join("\n",_activity.Latest(10)));}
    private void OnBotReply(ChatMessage message)=>Append(message.Sender,message.Text);
    private void Append(string sender,string message)
    {
        _conversation.SelectionStart=_conversation.TextLength;
        _conversation.SelectionColor=sender=="Cyber Bot"?Color.FromArgb(85,224,190):Color.FromArgb(143,197,255);
        _conversation.SelectionFont=new Font(_conversation.Font,FontStyle.Bold);
        _conversation.AppendText($"{sender}: ");
        _conversation.SelectionColor=Color.FromArgb(232,242,248);
        _conversation.SelectionFont=new Font(_conversation.Font,FontStyle.Regular);
        _conversation.AppendText(message+Environment.NewLine+Environment.NewLine);
        _conversation.ScrollToCaret();
    }
}