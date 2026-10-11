namespace EzItam.Agent;

public sealed class AgentOptions
{
    /// <summary>빌드(csproj Version)에서 읽은 에이전트 버전. 업데이트 판단과 서버 보고에 쓰입니다.</summary>
    public static readonly string AgentVersion = typeof(AgentOptions).Assembly.GetName().Version!.ToString(3);

    /// <summary>서버 기본 주소. 에이전트가 아웃바운드 HTTPS(443)로만 접속합니다.</summary>
    public string ServerUrl { get; set; } = "https://itam.corp.local/";

    public string EnrollmentKey { get; set; } = string.Empty;

    /// <summary>인벤토리 전송 최소 간격(시간). 서버 응답의 InventoryDue 가 우선합니다.</summary>
    public int InventoryHours { get; set; } = 24;

    /// <summary>true 이면 WMI 대신 가짜 데이터를 사용합니다 (개발·프로토콜 검증용).</summary>
    public bool UseFakeCollector { get; set; }

    /// <summary>업데이트 적용 전 무작위 대기(분). 300대가 동시에 교체되지 않도록 분산합니다.</summary>
    public int UpdateJitterMinutes { get; set; } = 30;

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
