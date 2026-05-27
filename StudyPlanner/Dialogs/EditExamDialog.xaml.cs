using System.Windows;
using StudyPlanner.Models;
using MessageBox = System.Windows.MessageBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Key = System.Windows.Input.Key;

namespace StudyPlanner.Dialogs
{
    // 시험 편집 다이얼로그 (모달)
    public partial class EditExamDialog : Window
    {
        private readonly Exam exam;
        public bool DeleteRequested { get; private set; } = false;

        public EditExamDialog(Exam exam)
        {
            InitializeComponent();
            this.exam = exam;

            txtSubject.Text = exam.Subject;
            txtName.Text = exam.Name;
            dpExamDate.SelectedDate = exam.ExamDate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtSubject.Focus();
            txtSubject.CaretIndex = txtSubject.Text?.Length ?? 0;
        }

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
                $"'{exam.Subject} - {exam.Name}' 시험을 정말 삭제하시겠습니까?",
                "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            DeleteRequested = true;
            DialogResult = true;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                MessageBox.Show("과목을 입력해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dpExamDate.SelectedDate == null)
            {
                MessageBox.Show("시험일을 선택해주세요.", "알림",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            exam.Subject = txtSubject.Text.Trim();
            exam.Name = string.IsNullOrWhiteSpace(txtName.Text) ? "시험" : txtName.Text.Trim();
            exam.ExamDate = dpExamDate.SelectedDate.Value;

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
