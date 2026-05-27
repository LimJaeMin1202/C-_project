using MaterialDesignThemes.Wpf;

namespace StudyPlanner.Services
{
    // MaterialDesign 테마(Light/Dark) 전환 서비스
    // - PaletteHelper로 전체 앱의 색상 팔레트를 런타임에 교체
    // - DynamicResource로 바인딩된 모든 컨트롤이 자동으로 갱신됨
    public static class ThemeService
    {
        public static void ApplyTheme(bool isDark)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
    }
}
