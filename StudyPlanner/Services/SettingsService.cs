using System.IO;
using System.Text.Json;

namespace StudyPlanner.Services
{
    // 앱 설정(다크모드 등)을 JSON 파일로 영속화한다.
    // - 실행 폴더의 settings.json 사용
    // - 첫 호출 시 메모리에 캐싱
    public static class SettingsService
    {
        private const string FileName = "settings.json";

        // 저장 대상 설정
        public class AppSettings
        {
            public bool DarkMode { get; set; } = false;

            // ── 자동 알림 ──
            public bool DailyNotificationEnabled { get; set; } = false;
            public string DailyNotificationTime { get; set; } = "09:00";     // "HH:mm" 형식
            public string LastNotificationDate { get; set; } = "";           // 마지막으로 알림 발송한 날 ("yyyy-MM-dd")
        }

        private static AppSettings? cached;

        // 디스크에서 설정을 읽어옴 (없으면 기본값)
        public static AppSettings Load()
        {
            if (cached != null) return cached;
            if (!File.Exists(FileName))
            {
                cached = new AppSettings();
                return cached;
            }
            try
            {
                cached = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FileName))
                         ?? new AppSettings();
            }
            catch
            {
                // 파일 손상 등 → 기본값 사용
                cached = new AppSettings();
            }
            return cached;
        }

        // 현재 캐싱된 설정을 디스크에 저장
        public static void Save()
        {
            if (cached == null) return;
            var json = JsonSerializer.Serialize(cached,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }
    }
}
