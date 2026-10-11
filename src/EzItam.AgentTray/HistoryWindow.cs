using System.Windows.Forms;

namespace EzItam.AgentTray;

public sealed class HistoryWindow : Form
{
    private readonly AgentStatusReader _reader;
    private readonly Label _summaryLabel;
    private readonly ListView _historyList;

    public HistoryWindow(AgentStatusReader reader)
    {
        _reader = reader;

        Text = "EzItam 에이전트 - 수집 내역";
        Width = 640;
        Height = 420;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = true;
        MaximizeBox = false;

        _summaryLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(10),
            Text = "불러오는 중..."
        };

        _historyList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        _historyList.Columns.Add("시각 (로컬)", 160);
        _historyList.Columns.Add("종류", 90);
        _historyList.Columns.Add("상태", 100);
        _historyList.Columns.Add("내용", 260);

        var refreshButton = new Button { Text = "새로고침", Dock = DockStyle.Bottom, Height = 32 };
        refreshButton.Click += (_, _) => Refresh_Data();

        Controls.Add(_historyList);
        Controls.Add(refreshButton);
        Controls.Add(_summaryLabel);

        // 트레이 메뉴의 "종료"로만 완전히 닫히도록, 창의 X 버튼은 숨기기만 합니다.
        FormClosing += (_, e) =>
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        };

        Refresh_Data();
    }

    public void Refresh_Data()
    {
        var summary = _reader.ReadSummary();
        _summaryLabel.Text = summary.Enrolled
            ? $"AgentId: {summary.AgentId}   마지막 수집(UTC): {summary.LastInventoryUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? "없음"}"
            : "아직 서버에 등록되지 않았습니다.";

        _historyList.BeginUpdate();
        _historyList.Items.Clear();
        foreach (var entry in _reader.ReadHistory())
        {
            var item = new ListViewItem(entry.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(entry.Kind);
            item.SubItems.Add(entry.Status);
            item.SubItems.Add(entry.Message);
            _historyList.Items.Add(item);
        }
        _historyList.EndUpdate();
    }
}
