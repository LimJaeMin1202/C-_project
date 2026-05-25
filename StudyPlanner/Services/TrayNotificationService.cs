using System.Drawing;
using System.Windows.Forms;

namespace StudyPlanner.Services
{
    // Windows 작업 표시줄(트레이) 아이콘과 알림(balloon tip) 관리
    // - 앱이 실행되는 동안 시스템 트레이에 아이콘 표시
    // - 트레이 아이콘 더블클릭 / 우클릭 메뉴로 창 열기·종료
    // - ShowNotification()으로 풍선 알림 표시
    public class TrayNotificationService : IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Action onOpenRequested;

        public TrayNotificationService(Action onOpenRequested)
        {
            this.onOpenRequested = onOpenRequested;

            // 트레이 아이콘 생성 (Windows 기본 경보/알람 아이콘 사용)
            // ※ 진짜 종 모양 아이콘을 원하면 .ico 파일을 리소스에 추가하면 됨
            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Exclamation,
                Visible = true,
                Text = "망각곡선 학습 플래너"
            };

            // 더블클릭 → 본 창 열기
            notifyIcon.DoubleClick += (s, e) => this.onOpenRequested?.Invoke();

            // 우클릭 컨텍스트 메뉴
            var menu = new ContextMenuStrip();
            menu.Items.Add("열기", null, (s, e) => this.onOpenRequested?.Invoke());
            menu.Items.Add("종료", null, (s, e) => System.Windows.Application.Current.Shutdown());
            notifyIcon.ContextMenuStrip = menu;
        }

        // 풍선 알림 표시 (Windows 10/11에선 토스트 형태로 우측 하단에 뜸)
        public void ShowNotification(string title, string message, int durationMs = 5000)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(durationMs);
        }

        // 자원 해제 (앱 종료 시 호출)
        public void Dispose()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }
}
