using System.Drawing;
using System.Drawing.Drawing2D;
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

            // 트레이 아이콘 생성 (종 모양을 GDI+로 직접 그려서 사용)
            notifyIcon = new NotifyIcon
            {
                Icon = CreateBellIcon(),
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

        // 간단한 종(bell) 모양 아이콘을 GDI+로 직접 그려 반환
        // (Windows 시스템 아이콘에 종 모양이 없어서 작은 비트맵을 그려 사용)
        private static Icon CreateBellIcon()
        {
            int size = 32;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Color bellColor = Color.FromArgb(63, 81, 181); // indigo (앱 메인 색)
                using (var brush = new SolidBrush(bellColor))
                {
                    // 윗쪽 손잡이 (작은 막대)
                    g.FillRectangle(brush, 14, 2, 4, 3);

                    // 종 본체 (위는 둥근 원, 아래는 사다리꼴로 벌어짐)
                    g.FillEllipse(brush, 7, 5, 18, 16);
                    Point[] flare = {
                        new Point(7, 14),
                        new Point(25, 14),
                        new Point(28, 22),
                        new Point(4, 22),
                    };
                    g.FillPolygon(brush, flare);

                    // 아래 테두리
                    g.FillRectangle(brush, 3, 22, 26, 2);

                    // 종 추(clapper)
                    g.FillEllipse(brush, 14, 25, 4, 4);
                }
            }
            return Icon.FromHandle(bmp.GetHicon());
        }
    }
}
