using System.Windows;
using StudyPlanner.Models;
using MessageBox = System.Windows.MessageBox;

namespace StudyPlanner.Dialogs
{
    // 시험 편집 다이얼로그 (모달)
    public partial class EditExamDialog : Window
    {
        private readonly Exam exam;

        public EditExamDialog(Exam exam)
        {
            InitializeComponent();
            this.exam = exam;

            txtSubject.Text = exam.Subject;
            txtName.Text = exam.Name;
            dpExamDate.SelectedDate = exam.ExamDate;
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
