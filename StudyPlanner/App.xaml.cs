using System.Configuration;
using System.Data;
using System.Windows;

namespace StudyPlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // WinForms 참조가 켜져 있어서 'Application' 이름이 모호함 →
    // System.Windows.Application(WPF) 임을 명시
    public partial class App : System.Windows.Application
    {
    }

}
