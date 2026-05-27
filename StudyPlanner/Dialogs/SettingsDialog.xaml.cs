using System.Windows;
using StudyPlanner.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;

namespace StudyPlanner.Dialogs
{
    // 앱 설정 다이얼로그 — 현재는 자동 알림 활성화/시각만 관리
    // (다크모드는 대시보드 토글 버튼에서 처리)
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();

            // ComboBox 채우기 (시 0~23, 분 0/5/10/.../55)
            cmbHour.ItemsSource = Enumerable.Range(0, 24).Select(h => h.ToString("D2")).ToList();
            cmbMinute.ItemsSource = Enumerable.Range(0, 12).Select(i => (i * 5).ToString("D2")).ToList();

            // 현재 설정 불러와 폼에 채우기
            var s = SettingsService.Load();
            chkNotifyEnabled.IsChecked = s.DailyNotificationEnabled;

            var parts = s.DailyNotificationTime.Split(':');
            string hour = parts.Length >= 1 ? parts[0].PadLeft(2, '0') : "09";
            string minute = parts.Length >= 2 ? parts[1].PadLeft(2, '0') : "00";

            // 분이 5분 단위에 정확히 안 맞으면 가장 가까운 5분 단위로 보정
            if (int.TryParse(minute, out int m))
                minute = ((m / 5) * 5).ToString("D2");

            cmbHour.SelectedItem = hour;
            cmbMinute.SelectedItem = minute;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var s = SettingsService.Load();
            s.DailyNotificationEnabled = chkNotifyEnabled.IsChecked == true;
            s.DailyNotificationTime = $"{cmbHour.SelectedItem}:{cmbMinute.SelectedItem}";

            // 설정 변경 시 "오늘 이미 알림 보냄" 상태 초기화 (변경 후 즉시 적용 가능하도록)
            s.LastNotificationDate = "";

            SettingsService.Save();

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chkNotifyEnabled.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                btnCancel_Click(this, new RoutedEventArgs());
            }
        }
    }
}
