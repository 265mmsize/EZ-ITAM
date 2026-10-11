namespace EzItam.Agent;

public sealed class AgentOptions
{
    public const string AgentVersion = "1.0.0";

    /// <summary>서버 기본 주소. 에이전트가 아웃바운드 HTTPS(443)로만 접속합니다.</summary>
    public string ServerUrl { get; set; } = "https://itam.corp.local/";

    public string EnrollmentKey { get; set; } = string.Empty;

    /// <summary>인벤토리 전송 최소 간격(시간). 서버 응답의 InventoryDue 가 우선합니다.</summary>
    public int InventoryHours { get; set; } = 24;

    /// <summary>true 이면 WMI 대신 가짜 데이터를 사용합니다 (개발·프로토콜 검증용).</summary>
    public bool UseFakeCollector { get; set; }

    public string FakeDeviceUuid { get; set; } = "FAKE-UUID-0001";

    /// <summary>상태 파일 위치. 비우면 Windows 는 %ProgramData%\EzItam\Agent, 그 외는 실행 폴더를 씁니다.</summary>
    public string StateDirectory { get; set; } = string.Empty;

    public string ResolveStateDirectory()
    {
        if (!string.IsNullOrWhiteSpace(StateDirectory)) return StateDirectory;
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EzItam", "Agent");
        return Path.Combine(AppContext.BaseDirectory, "state");
    }
}
