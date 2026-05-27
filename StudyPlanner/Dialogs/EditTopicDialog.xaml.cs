using System.Windows;
using StudyPlanner.Models;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;

namespace StudyPlanner.Dialogs
{
    // 학습 주제 편집 다이얼로그 (모달)
    // - 호출 측에서 StudyTopic 객체를 넘기면 폼에 채워 보여주고
    // - 저장 시 같은 객체에 변경 내용을 반영한 뒤 DialogResult=true 로 닫힘
    public partial class EditTopicDialog : Window
    {
        private readonly StudyTopic topic;

        // 사용자가 '삭제'를 눌렀는지 외부에서 알 수 있도록 (호출 측에서 DB 삭제 처리)
        public bool DeleteRequested { get; private set; } = false;

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

        // 다이얼로그 뜨자마자 과목 필드에 포커스
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSubject.Focus();
            txtSubject.CaretIndex = txtSubject.Text?.Length ?? 0;
        }

        // Esc → 취소
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                btnCancel_Click(this, new RoutedEventArgs());
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                $"'{topic.Subject} - {topic.Unit}' 주제를 정말 삭제하시겠습니까?",
                "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            DeleteRequested = true;
            DialogResult = true;   // 호출 측이 DeleteRequested 확인 후 처리
            Close();
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
