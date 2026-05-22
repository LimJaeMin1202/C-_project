using System.Windows;
using StudyPlanner.Data;
using StudyPlanner.Models;

namespace StudyPlanner
{
    /// <summary>
    /// 메인 화면 — 학습 주제 등록 및 목록 조회
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 프로그램 시작 시: DB 파일이 없으면 자동 생성
            using (var db = new StudyDbContext())
            {
                db.Database.EnsureCreated();
            }

            dpStudyDate.SelectedDate = DateTime.Today;  // 학습일 기본값 = 오늘
            LoadTopics();                               // 저장돼 있던 데이터 불러오기
        }

        // DB에서 학습 주제 목록을 불러와 오른쪽 표(DataGrid)에 표시
        private void LoadTopics()
        {
            using (var db = new StudyDbContext())
            {
                // 학습일 최신순으로 정렬해서 가져오기
                dgTopics.ItemsSource = db.StudyTopics
                                         .OrderByDescending(t => t.StudyDate)
                                         .ToList();
            }
        }

        // [등록] 버튼 클릭 시 실행
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // --- 입력 검증 ---
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

            // 학습일 (선택 안 했으면 오늘 날짜 사용)
            DateTime studyDate = dpStudyDate.SelectedDate ?? DateTime.Today;

            // --- 새 학습 주제 객체 생성 ---
            var topic = new StudyTopic
            {
                Subject = txtSubject.Text.Trim(),
                Unit = txtUnit.Text.Trim(),
                StudyDate = studyDate,
                Memo = txtMemo.Text.Trim(),
                NextReviewDate = studyDate.AddDays(1)  // 1주차: 우선 다음날을 첫 복습일로 (2주차에 SM-2로 교체)
            };

            // --- DB에 저장 ---
            using (var db = new StudyDbContext())
            {
                db.StudyTopics.Add(topic);
                db.SaveChanges();
            }

            // --- 입력 폼 초기화 + 목록 새로고침 ---
            txtSubject.Clear();
            txtUnit.Clear();
            txtMemo.Clear();
            dpStudyDate.SelectedDate = DateTime.Today;
            LoadTopics();
        }
    }
}
