using System.Drawing;

namespace CyberSecurityAwarenessChatbot;

public sealed class TaskManagerForm : Form
{
    private readonly TaskRepository _repo;
    private readonly ActivityLog _log;
    private readonly DataGridView _grid=new();

    public TaskManagerForm(TaskRepository repo,ActivityLog log)
    {
        _repo=repo;_log=log;Text="Cyber Bot — Task Manager";Size=new Size(900,540);StartPosition=FormStartPosition.CenterParent;
        BackColor=Color.FromArgb(12,24,43);ForeColor=Color.White;
        _grid.Dock=DockStyle.Fill;_grid.ReadOnly=true;_grid.AutoGenerateColumns=false;_grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect=false;_grid.AllowUserToAddRows=false;_grid.BackgroundColor=Color.FromArgb(19,35,57);
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="ID",DataPropertyName=nameof(TaskItem.Id),Width=55});
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="Title",DataPropertyName=nameof(TaskItem.Title),Width=190});
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="Description",DataPropertyName=nameof(TaskItem.Description),Width=260});
        _grid.Columns.Add(new DataGridViewTextBoxColumn{HeaderText="Reminder",DataPropertyName=nameof(TaskItem.ReminderAt),Width=160});
        _grid.Columns.Add(new DataGridViewCheckBoxColumn{HeaderText="Complete",DataPropertyName=nameof(TaskItem.IsCompleted),Width=85});
        var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=58,Padding=new Padding(8)};
        AddButton(bar,"Add Task",AddTaskAsync);AddButton(bar,"Set Reminder",SetReminderAsync);AddButton(bar,"Mark Complete",CompleteAsync);AddButton(bar,"Delete",DeleteAsync);AddButton(bar,"Refresh",RefreshAsync);
        Controls.Add(_grid);Controls.Add(bar);Shown+=async(_,_)=>await RefreshAsync();
    }

    private static void AddButton(Control parent,string label,Func<Task> action)
    {
        var b=new Button{Text=label,AutoSize=true,BackColor=Color.Teal,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
        b.Click+=async(_,_)=>await action();parent.Controls.Add(b);
    }
    private TaskItem? Selected=>_grid.CurrentRow?.DataBoundItem as TaskItem;

    private async Task RefreshAsync()
    {
        try{_grid.DataSource=await _repo.GetAllAsync();}
        catch(Exception ex){MessageBox.Show(this,ex.Message,"Database error",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }

    private async Task AddTaskAsync()
    {
        using var f=new Form{Text="Add Task",Size=new Size(430,310),StartPosition=FormStartPosition.CenterParent};
        var title=new TextBox{Left=18,Top=38,Width=375};var desc=new TextBox{Left=18,Top=100,Width=375,Height=90,Multiline=true};
        f.Controls.AddRange([new Label{Text="Task title",Left=18,Top=15},title,new Label{Text="Description",Left=18,Top=77},desc]);
        var ok=new Button{Text="Save",Left=300,Top=215,DialogResult=DialogResult.OK};f.Controls.Add(ok);f.AcceptButton=ok;
        if(f.ShowDialog(this)==DialogResult.OK)
        {
            if(string.IsNullOrWhiteSpace(title.Text)){MessageBox.Show(this,"Enter a task title.");return;}
            await _repo.AddAsync(new TaskItem{Title=title.Text.Trim(),Description=desc.Text.Trim()});
            _log.Add("Task created: "+title.Text.Trim());await RefreshAsync();
        }
    }

    private async Task SetReminderAsync()
    {
        var task=Selected;if(task is null){MessageBox.Show(this,"Select a task first.");return;}
        using var f=new Form{Text="Set Reminder",Size=new Size(350,165),StartPosition=FormStartPosition.CenterParent};
        var picker=new DateTimePicker{Left=16,Top=18,Width=300,Format=DateTimePickerFormat.Custom,CustomFormat="yyyy-MM-dd HH:mm",ShowUpDown=true};
        var ok=new Button{Text="Save",Left=240,Top=65,DialogResult=DialogResult.OK};f.Controls.AddRange([picker,ok]);
        if(f.ShowDialog(this)==DialogResult.OK){await _repo.SetReminderAsync(task.Id,picker.Value);_log.Add($"Reminder set for task #{task.Id}");await RefreshAsync();}
    }

    private async Task CompleteAsync()
    {
        var task=Selected;if(task is null){MessageBox.Show(this,"Select a task first.");return;}
        await _repo.SetCompletedAsync(task.Id,true);_log.Add($"Task completed: {task.Title}");await RefreshAsync();
    }

    private async Task DeleteAsync()
    {
        var task=Selected;if(task is null){MessageBox.Show(this,"Select a task first.");return;}
        if(MessageBox.Show(this,$"Delete '{task.Title}'?","Confirm deletion",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes)
        {await _repo.DeleteAsync(task.Id);_log.Add($"Task deleted: {task.Title}");await RefreshAsync();}
    }
}