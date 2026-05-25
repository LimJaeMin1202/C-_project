using System.Windows;
using StudyPlanner.Models;
using MessageBox = System.Windows.MessageBox;

namespace StudyPlanner.Dialogs
{
    // 학습 주제 편집 다이얼로그 (모달)
    // - 호출 측에서 StudyTopic 객체를 넘기면 폼에 채워 보여주고
    // - 저장 시 같은 객체에 변경 내용을 반영한 뒤 DialogResult=true 로 닫힘
    public partial class EditTopicDialog : Window
    {
        private readonly StudyTopic topic;

        public EditTopicDialog(StudyTopic topic)
        {
            InitializeComponent();
            this.topic = topic;

            // 폼에 기존 값 채우기
            txtSubject.Text = topic.Subject;
            txtUnit.Text = topic.Unit;
            dpStudyDate.SelectedDate = topic.StudyDate;
            txtMemo.Text = topic.Memo;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 입력 검증
            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                MessageBox.Show("과목을 입력해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                MessageBox.Show("단원/주제를 입력해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 객체에 반영 (DB 저장은 호출 측에서)
            topic.Subject = txtSubject.Text.Trim();
            topic.Unit = txtUnit.Text.Trim();
            topic.StudyDate = dpStudyDate.SelectedDate ?? DateTime.Today;
            topic.Memo = txtMemo.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
