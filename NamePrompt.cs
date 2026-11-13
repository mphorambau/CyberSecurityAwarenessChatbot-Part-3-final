using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

/// <summary>Validated personalised welcome prompt retained from the original chatbot flow.</summary>
internal sealed class NamePrompt : Form
{
    private readonly TextBox _name = new();
    private readonly Label _validation = new();
    public string NameValue => _name.Text.Trim();

    public NamePrompt()
    {
        Text = "Welcome to Cyber Bot";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(410, 190);
        MaximizeBox = false; MinimizeBox = false;
        BackColor = Color.FromArgb(18, 42, 66);
        Controls.Add(new Label { AutoSize=true, Location=new Point(24,22), Text="What should Cyber Bot call you?", ForeColor=Color.White, Font=new Font("Segoe UI",12,FontStyle.Bold) });
        Controls.Add(new Label { AutoSize=true, Location=new Point(24,55), Text="Your name helps personalise the advice.", ForeColor=Color.LightBlue });
        _name.SetBounds(24,88,360,26); _name.Font=new Font("Segoe UI",10); Controls.Add(_name);
        _validation.AutoSize=true; _validation.SetBounds(24,145,250,22); _validation.ForeColor=Color.Orange; Controls.Add(_validation);
        var ok=new Button { Text="Continue", Left=279, Top=137, Size=new Size(105,32), BackColor=Color.MediumAquamarine, FlatStyle=FlatStyle.Flat };
        ok.Click+=(_,_)=>{if(NameValue.Length<2){_validation.Text="Please enter at least two characters.";return;} DialogResult=DialogResult.OK;};
        Controls.Add(ok); AcceptButton=ok;
    }
}